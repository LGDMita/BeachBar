# Belix

Gestionale web per il bar di uno stabilimento balneare. Lo staff può aprire, gestire e chiudere i conti dei clienti per ogni ombrellone o come conto volante, consultare tutto ciò che è aperto in un colpo solo, navigare lo storico per data e configurare la mappa della spiaggia in modo personalizzato.

Il sistema si chiama **Belix** ed è accessibile via browser — progettato per essere usato su tablet dallo staff in movimento.

---

## Tecnologie

| Layer | Tecnologia |
|---|---|
| Frontend | Blazor Web App (Interactive Server, SignalR) |
| Backend / API | ASP.NET Core 10, controller REST |
| ORM | Entity Framework Core 10 |
| Database | PostgreSQL (driver Npgsql 10) |
| Autenticazione UI | Cookie session (ASP.NET Core Cookie Auth) |
| Autenticazione API | JWT Bearer (HS256) |
| Hash password | BCrypt (work factor 12) |
| Linguaggio | C# 13, .NET 10 |
| Interazione drag | Vanilla JS con Pointer Events API |

---

## Prerequisiti

### 1. .NET 10 SDK

Scarica e installa da [https://dotnet.microsoft.com/download/dotnet/10.0](https://dotnet.microsoft.com/download/dotnet/10.0).

Verifica:
```bash
dotnet --version
# deve stampare 10.x.x
```

### 2. PostgreSQL

Installa PostgreSQL (versione 14 o superiore). Il database viene creato automaticamente dalle migrations.

Verifica che il servizio sia avviato:
```bash
psql -U postgres -c "SELECT version();"
```

### 3. dotnet-ef

```bash
dotnet tool install --global dotnet-ef
dotnet ef --version
```

---

## Configurazione

Modifica `BeachBar/appsettings.json`:

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

**Credenziali di default:** `admin` / `admin123`

Per generare un nuovo hash BCrypt:
```csharp
using BC = BCrypt.Net.BCrypt;
Console.WriteLine(BC.HashPassword("nuova-password", 12));
```

---

## Avvio dal primo clone

```bash
# 1. Ripristina i pacchetti NuGet
dotnet restore

# 2. Crea il database e applica tutte le migrations
dotnet ef database update --project BeachBar.Infrastructure --startup-project BeachBar

# 3. Avvia l'applicazione
dotnet run --project BeachBar
```

Con HTTPS:
```bash
dotnet run --project BeachBar --launch-profile https
```

| Risorsa | URL |
|---|---|
| Login | `http://localhost:5286/login` |
| App (HTTP) | `http://localhost:5286` |
| App (HTTPS) | `https://localhost:7298` |

---

## Autenticazione

### UI Blazor — cookie di sessione

La pagina `/login` è una Razor Page SSR (non Blazor interattivo, per poter scrivere il cookie HTTP). Tutte le pagine Blazor hanno `@attribute [Authorize]`: chi non è autenticato viene reindirizzato al login. Il logout è disponibile nella tab **Gestione** in Impostazioni o direttamente a `/logout`.

### API REST — JWT Bearer

```http
POST /api/auth/login
Content-Type: application/json

{ "username": "admin", "password": "admin123" }
```

Risposta:
```json
{ "token": "eyJ...", "expiresAt": "2024-06-01T20:00:00Z" }
```

Tutte le chiamate successive:
```http
Authorization: Bearer eyJ...
```

---

## Pagine Blazor

### Dashboard (`/`)

Pagina principale ad uso operativo quotidiano.

**Navigatore data** — in cima compare sempre un selettore di data con frecce ◀ ▶ e il tasto "Oggi" (visibile solo quando non si è sulla data corrente). La data selezionata è uno stato globale (`DateContext`, servizio Scoped) condiviso con tutte le altre pagine: Storico usa la stessa data, le sessioni vengono filtrate per `DataRiferimento`.

**Statistiche** — tre card sempre visibili mostrano: ombrelloni attivi, totale in corso (sessioni ancora aperte), incassato oggi (sessioni chiuse dopo l'ultimo reset visivo). Quando si naviga su una data passata, "Incassato oggi" diventa "Incassato" e non applica il filtro reset.

**Griglia ombrelloni** — tre modalità di rendering, selezionate automaticamente in base alla configurazione:

| Condizione | Rendering |
|---|---|
| Nessun ombrellone ha `CellaIndice` e nessun bordo configurato | Griglia sequenziale classica (CSS Grid, N colonne) |
| Nessun `CellaIndice` ma bordi configurati | Righe flex con separatori, ombrelloni in sequenza |
| Almeno un ombrellone ha `CellaIndice` | Layout custom: ogni ombrellone va nella sua cella, le celle vuote mostrano un quadrato grigio della stessa dimensione |

Ogni cella dell'ombrellone mostra: numero, stato (Libero / Occupato / N conti), nome cliente se presente. L'altezza è fissa e uguale per tutti — ombrelloni e placeholder grigie — per non far variare le righe al variare del contenuto.

Il click su un ombrellone con una sola sessione aperta va direttamente a `/conto/{id}`. Con più sessioni aperte va a `/ombrellone/{id}` che le elenca tutte.

**Stato vuoto** — se non esistono ombrelloni nel database, compare un empty state con link diretto alle Impostazioni.

**Conti volanti** — sezione separata sotto la griglia per le sessioni senza postazione fissa (clienti al banco, asporto, ecc.). Il pulsante "Nuovo" apre un form inline per inserire il nome del cliente e, opzionalmente, associare il conto a un ombrellone specifico. I conti volanti esistenti vengono elencati come card con nome, prodotti e totale parziale.

---

### Scontrini aperti (`/scontrini`)

Vista operativa che mostra tutti i conti aperti in questo momento, indipendentemente dalla data selezionata.

- **Ricerca live** per nome cliente o numero ombrellone (filtro sul campo mentre si digita)
- **Filtro chip** — Tutti / Ombrelloni / Volanti
- **Ordinamento toggle** — per orario di apertura (default: dal più vecchio) o per importo (dal più alto)
- **Riepilogo in testa** — numero di conti aperti e totale complessivo in corso
- **Ogni riga** mostra: badge colorato (blu = ombrellone, viola = volante), nome cliente, ora di apertura, numero prodotti, totale — il tap sulla riga va direttamente al conto

Utile per trovare rapidamente un cliente senza dover cercare nella griglia.

---

### Conto (`/conto/[id]`)

Scheda operativa della singola sessione, layout a due colonne.

**Colonna sinistra — prodotti:**
- Titolo: "Ombrellone N" o "Conto Extra" con la data di riferimento
- Nome cliente modificabile inline (tasto matita → campo input → Enter o ✔ per salvare)
- Tab per categoria prodotto
- Griglia prodotti: tap = aggiungi una consumazione

**Colonna destra — scontrino live:**
- Lista consumazioni con quantità e prezzi
- Tasto − per decrementare / rimuovere ogni riga
- Totale aggiornato in tempo reale
- Tasto "Chiudi conto" con conferma → registra la chiusura, libera l'ombrellone, torna alla dashboard
- Tasto "Annulla" con conferma → elimina la sessione senza tracciarla nello storico

---

### Ombrellone (`/ombrellone/[id]`)

Pagina intermedia che compare quando un ombrellone ha più di una sessione aperta (multi-conto).

- Elenca tutti i conti aperti per quell'ombrellone nella data selezionata, con nome cliente, conteggio prodotti e totale
- Ogni conto ha un tasto "Gestisci →" che va a `/conto/{id}`
- In fondo: form per aprire un conto aggiuntivo sullo stesso ombrellone (o prenotarne uno futuro se la data selezionata non è oggi)

---

### Storico (`/storico`)

Archivio delle sessioni chiuse, filtrato per data.

- Stesso navigatore data della dashboard (condividono il `DateContext`)
- Il filtro usa `DataRiferimento` (giorno in cui è stata aperta la sessione), non la data di chiusura — così una sessione aperta ieri sera e chiusa stamattina è trovabile sulla data di ieri
- Tabella con: orario chiusura, postazione (Ombrellone #N o Conto Extra + ID sessione), cliente, totale incassato
- Tasto "Elimina" con conferma JavaScript per rimuovere singoli record dallo storico
- Empty state con emoji quando non ci sono sessioni per la data selezionata

---

### Impostazioni (`/impostazioni`)

Tre tab di configurazione.

#### Tab Spiaggia

**Dimensioni griglia** — form con numero di righe e colonne. Se il layout ha già ombrelloni posizionati, un banner di conferma avverte che il ridimensionamento azzera tutte le posizioni e i bordi fuori dai nuovi limiti.

**Mappa ombrelloni** — editor interattivo della griglia:

- Header con counter "X / N celle occupate" e indicatore del prossimo numero da piazzare
- Toolbar con testo di aiuto e tasto "✕ Cancella" per attivare la modalità rimozione (utile su touch dove il click destro non è disponibile)
- Etichette numeriche per righe e colonne
- **Click su cella vuota** → posiziona il prossimo ombrellone non ancora assegnato; se non ne restano, ne crea uno nuovo
- **Click destro su cella piena** → rimuove l'ombrellone dalla cella (torna al pool)
- **Drag su celle vuote** (mouse e touch) → riempie in sequenza tutte le celle trascinate in un'unica operazione, con anteprima azzurra in tempo reale; il salvataggio avviene in un'unica transazione al rilascio
- **Click sul separatore tra celle** → attiva/disattiva un bordo di zona (linea scura verticale o orizzontale) per delimitare aree della spiaggia
- Pool degli ombrelloni non posizionati visibile sotto la griglia
- "⚡ Popola griglia intera" → riempie tutte le celle in sequenza creando gli ombrelloni mancanti
- "🗑️ Azzera posizioni" con conferma → rimuove tutti dall'editor senza eliminare i dati delle sessioni

Le celle vuote nella dashboard mostrano un quadrato grigio della stessa dimensione degli ombrelloni, così il layout della spiaggia è sempre visibile anche per le postazioni non assegnate. Lasciare celle vuote è una scelta legittima (es. zone senza servizio).

#### Tab Prodotti

- **Categorie**: lista con rename inline e eliminazione con conferma (elimina anche tutti i prodotti della categoria)
- **Prodotti**: form di aggiunta con nome, prezzo, categoria; tabella con modifica inline e eliminazione

#### Tab Gestione

- **Azzera contatori** — resetta il punto di partenza per "Incassato oggi" nella dashboard senza toccare i dati; i record rimangono consultabili nello storico
- **Reset giornaliero (Forzato)** — chiude tutte le sessioni aperte forzatamente; da usare solo a fine giornata in caso di ombrelloni rimasti aperti per errore
- **Esci** — logout e redirect al login

---

## Modello dei dati

```
ImpostazioniSpiaggia (1 riga)
├── NumeroOmbrelloni
├── NumeroColonne
├── NumeroRighe
├── BordiVerticali  ← indici colonne con separatore (CSV: "1,3")
├── BordiOrizzontali ← indici righe con separatore (CSV: "2")
└── UltimoResetStatistiche

Ombrellone
├── Numero
├── Occupato ← flag runtime per oggi; per date passate si ricalcola
└── CellaIndice ← posizione nella griglia custom (null = non posizionato)

Sessione
├── OmbrelloneId ← nullable (null = conto volante)
├── NomeCliente
├── Apertura (DateTime UTC)
├── Chiusura (DateTime? UTC)
├── Chiusa (bool)
├── DataRiferimento (DateOnly?) ← data operativa della sessione
└── Consumazioni[]

Consumazione
├── SessioneId
├── ProdottoId
└── Quantita

Prodotto
├── Nome
├── Prezzo
├── Categoria
└── Disponibile
```

### Migrations in ordine

| Migration | Data | Contenuto |
|---|---|---|
| `InitialCreate` | 2026-05-13 | Schema iniziale: Ombrellone, Sessione, Consumazione, Prodotto, ImpostazioniSpiaggia, seed dati |
| `AddDataRiferimento` | 2026-05-26 | `DataRiferimento DateOnly?` su Sessione; backfill da `Apertura::date` |
| `NullableOmbrelloneId` | 2026-05-26 | `OmbrelloneId` diventa nullable per supportare i conti volanti |
| `AddLayoutPersonalizzato` | 2026-05-27 | `CellaIndice int?` su Ombrellone; `NumeroRighe`, `BordiVerticali`, `BordiOrizzontali` su ImpostazioniSpiaggia |

---

## Struttura del progetto

```
BeachBar/
├── Components/
│   ├── Pages/
│   │   ├── Home.razor          ← Dashboard (griglia + conti volanti)
│   │   ├── Scontrini.razor     ← Lista scontrini aperti con ricerca e filtri
│   │   ├── Conto.razor         ← Gestione singola sessione
│   │   ├── Ombrellone.razor    ← Lista conti su un ombrellone multi-sessione
│   │   ├── Storico.razor       ← Archivio sessioni chiuse per data
│   │   ├── Impostazioni.razor  ← Config spiaggia, prodotti, gestione
│   │   ├── NotFound.razor      ← Pagina 404 personalizzata
│   │   └── Error.razor
│   ├── Layout/
│   │   ├── MainLayout.razor    ← Shell con navbar Belix
│   │   └── ReconnectModal.razor
│   └── App.razor               ← HTML root, caricamento CSS/JS
├── Controllers/                ← REST API
│   ├── AuthController.cs
│   ├── ProdottiController.cs
│   ├── SessioniController.cs
│   ├── OrdiniController.cs
│   └── Dto/
├── Pages/                      ← Razor Pages SSR
│   ├── Login.cshtml
│   └── Logout.cshtml
├── Services/
│   └── DateContext.cs          ← Stato data globale (Scoped)
├── wwwroot/
│   ├── belix.css               ← Shell, navbar, footer
│   ├── app.css                 ← Componenti globali riusabili
│   ├── dashboard.css           ← Stili pagina Home
│   ├── impostazioni.css        ← Stili pagina Impostazioni (editor incluso)
│   ├── ombrellone.css          ← Stili pagine Conto e Ombrellone
│   ├── scontrini.css           ← Stili pagina Scontrini
│   ├── storico.css             ← Stili pagina Storico
│   └── editor-drag.js          ← Drag pointer-events per l'editor mappa
└── appsettings.json

BeachBar.Core/
└── Entities/
    ├── ImpostazioniSpiaggia.cs
    ├── Ombrellone.cs
    ├── Sessione.cs
    ├── Consumazione.cs
    └── Prodotto.cs

BeachBar.Infrastructure/
├── Data/
│   └── BeachBarDbContext.cs
├── Migrations/
└── Services/
    ├── ISessioniService.cs / SessioniService.cs
    ├── IImpostazioniService.cs / ImpostazioniService.cs
    ├── IProdottiService.cs / ProdottiService.cs
    └── IConsumazioniService.cs / ConsumazioniService.cs
```

---

## Endpoints REST API

Tutti gli endpoint tranne il login richiedono `Authorization: Bearer <token>`.

### Autenticazione

| Metodo | Endpoint | Descrizione |
|---|---|---|
| POST | `/api/auth/login` | Restituisce un JWT |

### Prodotti

| Metodo | Endpoint | Descrizione |
|---|---|---|
| GET | `/api/prodotti` | Tutti i prodotti |
| GET | `/api/prodotti/{id}` | Singolo prodotto |
| GET | `/api/prodotti/categoria/{categoria}` | Prodotti per categoria |

### Sessioni

| Metodo | Endpoint | Descrizione |
|---|---|---|
| GET | `/api/sessioni` | Tutte le sessioni |
| GET | `/api/sessioni/aperte` | Solo sessioni aperte |
| GET | `/api/sessioni/{id}` | Sessione con consumazioni |
| POST | `/api/sessioni` | Apre nuova sessione su un ombrellone |
| POST | `/api/sessioni/extra` | Apre un conto volante (senza ombrellone) |
| PUT | `/api/sessioni/{id}/chiudi` | Chiude la sessione |

### Ordini

| Metodo | Endpoint | Descrizione |
|---|---|---|
| GET | `/api/sessioni/{sessioneId}/ordini` | Consumazioni della sessione |
| POST | `/api/sessioni/{sessioneId}/ordini` | Aggiunge una consumazione |
| DELETE | `/api/sessioni/{sessioneId}/ordini/{ordineId}` | Rimuove una consumazione |

### Codici di risposta

| Codice | Significato |
|---|---|
| `200 OK` | Richiesta riuscita |
| `201 Created` | Risorsa creata, header `Location` punta alla nuova risorsa |
| `204 No Content` | Eliminazione riuscita |
| `400 Bad Request` | Body non valido |
| `401 Unauthorized` | Token JWT assente, scaduto o non valido |
| `404 Not Found` | Risorsa non trovata |
| `409 Conflict` | Operazione non permessa nello stato attuale |
| `500 Internal Server Error` | Errore imprevisto lato server |

Per testare le API usa Postman con il file `BeachBar.postman_collection.json` incluso nella root. Lo script nella tab *Tests* della request Login salva automaticamente il token nella variabile `{{token}}` usata da tutte le altre request.

---

## Scelte tecniche

### DateContext — stato data globale

`BeachBar.Services.DateContext` è un servizio Scoped (un'istanza per circuito Blazor) che mantiene la data selezionata dallo staff. Tutte le pagine leggono e scrivono la stessa istanza: cambiare data sulla dashboard aggiorna anche Storico e viceversa. I metodi `Imposta`, `Avanza`, `Arretra`, `TornaOggi` e la proprietà `IsOggi` sono l'unica fonte di verità sulla data operativa corrente.

### Tre layout di griglia

Il rendering della griglia ombrelloni supporta tre modalità per coprire tre casi d'uso reali senza richiedere configurazione esplicita:

1. **Classico** — nessuna configurazione, tutto sequenziale. Basta impostare il numero di colonne.
2. **Sequenziale con zone** — nessun posizionamento custom ma con separatori di zona configurati. Il codice calcola le righe necessarie e le riempie in ordine, rispettando i separatori.
3. **Mappa custom** — ogni ombrellone ha un `CellaIndice` che lo posiziona in una cella precisa della griglia. Le celle vuote mostrano un placeholder grigio della stessa dimensione.

La scelta avviene in `CaricaDati()` in `Home.razor` in base alle proprietà degli ombrelloni caricati, senza nessun flag aggiuntivo nel DB.

### Drag con Pointer Events (mouse + touch)

Il vecchio meccanismo di drag usava `@onmousedown`/`@onmouseenter` su ogni cella Blazor — un round-trip al server per ogni cella sfiorata e incompatibile con touch (il browser cattura il touch sull'elemento iniziale e non propaga mai `mouseenter` ad altri elementi).

Il nuovo meccanismo usa `editor-drag.js` con Pointer Events API (`pointerdown`, `pointermove`, `pointerup`, `pointercancel`): funziona identicamente con mouse, dito e penna. Durante il drag, JS aggiorna le classi CSS direttamente nel DOM (anteprima azzurra senza round-trip). Al rilascio, una sola chiamata `[JSInvokable] CompletaDragJS(int[] indici)` porta la lista delle celle a Blazor, che esegue `AssegnaCelleAsync` in un'unica transazione DB. `touch-action: none` sul `.layout-editor` impedisce lo scroll della pagina durante il drag su touch.

### Conti volanti (OmbrelloneId nullable)

`Sessione.OmbrelloneId` è nullable. Un valore `null` indica un conto volante — un cliente senza postazione fissa. Questo permette di gestire consumazioni al banco, asporto o prenotazioni di servizi senza dover assegnare un ombrellone. I conti volanti possono essere associati facoltativamente a un ombrellone (per la fatturazione) ma non influiscono sul flag `Occupato` dell'ombrellone.

### Gestione errori a tre livelli

- **Service layer** — lancia `InvalidOperationException` se la risorsa non esiste; restituisce `null` per i metodi di sola lettura su risorsa singola; non lancia se un set è vuoto.
- **Controller layer** — gestisce i casi attesi con i codici HTTP corretti, cattura le eccezioni impreviste con `LogError` e restituisce `500` senza esporre dettagli.
- **Blazor layer** — ogni componente mantiene una variabile `string? errore` mostrata come banner rosso; ogni metodo async azzera `errore` prima della chiamata e lo imposta nel `catch`.

### Struttura CSS a file separati

Ogni pagina ha il proprio file CSS caricato globalmente da `App.razor`. Le classi sono organizzate per evitare conflitti di nomi:

- Prefisso `omb-` per classi usate dentro le celle ombrellone (`omb-nome`) per non sovrascrivere le stesse classi usate nella pagina Conto con semantica diversa.
- Le classi veramente globali (date navigator, alert-errore, caricamento, empty-state, alert-conferma, titolo, sezione-card) stanno in `app.css`.
- L'ordine di caricamento in `App.razor` è: shell → globali → pagine in ordine alfabetico.

---

## Cosa manca / da aggiungere

- **Aggiornamento automatico della dashboard** — se due tablet sono aperti simultaneamente, le modifiche da uno non appaiono sull'altro senza ricaricare. Blazor ha già SignalR attivo: basterebbe un timer o un meccanismo di broadcast per aggiornare la griglia ogni N secondi.
- **Report e statistiche** — endpoint `GET /api/statistiche` con totale incassato per data, per ombrellone, per categoria prodotto. Al momento le statistiche sono solo sul totale giornaliero.
- **Export CSV/PDF** dello storico giornaliero per la contabilità.
- **Logging strutturato con Serilog** — `ILogger` è già iniettato in ogni controller; manca solo il sink su file o su Seq con correlazione per request ID.
- **Unit test** — i servizi sono testabili con `UseInMemoryDatabase` di EF Core; manca la suite xUnit con copertura dei casi limite (sessione chiusa, quantità fuori range, ombrellone inesistente).
- **Rate limiting sull'endpoint di login** — per mitigare attacchi brute-force. ASP.NET Core 8+ ha `RateLimiter` built-in.
- **Refresh token** — il JWT dura 8 ore (configurabile); un refresh token eviterebbe il re-login.
- **Gestione multi-utente** — al momento esiste un solo account admin configurato in `appsettings.json`. Una tabella `Utenti` con ruoli (cameriere vs. gestore) permetterebbe di tracciare chi ha aperto o chiuso una sessione.
- **Supporto PWA / installazione su tablet** — aggiungere un manifest e un service worker minimal permetterebbe di installare l'app come PWA sulla schermata home del tablet, con icona dedicata e avvio senza barra del browser.
