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
graph TD
    subgraph Browser [Layer Client - Browser]
        UI[Interfaccia Utente centralizzata<br/>Vue.js + HTML5]
        
        Dash(Dashboard Utente)
        Esp(Sezione Esplora Appunti)
        Stanza(Stanza Studio<br/>Widget Fluttuante)
        
        UI --- Dash
        UI --- Esp
        UI --- Stanza
    end

    subgraph Server [Layer Server - ASP.NET Core]
        MVC[ASP.NET Core MVC<br/>Controllers & APIs]
        Hub[SignalR Hub<br/>TemplateHub]
        EF[Entity Framework Core]
    end

    subgraph Database [Layer Persistenza]
        DB[(Database MySQL)]
    end

    %% Relazioni Client -> Server
    Dash -->|Richieste API / Dati statistici| MVC
    Esp -->|Filtri & ricerca| MVC
    Stanza -->|Timer, chat, conteggio utenti| Hub
    
    %% Relazioni Bidirezionali e Broadcast
    UI <-->|Connessione WebSocket| Hub
    Hub -.->|Broadcast Live| Stanza

    %% Relazioni Server -> Database
    MVC -->|Query / Comandi| EF
    Hub -->|Aggiorna stato sessione| EF
    EF <-->|Persiste / Recupera Dati| DB

    %% Stili personalizzati per migliorare la resa visiva
    classDef browserLayer fill:#2c3e50,stroke:#34495e,stroke-width:2px,color:#ecf0f1;
    classDef serverLayer fill:#16a085,stroke:#1abc9c,stroke-width:2px,color:#fff;
    classDef dbLayer fill:#c0392b,stroke:#e74c3c,stroke-width:2px,color:#fff;
    
    class UI,Dash,Esp,Stanza browserLayer;
    class MVC,Hub,EF serverLayer;
    class DB dbLayer;
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


