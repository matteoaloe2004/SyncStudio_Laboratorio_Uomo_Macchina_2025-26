using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Template.Services;
using Template.Services.Shared;
using Template.Web.Areas;

namespace Template.Web.Features.Notifiche
{
    public partial class NotificheController : AuthenticatedBaseController
    {
        private readonly TemplateDbContext _dbContext;

        public NotificheController(TemplateDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async virtual Task<IActionResult> GetUnreadNotifiche()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Challenge();
            }

            var userId = Guid.Parse(userIdClaim.Value);

            var notifiche = await _dbContext.Notifiche
                .Where(n => n.UserId == userId && !n.Letta)
                .OrderByDescending(n => n.DataCreazione)
                .AsNoTracking()
                .ToListAsync();

            var data = notifiche.Select(n => new
            {
                Id = n.Id,
                Messaggio = n.Messaggio,
                DataCreazione = n.DataCreazione.ToString("dd/MM/yyyy HH:mm"),
                ElementoCorrelatoId = n.ElementoCorrelatoId
            }).ToList();

            return Json(new { success = true, notifiche = data });
        }

        [HttpPost]
        public async virtual Task<IActionResult> MarkAsRead(Guid id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Challenge();
            }

            var userId = Guid.Parse(userIdClaim.Value);

            var notifica = await _dbContext.Notifiche.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
            if (notifica == null)
            {
                return Json(new { success = false, message = "Notifica non trovata." });
            }

            notifica.Letta = true;
            await _dbContext.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async virtual Task<IActionResult> MarkAllAsRead()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Challenge();
            }

            var userId = Guid.Parse(userIdClaim.Value);

            var notifiche = await _dbContext.Notifiche.Where(n => n.UserId == userId && !n.Letta).ToListAsync();
            foreach (var n in notifiche)
            {
                n.Letta = true;
            }

            await _dbContext.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}
