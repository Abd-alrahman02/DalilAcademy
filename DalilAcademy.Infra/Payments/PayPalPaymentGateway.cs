using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DalilAcademy.Core.DTOs.Payments;
using DalilAcademy.Core.Service;
using Microsoft.Extensions.Options;

namespace DalilAcademy.Infra.Payments
{
    public class PayPalPaymentGateway(
        HttpClient httpClient,
        IOptions<PayPalSettings> options) : IPaymentGateway
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly PayPalSettings _settings = options.Value;

        public async Task<PayPalOrderResult> CreateOrderAsync(
            decimal amountUsd,
            string returnUrl,
            string cancelUrl,
            CancellationToken cancellationToken = default)
        {
            var token = await GetAccessTokenAsync(cancellationToken);

            using var request = new HttpRequestMessage(
                HttpMethod.Post, "/v2/checkout/orders");
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            request.Content = JsonContent.Create(new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                    new
                    {
                        amount = new
                        {
                            currency_code = "USD",
                            value = amountUsd.ToString("F2", CultureInfo.InvariantCulture)
                        }
                    }
                },
                application_context = new
                {
                    return_url = returnUrl,
                    cancel_url = cancelUrl
                }
            });

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            try
            {
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                var orderId = root.TryGetProperty("id", out var idElement)
                    ? idElement.GetString()
                    : null;

                string? approvalUrl = null;
                if (root.TryGetProperty("links", out var linksElement) &&
                    linksElement.ValueKind == JsonValueKind.Array)
                {
                    var approvalLink = linksElement.EnumerateArray()
                        .FirstOrDefault(link =>
                        {
                            return link.TryGetProperty("rel", out var relElement) &&
                                   relElement.GetString() is "approve" or "payer-action";
                        });

                    if (approvalLink.ValueKind != JsonValueKind.Undefined &&
                        approvalLink.TryGetProperty("href", out var hrefElement))
                    {
                        approvalUrl = hrefElement.GetString();
                    }
                }

                if (string.IsNullOrWhiteSpace(orderId) ||
                    string.IsNullOrWhiteSpace(approvalUrl))
                {
                    throw new HttpRequestException(
                        "PayPal did not return a valid order ID and approval URL.");
                }

                return new PayPalOrderResult(orderId, approvalUrl);
            }
            catch (JsonException exception)
            {
                throw new HttpRequestException(
                    "PayPal returned an invalid order response.",
                    exception);
            }
        }

        public async Task<bool> CaptureOrderAsync(
            string orderId,
            CancellationToken cancellationToken = default)
        {
            var token = await GetAccessTokenAsync(cancellationToken);

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/v2/checkout/orders/{Uri.EscapeDataString(orderId)}/capture");
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent("{}", Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if ((int)response.StatusCode >= 500)
            {
                throw new HttpRequestException(
                    "PayPal service returned a server error.",
                    null,
                    response.StatusCode);
            }

            if (!response.IsSuccessStatusCode) return false;

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            try
            {
                using var document = JsonDocument.Parse(json);
                return document.RootElement.TryGetProperty("status", out var status) &&
                       string.Equals(status.GetString(), "COMPLETED",
                           StringComparison.OrdinalIgnoreCase);
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private async Task<string> GetAccessTokenAsync(
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_settings.ClientId) ||
                string.IsNullOrWhiteSpace(_settings.ClientSecret))
                throw new InvalidOperationException(
                    "PayPal ClientId and ClientSecret must be configured in User Secrets.");

            using var request = new HttpRequestMessage(
                HttpMethod.Post, "/v1/oauth2/token");

            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(
                $"{_settings.ClientId}:{_settings.ClientSecret}"));
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Basic", credentials);
            request.Content = new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials"
                });

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            AccessTokenResponse? token;
            try
            {
                token = await response.Content.ReadFromJsonAsync<AccessTokenResponse>(
                    cancellationToken: cancellationToken);
            }
            catch (JsonException exception)
            {
                throw new HttpRequestException(
                    "PayPal returned an invalid access token response.",
                    exception);
            }

            if (string.IsNullOrWhiteSpace(token?.AccessToken))
                throw new HttpRequestException("PayPal did not return an access token.");

            return token.AccessToken;
        }

        private sealed class AccessTokenResponse
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; } = string.Empty;

            [JsonPropertyName("token_type")]
            public string TokenType { get; set; } = string.Empty;

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }
        }
    }
}
