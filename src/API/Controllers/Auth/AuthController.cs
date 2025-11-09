using Application.Services.Identity;
using DTO.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace API.Controllers.Auth
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IIdentityService _identityService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IIdentityService identityService, ILogger<AuthController> logger)
        {
            _identityService = identityService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (request.Password != request.ConfirmPassword)
            {
                ModelState.AddModelError(nameof(request.ConfirmPassword), "Passwords do not match.");
                return ValidationProblem(ModelState);
            }

            var result = await _identityService.RegisterAsync(request.Email, request.Password, ct);
            if (!result.Succeeded)
            {
                _logger.LogWarning("User registration failed for email {Email}. Errors: {Errors}", request.Email, string.Join(", ", result.Errors));
                return BadRequest(new { errors = result.Errors });
            }

            return Ok(new AuthResponseDto
            {
                AccessToken = result.AccessToken,
                ExpiresAt = result.ExpiresAt
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var result = await _identityService.LoginAsync(request.Email, request.Password, ct);
            if (!result.Succeeded)
            {
                _logger.LogWarning("User login failed for email {Email}.", request.Email);
                return Unauthorized(new { errors = result.Errors });
            }

            return Ok(new AuthResponseDto
            {
                AccessToken = result.AccessToken,
                ExpiresAt = result.ExpiresAt
            });
        }
    }
}

