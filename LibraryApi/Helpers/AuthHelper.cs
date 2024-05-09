using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;


namespace LibraryApi.Helpers
{
    public class AuthHelper
    {
        private IConfiguration _config;
        public AuthHelper(IConfiguration IConfig)
        {
            _config = IConfig;
        }
        public string CreateToken(int userId, bool admin = false)
        {
            string? tokenKeyString = _config.GetSection("AppSettings:TokenKey").Value;
            Claim[] claims;
            if (!admin)
                claims = new Claim[]{
                    new Claim("userId", userId.ToString())
                };
            else
                claims = new Claim[]{
                    new Claim("userId", userId.ToString()),
                    new Claim(ClaimTypes.Role, "Admin")
                };

            SymmetricSecurityKey tokenKey = new(
                Encoding.UTF8.GetBytes(
                    tokenKeyString != null ? tokenKeyString : ""));

            SigningCredentials credentials = new(tokenKey, SecurityAlgorithms.HmacSha512Signature);

            SecurityTokenDescriptor descriptor = new()
            {
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = credentials,
                Expires = DateTime.Now.AddDays(1)
            };

            JwtSecurityTokenHandler tokenHandler = new();

            SecurityToken token = tokenHandler.CreateToken(descriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
