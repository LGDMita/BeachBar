# BeachBar

Applicazione web per il bar di uno stabilimento balneare. Il gestore può:

- **aprire un conto** per ogni ombrellone quando il cliente ordina la prima volta
- **aggiungere consumazioni** nel corso della giornata, senza chiudere il conto
- **chiudere il conto** a fine servizio, con calcolo automatico del totale
- **consultare lo storico** di tutti gli scontrini passati

Include un **frontend Blazor** per l'operatività interna e una **REST API protetta da JWT** per integrazioni esterne (documentata con collection Postman).

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

Per testare le API usa Postman con la collection inclusa nel progetto. Vedi la sezione [Test con Postman](#test-con-postman) per i dettagli.

---

## Test con Postman

Il file `BeachBar.postman_collection.json` nella root del progetto contiene tutte le request già configurate e pronte all'uso.

### Importazione

1. Apri Postman
2. `File → Import` e seleziona `BeachBar.postman_collection.json`
3. La collection apparirà con quattro cartelle: **Auth**, **Prodotti**, **Sessioni**, **Ordini**

### Flusso di autenticazione

**È sufficiente fare login una sola volta.** Il token JWT dura 8 ore (configurabile in `appsettings.json` con `Jwt:ExpiresMinutes`).

1. Apri la cartella **Auth** ed esegui la request **Login**
   - Username e password sono già precompilati (`admin` / `admin123`)
   - Alla ricezione della risposta, uno script automatico nella tab *Tests* estrae il token e lo salva nella variabile di collection `{{token}}`
2. Da questo momento puoi eseguire qualsiasi altra request — il token viene incluso automaticamente nell'header `Authorization: Bearer {{token}}` senza nessuna azione manuale
3. Se il token scade (dopo 8 ore), ri-esegui semplicemente **Login**

Lo script che salva il token è visibile nella tab **Tests** della request Login:

```javascript
var json = pm.response.json();
pm.collectionVariables.set('token', json.token);
```

### Variabili di collection

| Variabile | Valore iniziale | Descrizione |
|---|---|---|
| `baseUrl` | `https://localhost:7298` | URL base dell'applicazione. Cambialo se usi una porta diversa o un dominio pubblico. |
| `token` | *(vuoto)* | Popolato automaticamente dopo il Login. Non modificarlo a mano. |

### Note sulle request

- Gli ID nelle URL (es. `/Sessioni/1`, `/Prodotti/1`) sono valori di esempio — sostituiscili con ID reali presenti nel tuo database
- La request **POST apri sessione** richiede un `ombrelloneId` valido
- La request **POST aggiungi ordine** richiede `prodottoId` e `quantita` (1–99)
- **DELETE elimina ordine** richiede sia `sessioneId` che `ordineId`

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

Di seguito il riepilogo degli endpoint. Per testare, usa la collection Postman inclusa nella root del progetto.

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
| `500 Internal Server Error` | Errore imprevisto lato server (es. database irraggiungibile, constraint violation) |

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

### Gestione degli errori

La gestione degli errori segue un pattern a tre livelli: i servizi rilevano, i controller e i componenti Blazor gestiscono.

**Service layer** — i metodi che operano su una singola risorsa lanciano `InvalidOperationException` se quella risorsa non esiste, invece di restituire silenziosamente. Le eccezioni si propagano al chiamante senza essere intercettate nel service stesso.

```csharp
var ombrellone = await _db.Ombrelloni.FindAsync(ombrelloneId)
    ?? throw new InvalidOperationException($"Ombrellone {ombrelloneId} non trovato.");
```

I metodi di sola lettura restituiscono `null` quando la risorsa non esiste — il chiamante decide se è un errore in base al contesto. I metodi che operano su insiemi (`RinominaCategoriaAsync`, `EliminaCategoriaAsync`) non lanciano se il filtro non produce risultati: zero elementi affetti è un esito valido, non un errore.

**Controller layer** — ogni action gestisce i casi attesi con i codici HTTP appropriati (`404`, `409`, `400`) e cattura le eccezioni impreviste in un `catch` generico che logga con `LogError` e restituisce `500` senza esporre dettagli interni al client. La validazione `ModelState` avviene prima del `try` perché è sincrona e non coinvolge I/O.

```csharp
if (!ModelState.IsValid)
    return BadRequest(ModelState);

try
{
    // logica e null-check con 404/409
}
catch (Exception ex)
{
    _logger.LogError(ex, "...");
    return StatusCode(500, "Errore interno del server.");
}
```

Tutti i codici di risposta possibili sono dichiarati con `[ProducesResponseType]`.

**Blazor layer** — ogni componente mantiene una variabile `string? errore` mostrata come banner rosso nel template. Ogni metodo async azzera `errore` all'inizio, esegue la chiamata in un `try`, e imposta `errore` nel `catch`. La navigazione post-operazione (`Nav.NavigateTo`) è sempre dentro il `try`, così non avviene in caso di errore. I metodi helper interni (es. `CaricaProdotti`) non hanno `try-catch` proprio: le eccezioni risalgono al metodo chiamante che ha il contesto per gestirle.

---

## Cosa aggiungerei con più tempo

- **Logging strutturato con Serilog**: correlazione per request ID, sink su file e/o Seq; `ILogger` è già iniettato in ogni controller, basta collegarlo
- **Endpoint report giornaliero**: `GET /api/statistiche/oggi` con totale incassato e dettaglio per ombrellone
- **Unit test con xUnit**: testare i servizi con `UseInMemoryDatabase` di EF Core, coprire i casi limite (sessione già chiusa, quantità fuori range, ombrellone inesistente)
- **Rate limiting**: limitare le chiamate per IP sull'endpoint di login per mitigare attacchi brute-force
- **Refresh token**: estendere il flusso JWT con un refresh token a lunga scadenza per evitare di ri-autenticarsi ogni 8 ore
