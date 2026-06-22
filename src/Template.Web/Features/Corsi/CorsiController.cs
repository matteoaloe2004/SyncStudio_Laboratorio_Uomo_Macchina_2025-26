using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Template.Services;
using Template.Services.Shared;
using Template.Web.Areas;

namespace Template.Web.Features.Corsi
{
    public class CorsoCustomDTO : CorsoDTO
    {
        public int StanzeCount { get; set; }
        public bool CanDelete => AppuntiCount == 0 && StanzeCount == 0;
    }

    public class CorsiViewModel
    {
        public System.Collections.Generic.List<CorsoCustomDTO> Corsi { get; set; } = new System.Collections.Generic.List<CorsoCustomDTO>();
    }

    public partial class CorsiController : AuthenticatedBaseController
    {
        private readonly TemplateDbContext _dbContext;
        private readonly SharedService _sharedService;

        public CorsiController(TemplateDbContext dbContext, SharedService sharedService)
        {
            _dbContext = dbContext;
            _sharedService = sharedService;
        }

        [HttpGet]
        public async virtual Task<IActionResult> Index()
        {
            var corsi = await _sharedService.Query(new CorsiAllQuery());
            
            var viewCorsi = new System.Collections.Generic.List<CorsoCustomDTO>();
            foreach (var c in corsi)
            {
                int roomsCount = await _dbContext.StanzeStudio.CountAsync(s => s.CorsoId == c.Id);
                viewCorsi.Add(new CorsoCustomDTO
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    Anno = c.Anno,
                    AppuntiCount = c.AppuntiCount,
                    StanzeCount = roomsCount
                });
            }

            var model = new CorsiViewModel
            {
                Corsi = viewCorsi
            };

            return View(model);
        }

        [HttpPost]
        public async virtual Task<IActionResult> Create(string nome, int anno)
        {
            if (string.IsNullOrWhiteSpace(nome))
            {
                TempData["ErrorMessage"] = "Il nome del corso non può essere vuoto.";
                return RedirectToAction("Index");
            }

            if (anno < 1 || anno > 3)
            {
                TempData["ErrorMessage"] = "L'anno deve essere compreso tra 1 e 3.";
                return RedirectToAction("Index");
            }

            // Check duplicate name
            bool exists = await _dbContext.Corsi.AnyAsync(c => c.Nome.ToLower() == nome.Trim().ToLower());
            if (exists)
            {
                TempData["ErrorMessage"] = "Un corso con questo nome esiste già.";
                return RedirectToAction("Index");
            }

            var nuovoCorso = new Corso
            {
                Id = Guid.NewGuid(),
                Nome = nome.Trim(),
                Anno = anno
            };

            _dbContext.Corsi.Add(nuovoCorso);
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Corso '{nuovoCorso.Nome}' creato con successo!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async virtual Task<IActionResult> Delete(Guid id)
        {
            var corso = await _dbContext.Corsi.FindAsync(id);
            if (corso == null)
            {
                TempData["ErrorMessage"] = "Corso non trovato.";
                return RedirectToAction("Index");
            }

            // Controlled check: verify if any Appunti or StanzeStudio are associated
            bool hasAppunti = await _dbContext.Appunti.AnyAsync(a => a.CorsoId == id);
            bool hasStanze = await _dbContext.StanzeStudio.AnyAsync(s => s.CorsoId == id);

            if (hasAppunti || hasStanze)
            {
                var reasons = new System.Collections.Generic.List<string>();
                if (hasAppunti) reasons.Add("appunti caricati");
                if (hasStanze) reasons.Add("stanze di studio attive");

                TempData["ErrorMessage"] = $"Impossibile eliminare il corso '{corso.Nome}' perché ha {string.Join(" e ", reasons)} associati. Modifica o elimina gli elementi collegati prima di procedere.";
                return RedirectToAction("Index");
            }

            _dbContext.Corsi.Remove(corso);
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Corso '{corso.Nome}' eliminato con successo.";
            return RedirectToAction("Index");
        }
    }
}
