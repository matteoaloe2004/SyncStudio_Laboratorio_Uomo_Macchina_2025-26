using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template.Services.Shared
{
    public class StanzaStudio
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public string Nome { get; set; }

        public TimeSpan TempoRimanente { get; set; }

        [Required]
        public bool IsInEsecuzione { get; set; }

        [Required]
        [ForeignKey("Corso")]
        public Guid CorsoId { get; set; }

        public Corso Corso { get; set; }

        public int MaxCapacity { get; set; } = 8;

        public string Password { get; set; }

        public string Descrizione { get; set; }

        [NotMapped]
        public bool IsPrivate => !string.IsNullOrEmpty(Password);
    }
}
