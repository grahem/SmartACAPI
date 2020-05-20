using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartACDeviceAPI.Options;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartACDeviceAPI.Security
{
    //We use JWT token generation in both Device and User auth.
    //This helper class is used to reduce dupliacte code.
    public class JWTHelper
    {
        public static string GenerateJWTToken(IOptionsMonitor<AuthZOptions> options)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.CurrentValue.SecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var token = new JwtSecurityToken(
                issuer: options.CurrentValue.Issuer,
                audience: options.CurrentValue.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
