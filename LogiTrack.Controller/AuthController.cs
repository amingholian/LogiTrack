using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LogiTrack.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace LogiTrack.Controller
{
  [ApiController]
  [Route("api/auth")]
  [Produces("application/json")]
  public class AuthController : ControllerBase
  {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _config;

    public AuthController(
      UserManager<ApplicationUser> userManager,
      SignInManager<ApplicationUser> signInManager,
      IConfiguration config)
    {
      _userManager = userManager;
      _signInManager = signInManager;
      _config = config;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
      var user = new ApplicationUser
      {
        UserName = request.Email,
        Email = request.Email,
        FullName = request.FullName
      };

      var result = await _userManager.CreateAsync(user, request.Password);

      if (!result.Succeeded)
        return BadRequest(result.Errors);

      await _userManager.AddToRoleAsync(user, "User");

      return Ok(new { message = "User registered successfully." });
    }

    [HttpPost("promote")]
    [Authorize(Roles = "Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Promote([FromBody] PromoteRequest request)
    {
      var user = await _userManager.FindByEmailAsync(request.Email);
      if (user is null)
        return NotFound(new { message = $"User '{request.Email}' was not found." });

      await _userManager.AddToRoleAsync(user, "Manager");
      return Ok(new { message = $"{request.Email} has been promoted to Manager." });
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
      var user = await _userManager.FindByEmailAsync(request.Email);
      if (user is null)
        return Unauthorized(new { message = "Invalid email or password." });

      var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
      if (!result.Succeeded)
      {
        if (result.IsLockedOut)
          return StatusCode(StatusCodes.Status429TooManyRequests, new { message = "Account locked due to too many failed attempts. Try again later." });
        return Unauthorized(new { message = "Invalid email or password." });
      }

      var token = await GenerateJwtToken(user);
      return Ok(new { token });
    }

    private async Task<string> GenerateJwtToken(ApplicationUser user)
    {
      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
      var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
      var expires = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiresInMinutes"]!));

      var claims = new List<Claim>
      {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
        new Claim(JwtRegisteredClaimNames.Email, user.Email!),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
      };

      var roles = await _userManager.GetRolesAsync(user);
      claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

      var token = new JwtSecurityToken(
        issuer: _config["Jwt:Issuer"],
        audience: _config["Jwt:Audience"],
        claims: claims,
        expires: expires,
        signingCredentials: creds
      );

      return new JwtSecurityTokenHandler().WriteToken(token);
    }
  }

  public record RegisterRequest(
    [Required][EmailAddress] string Email,
    [Required][MinLength(8)] string Password,
    string? FullName);

  public record LoginRequest(
    [Required][EmailAddress] string Email,
    [Required] string Password);

  public record PromoteRequest([Required][EmailAddress] string Email);
}
