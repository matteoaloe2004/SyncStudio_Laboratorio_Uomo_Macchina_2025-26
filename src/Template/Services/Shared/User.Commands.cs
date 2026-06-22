using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Template.Services.Shared
{
    public class AddOrUpdateUserCommand
    {
        public Guid? Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
    }

    public class RegisterUserCommand
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
    }

    public partial class SharedService
    {
        public async Task<Guid> Handle(AddOrUpdateUserCommand cmd)
        {
            var user = await _dbContext.Users
                .Where(x => x.Id == cmd.Id)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                user = new User
                {
                    Email = cmd.Email,
                };
                _dbContext.Users.Add(user);
            }

            user.FirstName = cmd.FirstName;
            user.LastName = cmd.LastName;
            user.NickName = cmd.NickName;

            await _dbContext.SaveChangesAsync();

            return user.Id;
        }

        public async Task<Guid> Handle(RegisterUserCommand cmd)
        {
            if (string.IsNullOrWhiteSpace(cmd.Email) || string.IsNullOrWhiteSpace(cmd.Password))
                throw new System.Exception("Email e password sono obbligatorie");

            var existing = await _dbContext.Users.AnyAsync(x => x.Email == cmd.Email);
            if (existing)
                throw new System.Exception("Un utente con questa email esiste già");

            var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashedPassword = System.Convert.ToBase64String(sha256.ComputeHash(System.Text.Encoding.ASCII.GetBytes(cmd.Password)));

            var user = new User
            {
                Email = cmd.Email,
                Password = hashedPassword,
                FirstName = cmd.FirstName,
                LastName = cmd.LastName,
                NickName = cmd.NickName
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return user.Id;
        }
    }
}