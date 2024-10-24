using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TherapyApp.Entities;

namespace TherapyApp.Services
{
    public class AccountService : IAccountService
    {
        private readonly TherapyDbContext _db;
        private readonly RoleManager<AppRole> _roleManager;

        public AccountService(TherapyDbContext dbContext, RoleManager<AppRole> roleManager) { 
            _db = dbContext;
            _roleManager = roleManager; 
        }

        public async Task<AppUser> GetUserAsync(string id)
        {
            var user = await _db.Users.Where(x => x.Id == id)
               .Select(c => new AppUser()
               {
                   Id = c.Id,
                   FirstName = c.FirstName,
                   FatherName = c.FatherName,
                   LastName = c.LastName,
                   Email = c.Email
               }).SingleOrDefaultAsync();


            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            return user;
        }

        public async Task<string> GetUserEmail(string email)
        {
            var emailAdress = await _db.Users.Where(u => u.Email == email).Select(x => x.Email).FirstOrDefaultAsync();

            return emailAdress;
        }


        public async Task<List<string>> GetRoles()
        {
            var roles = new[]
            {
                Roles.Therapist,
                Roles.Client,
                Roles.Admin
            };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    var idRole = new AppRole { Name = role };
                    await _roleManager.CreateAsync(idRole);
                }
            }

            var identityRoles = await _roleManager.Roles.ToListAsync();

            var roleList = new List<string>();

            foreach (var role in identityRoles)
            {

                if (role.Name != null)
                {
                    roleList.Add(role.Name);
                }
            }

            return roleList;
        }

        public async Task SaveAllAsync()
        {
            await _db.SaveChangesAsync();
        }

    }
}
