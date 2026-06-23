using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;

namespace Template.Services.Shared
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }

        public double StudioOreLunedici { get; set; } = 1.5;
        public double StudioOreMartedici { get; set; } = 2.0;
        public double StudioOreMercoledici { get; set; } = 1.0;
        public double StudioOreGiovedici { get; set; } = 3.0;
        public double StudioOreVenerdici { get; set; } = 2.0;
        public double StudioOreSabato { get; set; } = 0.0;
        public double StudioOreDomenica { get; set; } = 0.0;
        public int GiorniDiFila { get; set; } = 3;

        /// <summary>
        /// Checks if password passed as parameter matches with the Password of the current user
        /// </summary>
        /// <param name="password">password to check</param>
        /// <returns>True if passwords match. False otherwise.</returns>
        public bool IsMatchWithPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;

            var sha256 = SHA256.Create();
            var testPassword = System.Convert.ToBase64String(sha256.ComputeHash(Encoding.ASCII.GetBytes(password)));

            return this.Password == testPassword;
        }
    }
}
