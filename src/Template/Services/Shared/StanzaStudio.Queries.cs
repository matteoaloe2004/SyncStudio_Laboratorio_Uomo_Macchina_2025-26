using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Template.Services.Shared
{
    public class StanzeStudioAllQuery
    {
    }

    public class StanzaStudioDTO
    {
        public System.Guid Id { get; set; }
        public string Nome { get; set; }
        public System.TimeSpan TempoRimanente { get; set; }
        public bool IsInEsecuzione { get; set; }
        public System.Guid CorsoId { get; set; }
        public string CorsoNome { get; set; }
        public int OnlineCount { get; set; }
        public int MaxCapacity { get; set; }
    }

    public partial class SharedService
    {
        public async Task<List<StanzaStudioDTO>> Query(StanzeStudioAllQuery qry)
        {
            var list = await _dbContext.StanzeStudio
                .AsNoTracking()
                .Include(x => x.Corso)
                .ToListAsync();

            var result = new List<StanzaStudioDTO>();
            int index = 1;
            foreach (var s in list)
            {
                // Assign a nice mock capacity and online count for aesthetics
                int mockOnline = (index % 3 == 0) ? 2 : (index % 2 == 0) ? 3 : 5;
                int mockCapacity = 8;
                if (s.Nome.Contains("Fisica")) { mockOnline = 3; mockCapacity = 5; }
                else if (s.Nome.Contains("Analisi")) { mockOnline = 5; mockCapacity = 8; }
                else if (s.Nome.Contains("Algebra")) { mockOnline = 7; mockCapacity = 10; }
                else if (s.Nome.Contains("Chimica")) { mockOnline = 2; mockCapacity = 4; }
                else if (s.Nome.Contains("Probabilità")) { mockOnline = 4; mockCapacity = 8; }
                else if (s.Nome.Contains("Geometria")) { mockOnline = 4; mockCapacity = 8; }

                result.Add(new StanzaStudioDTO
                {
                    Id = s.Id,
                    Nome = s.Nome,
                    TempoRimanente = s.TempoRimanente,
                    IsInEsecuzione = s.IsInEsecuzione,
                    CorsoId = s.CorsoId,
                    CorsoNome = s.Corso != null ? s.Corso.Nome : "Materia",
                    OnlineCount = mockOnline,
                    MaxCapacity = mockCapacity
                });
                index++;
            }

            return result;
        }
    }
}
