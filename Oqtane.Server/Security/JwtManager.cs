using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Oqtane.Models;

namespace Oqtane.Security
{
    public interface IJwtManager
    {
        string GenerateToken(User user, string secret);
        User ValidateToken(string token, string secret);
    }

    public class JwtManager : IJwtManager
    {
        public string GenerateToken(User user, string secret)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.UserId.ToString()), new Claim("name", user.Username) }),
                Expires = DateTime.UtcNow.AddYears(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public User ValidateToken(string token, string secret)
        {
            if (!string.IsNullOrEmpty(token))
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(secret);
                try
                {
                    tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ClockSkew = TimeSpan.Zero
                    }, out SecurityToken validatedToken);

                    var jwtToken = (JwtSecurityToken)validatedToken;
                    var user = new User
                    {
                        UserId = int.Parse(jwtToken.Claims.FirstOrDefault(item => item.Type == "id")?.Value),
                        Username = jwtToken.Claims.FirstOrDefault(item => item.Type == "name")?.Value
                    };
                    return user;
                }
                catch
                {
                    // error validating token
                }
            }
            return null;
        }
    }
}
