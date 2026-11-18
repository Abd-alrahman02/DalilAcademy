using DalilAcademy.Core.DTOs.Auth;
using DalilAcademy.Core.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DalilAcademy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService auth) : ControllerBase
    {
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseDto>> Login(
            LoginRequestDto dto)
        {
            return Ok(await auth.LoginAsync(dto));
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseDto>> Register(
            RegisterRequestDto dto)
        {
            return Ok(await auth.RegisterAsync(dto));
        }
    }
}