using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template.Services.Shared
{
    public class CommentoAppunto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public string Testo { get; set; }

        [Required]
        public DateTime Data { get; set; }

        [Required]
        [ForeignKey("Appunto")]
        public Guid AppuntoId { get; set; }

        public Appunto Appunto { get; set; }

        [Required]
        [ForeignKey("User")]
        public Guid UserId { get; set; }

        public User User { get; set; }
    }
}
