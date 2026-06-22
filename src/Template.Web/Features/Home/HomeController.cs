using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System;

using Template.Web.Areas;
using Template.Services.Shared;
using System.Threading.Tasks;

namespace Template.Web.Features.Home
{
    public partial class HomeController : AuthenticatedBaseController
    {
        private readonly SharedService _sharedService;

        public HomeController(SharedService sharedService)
        {
            _sharedService = sharedService;
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

            var model = new HomeViewModel
            {
                Corsi = corsi,
                Stanze = stanze,
                NickName = nickname
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
