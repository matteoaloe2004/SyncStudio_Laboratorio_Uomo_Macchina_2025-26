using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template.Services.Shared
{
    public class SessioneRipasso
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("Esame")]
        public Guid EsameId { get; set; }

        public Esame Esame { get; set; }

        [Required]
        public DateTime Data { get; set; }

        public string Descrizione { get; set; }
    }
}
