using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using Nexora.Domain.Entities;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Nexora.Infrastructure.JWT;

public class JwtTokenHandler
{
   private readonly UserManager<ApplicationUser> _userManager;
   private readonly IConfiguration _config;

   public JwtTokenHandler(UserManager<ApplicationUser> userManager, IConfiguration config)
   {
      _userManager = userManager;
      _config = config;
   }

   public async Task<string> CreateToken(ApplicationUser user)
   {
      var claims = new List<Claim>
      {
         new Claim(ClaimTypes.Email, user.Email),
         new Claim(ClaimTypes.NameIdentifier, user.Id),
         new Claim(JwtRegisteredClaimNames.Sub, user.Email),
         new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
      };
      var roles = await _userManager.GetRolesAsync(user);
      foreach(var role in roles)
      {
         claims.Add(new Claim(ClaimTypes.Role, role));
      }

      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? string.Empty));
      var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

      var token = new JwtSecurityToken(
         issuer: _config["Jwt:Issuer"],
         claims: claims,
         expires: DateTime.Now.AddDays(31),
         signingCredentials: creds
      );
      return new JwtSecurityTokenHandler().WriteToken(token);
   }
}