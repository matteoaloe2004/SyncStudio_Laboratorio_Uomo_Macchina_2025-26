using System.ComponentModel.DataAnnotations;

namespace Template.Web.Features.Login
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "L'email è obbligatoria")]
        [EmailAddress(ErrorMessage = "Inserisci un indirizzo email valido")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La password è obbligatoria")]
        [DataType(DataType.Password)]
        [MinLength(4, ErrorMessage = "La password deve contenere almeno 4 caratteri")]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Il nome è obbligatorio")]
        [Display(Name = "Nome")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Il cognome è obbligatorio")]
        [Display(Name = "Cognome")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Il nickname è obbligatorio")]
        [Display(Name = "Nickname")]
        public string NickName { get; set; }
    }
}
