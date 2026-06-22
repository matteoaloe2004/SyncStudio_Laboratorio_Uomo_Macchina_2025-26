using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Template.Services;
using Template.Services.Shared;
using Template.Web.Areas;

namespace Template.Web.Features.StanzeStudio
{
    public class StanzeStudioViewModel
    {
        public List<StanzaStudioDTO> Stanze { get; set; }
        public List<CorsoDTO> Corsi { get; set; }
        public string SearchTerm { get; set; }
        public Guid? SelectedCorsoId { get; set; }
    }

    public class StanzaStudioRoomViewModel
    {
        public Guid RoomId { get; set; }
        public string RoomName { get; set; }
        public string CorsoName { get; set; }
        public string UserNickname { get; set; }
        public int DefaultDurationMinutes { get; set; }
    }

    public class StanzeStudioController : AuthenticatedBaseController
    {
        private readonly TemplateDbContext _dbContext;
        private readonly SharedService _sharedService;

        public StanzeStudioController(TemplateDbContext dbContext, SharedService sharedService)
        {
            _dbContext = dbContext;
            _sharedService = sharedService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string searchTerm, Guid? corsoId)
        {
            var stanze = await _sharedService.Query(new StanzeStudioAllQuery());
            var corsi = await _sharedService.Query(new CorsiAllQuery());

            // Simple filtering logic if parameters are provided
            var filteredStanze = new List<StanzaStudioDTO>();
            foreach (var s in stanze)
            {
                bool matchSearch = string.IsNullOrEmpty(searchTerm) || 
                                   s.Nome.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                   s.CorsoNome.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
                bool matchCorso = !corsoId.HasValue || s.CorsoId == corsoId.Value;

                if (matchSearch && matchCorso)
                {
                    filteredStanze.Add(s);
                }
            }

            var model = new StanzeStudioViewModel
            {
                Stanze = filteredStanze,
                Corsi = corsi,
                SearchTerm = searchTerm,
                SelectedCorsoId = corsoId
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Room(Guid id)
        {
            var stanza = await _dbContext.StanzeStudio
                .Include(x => x.Corso)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (stanza == null)
            {
                return NotFound();
            }

            var userIdString = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            string nickname = "Studente";
            if (!string.IsNullOrEmpty(userIdString) && Guid.TryParse(userIdString, out var userId))
            {
                var user = await _sharedService.Query(new UserDetailQuery { Id = userId });
                if (user != null)
                {
                    nickname = user.NickName ?? user.FirstName ?? user.Email;
                }
            }

            var model = new StanzaStudioRoomViewModel
            {
                RoomId = stanza.Id,
                RoomName = stanza.Nome,
                CorsoName = stanza.Corso != null ? stanza.Corso.Nome : "Materia",
                UserNickname = nickname,
                DefaultDurationMinutes = (int)stanza.TempoRimanente.TotalMinutes
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string name, Guid corsoId, int durationMinutes)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorMessage"] = "Il nome della stanza è obbligatorio.";
                return RedirectToAction(nameof(Index));
            }

            if (durationMinutes <= 0 || durationMinutes > 180)
            {
                durationMinutes = 25; // default to 25 mins
            }

            var newStanza = new StanzaStudio
            {
                Id = Guid.NewGuid(),
                Nome = name,
                TempoRimanente = TimeSpan.FromMinutes(durationMinutes),
                IsInEsecuzione = false,
                CorsoId = corsoId
            };

            _dbContext.StanzeStudio.Add(newStanza);
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Stanza '{name}' creata con successo!";
            return RedirectToAction(nameof(Room), new { id = newStanza.Id });
        }
    }
}
