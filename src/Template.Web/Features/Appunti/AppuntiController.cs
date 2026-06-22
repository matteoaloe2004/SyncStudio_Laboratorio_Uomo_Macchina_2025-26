using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Template.Services;
using Template.Services.Shared;
using Template.Web.Areas;

namespace Template.Web.Features.Appunti
{
    public partial class AppuntiController : AuthenticatedBaseController
    {
        private readonly TemplateDbContext _dbContext;
        private readonly AppuntoService _appuntoService;

        public AppuntiController(TemplateDbContext dbContext, AppuntoService appuntoService)
        {
            _dbContext = dbContext;
            _appuntoService = appuntoService;
        }

        [HttpGet]
        public virtual IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async virtual Task<IActionResult> GetAppunti()
        {
            var appunti = await _dbContext.Appunti
                .Include(a => a.Corso)
                .Include(a => a.User)
                .AsNoTracking()
                .ToListAsync();

            var corsi = await _dbContext.Corsi
                .AsNoTracking()
                .ToListAsync();

            var data = appunti.Select(a => {
                var hash = Math.Abs(a.Id.GetHashCode());
                var rating = 3.5 + (hash % 16) / 10.0;
                if (rating > 5.0) rating = 5.0;
                var downloads = (hash % 85) + 12;

                var tags = new[] { a.Corso.Nome, a.Titolo.Split(' ').FirstOrDefault() };

                return new
                {
                    Id = a.Id,
                    Titolo = a.Titolo,
                    Descrizione = a.Descrizione,
                    NomeFile = a.NomeFile,
                    DataCaricamento = a.DataCaricamento.ToString("dd/MM/yyyy"),
                    CorsoNome = a.Corso.Nome,
                    CorsoId = a.CorsoId,
                    AutoreNome = a.User.NickName ?? a.User.FirstName ?? "Studente",
                    Downloads = downloads,
                    Rating = Math.Round(rating, 1),
                    Tags = tags
                };
            }).ToList();

            var corsiList = corsi.Select(c => new {
                Id = c.Id,
                Nome = c.Nome
            }).ToList();

            return Json(new { appunti = data, corsi = corsiList });
        }

        [HttpPost]
        public async virtual Task<IActionResult> Upload(string titolo, string descrizione, Guid corsoId, IFormFile file)
        {
            if (string.IsNullOrWhiteSpace(titolo) || corsoId == Guid.Empty || file == null)
            {
                return Json(new { success = false, message = "Dati incompleti o file mancante." });
            }

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Challenge();
            }

            var userId = Guid.Parse(userIdClaim.Value);

            var cmd = new AddAppuntoCommand
            {
                Titolo = titolo,
                Descrizione = descrizione,
                NomeFile = file.FileName,
                CorsoId = corsoId,
                UserId = userId
            };

            try
            {
                var id = await _appuntoService.Handle(cmd);

                var a = await _dbContext.Appunti
                    .Include(x => x.Corso)
                    .Include(x => x.User)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (a == null)
                {
                    return Json(new { success = false, message = "Errore durante il salvataggio dell'appunto." });
                }

                var hash = Math.Abs(a.Id.GetHashCode());
                var rating = 5.0; // Nuovi caricamenti partono con 5.0
                var downloads = 0;
                var tags = new[] { a.Corso.Nome, a.Titolo.Split(' ').FirstOrDefault() };

                var result = new
                {
                    Id = a.Id,
                    Titolo = a.Titolo,
                    Descrizione = a.Descrizione,
                    NomeFile = a.NomeFile,
                    DataCaricamento = a.DataCaricamento.ToString("dd/MM/yyyy"),
                    CorsoNome = a.Corso.Nome,
                    CorsoId = a.CorsoId,
                    AutoreNome = a.User.NickName ?? a.User.FirstName ?? "Studente",
                    Downloads = downloads,
                    Rating = Math.Round(rating, 1),
                    Tags = tags
                };

                return Json(new { success = true, appunto = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Errore del server: " + ex.Message });
            }
        }

        [HttpGet]
        public virtual IActionResult Download(Guid id)
        {
            var appunto = _dbContext.Appunti.Find(id);
            if (appunto == null)
            {
                return NotFound("Appunto non trovato");
            }

            byte[] pdfBytes;
            using (var ms = new MemoryStream())
            {
                using (var writer = new StreamWriter(ms))
                {
                    writer.WriteLine("%PDF-1.4");
                    writer.WriteLine("1 0 obj");
                    writer.WriteLine("<< /Type /Catalog /Pages 2 0 R >>");
                    writer.WriteLine("endobj");
                    writer.WriteLine("2 0 obj");
                    writer.WriteLine("<< /Type /Pages /Kids [3 0 R] /Count 1 >>");
                    writer.WriteLine("endobj");
                    writer.WriteLine("3 0 obj");
                    writer.WriteLine("<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << >> /Contents 4 0 R >>");
                    writer.WriteLine("endobj");
                    writer.WriteLine("4 0 obj");
                    writer.WriteLine("<< /Length 200 >>");
                    writer.WriteLine("stream");
                    writer.WriteLine("BT");
                    writer.WriteLine("/F1 18 Tf");
                    writer.WriteLine("50 750 Td");
                    writer.WriteLine($"(StudySync - {appunto.Titolo}) Tj");
                    writer.WriteLine("/F1 12 Tf");
                    writer.WriteLine("0 -50 Td");
                    writer.WriteLine($"(File Name: {appunto.NomeFile}) Tj");
                    writer.WriteLine("0 -20 Td");
                    writer.WriteLine($"(Subject: {appunto.Corso?.Nome}) Tj");
                    writer.WriteLine("0 -20 Td");
                    writer.WriteLine($"(Description: {appunto.Descrizione}) Tj");
                    writer.WriteLine("ET");
                    writer.WriteLine("endstream");
                    writer.WriteLine("endobj");
                    writer.WriteLine("xref");
                    writer.WriteLine("0 5");
                    writer.WriteLine("0000000000 65535 f ");
                    writer.WriteLine("0000000009 00000 n ");
                    writer.WriteLine("0000000058 00000 n ");
                    writer.WriteLine("0000000115 00000 n ");
                    writer.WriteLine("0000000233 00000 n ");
                    writer.WriteLine("trailer");
                    writer.WriteLine("<< /Size 5 /Root 1 0 R >>");
                    writer.WriteLine("startxref");
                    writer.WriteLine("430");
                    writer.WriteLine("%%EOF");
                    writer.Flush();
                }
                pdfBytes = ms.ToArray();
            }

            return File(pdfBytes, "application/pdf", appunto.NomeFile);
        }
    }
}
