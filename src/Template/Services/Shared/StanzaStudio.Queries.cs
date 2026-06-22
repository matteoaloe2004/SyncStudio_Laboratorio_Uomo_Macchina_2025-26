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
        public bool IsPrivate { get; set; }
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
            foreach (var s in list)
            {
                result.Add(new StanzaStudioDTO
                {
                    Id = s.Id,
                    Nome = s.Nome,
                    TempoRimanente = s.TempoRimanente,
                    IsInEsecuzione = s.IsInEsecuzione,
                    CorsoId = s.CorsoId,
                    CorsoNome = s.Corso != null ? s.Corso.Nome : "Materia",
                    OnlineCount = 0,
                    MaxCapacity = s.MaxCapacity,
                    IsPrivate = s.IsPrivate
                });
            }

            return result;
        }
    }
}
