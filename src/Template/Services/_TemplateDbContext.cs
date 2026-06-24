using Template.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
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
            // Durante le migrations, questo non sarà eseguito poiché lo schema non esiste ancora.
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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Per il design-time (EF Core Migrations), usa una connection string di fallback
                optionsBuilder.UseMySql(
                    "Server=localhost;Port=3306;Database=studysync_db;Uid=root;Pwd=password_super_segreta;",
                    new MySqlServerVersion(new System.Version(9, 7, 1)),
                    mySqlOptions => mySqlOptions.EnableRetryOnFailure(5, System.TimeSpan.FromSeconds(10), null));
            }
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Corso> Corsi { get; set; }
        public DbSet<Appunto> Appunti { get; set; }
        public DbSet<StanzaStudio> StanzeStudio { get; set; }
        public DbSet<Esame> Esami { get; set; }
        public DbSet<SessioneRipasso> SessioniRipasso { get; set; }
        public DbSet<CommentoAppunto> CommentiAppunti { get; set; }
        public DbSet<Notifica> Notifiche { get; set; }
    }
}
