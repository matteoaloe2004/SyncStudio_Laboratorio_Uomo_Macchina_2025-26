using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template.Services.Shared
{
    public class Appunto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public string Titolo { get; set; }

        public string Descrizione { get; set; }

        [Required]
        public string NomeFile { get; set; }

        [Required]
        public DateTime DataCaricamento { get; set; }

        [Required]
        [ForeignKey("Corso")]
        public Guid CorsoId { get; set; }

        public Corso Corso { get; set; }

        [Required]
        [ForeignKey("User")]
        public Guid UserId { get; set; }

        public User User { get; set; }
    }
}
