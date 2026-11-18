namespace DalilAcademy.Infra.Payments
{
    public class PayPalSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        public string CancelUrl { get; set; } = string.Empty;
        public decimal JodToUsdRate { get; set; } = 1.41m;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
    }
}
