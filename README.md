# BeachBar

Applicazione web per la gestione degli ordini di uno stabilimento balneare. Permette di aprire sessioni per ombrellone, registrare le consumazioni, chiudere il conto e consultare lo storico. Include un frontend Blazor per uso interno e una REST API documentata con Swagger per integrazioni esterne.

---

## Tecnologie utilizzate

| Layer | Tecnologia |
|---|---|
| Frontend | Blazor Web App (Interactive Server) |
| Backend / API | ASP.NET Core 10, controller REST |
| ORM | Entity Framework Core 10 |
| Database | PostgreSQL |
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

Apri il file `BeachBar/appsettings.json` e modifica la stringa di connessione con i tuoi dati:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=beachbar;Username=postgres;Password=TUA_PASSWORD"
  }
}
```

| Parametro | Descrizione |
|---|---|
| `Host` | Indirizzo del server PostgreSQL (di solito `localhost`) |
| `Database` | Nome del database (verrà creato automaticamente) |
| `Username` | Utente PostgreSQL |
| `Password` | Password dell'utente PostgreSQL |

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
| Frontend Blazor | `http://localhost:5286` |
| Frontend Blazor (HTTPS) | `https://localhost:7298` |
| Swagger UI | `http://localhost:5286/swagger` |
| Swagger UI (HTTPS) | `https://localhost:7298/swagger` |

---

## Struttura del progetto

```
BeachBar/
├── Components/             ← Pagine e componenti Blazor
├── Controllers/            ← Controller REST API
│   └── Dto/               ← Data Transfer Objects (response e request)
│       └── Request/       ← DTO per il body delle richieste POST/PUT
├── Properties/
│   └── launchSettings.json
├── appsettings.json        ← Configurazione (connection string, logging)
└── Program.cs              ← Entry point, configurazione DI e pipeline HTTP

BeachBar.Core/
└── Entities/               ← Entità di dominio (POCO, nessuna dipendenza)
    ├── Ombrellone.cs
    ├── Sessione.cs
    ├── Consumazione.cs
    ├── Prodotto.cs
    └── ImpostazioniSpiaggia.cs

BeachBar.Infrastructure/
├── Data/
│   └── BeachBarDbContext.cs ← DbContext EF Core, seed data
├── Migrations/             ← Unica migrazione (stato corrente del DB)
└── Services/
    ├── IBeachBarService.cs ← Interfaccia del servizio
    └── BeachBarSerivce.cs  ← Implementazione con tutta la logica di business
```

---

## Struttura dei layer e perché sono separati

**`BeachBar.Core`** — contiene solo le entità di dominio come classi C# pure, senza dipendenze da EF Core o da altri framework. In questo modo le entità possono essere usate in qualsiasi layer senza introdurre accoppiamenti indesiderati.

**`BeachBar.Infrastructure`** — gestisce tutto ciò che riguarda la persistenza: il `BeachBarDbContext` per EF Core, le migrations, e il `BeachBarService` che racchiude tutta la logica di accesso ai dati. Dipende solo da `Core`.

**`BeachBar`** — è il punto di ingresso dell'applicazione. Ospita sia il frontend Blazor sia i controller REST. Non contiene logica di business: dipende da `Core` e `Infrastructure` e si limita a orchestrare le richieste.

---

## Endpoints REST API

La documentazione interattiva completa è disponibile su Swagger UI all'avvio. Di seguito il riepilogo degli endpoint.

### Prodotti — `GET /api/prodotti`

| Metodo | Endpoint | Descrizione | Risposta |
|---|---|---|---|
| GET | `/api/prodotti` | Lista di tutti i prodotti | `200 OK` |
| GET | `/api/prodotti/{id}` | Singolo prodotto per ID | `200 OK` / `404` |
| GET | `/api/prodotti/categoria/{categoria}` | Prodotti filtrati per categoria | `200 OK` |

### Sessioni — `GET /api/sessioni`

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

### Ordini — `GET /api/sessioni/{sessioneId}/ordini`

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
| `404 Not Found` | La risorsa richiesta non esiste |
| `409 Conflict` | Operazione non permessa nello stato attuale (es. sessione già chiusa, ombrellone già occupato) |

---

## Scelte tecniche

### Blazor e API nello stesso progetto

Il progetto `BeachBar` ospita sia il frontend Blazor sia i controller REST. Questa è una scelta deliberata, non una dimenticanza.

ASP.NET Core è un singolo host HTTP (Kestrel) che può servire contemporaneamente componenti Razor e controller API — non sono due tecnologie che si escludono. Co-hostarle nello stesso processo ha vantaggi concreti per questa applicazione:

- **Nessuna chiamata HTTP interna**: i controller iniettano `IBeachBarService` direttamente dal DI container, senza connessioni HTTP tra un backend e un frontend separati.
- **Deployment semplice**: un solo processo, una sola porta, nessuna configurazione CORS.
- **Scala adeguata al problema**: un gestionale per uno stabilimento balneare non richiede scaling indipendente tra UI e API.

Separare in un progetto `BeachBar.Api` autonomo avrebbe senso in presenza di client multipli indipendenti (app mobile, terminale POS, integrazioni esterne) o di team con cicli di deploy separati. In quel caso il refactor sarebbe semplice: la logica sta già tutta in `BeachBar.Infrastructure`.

### Controller come thin layer

I controller non contengono logica di business. Si limitano a: validare l'input, chiamare `IBeachBarService`, mappare il risultato su DTO e restituire il codice HTTP corretto. La logica rimane in un unico posto (il servizio) e i controller restano facilmente testabili.

### DTO invece di entità EF Core

Le entità EF Core portano con sé navigation properties e stato di tracking che non devono uscire dalle API. I DTO (`ProdottoDto`, `SessioneDto`, `ConsumazioneDto`) sono plain object che espongono solo i campi utili al client, disaccoppiando la forma dell'API dallo schema del database.

### Interfaccia `IBeachBarService`

Il servizio è registrato nel DI container tramite interfaccia (`AddScoped<IBeachBarService, BeachBarService>()`). I controller dipendono dall'interfaccia, non dalla classe concreta: questo rende possibile sostituire o mockare il servizio nei test senza modificare i controller.

### Gestione degli errori esplicita

Ogni action gestisce esplicitamente i propri casi di errore: `404` se la risorsa non esiste, `409` se lo stato non permette l'operazione, `400` se il body non è valido. Non ci sono catch-all globali: ogni errore produce una risposta HTTP semanticamente corretta.

### Swagger / Swashbuckle

Swagger offre una UI interattiva per esplorare e testare gli endpoint direttamente dal browser. Gli attributi `[ProducesResponseType]` sui controller documentano tutti i codici di risposta possibili, rendendo il contratto API leggibile anche da chi non conosce il codice.

---

## Cosa aggiungerei con più tempo

- **Autenticazione JWT**: distinguere gli operatori di cassa, proteggere le operazioni distruttive (chiusura sessione, eliminazione consumazione)
- **Logging strutturato con Serilog**: correlazione per request ID, sink su file e/o Seq; `ILogger` è già iniettato in ogni controller, basta collegarlo
- **Paginazione sugli endpoint di lista**: `GET /api/sessioni` e `GET /api/prodotti` diventano costosi su dataset grandi; aggiungere `?page=&pageSize=` con header `X-Total-Count`
- **Endpoint report giornaliero**: `GET /api/statistiche/oggi` con totale incassato e dettaglio per ombrellone
- **Unit test con xUnit**: testare `BeachBarService` con `UseInMemoryDatabase` di EF Core, coprire i casi limite (sessione già chiusa, quantità fuori range, ombrellone inesistente)
- **FluentValidation**: sostituire le `DataAnnotations` nei DTO di request con validator separati, più leggibili e componibili
- **Rate limiting**: limitare le chiamate per IP sugli endpoint di creazione ordine
