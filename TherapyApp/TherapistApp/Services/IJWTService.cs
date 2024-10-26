using TherapyApp.Entities;


namespace TherapyApp.Services
{
    public interface IJWTService
    {
        Token Authenticate(string id, string name, IEnumerable<string> roles);
        public Task<Token> CreateJwtTokenAsync(AppUser user);
    }
}
