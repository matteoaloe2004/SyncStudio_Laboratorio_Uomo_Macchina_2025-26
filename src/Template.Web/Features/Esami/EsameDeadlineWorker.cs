using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Template.Services;
using Template.Services.Shared;
using Template.Web.SignalR.Hubs;

namespace Template.Web.Features.Esami
{
    public class EsameDeadlineWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EsameDeadlineWorker> _logger;
        // Interval: 60 seconds for live demo/testing. Can be adjusted in production.
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(60);

        public EsameDeadlineWorker(IServiceProvider serviceProvider, ILogger<EsameDeadlineWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background Service EsameDeadlineWorker avviato.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ControllaScadenzeEsami();
                    await ControllaStanzePrenotate();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Errore durante il controllo del Background Service.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Background Service EsameDeadlineWorker arrestato.");
        }

        private async Task ControllaScadenzeEsami()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TemplateDbContext>();
                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<TemplateHub, ITemplateClientEvent>>();

                var today = DateTime.Today;
                var limitDate = today.AddDays(3);

                // Get exams scheduled in the next 3 days (inclusive of today)
                var examsInRedZone = await dbContext.Esami
                    .Include(e => e.Corso)
                    .Where(e => e.DueDate.Date >= today && e.DueDate.Date <= limitDate)
                    .ToListAsync();

                foreach (var exam in examsInRedZone)
                {
                    // Check if a warning notification has already been sent for this exam
                    var alreadyNotified = await dbContext.Notifiche
                        .AnyAsync(n => n.UserId == exam.UserId && n.ElementoCorrelatoId == exam.Id);

                    if (!alreadyNotified)
                    {
                        var daysRemaining = (exam.DueDate.Date - today).Days;
                        string msgText = daysRemaining == 0
                            ? $"Attenzione! L'esame '{exam.Nome}' ({exam.Corso.Nome}) è OGGI!"
                            : $"Attenzione! L'esame '{exam.Nome}' ({exam.Corso.Nome}) scade tra {daysRemaining} giorn{(daysRemaining == 1 ? "o" : "i")}! ({exam.DueDate:dd/MM/yyyy})";

                        _logger.LogInformation("Esame in scadenza rilevato. Invio notifica per l'esame: {ExamNome} all'utente: {UserId}", exam.Nome, exam.UserId);

                        // 1. Save notification to database
                        var notifica = new Notifica
                        {
                            UserId = exam.UserId,
                            Messaggio = msgText,
                            DataCreazione = DateTime.Now,
                            Letta = false,
                            ElementoCorrelatoId = exam.Id
                        };

                        dbContext.Notifiche.Add(notifica);
                        await dbContext.SaveChangesAsync();

                        // 2. Publish via SignalR
                        await hubContext.Clients.User(exam.UserId.ToString()).ReceiveNotification(msgText);
                    }
                }
            }
        }

        private async Task ControllaStanzePrenotate()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TemplateDbContext>();
                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<TemplateHub, ITemplateClientEvent>>();

                var now = DateTime.Now;

                var bookings = await dbContext.PrenotazioniStanze
                    .Include(p => p.StanzaStudio)
                    .Where(p => p.StanzaStudio.DataApertura != null)
                    .ToListAsync();

                foreach (var booking in bookings)
                {
                    var room = booking.StanzaStudio;
                    var timeDiff = room.DataApertura.Value - now;

                    // 1. Check if 1-day warning is applicable (less than 24 hours)
                    if (timeDiff.TotalHours > 0 && timeDiff.TotalHours <= 24)
                    {
                        var alreadyNotified = await dbContext.Notifiche
                            .AnyAsync(n => n.UserId == booking.UserId && 
                                           n.ElementoCorrelatoId == room.Id && 
                                           n.Messaggio.Contains("si aprirà domani"));

                        if (!alreadyNotified)
                        {
                            string msgText = $"La stanza di ripasso '{room.Nome}' si aprirà domani alle {room.DataApertura.Value:HH:mm}!";
                            
                            var notifica = new Notifica
                            {
                                UserId = booking.UserId,
                                Messaggio = msgText,
                                DataCreazione = DateTime.Now,
                                Letta = false,
                                ElementoCorrelatoId = room.Id
                            };
                            dbContext.Notifiche.Add(notifica);
                            await dbContext.SaveChangesAsync();

                            await hubContext.Clients.User(booking.UserId.ToString()).ReceiveNotification(msgText);
                        }
                    }

                    // 2. Check if starting now
                    if (timeDiff.TotalMinutes >= -10 && timeDiff.TotalMinutes <= 5)
                    {
                        var alreadyNotified = await dbContext.Notifiche
                            .AnyAsync(n => n.UserId == booking.UserId && 
                                           n.ElementoCorrelatoId == room.Id && 
                                           n.Messaggio.Contains("aperta adesso"));

                        if (!alreadyNotified)
                        {
                            string msgText = $"La stanza di ripasso '{room.Nome}' è aperta adesso! Entra a studiare.";

                            var notifica = new Notifica
                            {
                                UserId = booking.UserId,
                                Messaggio = msgText,
                                DataCreazione = DateTime.Now,
                                Letta = false,
                                ElementoCorrelatoId = room.Id
                            };
                            dbContext.Notifiche.Add(notifica);
                            await dbContext.SaveChangesAsync();

                            await hubContext.Clients.User(booking.UserId.ToString()).ReceiveNotification(msgText);
                        }
                    }
                }
            }
        }
    }
}
