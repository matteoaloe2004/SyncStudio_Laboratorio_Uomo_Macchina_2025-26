# ?? StudySync - Docker Setup Guide

Questo progetto è configurato per funzionare completamente con Docker Compose, incluso MySQL e l'applicazione .NET 8 Razor Pages.

## ?? Prerequisiti

- **Docker Desktop** (https://www.docker.com/products/docker-desktop)
- **Docker Compose** (incluso in Docker Desktop per Windows e Mac)

## ?? Quick Start

### Su Windows (PowerShell o CMD)
```bash
# Dai il permesso di esecuzione allo script
Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process

# Esegui lo script di setup
.\docker-setup.bat
```

### Su Linux/macOS (Bash)
```bash
# Dai il permesso di esecuzione allo script
chmod +x docker-setup.sh

# Esegui lo script di setup
./docker-setup.sh
```

## ?? Comandi Docker Compose Rapidi

### Avviare tutto
```bash
docker-compose up -d --build
```

### Fermare tutto
```bash
docker-compose down
```

### Visualizzare i log
```bash
docker-compose logs -f app
```

### Applicare le migrazioni
```bash
docker-compose exec app dotnet ef database update -p Template -s Template.Web
```

### Ricostruire dopo cambiamenti al codice
```bash
docker-compose up -d --build --force-recreate
```

### Accedere al container MySQL
```bash
docker-compose exec mysql mysql -uroot -ppassword_super_segreta studysync_db
```

### Pulire tutto (container, volumi, network)
```bash
docker-compose down -v
```

## ?? Accesso all'Applicazione

Una volta avviati i container:

- **URL HTTP**: http://localhost:5000
- **URL HTTPS**: https://localhost:5001

## ??? Credenziali MySQL

```
Host: mysql (da dentro Docker) / localhost:3306 (da host Windows)
User: root
Password: password_super_segreta
Database: studysync_db
```

Puoi connetterti da strumenti come:
- MySQL Workbench
- DBeaver
- phpMyAdmin (aggiungi il servizio al docker-compose se vuoi)

## ?? Struttura del Progetto

```
PROGETTO STUDIOSYNC/
??? docker-compose.yml      # Configurazione Docker Compose
??? docker-setup.sh        # Script setup per Linux/macOS
??? docker-setup.bat       # Script setup per Windows
??? init.sql              # Script di inizializzazione MySQL
??? README.md             # Questo file
??? src/
    ??? Dockerfile         # Dockerfile per l'app .NET
    ??? .dockerignore     # File da ignorare nel build Docker
    ??? Template/          # Progetto libreria
    ??? Template.Web/      # Progetto ASP.NET Core Razor Pages
    ?   ??? appsettings.json
    ?   ??? appsettings.Docker.json  # Config specifica Docker
    ?   ??? ...
    ??? ...
```

## ?? Configurazione

### ConnectionString

Il file `appsettings.Docker.json` contiene la configurazione specifica per Docker:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=mysql;Port=3306;Database=studysync_db;Uid=root;Pwd=password_super_segreta;AllowUserVariables=True;"
  }
}
```

Nota che `Server=mysql` fa riferimento al nome del servizio MySQL nel docker-compose.

### Versione MySQL

Il docker-compose usa **MySQL 9.7** configurato con:
- Character set: `utf8mb4`
- Collation: `utf8mb4_unicode_ci`
- Native password authentication

### Health Checks

- **MySQL**: Ping ogni 10 secondi, retry max 5
- **App**: HTTP check su `/health` ogni 30 secondi

## ?? Workflow di Sviluppo

### 1. Primo setup
```bash
docker-compose up -d --build
```

### 2. Applicare migrazioni al database
```bash
docker-compose exec app dotnet ef database update -p Template -s Template.Web
```

### 3. Sviluppare
Modifica i file nel tuo editor locale. I container rimonteranno automaticamente se configuri hot-reload.

### 4. Ricompilare
```bash
docker-compose up -d --build
```

### 5. Fermare
```bash
docker-compose down
```

## ?? Troubleshooting

### Errore: "Cannot connect to Docker daemon"
- Assicurati che Docker Desktop sia avviato
- Su Linux, assicurati di avere i permessi docker: `sudo usermod -aG docker $USER`

### Errore: "Port 3306 is already in use"
- Fermali i container MySQL precedenti: `docker ps -a` e `docker rm <container_id>`
- O cambia la porta nel docker-compose da `"3306:3306"` a `"3307:3306"`

### Errore di connessione MySQL
- Aspetta 10-15 secondi dopo aver avviato i container (MySQL impiega tempo ad inizializzarsi)
- Controlla i log: `docker-compose logs mysql`

### Migrazioni non si applicano
```bash
# Verifica lo stato dei container
docker-compose ps

# Leggi i log
docker-compose logs app

# Prova a ricostru ire
docker-compose up -d --build --force-recreate
```

## ?? Sicurezza (Produzione)

?? **IMPORTANTE**: Questo setup è per **SVILUPPO LOCALE SOLO**

Per la produzione:
- Cambia le password nel `docker-compose.yml`
- Usa secrets di Docker/Kubernetes
- Configura un reverse proxy (nginx)
- Abilita HTTPS correttamente
- Usa un environment file separato (`.env`)

## ?? Risorse Utili

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Reference](https://docs.docker.com/compose/compose-file/)
- [Microsoft .NET Docker Guide](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/container-docker-introduction/)
- [Pomelo MySQL Provider](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql)

## ? Domande?

Se hai problemi:
1. Controlla i log: `docker-compose logs -f`
2. Verifica che i container siano in esecuzione: `docker-compose ps`
3. Riavvia i container: `docker-compose restart`

---

**Buono sviluppo! ??**
