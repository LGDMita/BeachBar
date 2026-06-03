# Belix

Gestionale web per il bar di uno stabilimento balneare. Lo staff può aprire, gestire e chiudere le **liste** (conti) dei clienti per ogni ombrellone o come lista volante, con supporto a soggiorni multi-giorno e prenotazioni future.

Il sistema si chiama **Belix** ed è accessibile via browser — progettato per essere usato su tablet dallo staff in movimento. Più dispositivi possono essere aperti contemporaneamente: le modifiche fatte da uno si riflettono in tempo reale sugli altri.

---

## Indice

- [Guida rapida per lo staff](#guida-rapida-per-lo-staff)
- [Prerequisiti tecnici](#prerequisiti-tecnici)
- [Configurazione](#configurazione)
- [Avvio dal primo clone](#avvio-dal-primo-clone)
- [Pagine dell'applicazione](#pagine-dellapplicazione)
- [Modello dei dati](#modello-dei-dati)
- [Struttura del progetto](#struttura-del-progetto)
- [Endpoints REST API](#endpoints-rest-api)
- [Scelte tecniche](#scelte-tecniche)
- [Tecnologie](#tecnologie)
- [Roadmap](#roadmap)

---

## Guida rapida per lo staff

> Questa sezione è per chi usa l'app tutti i giorni, non per chi la installa.

### Flusso tipico di una giornata

1. **Apri la dashboard** (`/`) — vedi la mappa della spiaggia con lo stato di ogni ombrellone
2. **Tocca un ombrellone libero** → vai alla pagina ombrellone → inserisci nome cliente e date → "Apri lista"
3. **Durante la giornata**, tocca l'ombrellone rosso (con lista aperta) → "Gestisci →" → aggiungi prodotti dal menu
4. **A fine soggiorno**: "Chiudi ombrellone e incassa" (libera il posto) oppure "Chiudi lista e incassa" (il cliente resterà domani)
5. **Storico** — usa la freccia ◀ per navigare ai giorni precedenti e consultare i conti chiusi

### Colori sulla mappa

| Colore | Significato |
|---|---|
| 🔴 Rosso | Ombrellone occupato con lista aperta |
| 🟡 Giallo | Ombrellone occupato senza lista (il cliente c'è ma non ha un conto aperto) |
| 🟢 Verde | Ombrellone libero |

### Soggiorni multi-giorno

Quando apri una lista puoi indicare "Dal" (fisso sulla data che stai guardando) e "Al" (la data di fine soggiorno). L'app mostra una striscia colorata con ogni giorno del periodo:
- **Verde** = giorno libero
- **Rosso** = giorno già prenotato da un altro cliente

Il pulsante "Apri lista" resta grigio e disabilitato finché non risolvi i conflitti. Questo controllo avviene sia lato UI che lato server: anche se due tablet aprono la stessa pagina contemporaneamente, solo uno riuscirà a prenotare.

### Conti volanti

Clienti senza ombrellone fisso (es. chi viene solo al bar) → sezione "Conti volanti" in fondo alla dashboard → "+ Nuovo".

### Multi-tablet

Belix è pensato per essere aperto su più tablet contemporaneamente. Quando uno staff membro apre o chiude un ombrellone, la dashboard su tutti gli altri dispositivi si aggiorna entro circa un secondo — nessun bisogno di ricaricare manualmente.

---

## Prerequisiti tecnici

### 1. .NET 10 SDK

Scarica e installa da [https://dotnet.microsoft.com/download/dotnet/10.0](https://dotnet.microsoft.com/download/dotnet/10.0).

Verifica:
```bash
dotnet --version
# deve stampare 10.x.x
```

### 2. PostgreSQL

Installa PostgreSQL (versione 14 o superiore). Il database viene creato automaticamente dalle migrazioni. Non è necessario creare tabelle a mano.

### 3. dotnet-ef (strumento per le migrazioni)

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

Per generare un hash BCrypt per una nuova password:
```csharp
using BC = BCrypt.Net.BCrypt;
Console.WriteLine(BC.HashPassword("nuova-password", 12));
```
Incolla l'output nel campo `PasswordHash`.

---

## Avvio dal primo clone

```bash
# 1. Ripristina i pacchetti NuGet
dotnet restore

# 2. Crea il database e applica le migrazioni (schema + seed completo)
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
Poi trova il tuo IP con `ipconfig` (Windows) o `ip a` (Linux/Mac) e apri `http://192.168.x.x:5286` sul tablet.

| Risorsa | URL |
|---|---|
| Login | `http://localhost:5286/login` |
| App (HTTP) | `http://localhost:5286` |
| App (HTTPS) | `https://localhost:7298` |

### Applicare nuove migrazioni

Dopo ogni aggiornamento che modifica il modello dati (es. aggiunta di indici):

```bash
dotnet ef migrations add NomeDescrittivo --project BeachBar.Infrastructure --startup-project BeachBar
dotnet ef database update --project BeachBar.Infrastructure --startup-project BeachBar
```

### Reset completo del database

Per ripartire da zero mantenendo il layout spiaggia configurato nel seed:

```bash
dotnet ef database drop --force --project BeachBar.Infrastructure --startup-project BeachBar
dotnet ef database update --project BeachBar.Infrastructure --startup-project BeachBar
```

---

## Dati di seed

Il database viene popolato automaticamente al primo avvio con:

- **78 ombrelloni** (1–78) con le posizioni esatte nella griglia
- **Griglia 17 × 5**: 17 colonne, 5 righe, separatori di zona dopo la colonna 9 e dopo la colonna 15
- **Layout spiaggia** (fronte mare in cima):
  - Riga 0: ombrelloni 67–77 con spazio vuoto nelle celle 5–10
  - Riga 1: ombrelloni 50–66
  - Riga 2: ombrelloni 33–49
  - Riga 3: ombrelloni 16–32
  - Riga 4: ombrelloni 1–15, celle vuote agli estremi
  - Ombrellone 78: non posizionato in griglia
- **98 prodotti** in 11 categorie: Panini, Insalate, Pizza, Piatti, Pasta, Frutta, Caffetteria, Bibite, Birre, Vini, Aperitivi

Tutti questi dati possono essere modificati dall'interno dell'app nella sezione Impostazioni.

---

## Autenticazione

### UI Blazor — cookie di sessione

La pagina `/login` è una Razor Page SSR. Tutte le pagine Blazor hanno `@attribute [Authorize]`: chi non è autenticato viene reindirizzato al login. Il cookie dura 8 ore. Il logout è disponibile nella tab **Gestione** delle Impostazioni o direttamente a `/logout`.

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

Tutte le chiamate successive richiedono l'header:
```http
Authorization: Bearer eyJ...
```

---

## Pagine dell'applicazione

### Dashboard (`/`)

Pagina principale, ad uso operativo quotidiano.

**Navigatore data** — selettore con frecce ◀ ▶ e tasto "Oggi" (visibile solo quando non si è sulla data corrente). La data è uno stato globale (`DateContext`) condiviso tra Dashboard, Storico e Lista Aperte.

**Statistiche** — tre card: ombrelloni attivi, totale in corso (somma delle liste aperte che comprendono la data selezionata nel loro range), incassato nella data.

**Legenda colori** — riga compatta che spiega il significato dei tre colori (rosso / giallo / verde).

**Griglia ombrelloni** — si espande automaticamente a tutta la larghezza disponibile. Tre modalità di rendering selezionate automaticamente:

| Condizione | Rendering |
|---|---|
| Nessun `CellaIndice` e nessun bordo | Griglia sequenziale classica |
| Nessun `CellaIndice` ma bordi configurati | Righe flex con separatori di zona |
| Almeno un ombrellone con `CellaIndice` | Layout custom: celle vuote come quadrati grigi |

- Ombrellone con **una** lista → tap va direttamente a `/conto/{id}`
- Ombrellone con **più** liste → tap va a `/ombrellone/{id}`
- Ombrellone **rosso con nome**: il nome del cliente è visibile direttamente sulla cella (senza la scritta "Con lista")
- Ombrellone **giallo con nome**: mostra il nome del cliente che è presente ma senza lista aperta

**Conti volanti** — sezione sotto la griglia per clienti senza ombrellone fisso. "+ Nuovo" apre un form inline.

**Aggiornamento in tempo reale** — quando uno staff membro apre/chiude una lista su un dispositivo, la dashboard su tutti gli altri dispositivi connessi si aggiorna automaticamente senza ricaricare.

---

### Liste aperte (`/scontrini`)

Vista operativa: tutte le liste aperte in questo momento, indipendentemente dalla data selezionata.

- **Ricerca live** per nome cliente o numero ombrellone
- **Filtro** — Tutti / Ombrelloni / Volanti
- **Ordinamento toggle** — per orario di apertura o per importo
- **Riepilogo in testa** — conteggio totale e importo complessivo in corso
- **Tap su riga** → apre direttamente la lista

---

### Ombrellone (`/ombrellone/{id}`)

Pagina per aprire una nuova lista o gestire quelle già aperte su un ombrellone.

**Liste aperte** — elenco delle liste attive per la data corrente (inclusi soggiorni multi-giorno), con nome, range date, prodotti e totale parziale. "Gestisci →" apre la lista.

**Apri nuova lista**:
- Campo nome cliente (opzionale)
- **Dal** — data fissa (quella selezionata nella dashboard)
- **Al** — selezionabile; se uguale a "Dal" è un soggiorno di un solo giorno
- **Striscia disponibilità** — ogni giorno del periodo appare come pillola verde (libero) o rossa (già prenotato). Il pulsante rimane disabilitato finché ci sono conflitti
- Il controllo conflitti avviene sia lato UI che lato server: anche due tablet che premono "Apri" contemporaneamente non possono creare doppie prenotazioni

---

### Lista / Conto (`/conto/{id}`)

Scheda operativa della singola lista, layout a due colonne.

**Colonna sinistra — prodotti:**
- Intestazione: numero ombrellone o "Lista Volante", data (o range date per soggiorni multi-giorno)
- Nome cliente modificabile inline (✏️ → modifica → ✔️ per salvare)
- Tab per categoria prodotto
- Griglia prodotti: **tap = aggiungi** al conto del giorno corrente. L'aggiunta è istantanea (aggiornamento ottimistico) — nessuna attesa visibile anche su tablet lenti

**Colonna destra — lista:**
- Per soggiorni **single day**: lista piatta con tutti i prodotti
- Per soggiorni **multi-giorno**: prodotti raggruppati per giorno con intestazione data e subtotale; il bottone − è presente solo per il giorno corrente (i giorni passati sono in sola lettura)
- Totale complessivo in fondo

**Azioni:**

| Pulsante | Effetto |
|---|---|
| Torna alla spiaggia | Ritorna alla dashboard |
| **Chiudi ombrellone e incassa** | Chiude la lista e libera il posto — l'ombrellone torna verde |
| **Chiudi lista e incassa** | Chiude la lista ma l'ombrellone rimane giallo ("senza lista"): il cliente è ancora lì e probabilmente riaprirà domani |
| Annulla lista | Elimina la lista senza salvarla nello storico; usare solo in caso di errore di apertura |

---

### Storico (`/storico`)

Archivio delle liste chiuse, filtrate per `DataRiferimento` (giorno di *apertura*, non di chiusura). Naviga i giorni con le frecce ◀ ▶ per consultare i conti di una data specifica.

---

### Impostazioni (`/impostazioni`)

#### Tab Spiaggia

**Dimensioni griglia** — imposta righe × colonne. Il ridimensionamento azzera le posizioni e rimuove i bordi fuori dai nuovi limiti.

**Mappa ombrelloni** — editor interattivo:
- **Click su cella vuota** → posiziona il prossimo ombrellone non assegnato
- **Click destro su cella piena** (o modalità ✕ Cancella su touch) → rimuove
- **Drag** → riempie più celle in sequenza con un gesto
- **Click sul separatore** → attiva/disattiva bordo di zona (linea visiva)
- **⚡ Popola griglia intera** → riempie tutto sequenzialmente, crea gli ombrelloni mancanti
- **🗑️ Azzera posizioni** → rimuove tutti con conferma

#### Tab Prodotti

1. **Categorie** — rinomina ed elimina categorie (con conferma: elimina anche i prodotti della categoria)
2. **Aggiungi prodotto** — form con nome, prezzo e categoria
3. **Lista prodotti** — ricerca live, filtro per categoria, modifica/elimina per ogni prodotto. I prodotti disabilitati non vengono nascosti dallo storico: il flag `Disponibile=false` li esclude solo dalla schermata di ordinazione

#### Tab Gestione

| Azione | Effetto |
|---|---|
| **Azzera contatori** | Resetta il punto zero dei totali in dashboard senza cancellare dati — utile dopo una cassa intermedia durante la giornata |
| **Reset giornaliero (Forzato)** | Chiude tutte le liste aperte; usare solo a fine giornata se qualche conto è rimasto aperto per errore |
| **Esci** | Logout |

---

## Modello dei dati

```
ImpostazioniSpiaggia (sempre 1 sola riga)
├── NumeroOmbrelloni
├── NumeroColonne / NumeroRighe
├── BordiVerticali       ← indici colonne con separatore (CSV: "9,15")
├── BordiOrizzontali     ← indici righe con separatore
└── UltimoResetStatistiche  ← soglia per il calcolo "incassato oggi"

Ombrellone
├── Numero
├── Occupato             ← flag per il giorno corrente (ricalcolato per date diverse)
├── CellaIndice          ← posizione nella griglia custom (null = non posizionato)
└── NomeClienteAttivo    ← [NotMapped] — caricato in memoria per gli ombrelloni "senza lista"

Sessione  ("Lista")
├── OmbrelloneId         ← nullable: null = lista volante
├── NomeCliente
├── Apertura / Chiusura  ← DateTime UTC
├── Chiusa               ← bool
├── DataRiferimento      ← DateOnly — giorno di apertura / filtro principale per data
├── DataFine             ← DateOnly? — ultimo giorno del soggiorno (null = singolo giorno)
└── Consumazioni[]

Consumazione
├── SessioneId / ProdottoId
├── Quantita
├── Timestamp            ← DateTime UTC
└── Giorno               ← DateOnly — giorno in cui il prodotto è stato ordinato

Prodotto
├── Nome / Prezzo / Categoria
└── Disponibile          ← false = nascosto nell'ordinazione, ma conservato nello storico
```

### Indici database

Oltre agli indici automatici sulle chiavi esterne, il modello definisce tre indici compositi per le query più frequenti (applicati con la migrazione `AddCompositeIndexes`):

| Indice | Colonne | Usato da |
|---|---|---|
| `IX_Sessioni_OmbrelloneId_Chiusa_Date` | OmbrelloneId, Chiusa, DataRiferimento, DataFine | Dashboard, GetOmbrelloniAsync |
| `IX_Sessioni_Chiusa_DataRiferimento` | Chiusa, DataRiferimento | Statistiche, Storico |
| `IX_Consumazioni_Sessione_Prodotto_Giorno` | SessioneId, ProdottoId, Giorno | AggiungiConsumazione, controllo conflitti |

### Migrations

La migration `InitialCreate` contiene schema + seed completo (ombrelloni, prodotti, griglia). Le migration successive aggiungono solo le modifiche incrementali.

---

## Struttura del progetto

```
BeachBar/
├── Components/
│   ├── Pages/
│   │   ├── Home.razor                  ← Dashboard (griglia + conti volanti + real-time)
│   │   ├── Scontrini.razor             ← Liste aperte con ricerca e filtri
│   │   ├── Conto.razor                 ← Gestione singola lista (prodotti + scontrino per giorno)
│   │   ├── Ombrellone.razor            ← Apertura lista con picker Dal/Al e controllo conflitti
│   │   ├── Storico.razor               ← Archivio liste chiuse per data
│   │   ├── Impostazioni.razor          ← Shell con tab + sezione Gestione (~100 righe)
│   │   ├── ImpostazioniSpiaggia.razor  ← Sub-componente: editor griglia e bordi
│   │   ├── ImpostazioniProdotti.razor  ← Sub-componente: CRUD prodotti e categorie
│   │   ├── NotFound.razor
│   │   └── Error.razor
│   ├── Layout/
│   │   ├── MainLayout.razor            ← Shell Belix con navbar
│   │   └── ReconnectModal.razor
│   └── App.razor                       ← HTML root, CSS/JS globali
├── Controllers/                        ← REST API (richiede JWT)
│   ├── AuthController.cs
│   ├── ProdottiController.cs
│   ├── SessioniController.cs
│   ├── OrdiniController.cs
│   └── Dto/
├── Helpers/
│   └── ItCulture.cs                    ← CultureInfo "it-IT" condiviso tra le view
├── Pages/
│   ├── Login.cshtml                    ← Razor Page SSR (necessaria per scrivere cookie)
│   └── Logout.cshtml
├── Services/
│   ├── DateContext.cs                  ← Stato data globale (Scoped: una istanza per tab)
│   ├── IDashboardEventService.cs       ← Interfaccia notifiche real-time tra circuiti
│   └── DashboardEventService.cs        ← Implementazione Singleton con C# event
├── wwwroot/
│   ├── belix.css                       ← Shell, navbar, footer
│   ├── app.css                         ← Componenti globali (alert, date navigator, empty state…)
│   ├── dashboard.css                   ← Pagina Home e griglia ombrelloni
│   ├── impostazioni.css                ← Editor griglia e tab impostazioni
│   ├── ombrellone.css                  ← Pagine Conto e Ombrellone (incluso pannelli conferma)
│   ├── scontrini.css                   ← Pagina Liste aperte
│   ├── storico.css                     ← Pagina Storico
│   └── editor-drag.js                  ← Drag pointer-events per l'editor mappa
└── appsettings.json

BeachBar.Core/
├── Entities/
│   ├── ImpostazioniSpiaggia.cs         ← include BordiVerticaliSet / BordiOrizzontaliSet [NotMapped]
│   ├── Ombrellone.cs
│   ├── Sessione.cs
│   ├── Consumazione.cs
│   └── Prodotto.cs

BeachBar.Infrastructure/
├── Data/
│   └── BeachBarDbContext.cs            ← DbContext + seed + definizioni indici compositi
├── Migrations/
│   ├── InitialCreate                   ← Schema completo + seed (ombrelloni, prodotti, griglia)
│   └── AddCompositeIndexes             ← Indici compositi per performance (da applicare)
└── Services/
    ├── ISessioniService.cs / SessioniService.cs
    ├── IImpostazioniService.cs / ImpostazioniService.cs
    ├── IProdottiService.cs / ProdottiService.cs
    └── IConsumazioniService.cs / ConsumazioniService.cs
```

---

## Endpoints REST API

Tutti gli endpoint (tranne `/api/auth/login`) richiedono `Authorization: Bearer <token>`.

### Autenticazione

| Metodo | Endpoint | Descrizione |
|---|---|---|
| POST | `/api/auth/login` | Restituisce un JWT valido per 8 ore |

### Prodotti

| Metodo | Endpoint | Descrizione |
|---|---|---|
| GET | `/api/prodotti` | Tutti i prodotti disponibili |
| GET | `/api/prodotti/{id}` | Singolo prodotto |
| GET | `/api/prodotti/categoria/{categoria}` | Prodotti per categoria |

### Sessioni (Liste)

| Metodo | Endpoint | Body / Note |
|---|---|---|
| GET | `/api/sessioni` | Tutte le liste |
| GET | `/api/sessioni/aperte` | Solo liste aperte |
| GET | `/api/sessioni/{id}` | Lista con consumazioni incluse |
| POST | `/api/sessioni` | `{ ombrelloneId, nomeCliente?, dataRiferimento, giorni }` |
| POST | `/api/sessioni/extra` | `{ nomeCliente?, dataRiferimento }` — lista volante |
| PUT | `/api/sessioni/{id}/chiudi` | Chiude la lista |

### Ordini (Consumazioni)

| Metodo | Endpoint | Body / Note |
|---|---|---|
| GET | `/api/sessioni/{sessioneId}/ordini` | Consumazioni della lista |
| POST | `/api/sessioni/{sessioneId}/ordini` | `{ prodottoId, quantita }` — aggiunge al giorno corrente |
| DELETE | `/api/sessioni/{sessioneId}/ordini/{ordineId}` | Rimuove una consumazione |

---

## Scelte tecniche

### Aggiornamento real-time tra tablet

`DashboardEventService` è un Singleton con un evento C# condiviso tra tutti i circuiti Blazor Server (uno per tab/browser). Quando uno staff membro apre o chiude una lista, la pagina chiama `Events.NotifyStateChanged()`. `Home.razor` è sottoscritta all'evento e richiama `InvokeAsync` per aggiornare la dashboard sul thread del proprio circuito. Non serve nessun polling né websocket aggiuntivo — il canale SignalR è già quello Blazor Server.

### Aggiornamenti ottimistici nel conto

In `Conto.razor`, quando si aggiunge o rimuove un prodotto il modello locale viene aggiornato immediatamente (UI istantanea) e la chiamata al DB avviene in background. Se il salvataggio fallisce, il modello viene ripristinato da un reload completo. Questo elimina il ritardo percepito su tablet con connessione lenta.

### Guard server-side contro doppie prenotazioni

`ApriSessioneAsync` chiama internamente `GetGiorniOccupatiAsync` prima di scrivere sul DB. Se due tablet aprono la stessa finestra di prenotazione contemporaneamente, solo il primo riuscirà — il secondo riceve un errore chiaro. Il controllo UI sulla striscia giorni è un aiuto visivo, non la difesa principale.

### DateContext — stato data globale

`DateContext` è Scoped (una istanza per circuito Blazor). Tutti i componenti nello stesso tab leggono la stessa data: cambiare data sulla dashboard aggiorna anche Storico e Liste. Due tab diversi hanno date indipendenti.

### Soggiorni multi-giorno

`Sessione.DataFine DateOnly?` estende la lista a più giorni. Tutte le query usano il range `DataRiferimento <= data && (DataFine == null || DataFine >= data)`.

`Consumazione.Giorno DateOnly` traccia il giorno esatto di ogni ordine, abilitando la visualizzazione raggruppata per giorno nella schermata della lista e la sola lettura dei giorni passati.

### Tre layout di griglia

Selezionati automaticamente senza configurazione esplicita:
1. **Classico** — nessuna configurazione, layout sequenziale con `grid-template-columns: repeat(N, 1fr)`
2. **Sequenziale con zone** — bordi configurati, nessun `CellaIndice`; righe flex con separatori
3. **Mappa custom** — `CellaIndice` su ogni ombrellone; la griglia si espande a piena larghezza, celle vuote come placeholder grigi

### Parsing CSV dei bordi nell'entità

`ImpostazioniSpiaggia` espone `BordiVerticaliSet` e `BordiOrizzontaliSet` come proprietà `[NotMapped]` che parsano il campo CSV interno. Dashboard e service usano queste proprietà — nessuna duplicazione del parser.

### Split di Impostazioni.razor

Il file monolitico (832 righe) è stato diviso in tre componenti:
- `ImpostazioniSpiaggia.razor` — editor griglia, drag JS, `IAsyncDisposable`
- `ImpostazioniProdotti.razor` — CRUD categorie e prodotti
- `Impostazioni.razor` — shell tab + sezione Gestione (~100 righe)

Ogni componente inietta direttamente i service (tutti Scoped per circuito).

### Drag con Pointer Events (mouse + touch)

`editor-drag.js` usa la Pointer Events API — funziona identicamente con mouse, dito e penna. Il `touch-action: none` viene applicato solo durante il drag attivo, non staticamente nel CSS, così lo scroll dell'editor è disponibile quando non si sta trascinando.

### CSS a file separati

Ogni pagina ha il suo CSS. Le classi globali riusabili stanno in `app.css`. L'ordine di caricamento in `App.razor` è: shell → globali → pagine in ordine alfabetico.

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

## Roadmap

- **Export CSV/PDF** dello storico per la contabilità
- **Report statistiche** — totale incassato per data, per ombrellone, per categoria prodotto
- **Logging strutturato con Serilog** — `ILogger` è già iniettato nei controller; manca solo il sink su file
- **Unit test** — i service sono testabili con `UseInMemoryDatabase`; manca la suite xUnit
- **Rate limiting** sull'endpoint di login (ASP.NET Core ha `RateLimiter` built-in)
- **Refresh token** — il JWT dura 8 ore; un refresh token eviterebbe il re-login
- **Multi-utente con ruoli** — un solo account admin; una tabella Utenti permetterebbe di tracciare chi ha aperto o chiuso ogni lista
- **PWA** — aggiungendo manifest + service worker minimal, l'app si installa come icona sulla schermata home del tablet senza barra del browser
