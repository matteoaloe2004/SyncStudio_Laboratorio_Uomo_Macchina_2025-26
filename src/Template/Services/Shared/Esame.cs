using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template.Services.Shared
{
    public class Esame
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public string Nome { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        [ForeignKey("Corso")]
        public Guid CorsoId { get; set; }

        public Corso Corso { get; set; }

        [Required]
        [ForeignKey("User")]
        public Guid UserId { get; set; }

        public User User { get; set; }

        public ICollection<SessioneRipasso> SessioniRipasso { get; set; } = new List<SessioneRipasso>();
    }
}
