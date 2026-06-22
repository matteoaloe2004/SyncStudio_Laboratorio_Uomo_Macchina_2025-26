using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Template.Services.Shared
{
    public class AppuntoDetailQuery
    {
        public Guid Id { get; set; }
    }

    public class AppuntoDetailDTO
    {
        public Guid Id { get; set; }
        public string Titolo { get; set; }
        public string Descrizione { get; set; }
        public string NomeFile { get; set; }
        public DateTime DataCaricamento { get; set; }
        public Guid CorsoId { get; set; }
        public Guid UserId { get; set; }
    }

    public partial class AppuntoService
    {
        /// <summary>
        /// Retrieves all appunti from the database
        /// </summary>
        /// <returns>List of all appunti</returns>
        public async Task<List<Appunto>> GetAllAppuntiAsync()
        {
            return await _dbContext.Appunti
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves an appunto by its Id
        /// </summary>
        /// <param name="id">The appunto Id</param>
        /// <returns>The appunto if found, null otherwise</returns>
        public async Task<Appunto> GetAppuntoByIdAsync(Guid id)
        {
            return await _dbContext.Appunti
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        /// <summary>
        /// Retrieves appunti filtered by CorsoId
        /// </summary>
        /// <param name="corsoId">The corso Id</param>
        /// <returns>List of appunti for the specified corso</returns>
        public async Task<List<Appunto>> GetAppuntiByCorsoIdAsync(Guid corsoId)
        {
            return await _dbContext.Appunti
                .AsNoTracking()
                .Where(x => x.CorsoId == corsoId)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves appunti filtered by UserId
        /// </summary>
        /// <param name="userId">The user Id</param>
        /// <returns>List of appunti for the specified user</returns>
        public async Task<List<Appunto>> GetAppuntiByUserIdAsync(Guid userId)
        {
            return await _dbContext.Appunti
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves detailed information about a specific appunto
        /// </summary>
        /// <param name="qry">Query with appunto Id</param>
        /// <returns>Detailed appunto information</returns>
        public async Task<AppuntoDetailDTO> Query(AppuntoDetailQuery qry)
        {
            var appunto = await _dbContext.Appunti
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == qry.Id);

            if (appunto == null)
                return null;

            return new AppuntoDetailDTO
            {
                Id = appunto.Id,
                Titolo = appunto.Titolo,
                Descrizione = appunto.Descrizione,
                NomeFile = appunto.NomeFile,
                DataCaricamento = appunto.DataCaricamento,
                CorsoId = appunto.CorsoId,
                UserId = appunto.UserId
            };
        }
    }
}
