using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Template.Services;
using Template.Services.Shared;
using Template.Web.Areas;

namespace Template.Web.Features.Esami
{
    public partial class EsamiController : AuthenticatedBaseController
    {
        private readonly TemplateDbContext _dbContext;

        public EsamiController(TemplateDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public virtual IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async virtual Task<IActionResult> GetEsami()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Challenge();
            }

            var userId = Guid.Parse(userIdClaim.Value);

            var esami = await _dbContext.Esami
                .Include(e => e.Corso)
                .Include(e => e.SessioniRipasso)
                .Where(e => e.UserId == userId)
                .OrderBy(e => e.DueDate)
                .AsNoTracking()
                .ToListAsync();

            var corsi = await _dbContext.Corsi
                .AsNoTracking()
                .ToListAsync();

            var data = esami.Select(e => new
            {
                Id = e.Id,
                Nome = e.Nome,
                DueDate = e.DueDate.ToString("yyyy-MM-ddTHH:mm"),
                DueDateFormatted = e.DueDate.ToString("dd/MM/yyyy 'alle' HH:mm"),
                CorsoNome = e.Corso.Nome,
                CorsoId = e.CorsoId,
                GiorniMancanti = (e.DueDate.Date - DateTime.Today).Days,
                SessioniRipasso = e.SessioniRipasso.OrderBy(s => s.Data).Select(s => new
                {
                    Id = s.Id,
                    Data = s.Data.ToString("yyyy-MM-ddTHH:mm"),
                    DataFormatted = s.Data.ToString("dd/MM/yyyy 'alle' HH:mm"),
                    Descrizione = s.Descrizione
                }).ToList()
            }).ToList();

            var corsiList = corsi.Select(c => new
            {
                Id = c.Id,
                Nome = c.Nome
            }).ToList();

            return Json(new { esami = data, corsi = corsiList });
        }

        [HttpPost]
        public async virtual Task<IActionResult> CreateEsame(string nome, DateTime dueDate, Guid corsoId)
        {
            if (string.IsNullOrWhiteSpace(nome) || corsoId == Guid.Empty || dueDate == default)
            {
                return Json(new { success = false, message = "Dati dell'esame incompleti." });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Challenge();
            }

            var userId = Guid.Parse(userIdClaim.Value);

            var esame = new Esame
            {
                Nome = nome.Trim(),
                DueDate = dueDate,
                CorsoId = corsoId,
                UserId = userId
            };

            _dbContext.Esami.Add(esame);
            await _dbContext.SaveChangesAsync();

            // Refresh representation
            var esameDb = await _dbContext.Esami
                .Include(e => e.Corso)
                .FirstOrDefaultAsync(e => e.Id == esame.Id);

            var result = new
            {
                Id = esameDb.Id,
                Nome = esameDb.Nome,
                DueDate = esameDb.DueDate.ToString("yyyy-MM-ddTHH:mm"),
                DueDateFormatted = esameDb.DueDate.ToString("dd/MM/yyyy 'alle' HH:mm"),
                CorsoNome = esameDb.Corso.Nome,
                CorsoId = esameDb.CorsoId,
                GiorniMancanti = (esameDb.DueDate.Date - DateTime.Today).Days,
                SessioniRipasso = new System.Collections.Generic.List<object>()
            };

            return Json(new { success = true, esame = result });
        }

        [HttpPost]
        public async virtual Task<IActionResult> AddSessioneRipasso(Guid esameId, DateTime data, string descrizione)
        {
            if (esameId == Guid.Empty || data == default || string.IsNullOrWhiteSpace(descrizione))
            {
                return Json(new { success = false, message = "Dati della sessione incompleti." });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Challenge();
            }

            var userId = Guid.Parse(userIdClaim.Value);

            var esame = await _dbContext.Esami.FirstOrDefaultAsync(e => e.Id == esameId && e.UserId == userId);
            if (esame == null)
            {
                return Json(new { success = false, message = "Esame non trovato." });
            }

            // HUX / SLA Error Prevention validation on server-side
            if (data > esame.DueDate)
            {
                return Json(new { success = false, message = $"Errore di pianificazione: Non puoi programmare una sessione di ripasso il {data:dd/MM/yyyy alle HH:mm} perché l'esame è fissato per il {esame.DueDate:dd/MM/yyyy alle HH:mm}." });
            }

            var sessione = new SessioneRipasso
            {
                EsameId = esameId,
                Data = data,
                Descrizione = descrizione.Trim()
            };

            _dbContext.SessioniRipasso.Add(sessione);

            var stanzaRipasso = new StanzaStudio
            {
                Id = Guid.NewGuid(),
                Nome = $"Ripasso: {esame.Nome}",
                CorsoId = esame.CorsoId,
                TempoRimanente = TimeSpan.FromMinutes(25),
                IsInEsecuzione = false,
                MaxCapacity = 8,
                Password = null,
                Descrizione = $"Studio collaborativo per {esame.Nome}. Argomenti: {descrizione.Trim()}",
                DataApertura = data
            };
            _dbContext.StanzeStudio.Add(stanzaRipasso);

            await _dbContext.SaveChangesAsync();

            var result = new
            {
                Id = sessione.Id,
                Data = sessione.Data.ToString("yyyy-MM-ddTHH:mm"),
                DataFormatted = sessione.Data.ToString("dd/MM/yyyy 'alle' HH:mm"),
                Descrizione = sessione.Descrizione
            };

            return Json(new { success = true, sessione = result });
        }

        [HttpPost]
        public async virtual Task<IActionResult> UpdateEsame(Guid id, string nome, DateTime dueDate, Guid corsoId)
        {
            if (id == Guid.Empty || string.IsNullOrWhiteSpace(nome) || corsoId == Guid.Empty || dueDate == default)
            {
                return Json(new { success = false, message = "Dati dell'esame incompleti." });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Challenge();
            }

            var userId = Guid.Parse(userIdClaim.Value);

            var esame = await _dbContext.Esami
                .Include(e => e.SessioniRipasso)
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (esame == null)
            {
                return Json(new { success = false, message = "Esame non trovato." });
            }

            var oldNome = esame.Nome;
            esame.Nome = nome.Trim();
            esame.DueDate = dueDate;
            esame.CorsoId = corsoId;

            if (esame.SessioniRipasso != null && esame.SessioniRipasso.Any())
            {
                var sessionDates = esame.SessioniRipasso.Select(sr => sr.Data).ToList();
                var associatedRooms = await _dbContext.StanzeStudio
                    .Where(s => s.CorsoId == esame.CorsoId && s.DataApertura.HasValue && sessionDates.Contains(s.DataApertura.Value))
                    .ToListAsync();
                foreach (var room in associatedRooms)
                {
                    room.Nome = $"Ripasso: {esame.Nome}";
                    room.Descrizione = $"Studio collaborativo per {esame.Nome}." + (room.Descrizione.Contains("Argomenti:") ? " Argomenti:" + room.Descrizione.Split("Argomenti:")[1] : "");
                }
            }

            await _dbContext.SaveChangesAsync();

            var esameDb = await _dbContext.Esami
                .Include(e => e.Corso)
                .Include(e => e.SessioniRipasso)
                .FirstOrDefaultAsync(e => e.Id == esame.Id);

            var result = new
            {
                Id = esameDb.Id,
                Nome = esameDb.Nome,
                DueDate = esameDb.DueDate.ToString("yyyy-MM-ddTHH:mm"),
                DueDateFormatted = esameDb.DueDate.ToString("dd/MM/yyyy 'alle' HH:mm"),
                CorsoNome = esameDb.Corso.Nome,
                CorsoId = esameDb.CorsoId,
                GiorniMancanti = (esameDb.DueDate.Date - DateTime.Today).Days,
                SessioniRipasso = esameDb.SessioniRipasso.OrderBy(s => s.Data).Select(s => new
                {
                    Id = s.Id,
                    Data = s.Data.ToString("yyyy-MM-ddTHH:mm"),
                    DataFormatted = s.Data.ToString("dd/MM/yyyy 'alle' HH:mm"),
                    Descrizione = s.Descrizione
                }).ToList()
            };

            return Json(new { success = true, esame = result });
        }

        [HttpPost]
        public async virtual Task<IActionResult> DeleteEsame(Guid id)
        {
            if (id == Guid.Empty)
            {
                return Json(new { success = false, message = "Id esame non specificato." });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Challenge();
            }

            var userId = Guid.Parse(userIdClaim.Value);

            var esame = await _dbContext.Esami
                .Include(e => e.SessioniRipasso)
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (esame == null)
            {
                return Json(new { success = false, message = "Esame non trovato o non autorizzato." });
            }

            if (esame.SessioniRipasso != null && esame.SessioniRipasso.Any())
            {
                var sessionDates = esame.SessioniRipasso.Select(sr => sr.Data).ToList();
                var associatedRooms = await _dbContext.StanzeStudio
                    .Where(s => s.CorsoId == esame.CorsoId && s.DataApertura.HasValue && sessionDates.Contains(s.DataApertura.Value))
                    .ToListAsync();

                if (associatedRooms.Any())
                {
                    _dbContext.StanzeStudio.RemoveRange(associatedRooms);
                }

                _dbContext.SessioniRipasso.RemoveRange(esame.SessioniRipasso);
            }

            _dbContext.Esami.Remove(esame);
            await _dbContext.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}
