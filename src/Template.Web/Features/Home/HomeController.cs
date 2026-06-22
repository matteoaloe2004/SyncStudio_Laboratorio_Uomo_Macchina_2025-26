using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

using Template.Web.Areas;
using Template.Services.Shared;
using Template.Web.Features.StanzeStudio;
using System.Threading.Tasks;

namespace Template.Web.Features.Home
{
    public partial class HomeController : AuthenticatedBaseController
    {
        private readonly SharedService _sharedService;
        private readonly IRoomStateManager _roomStateManager;

        public HomeController(SharedService sharedService, IRoomStateManager roomStateManager)
        {
            _sharedService = sharedService;
            _roomStateManager = roomStateManager;
        }

        [HttpGet]
        public async virtual Task<IActionResult> Index()
        {
            var corsi = await _sharedService.Query(new CorsiAllQuery());
            var stanze = await _sharedService.Query(new StanzeStudioAllQuery());

            var userIdString = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            string nickname = "Studente";
            if (!string.IsNullOrEmpty(userIdString) && System.Guid.TryParse(userIdString, out var userId))
            {
                var user = await _sharedService.Query(new UserDetailQuery { Id = userId });
                if (user != null)
                {
                    nickname = user.NickName ?? user.FirstName ?? user.Email;
                }
            }

            // Sync real-time state from RoomStateManager
            int totalOnline = 0;
            foreach (var s in stanze)
            {
                var state = _roomStateManager.GetOrCreateState(s.Id);
                s.OnlineCount = state.Participants.Count;
                s.TempoRimanente = TimeSpan.FromSeconds(state.RemainingSeconds);
                s.IsInEsecuzione = state.IsTimerRunning;
                totalOnline += s.OnlineCount;
            }

            // Recommended room: the one with the most participants > 0
            StanzaStudioDTO consigliata = stanze
                .Where(s => s.OnlineCount > 0)
                .OrderByDescending(s => s.OnlineCount)
                .FirstOrDefault();

            var model = new HomeViewModel
            {
                Corsi = corsi,
                Stanze = stanze,
                NickName = nickname,
                TotalStudentiOnline = totalOnline,
                StanzaConsigliata = consigliata
            };

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult ChangeLanguageTo(string cultureName)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(cultureName)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), Secure = true }    // Secure assicura che il cookie sia inviato solo per connessioni HTTPS
            );

            return Redirect(Request.GetTypedHeaders().Referer.ToString());
        }
    }
}
