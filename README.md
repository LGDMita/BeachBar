# BeachBar

Applicazione web per la gestione degli ordini di uno stabilimento balneare. Permette di aprire sessioni per ombrellone, registrare le consumazioni, chiudere il conto e consultare lo storico. Include un frontend Blazor per uso interno e una REST API documentata con Swagger per integrazioni esterne, entrambi protetti da autenticazione.

---

## Tecnologie utilizzate

| Layer | Tecnologia |
|---|---|
| Frontend | Blazor Web App (Interactive Server) |
| Backend / API | ASP.NET Core 10, controller REST |
| ORM | Entity Framework Core 10 |
| Database | PostgreSQL |
| Autenticazione UI | Cookie session (ASP.NET Core Cookie Auth) |
| Autenticazione API | JWT Bearer (HS256) |
| Hash password | BCrypt (work factor 12) |
| Documentazione API | Swashbuckle / Swagger UI |
| Linguaggio | C# 13, .NET 10 |

---

## Prerequisiti

### 1. .NET 10 SDK

Scarica e installa da [https://dotnet.microsoft.com/download/dotnet/10.0](https://dotnet.microsoft.com/download/dotnet/10.0).

Verifica l'installazione:
```bash
dotnet --version
# deve stampare 10.x.x
```

### 2. PostgreSQL

Installa PostgreSQL (versione 14 o superiore). Durante l'installazione imposta una password per l'utente `postgres`.

Verifica che il servizio sia avviato:
```bash
psql -U postgres -c "SELECT version();"
```

### 3. dotnet-ef (tool per le migrations)

```bash
dotnet tool install --global dotnet-ef
```

Verifica:
```bash
dotnet ef --version
```

---

## Configurazione

Apri il file `BeachBar/appsettings.json` e modifica i parametri con i tuoi dati:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=beachbar;Username=postgres;Password=TUA_PASSWORD"
  },
  "Jwt": {
    "Key": "chiave-segreta-di-almeno-32-caratteri",
    "Issuer": "BeachBar",
    "Audience": "BeachBar",
    "ExpiresMinutes": 480
  },
  "Admin": {
    "Username": "admin",
    "PasswordHash": "$2a$12$hash-bcrypt-della-tua-password"
  }
}
```

### Parametri database

| Parametro | Descrizione |
|---|---|
| `Host` | Indirizzo del server PostgreSQL (di solito `localhost`) |
| `Database` | Nome del database (verrà creato automaticamente) |
| `Username` | Utente PostgreSQL |
| `Password` | Password dell'utente PostgreSQL |

### Parametri JWT

| Parametro | Descrizione |
|---|---|
| `Jwt:Key` | Chiave segreta per firmare i token. Deve essere di almeno 32 caratteri. Cambiala in produzione. |
| `Jwt:Issuer` / `Audience` | Identificatori del token. Devono coincidere tra emittente e validatore. |
| `Jwt:ExpiresMinutes` | Durata del token in minuti (default: 480 = 8 ore). |

### Credenziali admin

Le credenziali dell'unico utente amministratore sono in `appsettings.json`. Non esiste registrazione: l'account è pre-configurato.

| Parametro | Descrizione |
|---|---|
| `Admin:Username` | Username dell'admin |
| `Admin:PasswordHash` | Hash BCrypt della password (work factor 12) |

**Credenziali di default:** `admin` / `admin123`

Per cambiare la password, genera un nuovo hash BCrypt e sostituiscilo in `PasswordHash`. Puoi usare uno strumento online o questo snippet C#:

```csharp
using BC = BCrypt.Net.BCrypt;
Console.WriteLine(BC.HashPassword("nuova-password", 12));
```

---

## Avvio dal primo clone

Esegui questi comandi in ordine dalla root della soluzione:

```bash
# 1. Ripristina i pacchetti NuGet
dotnet restore

# 2. Crea il database e applica la migrazione (crea tabelle e inserisce i dati di seed)
dotnet ef database update --project BeachBar.Infrastructure --startup-project BeachBar

# 3. Avvia l'applicazione
dotnet run --project BeachBar
```

Per usare HTTPS:
```bash
dotnet run --project BeachBar --launch-profile https
```

---

## URL dell'applicazione

| Risorsa | URL |
|---|---|
| Login | `http://localhost:5286/login` |
| Frontend Blazor | `http://localhost:5286` |
| Frontend Blazor (HTTPS) | `https://localhost:7298` |
| Swagger UI | `http://localhost:5286/swagger` |
| Swagger UI (HTTPS) | `https://localhost:7298/swagger` |

---

## Autenticazione

Il sistema ha due percorsi di autenticazione separati per i due client:

### UI Blazor — cookie di sessione

La pagina `/login` è una Razor Page SSR che valida le credenziali e scrive un cookie di autenticazione. Tutte le pagine Blazor richiedono il cookie (`@attribute [Authorize]`): chi non è autenticato viene reindirizzato automaticamente al login.

Il logout è disponibile tramite il pulsante in basso a destra su tutte le pagine, o navigando direttamente su `/logout`.

### API REST — JWT Bearer

Per chiamare qualsiasi endpoint protetto delle API è necessario prima ottenere un token:

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "admin123"
}
```

Risposta:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-06-01T20:00:00Z"
}
```

Usa il token nelle chiamate successive:
```http
GET /api/sessioni
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

Su Swagger UI: clicca il pulsante **Authorize** in alto a destra, incolla il token nel campo `Bearer` e conferma. Da quel momento tutte le richieste inviate dall'interfaccia includeranno l'header di autorizzazione.

---

## Struttura del progetto

```
BeachBar/
├── Components/             ← Pagine e componenti Blazor
│   ├── Pages/             ← Home, Ombrellone, Storico, Impostazioni (tutte @Authorize)
│   ├── Layout/            ← MainLayout con tasto logout
│   └── Routes.razor       ← AuthorizeRouteView + redirect al login
├── Controllers/            ← Controller REST API
│   ├── AuthController.cs  ← POST /api/auth/login (pubblico)
│   ├── ProdottiController.cs
│   ├── SessioniController.cs
│   ├── OrdiniController.cs
│   └── Dto/               ← Data Transfer Objects
│       ├── TokenDto.cs
│       ├── ProdottoDto.cs
│       ├── SessioneDto.cs
│       ├── ConsumazioneDto.cs
│       └── Request/       ← DTO per il body delle richieste
│           ├── LoginRequest.cs
│           ├── ApriSessioneRequest.cs
│           └── AggiungiConsumazioneRequest.cs
├── Pages/                  ← Razor Pages SSR (necessarie per scrivere cookie HTTP)
│   ├── Login.cshtml        ← Form di login
│   └── Logout.cshtml       ← Cancella il cookie e reindirizza
├── appsettings.json        ← Connection string, JWT config, credenziali admin
└── Program.cs              ← Entry point, DI, pipeline HTTP, auth

BeachBar.Core/
└── Entities/               ← Entità di dominio (POCO, nessuna dipendenza)

BeachBar.Infrastructure/
├── Data/
│   └── BeachBarDbContext.cs ← DbContext EF Core, seed data
├── Migrations/             ← Unica migrazione (stato corrente del DB)
└── Services/               ← 4 servizi focalizzati, ognuno con la propria interfaccia
    ├── IProdottiService.cs / ProdottiService.cs
    ├── ISessioniService.cs / SessioniService.cs
    ├── IConsumazioniService.cs / ConsumazioniService.cs
    └── IImpostazioniService.cs / ImpostazioniService.cs
```

---

## Struttura dei layer e perché sono separati

**`BeachBar.Core`** — contiene solo le entità di dominio come classi C# pure, senza dipendenze da EF Core o da altri framework. In questo modo le entità possono essere usate in qualsiasi layer senza introdurre accoppiamenti indesiderati.

**`BeachBar.Infrastructure`** — gestisce tutto ciò che riguarda la persistenza: il `BeachBarDbContext` per EF Core, le migrations, e i quattro servizi applicativi che racchiudono la logica di accesso ai dati. Dipende solo da `Core`.

**`BeachBar`** — è il punto di ingresso dell'applicazione. Ospita sia il frontend Blazor sia i controller REST. Non contiene logica di business: dipende da `Core` e `Infrastructure` e si limita a orchestrare le richieste.

---

## Endpoints REST API

La documentazione interattiva completa è disponibile su Swagger UI all'avvio. Di seguito il riepilogo degli endpoint.

> Tutti gli endpoint tranne `POST /api/auth/login` richiedono il token JWT nell'header `Authorization: Bearer <token>`.

### Autenticazione

| Metodo | Endpoint | Descrizione | Risposta |
|---|---|---|---|
| POST | `/api/auth/login` | Valida le credenziali e restituisce un JWT | `200 OK` / `401` |

**Body:**
```json
{ "username": "admin", "password": "admin123" }
```

### Prodotti

| Metodo | Endpoint | Descrizione | Risposta |
|---|---|---|---|
| GET | `/api/prodotti` | Lista di tutti i prodotti | `200 OK` |
| GET | `/api/prodotti/{id}` | Singolo prodotto per ID | `200 OK` / `404` |
| GET | `/api/prodotti/categoria/{categoria}` | Prodotti filtrati per categoria | `200 OK` |

### Sessioni

| Metodo | Endpoint | Descrizione | Risposta |
|---|---|---|---|
| GET | `/api/sessioni` | Tutte le sessioni (aperte e chiuse) | `200 OK` |
| GET | `/api/sessioni/aperte` | Solo le sessioni attualmente aperte | `200 OK` |
| GET | `/api/sessioni/{id}` | Singola sessione con le sue consumazioni | `200 OK` / `404` |
| POST | `/api/sessioni` | Apre una nuova sessione per un ombrellone | `201 Created` / `400` / `404` / `409` |
| PUT | `/api/sessioni/{id}/chiudi` | Chiude la sessione e calcola il totale | `200 OK` / `404` / `409` |

**Body POST `/api/sessioni`:**
```json
{
  "ombrelloneId": 5,
  "nomeCliente": "Rossi"
}
```

### Ordini

| Metodo | Endpoint | Descrizione | Risposta |
|---|---|---|---|
| GET | `/api/sessioni/{sessioneId}/ordini` | Consumazioni della sessione | `200 OK` / `404` |
| POST | `/api/sessioni/{sessioneId}/ordini` | Aggiunge un prodotto alla sessione | `201 Created` / `400` / `404` / `409` |
| DELETE | `/api/sessioni/{sessioneId}/ordini/{ordineId}` | Rimuove una consumazione | `204 No Content` / `404` / `409` |

**Body POST ordine:**
```json
{
  "prodottoId": 3,
  "quantita": 2
}
```

### Codici di risposta

| Codice | Significato |
|---|---|
| `200 OK` | Richiesta riuscita |
| `201 Created` | Risorsa creata, header `Location` punta alla nuova risorsa |
| `204 No Content` | Eliminazione riuscita |
| `400 Bad Request` | Body della richiesta non valido (campo obbligatorio mancante, valore fuori range) |
| `401 Unauthorized` | Token JWT assente, scaduto o non valido |
| `404 Not Found` | La risorsa richiesta non esiste |
| `409 Conflict` | Operazione non permessa nello stato attuale (es. sessione già chiusa, ombrellone già occupato) |

---

## Scelte tecniche

### Blazor e API nello stesso progetto

Il progetto `BeachBar` ospita sia il frontend Blazor sia i controller REST. Questa è una scelta deliberata, non una dimenticanza.

ASP.NET Core è un singolo host HTTP (Kestrel) che può servire contemporaneamente componenti Razor e controller API — non sono due tecnologie che si escludono. Co-hostarle nello stesso processo ha vantaggi concreti per questa applicazione:

- **Nessuna chiamata HTTP interna**: i controller iniettano i servizi direttamente dal DI container, senza connessioni HTTP tra un backend e un frontend separati.
- **Deployment semplice**: un solo processo, una sola porta, nessuna configurazione CORS.
- **Scala adeguata al problema**: un gestionale per uno stabilimento balneare non richiede scaling indipendente tra UI e API.

Separare in un progetto `BeachBar.Api` autonomo avrebbe senso in presenza di client multipli indipendenti (app mobile, terminale POS, integrazioni esterne) o di team con cicli di deploy separati. In quel caso il refactor sarebbe semplice: la logica sta già tutta in `BeachBar.Infrastructure`.

### Due schemi di autenticazione in parallelo

L'applicazione registra contemporaneamente Cookie Authentication e JWT Bearer. Il motivo è tecnico: le Razor Pages (Blazor) e le REST API vivono nello stesso processo ma hanno client diversi.

- Il **cookie** è lo schema naturale per un browser: viene inviato automaticamente a ogni richiesta, è trasparente all'utente, e permette la protezione CSRF built-in di ASP.NET Core.
- Il **JWT** è lo schema naturale per un client API (Postman, app mobile, script): stateless, non richiede sessione lato server, trasmesso esplicitamente nell'header `Authorization`.

I controller API dichiarano esplicitamente `[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]` per essere valutati solo dallo schema JWT, indipendentemente dallo schema di default.

### Login tramite Razor Page (non Blazor)

La pagina di login è implementata come Razor Page SSR e non come componente Blazor Interactive Server. Il motivo è una limitazione di Blazor Server: i componenti interattivi vengono eseguiti in una connessione SignalR persistente, nella quale la risposta HTTP iniziale è già stata inviata. Non è quindi possibile scrivere un cookie di autenticazione dall'interno di un componente interattivo. La Razor Page, eseguita in modo classico request/response, non ha questo vincolo.

### Quattro servizi focalizzati

La logica applicativa è divisa in quattro servizi con responsabilità distinte:

| Servizio | Responsabilità |
|---|---|
| `IProdottiService` | CRUD prodotti e categorie |
| `ISessioniService` | Ciclo di vita delle sessioni e accesso agli ombrelloni |
| `IConsumazioniService` | Aggiunta e rimozione di consumazioni |
| `IImpostazioniService` | Configurazione spiaggia e statistiche |

Ogni controller e ogni pagina Blazor inietta solo i servizi di cui ha bisogno, rendendo esplicite le dipendenze e facilitando il testing.

### Controller come thin layer

I controller non contengono logica di business. Si limitano a: validare l'input, chiamare il servizio appropriato, mappare il risultato su DTO e restituire il codice HTTP corretto. La logica rimane in un unico posto (i servizi) e i controller restano facilmente testabili.

### DTO invece di entità EF Core

Le entità EF Core portano con sé navigation properties e stato di tracking che non devono uscire dalle API. I DTO (`ProdottoDto`, `SessioneDto`, `ConsumazioneDto`) sono plain object che espongono solo i campi utili al client, disaccoppiando la forma dell'API dallo schema del database.

### Gestione degli errori esplicita

Ogni action gestisce esplicitamente i propri casi di errore: `404` se la risorsa non esiste, `409` se lo stato non permette l'operazione, `400` se il body non è valido. Non ci sono catch-all globali: ogni errore produce una risposta HTTP semanticamente corretta.

### Swagger / Swashbuckle

Swagger offre una UI interattiva per esplorare e testare gli endpoint direttamente dal browser. Gli attributi `[ProducesResponseType]` sui controller documentano tutti i codici di risposta possibili. Il pulsante **Authorize** nella UI permette di inserire il JWT e testare gli endpoint protetti senza uscire dal browser.

---

## Cosa aggiungerei con più tempo

- **Logging strutturato con Serilog**: correlazione per request ID, sink su file e/o Seq; `ILogger` è già iniettato in ogni controller, basta collegarlo
- **Endpoint report giornaliero**: `GET /api/statistiche/oggi` con totale incassato e dettaglio per ombrellone
- **Unit test con xUnit**: testare i servizi con `UseInMemoryDatabase` di EF Core, coprire i casi limite (sessione già chiusa, quantità fuori range, ombrellone inesistente)
- **Rate limiting**: limitare le chiamate per IP sull'endpoint di login per mitigare attacchi brute-force
- **Refresh token**: estendere il flusso JWT con un refresh token a lunga scadenza per evitare di ri-autenticarsi ogni 8 ore
