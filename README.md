# Belix

Gestionale web per il bar di uno stabilimento balneare. Lo staff può aprire, gestire e chiudere le **liste** (conti) dei clienti per ogni ombrellone o come lista volante, con supporto a soggiorni multi-giorno, consultare tutto ciò che è aperto in un colpo solo, navigare lo storico per data e configurare la mappa della spiaggia in modo personalizzato.

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

# 2. Crea il database e applica la migration con tutti i dati di seed
dotnet ef database update --project BeachBar.Infrastructure --startup-project BeachBar

# 3. Avvia l'applicazione
dotnet run --project BeachBar
```

Con HTTPS:
```bash
dotnet run --project BeachBar --launch-profile https
```

Con accesso da rete locale (tablet nella stessa WiFi):
```bash
dotnet run --project BeachBar --launch-profile lan
```
Poi trova il tuo IP con `ipconfig` e apri `http://192.168.x.x:5286` sul tablet.

| Risorsa | URL |
|---|---|
| Login | `http://localhost:5286/login` |
| App (HTTP) | `http://localhost:5286` |
| App (HTTPS) | `https://localhost:7298` |

### Reset completo del database

Per ripartire da zero mantenendo il layout spiaggia configurato nel seed:

```bash
dotnet ef database drop --force --project BeachBar.Infrastructure --startup-project BeachBar
dotnet ef database update --project BeachBar.Infrastructure --startup-project BeachBar
```

---

## Dati di seed

Il database viene popolato automaticamente con:

- **78 ombrelloni** numerati da 1 a 78, con le posizioni esatte nella griglia
- **Griglia 17 × 5**: 17 colonne, 5 righe, con separatori di zona dopo la colonna 9 e dopo la colonna 15
- **Layout spiaggia**:
  - Riga 0 (fronte mare): ombrelloni 67–77, con uno spazio vuoto nelle celle 5–10
  - Riga 1: ombrelloni 50–66
  - Riga 2: ombrelloni 33–49
  - Riga 3: ombrelloni 16–32
  - Riga 4: ombrelloni 1–15, con celle vuote agli estremi
  - Ombrellone 78: non posizionato in griglia
- **98 prodotti** suddivisi in 11 categorie: Panini, Insalate, Pizza, Piatti, Pasta, Frutta, Caffetteria, Bibite, Birre, Vini, Aperitivi

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

**Navigatore data** — selettore con frecce ◀ ▶ e tasto "Oggi" (visibile solo quando non si è sulla data corrente). La data è uno stato globale (`DateContext`) condiviso con Storico e Liste.

**Statistiche** — tre card: ombrelloni attivi, totale in corso (somma delle liste aperte che includono la data corrente nel loro range), incassato oggi.

**Griglia ombrelloni** — tre modalità di rendering automatiche:

| Condizione | Rendering |
|---|---|
| Nessun `CellaIndice` e nessun bordo | Griglia sequenziale classica (CSS Grid, N colonne) |
| Nessun `CellaIndice` ma bordi configurati | Righe flex con separatori di zona |
| Almeno un ombrellone con `CellaIndice` | Layout custom: ogni ombrellone nella sua cella, celle vuote come quadrati grigi della stessa dimensione |

Le celle vuote mostrano un quadrato grigio — visibile ma neutro, per far capire il layout della spiaggia senza confondersi con gli ombrelloni liberi.

Clicking un ombrellone con una sola lista aperta va direttamente a `/conto/{id}`. Con più liste va a `/ombrellone/{id}`.

**Conti volanti** — sezione sotto la griglia per liste senza postazione fissa (bar, asporto, ecc.). Bottone "Nuova" apre un form inline con nome e ombrellone opzionale.

---

### Liste aperte (`/scontrini`)

Vista operativa: tutte le liste aperte in questo momento, indipendentemente dalla data.

- **Ricerca live** per nome cliente o numero ombrellone
- **Filtro chip** — Tutti / Ombrelloni / Volanti
- **Ordinamento toggle** — per orario di apertura (dal più vecchio) o per importo
- **Riepilogo in testa** — count totale e importo complessivo in corso
- **Tap su riga** → apre direttamente la lista

---

### Ombrellone (`/ombrellone/{id}`)

Pagina per aprire una nuova lista o gestire quelle esistenti su un ombrellone.

- **Liste aperte**: elenco delle liste attive per la data (o nel range multi-giorno), con nome, date, prodotti e totale parziale — tap "Gestisci →" va alla lista
- **Apri lista**: campo nome + campo **Giorni di soggiorno** (default 1) con anteprima della data di fine — crea una nuova lista con il range di date calcolato
- **Torna alla spiaggia**: bottone in basso che riporta alla dashboard (senza freccioline in alto)

---

### Lista / Conto (`/conto/{id}`)

Scheda operativa della singola lista, layout a due colonne.

**Colonna sinistra — prodotti:**
- Intestazione con numero ombrellone o "Lista Volante" + data (o range date per soggiorni multi-giorno)
- Nome cliente modificabile inline
- Tab per categoria prodotto
- Griglia prodotti: tap = aggiungi al conto del giorno corrente

**Colonna destra — lista:**
- Per soggiorni **single day**: lista piatta con tutti i prodotti
- Per soggiorni **multi-giorno**: prodotti raggruppati per giorno con intestazione data e subtotale per ogni giorno, totale complessivo in fondo
- Bottone − per decrementare/rimuovere (rimuove solo dal giorno corretto)
- **Torna alla spiaggia** — riporta alla dashboard
- **Chiudi e incassa** — chiude la lista e torna alla dashboard
- **Annulla lista** — elimina senza tracciare nello storico

---

### Storico (`/storico`)

Archivio delle liste chiuse, filtrate per `DataRiferimento` (giorno di apertura, non di chiusura).

---

### Impostazioni (`/impostazioni`)

#### Tab Spiaggia

**Dimensioni griglia** — imposta righe × colonne. Il ridimensionamento azzera le posizioni.

**Mappa ombrelloni** — editor interattivo della griglia spiaggia:
- **Click su cella vuota** → posiziona il prossimo ombrellone non assegnato
- **Click destro su cella piena** → rimuove dall'editor
- **Drag (mouse e touch)** → riempie più celle in sequenza con un solo gesto; il touch non blocca lo scroll dell'editor quando non si sta trascinando
- **Click sul separatore** → attiva/disattiva bordo di zona (linea verticale o orizzontale)
- **⚡ Popola griglia intera** → riempie tutto sequenzialmente, crea ombrelloni mancanti
- **🗑️ Azzera posizioni** → rimuove tutti con conferma
- Scroll orizzontale disponibile su tablet per griglie larghe

#### Tab Prodotti

Tre sezioni card distinte:

1. **Categorie** — rinomina e elimina categorie (con conferma: elimina anche i prodotti)
2. **Aggiungi prodotto** — form compatto con prefisso `€` inline, step `0.01` per il prezzo; distinto visivamente (bordo sinistro blu)
3. **Lista prodotti** — griglia auto-fill a più colonne (si adatta alla larghezza dello schermo: ~4 colonne su desktop, ~3 su tablet, ~2 su mobile) con:
   - Barra di ricerca live per nome
   - Chip filtro per categoria
   - Prodotti raggruppati per sezione categoria con header e contatore
   - Ogni prodotto come card: nome, prezzo, badge disponibilità, bottoni ✏️ 🗑️

#### Tab Gestione

- **Azzera contatori** — resetta il punto zero dell'incasso odierno senza cancellare dati (utile per casse intermedie)
- **Reset giornaliero (Forzato)** — chiude tutte le liste aperte; usare solo a fine giornata in caso di errori
- **Esci** — logout

---

## Modello dei dati

```
ImpostazioniSpiaggia (1 riga)
├── NumeroOmbrelloni
├── NumeroColonne
├── NumeroRighe
├── BordiVerticali     ← indici colonne con separatore (CSV: "9,15")
├── BordiOrizzontali   ← indici righe con separatore
└── UltimoResetStatistiche

Ombrellone
├── Numero
├── Occupato           ← flag runtime per oggi; ricalcolato per date passate
└── CellaIndice        ← posizione nella griglia custom (null = non posizionato)

Sessione  ("Lista")
├── OmbrelloneId       ← nullable: null = lista volante
├── NomeCliente
├── Apertura           ← DateTime UTC
├── Chiusura           ← DateTime? UTC
├── Chiusa             ← bool
├── DataRiferimento    ← DateOnly? — giorno di apertura
├── DataFine           ← DateOnly? — ultimo giorno del soggiorno (null = single day)
└── Consumazioni[]

Consumazione
├── SessioneId
├── ProdottoId
├── Quantita
├── Timestamp          ← DateTime UTC
└── Giorno             ← DateOnly — giorno in cui il prodotto è stato ordinato

Prodotto
├── Nome
├── Prezzo
├── Categoria
└── Disponibile
```

### Migrations

Una singola migration `InitialCreate` contiene l'intero schema + seed data (ombrelloni, prodotti, griglia). Non ci sono migration incrementali.

---

## Struttura del progetto

```
BeachBar/
├── Components/
│   ├── Pages/
│   │   ├── Home.razor          ← Dashboard (griglia + conti volanti)
│   │   ├── Scontrini.razor     ← Liste aperte con ricerca e filtri
│   │   ├── Conto.razor         ← Gestione singola lista (prodotti + scontrino per giorno)
│   │   ├── Ombrellone.razor    ← Apertura lista con giorni soggiorno
│   │   ├── Storico.razor       ← Archivio liste chiuse per data
│   │   ├── Impostazioni.razor  ← Config spiaggia, prodotti, gestione
│   │   ├── NotFound.razor
│   │   └── Error.razor
│   ├── Layout/
│   │   ├── MainLayout.razor    ← Shell Belix con navbar
│   │   └── ReconnectModal.razor
│   └── App.razor               ← HTML root, CSS/JS globali
├── Controllers/                ← REST API
│   ├── AuthController.cs
│   ├── ProdottiController.cs
│   ├── SessioniController.cs
│   ├── OrdiniController.cs
│   └── Dto/
├── Pages/
│   ├── Login.cshtml
│   └── Logout.cshtml
├── Services/
│   └── DateContext.cs          ← Stato data globale (Scoped)
├── wwwroot/
│   ├── belix.css               ← Shell, navbar, footer
│   ├── app.css                 ← Componenti globali (alert, date navigator, empty state…)
│   ├── dashboard.css           ← Pagina Home
│   ├── impostazioni.css        ← Pagina Impostazioni + editor griglia
│   ├── ombrellone.css          ← Pagine Conto e Ombrellone
│   ├── scontrini.css           ← Pagina Liste aperte
│   ├── storico.css             ← Pagina Storico
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
│   └── BeachBarDbContext.cs    ← DbContext + seed completo (ombrelloni, prodotti, griglia)
├── Migrations/
│   └── InitialCreate           ← Unica migration con schema + seed
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

### Sessioni (Liste)

| Metodo | Endpoint | Descrizione |
|---|---|---|
| GET | `/api/sessioni` | Tutte le liste |
| GET | `/api/sessioni/aperte` | Solo liste aperte |
| GET | `/api/sessioni/{id}` | Lista con consumazioni |
| POST | `/api/sessioni` | Apre nuova lista su un ombrellone |
| POST | `/api/sessioni/extra` | Apre una lista volante |
| PUT | `/api/sessioni/{id}/chiudi` | Chiude la lista |

### Ordini (Consumazioni)

| Metodo | Endpoint | Descrizione |
|---|---|---|
| GET | `/api/sessioni/{sessioneId}/ordini` | Consumazioni della lista |
| POST | `/api/sessioni/{sessioneId}/ordini` | Aggiunge una consumazione |
| DELETE | `/api/sessioni/{sessioneId}/ordini/{ordineId}` | Rimuove una consumazione |

---

## Scelte tecniche

### DateContext — stato data globale

`DateContext` è un servizio Scoped (una istanza per circuito Blazor) che mantiene la data selezionata. Tutti i componenti leggono e scrivono la stessa istanza: cambiare data sulla dashboard aggiorna anche Storico e liste.

### Soggiorni multi-giorno

`Sessione.DataFine DateOnly?` estende il concetto di lista a più giorni. Tutte le query che filtrano per data usano un range: `DataRiferimento <= data && (DataFine == null || DataFine >= data)`. Questo include automaticamente la dashboard, le statistiche e le liste attive.

`Consumazione.Giorno DateOnly` traccia il giorno in cui ogni prodotto è stato ordinato, abilitando la visualizzazione per-giorno nella schermata della lista.

### Tre layout di griglia

La griglia ombrelloni supporta tre modalità selezionate automaticamente senza configurazione esplicita:
1. **Classico** — nessuna configurazione, layout sequenziale
2. **Sequenziale con zone** — bordi configurati ma nessun `CellaIndice`
3. **Mappa custom** — `CellaIndice` su ogni ombrellone; celle vuote mostrano placeholder grigi della stessa dimensione

### Drag con Pointer Events (mouse + touch)

L'editor della mappa usa `editor-drag.js` con Pointer Events API — funziona identicamente con mouse, dito e penna. Il `touch-action: none` viene applicato solo durante il drag attivo (dal JS), non staticamente nel CSS, così lo scroll orizzontale dell'editor è disponibile quando non si sta trascinando.

### CSS a file separati

Ogni pagina ha il suo CSS. Le classi globali riusabili stanno in `app.css`. L'ordine di caricamento in `App.razor` è: shell → globali → pagine in ordine alfabetico.

### Seed completo nel DbContext

Il seed in `BeachBarDbContext.OnModelCreating` contiene l'intera configurazione iniziale: 78 ombrelloni con le posizioni esatte della griglia 17×5, bordi di zona, e tutti i 98 prodotti del menu. Una singola migration `InitialCreate` porta il databaseallo stato operativo completo.

---

## Cosa manca / da aggiungere

- **Aggiornamento automatico della dashboard** — con più tablet aperti contemporaneamente, le modifiche da uno non appaiono sull'altro senza ricaricare. Blazor ha già SignalR attivo: basterebbe un broadcast dal server alla chiusura/apertura di una lista
- **Export CSV/PDF** dello storico per la contabilità
- **Report statistiche** — endpoint `GET /api/statistiche` con totale incassato per data, per ombrellone, per categoria prodotto
- **Logging strutturato con Serilog** — `ILogger` è già iniettato in ogni controller; manca solo il sink su file
- **Unit test** — i servizi sono testabili con `UseInMemoryDatabase`; manca la suite xUnit
- **Rate limiting** sull'endpoint di login (ASP.NET Core 8+ ha `RateLimiter` built-in)
- **Refresh token** — il JWT dura 8 ore; un refresh token eviterebbe il re-login
- **Multi-utente con ruoli** — un solo account admin; una tabella Utenti con ruoli (cameriere/gestore) permetterebbe di tracciare chi ha aperto o chiuso una lista
- **PWA** — aggiungendo un manifest e un service worker minimal, l'app si installa come icona sulla schermata home del tablet senza barra del browser
