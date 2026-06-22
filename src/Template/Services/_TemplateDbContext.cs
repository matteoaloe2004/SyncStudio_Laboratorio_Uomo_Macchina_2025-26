using Template.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Template.Infrastructure;
using Template.Services.Shared;

namespace Template.Services
{
    public class TemplateDbContext : DbContext
    {
        public TemplateDbContext()
        {
        }

        public TemplateDbContext(DbContextOptions<TemplateDbContext> options) : base(options)
        {
            // Nota: DataGenerator è chiamato solo se il database è operativo.
            // Durante le migrations, questo non sarà eseguito poiché il schema non esiste ancora.
            try
            {
                // Verifica se il database esiste e può essere raggiunto
                if (Database.CanConnect())
                {
                    DataGenerator.InitializeUsers(this);
                }
            }
            catch
            {
                // Se il database non è disponibile (es. durante migrations), ignora l'errore
            }
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Corso> Corsi { get; set; }
        public DbSet<Appunto> Appunti { get; set; }
        public DbSet<StanzaStudio> StanzeStudio { get; set; }
    }
}
