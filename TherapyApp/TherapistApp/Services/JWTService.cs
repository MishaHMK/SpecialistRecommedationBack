using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TherapyApp.Entities;
using TherapyApp.Helpers.Secrets;

namespace TherapyApp.Services
{
    public class JWTService : IJWTService
    {
        private readonly IConfiguration _iconfiguration;

        private readonly JwtVariables _jwtEnvironment;
        private readonly string _secret;
        private readonly int _expiration;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly UserManager<AppUser> _userManager;
        private readonly JsonWebTokenHandler _tokenHandler;

        public JWTService(UserManager<AppUser> userManager,
            IOptions<JwtVariables> jwtEnvironment,
            JsonWebTokenHandler tokenHandler,
            IConfiguration iconfiguration)
        {
            _userManager = userManager;
            _iconfiguration = iconfiguration;
            _jwtEnvironment = jwtEnvironment.Value;
            _tokenHandler = tokenHandler;
            _secret = _jwtEnvironment.Secret;
            _expiration = _jwtEnvironment.ExpirationInMinutes;
            _issuer = _jwtEnvironment.Issuer;
            _audience = _jwtEnvironment.Audience;
        }

        public Token Authenticate(string id, string email, IEnumerable<string> roles)
        {
            var role = roles.FirstOrDefault();

            if (role == null)
            {
                throw new InvalidOperationException("Role not found");
            }

            var claims = new Claim[]
            {
                 new Claim("NameIdentifier", id),
                 new Claim("Email", email),
                 new Claim("Role", role)
            };

            // Generate JSON Web Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.UTF8.GetBytes(_iconfiguration["JWT:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddMinutes(180),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return new Token { TokenString = tokenHandler.WriteToken(token) };

        }

        public async Task<Token> CreateJwtTokenAsync(AppUser user)
        {
            string secretKey = _secret;
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_expiration),
                SigningCredentials = credentials,
                Issuer = _issuer,
                Audience = _audience
            };

            string token = _tokenHandler.CreateToken(tokenDescriptor) ??
                  throw new InvalidOperationException("Token generation failed");

            return new Token { TokenString = token };
        }
    }
}
