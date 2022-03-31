using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Oqtane.Models;

namespace Oqtane.Security
{
    public interface IJwtManager
    {
        string GenerateToken(Alias alias, ClaimsIdentity identity, string secret, string issuer, string audience, int lifetime);
        ClaimsIdentity ValidateToken(string token, string secret, string issuer, string audience);
    }

    public class JwtManager : IJwtManager
    {
        public string GenerateToken(Alias alias, ClaimsIdentity identity, string secret, string issuer, string audience, int lifetime)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(identity),
                Issuer = issuer,
                Audience = audience,
                Expires = DateTime.UtcNow.AddMinutes(lifetime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public ClaimsIdentity ValidateToken(string token, string secret, string issuer, string audience)
        {
            if (!string.IsNullOrEmpty(token))
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(secret);
                try
                {
                    tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = (!string.IsNullOrEmpty(issuer)),
                        ValidateAudience = (!string.IsNullOrEmpty(audience)),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    }, out SecurityToken validatedToken);

                    var jwtToken = (JwtSecurityToken)validatedToken;
                    var identity = new ClaimsIdentity();
                    foreach (var claim in jwtToken.Claims)
                    {
                        identity.AddClaim(claim);
                    }
                    return identity;
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
