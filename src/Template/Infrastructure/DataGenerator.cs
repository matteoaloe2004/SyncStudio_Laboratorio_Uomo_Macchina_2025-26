using Template.Services.Shared;
using System;
using System.Linq;
using Template.Services;

namespace Template.Infrastructure
{
    public class DataGenerator
    {
        public static void InitializeUsers(TemplateDbContext context)
        {
            if (context.Users.Any())
            {
                return;   // Data was already seeded
            }

            var user1 = new User
            {
                Id = Guid.Parse("3de6883f-9a0b-4667-aa53-0fbc52c4d300"), // Forced to specific Guid for tests
                Email = "email1@test.it",
                Password = "M0Cuk9OsrcS/rTLGf5SY6DUPqU2rGc1wwV2IL88GVGo=", // SHA-256 of text "Prova"
                FirstName = "Nome1",
                LastName = "Cognome1",
                NickName = "Nickname1",
                StudioOreLunedici = 2.5,
                StudioOreMartedici = 3.0,
                StudioOreMercoledici = 1.5,
                StudioOreGiovedici = 4.0,
                StudioOreVenerdici = 2.0,
                StudioOreSabato = 5.0,
                StudioOreDomenica = 3.5,
                GiorniDiFila = 5
            };

            var user2 = new User
            {
                Id = Guid.Parse("a030ee81-31c7-47d0-9309-408cb5ac0ac7"), // Forced to specific Guid for tests
                Email = "email2@test.it",
                Password = "Uy6qvZV0iA2/drm4zACDLCCm7BE9aCKZVQ16bg80XiU=", // SHA-256 of text "Test"
                FirstName = "Nome2",
                LastName = "Cognome2",
                NickName = "Nickname2",
                StudioOreLunedici = 1.0,
                StudioOreMartedici = 1.5,
                StudioOreMercoledici = 2.0,
                StudioOreGiovedici = 0.5,
                StudioOreVenerdici = 1.0,
                StudioOreSabato = 0.0,
                StudioOreDomenica = 0.0,
                GiorniDiFila = 2
            };

            var user3 = new User
            {
                Id = Guid.Parse("bfdef48b-c7ea-4227-8333-c635af267354"), // Forced to specific Guid for tests
                Email = "email3@test.it",
                Password = "Uy6qvZV0iA2/drm4zACDLCCm7BE9aCKZVQ16bg80XiU=", // SHA-256 of text "Test"
                FirstName = "Nome3",
                LastName = "Cognome3",
                NickName = "Nickname3",
                StudioOreLunedici = 4.0,
                StudioOreMartedici = 3.5,
                StudioOreMercoledici = 5.0,
                StudioOreGiovedici = 2.5,
                StudioOreVenerdici = 3.0,
                StudioOreSabato = 1.0,
                StudioOreDomenica = 2.0,
                GiorniDiFila = 7
            };

            var user4 = new User
            {
                Id = Guid.Parse("f62e8417-64df-419b-abff-5823528b8098"), // Forced Guid for presentation
                Email = "matteoaloe2004@libero.it",
                Password = "M0Cuk9OsrcS/rTLGf5SY6DUPqU2rGc1wwV2IL88GVGo=", // SHA-256 of text "Prova"
                FirstName = "Matteo",
                LastName = "Aloe",
                NickName = "Matteo",
                StudioOreLunedici = 3.5,
                StudioOreMartedici = 4.0,
                StudioOreMercoledici = 2.0,
                StudioOreGiovedici = 5.5,
                StudioOreVenerdici = 4.5,
                StudioOreSabato = 2.5,
                StudioOreDomenica = 1.0,
                GiorniDiFila = 12
            };

            context.Users.AddRange(user1, user2, user3, user4);

            // Seed Corsi (Courses)
            var corsoAnalisi = new Corso
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Nome = "Analisi 1",
                Anno = 1
            };
            var corsoFisica = new Corso
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Nome = "Fisica 1",
                Anno = 1
            };
            var corsoAlgebra = new Corso
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Nome = "Algebra Lineare",
                Anno = 1
            };
            var corsoProbabilita = new Corso
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Nome = "Probabilità",
                Anno = 2
            };
            var corsoChimica = new Corso
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Nome = "Chimica Organica",
                Anno = 2
            };
            var corsoGeometria = new Corso
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                Nome = "Geometria",
                Anno = 1
            };

            context.Corsi.AddRange(corsoAnalisi, corsoFisica, corsoAlgebra, corsoProbabilita, corsoChimica, corsoGeometria);

            // Seed StanzeStudio (Study Rooms)
            context.StanzeStudio.AddRange(
                new StanzaStudio
                {
                    Id = Guid.Parse("10101010-1010-1010-1010-101010101010"),
                    Nome = "Aula Analisi 1",
                    TempoRimanente = new TimeSpan(0, 18, 42),
                    IsInEsecuzione = true,
                    CorsoId = corsoAnalisi.Id,
                    Descrizione = "Ripasso generale su limiti, derivate e calcolo di integrali definiti/indefiniti in preparazione all'esame scritto."
                },
                new StanzaStudio
                {
                    Id = Guid.Parse("20202020-2020-2020-2020-202020202020"),
                    Nome = "Aula Fisica 1",
                    TempoRimanente = new TimeSpan(0, 5, 0),
                    IsInEsecuzione = false,
                    CorsoId = corsoFisica.Id,
                    Descrizione = "Risoluzione esercizi su cinematica, dinamica del punto e leggi di conservazione dell'energia."
                },
                new StanzaStudio
                {
                    Id = Guid.Parse("30303030-3030-3030-3030-303030303030"),
                    Nome = "Aula Algebra Lineare",
                    TempoRimanente = new TimeSpan(0, 22, 11),
                    IsInEsecuzione = true,
                    CorsoId = corsoAlgebra.Id,
                    Descrizione = "Esercitazioni su matrici, calcolo del determinante, sistemi lineari e diagonalizzazione."
                },
                new StanzaStudio
                {
                    Id = Guid.Parse("40404040-4040-4040-4040-404040404040"),
                    Nome = "Aula Chimica Organica",
                    TempoRimanente = new TimeSpan(0, 11, 30),
                    IsInEsecuzione = true,
                    CorsoId = corsoChimica.Id,
                    Descrizione = "Studio dei meccanismi di reazione, sintesi degli alcheni e risonanza."
                },
                new StanzaStudio
                {
                    Id = Guid.Parse("50505050-5050-5050-5050-505050505050"),
                    Nome = "Aula Probabilità",
                    TempoRimanente = new TimeSpan(0, 5, 0),
                    IsInEsecuzione = false,
                    CorsoId = corsoProbabilita.Id,
                    Descrizione = "Calcolo delle probabilità condizionate, variabili aleatorie e teoremi limite."
                },
                new StanzaStudio
                {
                    Id = Guid.Parse("60606060-6060-6060-6060-606060606060"),
                    Nome = "Aula Geometria",
                    TempoRimanente = new TimeSpan(0, 7, 55),
                    IsInEsecuzione = true,
                    CorsoId = corsoGeometria.Id,
                    Descrizione = "Ripasso di rette e piani nello spazio, coni, cilindri e superfici di rotazione."
                }
            );

            // Seed Appunti (Notes)
            context.Appunti.AddRange(
                new Appunto
                {
                    Id = Guid.NewGuid(),
                    Titolo = "Integrali Multipli e Cambi di Variabile",
                    Descrizione = "Appunti dettagliati sugli integrali doppi e tripli, formule di passaggio a coordinate polari, sferiche e cilindriche con relativi esercizi.",
                    NomeFile = "integrali_multipli.pdf",
                    DataCaricamento = DateTime.Now.AddDays(-5),
                    CorsoId = corsoAnalisi.Id,
                    UserId = user1.Id
                },
                new Appunto
                {
                    Id = Guid.NewGuid(),
                    Titolo = "Spazi Vettoriali e Basi - Riepilogo",
                    Descrizione = "Definizione di spazio vettoriale, sottospazi, indipendenza lineare, basi e dimensione. Teorema del completamento della base.",
                    NomeFile = "spazi_vettoriali.pdf",
                    DataCaricamento = DateTime.Now.AddDays(-2),
                    CorsoId = corsoAlgebra.Id,
                    UserId = user2.Id
                },
                new Appunto
                {
                    Id = Guid.NewGuid(),
                    Titolo = "Limiti e Continuità - Teoria completa",
                    Descrizione = "Tutti i teoremi sui limiti e funzioni continue: Teorema degli zeri, dei valori intermedi, di Weierstrass con relative dimostrazioni.",
                    NomeFile = "limiti_e_continuita.pdf",
                    DataCaricamento = DateTime.Now.AddDays(-10),
                    CorsoId = corsoAnalisi.Id,
                    UserId = user2.Id
                },
                new Appunto
                {
                    Id = Guid.NewGuid(),
                    Titolo = "Dinamica del Punto Materiale",
                    Descrizione = "Le leggi di Newton, forze d'attrito, tensione, forza centrifuga ed esempi svolti sui piani inclinati.",
                    NomeFile = "dinamica_punto_materiale.pdf",
                    DataCaricamento = DateTime.Now.AddDays(-1),
                    CorsoId = corsoFisica.Id,
                    UserId = user3.Id
                }
            );

            context.SaveChanges();
        }
    }
}
