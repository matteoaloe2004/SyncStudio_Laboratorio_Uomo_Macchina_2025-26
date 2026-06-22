using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Template.Services.Shared
{
    public class CorsiAllQuery
    {
    }

    public class CorsoDTO
    {
        public System.Guid Id { get; set; }
        public string Nome { get; set; }
        public int Anno { get; set; }
        public int AppuntiCount { get; set; }
    }

    public partial class SharedService
    {
        public async Task<List<CorsoDTO>> Query(CorsiAllQuery qry)
        {
            var list = await _dbContext.Corsi
                .AsNoTracking()
                .ToListAsync();

            var result = new List<CorsoDTO>();
            foreach (var c in list)
            {
                var appuntiCount = await _dbContext.Appunti.CountAsync(a => a.CorsoId == c.Id);
                result.Add(new CorsoDTO
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    Anno = c.Anno,
                    AppuntiCount = appuntiCount
                });
            }

            return result;
        }
    }
}
