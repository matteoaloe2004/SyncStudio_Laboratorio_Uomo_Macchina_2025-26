# StudySync: Piattaforma di Studio Collaborativo

[![Project](https://img.shields.io/badge/Project-StudySync-orange?style=flat-square)](#)
[![ASP.NET](https://img.shields.io/badge/Backend-ASP.NET_Core-blue?style=flat-square)](#)
[![Vue.js](https://img.shields.io/badge/Frontend-Vue.js-green?style=flat-square)](#)
[![SignalR](https://img.shields.io/badge/Realtime-SignalR-red?style=flat-square)](#)

## Partecipanti
**Gruppo**: StudySync

* **Matteo Aloè** - [matteo.aloe3@studio.unibo.it](mailto:matteo.aloe3@studio.unibo.it)
* **Elia Strazzella** - [elia.strazzella@studio.unibo.it](mailto:elia.strazzella@studio.unibo.it)

---

## Idea generale del progetto
StudySync è una piattaforma web pensata per facilitare lo studio collaborativo tra studenti universitari. L’applicazione consente di:
- Condividere appunti e risorse didattiche
- Creare *Stanze Studio* in tempo reale con chat, timer Pomodoro sincronizzato e conteggio partecipanti
- Esplorare appunti con filtri dinamici e ricerca immediata grazie a Vue.js
- Visualizzare statistiche personali mediante dashboard interattive

L’obiettivo è offrire un’esperienza fluida, reattiva e altamente coinvolgente, superando il classico approccio CRUD.

---

## Tecnologie principali
- **ASP.NET Core MVC** – Backend e gestione delle pagine server‑side.
- **Entity Framework Core** – ORM per persistenza di utenti, corsi, appunti e sessioni.
- **SignalR** – Comunicazione in tempo reale per le *Stanze Studio* (chat, timer, conteggio utenti).
- **Vue.js** – Frontend reattivo per filtri, ricerca e interfaccia dinamica.
- **Bootstrap + CSS personalizzato** – Styling moderno, responsivo e accessibile.

---

## Architettura del Sistema (Mermaid)
```mermaid
flowchart TD
    subgraph Browser
        UI[Interfaccia Utente (Vue.js + Bootstrap)]
        Dashboard[Dashboard Utente]
        Explore[Sezione "Esplora Appunti"]
        Room[Stanza Studio (Widget Fluttuante)]
    end
    subgraph Server
        MVC[ASP.NET Core MVC]
        Hub[SignalR Hub (TemplateHub)]
        EF[Entity Framework Core]
        DB[(Database SQL)]
    end
    
    UI -->|Richieste API| MVC
    MVC -->|Query/Comandi| EF
    EF -->|Persist/Recupera| DB
    
    UI -->|WebSocket| Hub
    Hub -->|Broadcast| UI
    Hub -->|Aggiorna| EF
    
    Dashboard -->|Dati statistici| MVC
    Explore -->|Filtri & ricerca| MVC
    Room -->|Timer, chat, conteggio utenti| Hub
    
    classDef external fill:#f9f,stroke:#333,stroke-width:2px;
    class UI,Dashboard,Explore,Room external;
```

---

## Come avviare il progetto (modalità sviluppo)
```powershell
# Posizionarsi nella cartella del progetto
cd "c:/Users/matte/Desktop/Progetti Unibo/3 Anno/Laboratorio Uomo-Macchina/PROGETTO STUDIOSYNC/src/Template.Web"

# Ripristinare le dipendenze
dotnet restore

# Avviare il server con hot‑reload
dotnet watch run
```

Il frontend Vue.js è integrato direttamente nelle view Razor; non è necessario avviare un server separato.

---

## Funzionalità chiave
- **Stanze Studio Collaborative**: chat in tempo reale, timer Pomodoro sincronizzato, widget fluttuante per sessioni attive in background.
- **Esplora Appunti**: filtri per corso e tag, ricerca full‑text, aggiornamento istantaneo dei risultati.
- **Dashboard Utente**: grafici interattivi (es. giorni consecutivi di studio), statistiche personalizzate.
- **Gestione Utenti & Dati**: registrazione, login, persistenza di corsi, appunti e sessioni.

---

## Contribuire
1. Forkare il repository.
2. Creare un nuovo branch per la feature o il bugfix.
3. Aprire una Pull Request descrivendo le modifiche.

---

## Licenza
Questo progetto è rilasciato sotto licenza MIT.
