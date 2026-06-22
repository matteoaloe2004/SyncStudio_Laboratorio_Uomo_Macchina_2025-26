using System;

namespace Template.Services.Shared
{
    public partial class AppuntoService
    {
        private readonly TemplateDbContext _dbContext;

        public AppuntoService(TemplateDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }
    }
}
