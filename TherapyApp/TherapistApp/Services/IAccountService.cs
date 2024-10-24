using TherapyApp.Entities;

namespace TherapyApp.Services
{
    public interface IAccountService
    {
        Task<AppUser> GetUserAsync(string id);
        Task<List<string>> GetRoles();
        Task<string> GetUserEmail(string email);
        Task SaveAllAsync();
    }
}
