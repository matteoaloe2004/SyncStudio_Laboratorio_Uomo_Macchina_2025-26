using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template.Services.Shared
{
    public class Notifica
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public Guid UserId { get; set; }

        public User User { get; set; }

        [Required]
        public string Messaggio { get; set; }

        [Required]
        public DateTime DataCreazione { get; set; }

        [Required]
        public bool Letta { get; set; } = false;

        // Optionally, reference to relevant exam or notes (null if generic)
        public Guid? ElementoCorrelatoId { get; set; }
    }
}
