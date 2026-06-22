using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Template.Services.Shared
{
    public class AddAppuntoCommand
    {
        public string Titolo { get; set; }
        public string Descrizione { get; set; }
        public string NomeFile { get; set; }
        public Guid CorsoId { get; set; }
        public Guid UserId { get; set; }
    }

    public partial class AppuntoService
    {
        /// <summary>
        /// Adds a new appunto to the database
        /// </summary>
        /// <param name="appunto">The appunto entity to add</param>
        /// <returns>The Id of the newly created appunto</returns>
        public async Task<Guid> AddAppuntoAsync(Appunto appunto)
        {
            if (appunto == null)
                throw new ArgumentNullException(nameof(appunto));

            appunto.DataCaricamento = DateTime.UtcNow;

            _dbContext.Appunti.Add(appunto);
            await _dbContext.SaveChangesAsync();

            return appunto.Id;
        }

        /// <summary>
        /// Handles the AddAppuntoCommand to add a new appunto
        /// </summary>
        /// <param name="cmd">The command containing appunto details</param>
        /// <returns>The Id of the newly created appunto</returns>
        public async Task<Guid> Handle(AddAppuntoCommand cmd)
        {
            var appunto = new Appunto
            {
                Titolo = cmd.Titolo,
                Descrizione = cmd.Descrizione,
                NomeFile = cmd.NomeFile,
                CorsoId = cmd.CorsoId,
                UserId = cmd.UserId,
                DataCaricamento = DateTime.UtcNow
            };

            _dbContext.Appunti.Add(appunto);
            await _dbContext.SaveChangesAsync();

            return appunto.Id;
        }
    }
}
