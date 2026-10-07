using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserViews;

namespace Utenti.Services
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(UserDto user)
        {
            string jwtKey =
                _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException(
                    "Jwt:Key non configurato"
                );

            string jwtIssuer =
                _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException(
                    "Jwt:Issuer non configurato"
                );

            string jwtAudience =
                _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException(
                    "Jwt:Audience non configurato"
                );

            int expirationMinutes =
                int.Parse(
                    _configuration["Jwt:ExpirationMinutes"]
                    ?? "60"
                );

            List<Claim> claims =
            [
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.id
                ),

                new Claim(
                    JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString()
                )
            ];

            SymmetricSecurityKey key =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey)
                );

            SigningCredentials credentials =
                new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256
                );

            JwtSecurityToken token =
                new JwtSecurityToken(
                    issuer: jwtIssuer,
                    audience: jwtAudience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(
                        expirationMinutes
                    ),
                    signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}