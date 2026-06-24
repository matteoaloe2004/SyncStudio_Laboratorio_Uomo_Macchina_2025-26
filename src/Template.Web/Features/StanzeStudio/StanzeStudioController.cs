using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public string RoomDescription { get; set; }
        public string UserNickname { get; set; }
        public int DefaultDurationMinutes { get; set; }
        public int DefaultDurationSeconds { get; set; }
        public bool IsPrivate { get; set; }
        public int MaxCapacity { get; set; }
    }

    public partial class StanzeStudioController : AuthenticatedBaseController
    {
        private readonly TemplateDbContext _dbContext;
        private readonly SharedService _sharedService;
        private readonly IRoomStateManager _roomStateManager;

        public StanzeStudioController(TemplateDbContext dbContext, SharedService sharedService, IRoomStateManager roomStateManager)
        {
            _dbContext = dbContext;
            _sharedService = sharedService;
            _roomStateManager = roomStateManager;
        }

        [HttpGet]
        public async virtual Task<IActionResult> Index(string searchTerm, Guid? corsoId)
        {
            var stanze = await _sharedService.Query(new StanzeStudioAllQuery());
            var corsi = await _sharedService.Query(new CorsiAllQuery());

            var userIdString = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            Guid.TryParse(userIdString, out Guid userId);

            // Simple filtering logic if parameters are provided
            var filteredStanze = new List<StanzaStudioDTO>();
            foreach (var s in stanze)
            {
                // Sync dynamic state from RoomStateManager
                var state = _roomStateManager.GetOrCreateState(s.Id);
                s.OnlineCount = state.Participants.Count;
                s.TempoRimanente = TimeSpan.FromSeconds(state.RemainingSeconds);
                s.IsInEsecuzione = state.IsTimerRunning;

                // Sync booking info
                var bookings = await _dbContext.PrenotazioniStanze.Where(p => p.StanzaStudioId == s.Id).ToListAsync();
                s.BookingCount = bookings.Count;
                s.IsUserBooked = userId != Guid.Empty && bookings.Any(p => p.UserId == userId);

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
        public async virtual Task<IActionResult> Room(Guid id, string pwd)
        {
            var stanza = await _dbContext.StanzeStudio
                .Include(x => x.Corso)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (stanza == null)
            {
                return NotFound();
            }

            var userIdString = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            Guid.TryParse(userIdString, out Guid userId);

            if (userId != Guid.Empty)
            {
                var activeRoomId = _roomStateManager.GetActiveRoomIdForUser(userId);
                if (activeRoomId.HasValue && activeRoomId.Value != id)
                {
                    TempData["ErrorMessage"] = "Sei già all'interno di un'altra stanza di studio. Per favore, esci dalla stanza corrente prima di accedere ad una nuova.";
                    return RedirectToAction(nameof(Index));
                }
            }

            // If private room and no/wrong password, show password form
            if (stanza.IsPrivate && stanza.Password != pwd)
            {
                ViewData["RoomId"] = id;
                ViewData["RoomName"] = stanza.Nome;
                ViewData["CorsoName"] = stanza.Corso?.Nome ?? "Materia";
                ViewData["WrongPassword"] = !string.IsNullOrEmpty(pwd);
                return View("RoomPassword");
            }

            string nickname = "Studente";
            if (userId != Guid.Empty)
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
                RoomDescription = stanza.Descrizione,
                UserNickname = nickname,
                DefaultDurationMinutes = (int)stanza.TempoRimanente.TotalMinutes,
                DefaultDurationSeconds = (int)stanza.TempoRimanente.TotalSeconds,
                IsPrivate = stanza.IsPrivate,
                MaxCapacity = stanza.MaxCapacity
            };

            return View(model);
        }

        [HttpPost]
        public async virtual Task<IActionResult> Create(string name, Guid corsoId, int durationMinutes, int maxCapacity, string password, string description, DateTime? dataApertura)
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

            if (maxCapacity < 2 || maxCapacity > 20)
            {
                maxCapacity = 8; // default
            }

            var newStanza = new StanzaStudio
            {
                Id = Guid.NewGuid(),
                Nome = name,
                TempoRimanente = TimeSpan.FromMinutes(durationMinutes),
                IsInEsecuzione = false,
                CorsoId = corsoId,
                MaxCapacity = maxCapacity,
                Password = string.IsNullOrWhiteSpace(password) ? null : password.Trim(),
                Descrizione = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                DataApertura = dataApertura
            };

            _dbContext.StanzeStudio.Add(newStanza);
            await _dbContext.SaveChangesAsync();

            if (newStanza.DataApertura.HasValue && newStanza.DataApertura.Value > DateTime.Now)
            {
                TempData["SuccessMessage"] = $"Stanza '{name}' pianificata con successo per il {newStanza.DataApertura.Value:dd/MM/yyyy} alle {newStanza.DataApertura.Value:HH:mm}!";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = $"Stanza '{name}' creata con successo!";
            // If private, redirect with pwd so creator can enter
            if (newStanza.IsPrivate)
            {
                return RedirectToAction(nameof(Room), new { id = newStanza.Id, pwd = newStanza.Password });
            }
            return RedirectToAction(nameof(Room), new { id = newStanza.Id });
        }

        [HttpPost]
        public async virtual Task<IActionResult> PrenotaStanza(Guid roomId)
        {
            var userIdString = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                return Challenge();
            }

            var stanza = await _dbContext.StanzeStudio.FirstOrDefaultAsync(s => s.Id == roomId);
            if (stanza == null)
            {
                return Json(new { success = false, message = "Stanza non trovata." });
            }

            var existingBooking = await _dbContext.PrenotazioniStanze
                .FirstOrDefaultAsync(p => p.StanzaStudioId == roomId && p.UserId == userId);

            bool isBookedNow = false;
            if (existingBooking != null)
            {
                _dbContext.PrenotazioniStanze.Remove(existingBooking);
                isBookedNow = false;
            }
            else
            {
                var booking = new PrenotazioneStanza
                {
                    StanzaStudioId = roomId,
                    UserId = userId,
                    DataPrenotazione = DateTime.UtcNow
                };
                _dbContext.PrenotazioniStanze.Add(booking);
                isBookedNow = true;
            }

            await _dbContext.SaveChangesAsync();

            var count = await _dbContext.PrenotazioniStanze.CountAsync(p => p.StanzaStudioId == roomId);

            return Json(new { success = true, isBooked = isBookedNow, bookingCount = count });
        }
    }
}
