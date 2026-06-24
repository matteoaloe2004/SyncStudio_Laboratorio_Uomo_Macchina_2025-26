using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template.Services.Shared
{
    public class PrenotazioneStanza
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("StanzaStudio")]
        public Guid StanzaStudioId { get; set; }

        public StanzaStudio StanzaStudio { get; set; }

        [Required]
        [ForeignKey("User")]
        public Guid UserId { get; set; }

        public User User { get; set; }

        [Required]
        public DateTime DataPrenotazione { get; set; } = DateTime.UtcNow;
    }
}
