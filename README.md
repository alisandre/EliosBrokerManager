# EliosBrokerManager

Servizio Windows .NET per la sincronizzazione bidirezionale tra il sistema Jibria e il sistema PACS Elios.

## 📋 Descrizione

EliosBrokerManager è un servizio Windows sviluppato in .NET 9 che funge da broker tra due sistemi:
- **Jibria**: Sistema gestionale esterno
- **Elios PACS**: Sistema di archiviazione e comunicazione di immagini medicali

Il servizio esegue un polling continuo per sincronizzare i dati degli esami radiologici, gestendo l'intero ciclo di vita delle richieste di esame.

## 🔄 Funzionalità Principali

Il servizio esegue tre operazioni principali ogni 30 secondi:

### 1. Invio Richieste da Jibria a Elios
- Legge dalla coda di Jibria gli esami in stato `IN_ATTESA`
- Crea o aggiorna nel database Elios:
  - Anagrafica paziente
  - Tipologia esame
  - Accettazione
  - Dettaglio accettazione
- Marca gli elementi come `INVIATO` dopo l'invio riuscito

### 2. Ricezione Feedback dal PACS
- Recupera gli aggiornamenti di stato dal sistema PACS Elios
- Aggiorna lo stato PACS nella coda Jibria
- Gestisce gli stati finali:
  - Stato `110`: esame completato → marcato come `ESEGUITO`
  - Altri stati: esame in errore → marcato come `IN_ERRORE`

### 3. Notifica Completamento a Elios
- Invia il feedback finale per gli esami completati (stato PACS = `110`)
- Aggiorna lo stato a `COMPLETATO` nella coda Jibria
- Imposta lo stato esterno a `120` nel database Elios

## 🏗️ Architettura

### Componenti Principali

- **QueueWorker**: Background service che esegue il polling continuo
- **JibriaQueueProvider**: Gestisce le operazioni sulla coda Jibria
- **EliosBrokerProvider**: Gestisce le operazioni sul database Elios PACS

### Ciclo di Vita degli Stati

```
IN_ATTESA → INVIATO → ESEGUITO/IN_ERRORE → COMPLETATO
```

### Database

Il servizio interagisce con due database distinti:
- **JibriaDB**: Contiene la tabella `EliosQueue` con gli elementi da sincronizzare
- **EliosDB**: Contiene le tabelle del sistema PACS (Paziente, Accettazione, AccettazioneDett, TabEsame)

## 🔄 Stati dei Sistemi

### Stati Jibria (`StatoInvio` nella tabella `EliosQueue`)

Il sistema Jibria gestisce gli stati di sincronizzazione attraverso il campo `StatoInvio`:

| Stato | Valore | Descrizione | Transizione |
|-------|--------|-------------|-------------|
| **IN_ATTESA** | `IN_ATTESA` | Richiesta di esame creata e pronta per l'invio a Elios PACS | Stato iniziale |
| **INVIATO** | `INVIATO` | Richiesta inviata con successo al sistema Elios. Dati paziente, esame e accettazione creati nel database PACS | Da `IN_ATTESA` dopo inserimento riuscito in Elios |
| **ESEGUITO** | `ESEGUITO` | Esame completato con successo nel sistema PACS. Il PACS ha confermato l'esecuzione (stato `110`) | Da `INVIATO` quando `StatoPacs = 110` |
| **IN_ERRORE** | `IN_ERRORE` | Errore durante l'elaborazione dell'esame nel sistema PACS | Da `INVIATO` quando `StatoPacs ≠ 110` |
| **COMPLETATO** | `COMPLETATO` | Ciclo completato. Il feedback finale è stato inviato al sistema Elios | Da `ESEGUITO` dopo notifica di completamento a Elios |

#### Campi Aggiuntivi nella Coda Jibria

- **`StatoPacs`**: Stato ricevuto dal sistema PACS Elios (es. `110` = completato)
- **`ErrorePacs`**: Messaggio di errore dal PACS (se presente)
- **`DataInvio`**: Timestamp dell'invio al sistema Elios
- **`DataUltAggPacs`**: Timestamp dell'ultimo aggiornamento dal PACS
- **`DataInserimento`**: Timestamp di creazione della richiesta

### Stati Elios PACS

Il sistema Elios PACS utilizza stati numerici separati per l'accettazione e il dettaglio accettazione:

#### Stati Esterni (`EsternoStato`)

Stati gestiti dal sistema esterno (Jibria) e sincronizzati con Elios:

| Livello | Stato | Descrizione | Quando viene impostato |
|---------|-------|-------------|------------------------|
| **Accettazione** | `10` | Accettazione ricevuta dal sistema esterno | Creazione/aggiornamento iniziale dell'accettazione |
| **Dettaglio Accettazione** | `70` | Dettaglio esame ricevuto | Creazione/aggiornamento del dettaglio esame |
| **Completamento** | `120` | Esame completato e confermato | Dopo ricezione feedback `StatoPacs = 110` da Jibria |

#### Stati Interni PACS (`EliosStato`)

Stati gestiti internamente dal sistema PACS Elios:

| Stato | Descrizione | Significato |
|-------|-------------|-------------|
| `110` | **Esame Completato** | L'esame è stato eseguito con successo e le immagini sono disponibili |
| Altri valori | **Esami in elaborazione o in errore** | L'esame è ancora in corso, in attesa, o ha riscontrato problemi |

#### Tabelle Elios PACS

**Tabella `Accettazione`:**
- `EsternoStato`: Stato della sincronizzazione con il sistema esterno
- `EsternoStatoData`: Data/ora dell'ultimo aggiornamento di stato esterno
- `EliosStato`: Stato interno del PACS (gestito dal sistema PACS)
- `EliosStatoData`: Data/ora dell'ultimo aggiornamento di stato PACS

**Tabella `AccettazioneDett`:**
- `EsternoStato`: Stato del dettaglio esame con il sistema esterno
- `EsternoStatoData`: Data/ora dell'ultimo aggiornamento
- `EliosStato`: Stato interno del dettaglio esame (es. `110` per completato)
- `EliosStatoData`: Data/ora dell'ultimo aggiornamento PACS
- `EliosNote`: Eventuali note o messaggi di errore dal PACS

## 🔀 Flusso Completo di Sincronizzazione

### Fase 1: Invio Richiesta (Jibria → Elios)

1. **Creazione Richiesta in Jibria:**
   - L'operatore crea una nuova richiesta di esame nel sistema Jibria
   - Il sistema imposta automaticamente lo stato `IN_ATTESA`
   - Vengono popolati i dati del paziente (nome, cognome, codice fiscale, data di nascita)
   - Vengono specificati i dettagli dell'esame (codice esame, descrizione, data accettazione)
   - Viene registrata la `DataInserimento`

2. **Polling del Servizio EliosBrokerManager:**
   - Ogni 30 secondi, il servizio interroga la tabella `EliosQueue` di Jibria
   - Seleziona tutte le richieste con `StatoInvio = 'IN_ATTESA'`
   - Per ogni richiesta trovata, procede con l'invio a Elios

3. **Inserimento Dati in Elios PACS:**
   - **Paziente**: Verifica se il paziente esiste (tramite codice fiscale), altrimenti lo crea
   - **Esame**: Verifica se l'esame esiste (tramite `IdEsameEsterno`), altrimenti lo crea
   - **Accettazione**: Crea o aggiorna l'accettazione con `EsternoStato = 10`
   - **Dettaglio Accettazione**: Crea o aggiorna il dettaglio con `EsternoStato = 70`
   - Tutte le operazioni avvengono in una transazione unica

4. **Aggiornamento Stato in Jibria:**
   - Se l'inserimento è riuscito: `StatoInvio = 'INVIATO'` e aggiorna `DataInvio`
   - Se l'inserimento fallisce: la richiesta rimane in `IN_ATTESA` per un nuovo tentativo

5. **Pausa:** Attende 1 secondo prima di processare la richiesta successiva

### Fase 2: Elaborazione PACS ed Aggiornamento Stato

1. **Elaborazione in PACS:**
   - Il sistema PACS Elios elabora internamente la richiesta di esame
   - Gli operatori del PACS eseguono l'esame e caricano le immagini
   - Il PACS aggiorna automaticamente `EliosStato` nel dettaglio accettazione
   - Quando l'esame è completato: `EliosStato = 110` e aggiorna `EliosStatoData`

2. **Polling Feedback dal PACS:**
   - Ogni 30 secondi, il servizio interroga le tabelle Elios
   - Seleziona le accettazioni con `EliosStatoData >= [data di 1 giorno fa]`
   - Per ogni accettazione recupera:
     - Dati del paziente
     - Dettagli dell'accettazione con stato PACS aggiornato
     - Eventuali note o messaggi di errore

3. **Aggiornamento Stato PACS in Jibria:**
   - Per ogni elemento trovato, aggiorna nella coda Jibria:
     - `StatoPacs = EliosStato` (es. `110`)
     - `DataUltAggPacs = EliosStatoData`
     - `ErrorePacs = EliosNote` (se presente)

4. **Gestione Stati Finali:**
   - **Se `StatoPacs = 110`**: 
     - `StatoInvio = 'ESEGUITO'`
     - L'esame è completato con successo
   - **Se `StatoPacs ≠ 110`**:
     - `StatoInvio = 'IN_ERRORE'`
     - Il campo `ErrorePacs` contiene la descrizione dell'errore

5. **Pausa:** Attende 1 secondo prima di processare il feedback successivo

### Fase 3: Notifica Completamento (Jibria → Elios)

1. **Polling Richieste Completate:**
   - Ogni 30 secondi, il servizio interroga la coda Jibria
   - Seleziona le richieste con `StatoPacs = '110'` (esami completati)

2. **Invio Conferma Finale a Elios:**
   - Per ogni richiesta completata:
     - Aggiorna `Accettazione.EsternoStato = 120` e `EsternoStatoData`
     - Aggiorna `AccettazioneDett.EsternoStato = 120` e `EsternoStatoData`
   - Operazione in transazione per garantire coerenza

3. **Aggiornamento Finale in Jibria:**
   - Se la conferma è riuscita: `StatoInvio = 'COMPLETATO'`
   - La richiesta ha completato l'intero ciclo di vita

4. **Pausa:** Attende 1 secondo prima di processare la conferma successiva

### Timeline Tipica di un Esame

```
T0:     Creazione richiesta in Jibria (IN_ATTESA)
T+30s:  Invio a Elios PACS (INVIATO, EsternoStato=10/70)
T+X:    Elaborazione esame nel PACS
T+Y:    Completamento esame (EliosStato=110)
T+Y+30s: Lettura feedback (ESEGUITO, StatoPacs=110)
T+Y+60s: Invio conferma finale (EsternoStato=120)
T+Y+90s: Ciclo completato (COMPLETATO)
```

## 📊 Diagramma di Stato Completo

```
stateDiagram-v2
    [*] --> IN_ATTESA: Creazione richiesta
    IN_ATTESA --> INVIATO: Invio a Elios (Esterno=10/70)
    INVIATO --> ESEGUITO: Feedback PACS (Elios=110)
    INVIATO --> IN_ERRORE: Feedback PACS (Elios≠110)
    ESEGUITO --> COMPLETATO: Conferma a Elios (Esterno=120)
    COMPLETATO --> [*]
    
    note right of IN_ATTESA
        Jibria: StatoInvio = IN_ATTESA
        Attesa invio al PACS
    end note
    
    note right of INVIATO
        Jibria: StatoInvio = INVIATO
        Elios: EsternoStato = 10/70
        In elaborazione nel PACS
    end note
    
    note right of ESEGUITO
        Jibria: StatoInvio = ESEGUITO
        Jibria: StatoPacs = 110
        Elios: EliosStato = 110
        Esame completato con successo
    end note
    
    note right of IN_ERRORE
        Jibria: StatoInvio = IN_ERRORE
        ErrorePacs popolato
        Richiesta gestione errore
    end note
    
    note right of COMPLETATO
        Jibria: StatoInvio = COMPLETATO
        Elios: EsternoStato = 120
        Ciclo terminato
    end note
```

## ⚙️ Configurazione

La configurazione del servizio è gestita tramite il file `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "JibriaDB": "Server=server_jibria;Database=JibriaDB;User Id=user;Password=password;",
    "EliosDB": "Server=server_elios;Database=EliosDB;User Id=user;Password=password;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information",
      "EliosBrokerService": "Information"
    },
    "EventLog": {
      "SourceName": "EliosBrokerService",
      "LogName": "Application"
    }
  }
}
```

### Parametri Configurabili

- **Connection Strings**: Stringhe di connessione ai database Jibria e Elios
- **Logging Levels**: Livelli di logging per diverse categorie
- **EventLog Settings**: Configurazione del logging su Event Viewer di Windows

## 🔒 Parametri Leocata (dati da Lisa Lionti - Elios il 19/01/2026)

- IP: 172.16.1.240
- Username: eliosbroker
- Password: brokerelios

## 🚀 Installazione come Servizio Windows

### Prerequisiti
- .NET 9.0 Runtime installato
- Privilegi di amministratore
- Accesso di rete ai database

### Procedura di Installazione

1. **Compilare il progetto in modalità Release:**
   ```powershell
   dotnet publish -c Release -o C:\Services\EliosBrokerManager
   ```

2. **Installare il servizio utilizzando PowerShell con privilegi di amministratore:**
   ```powershell
   sc.exe create "EliosBrokerService" binPath= "C:\Services\EliosBrokerManager\EliosBrokerService.exe"
   ```

3. **Configurare il servizio per l'avvio automatico:**
   ```powershell
   sc.exe config EliosBrokerService start= auto
   ```

4. **Configurare il servizio per il riavvio automatico in caso di errore:**
   ```powershell
   sc.exe failure EliosBrokerService reset= 86400 actions= restart/60000/restart/60000/restart/60000
   ```

5. **Avviare il servizio:**
   ```powershell
   sc.exe start EliosBrokerService
   ```

### Gestione del Servizio

**Verificare lo stato:**
```powershell
sc.exe query EliosBrokerService
```

**Arrestare il servizio:**
```powershell
sc.exe stop EliosBrokerService
```

**Rimuovere il servizio:**
```powershell
sc.exe delete EliosBrokerService
```

## 📊 Logging

Il servizio registra eventi nel registro eventi di Windows utilizzando `EventLog`. 

### Eventi Principali

| Tipo | Descrizione | Quando viene generato |
|------|-------------|----------------------|
| **Information** | Avvio servizio | All'avvio di `QueueWorker` |
| **Information** | Arresto servizio | Alla chiusura di `QueueWorker` |
| **Information** | Operazioni completate | Dopo ogni ciclo di sincronizzazione |
| **Error** | Errori di elaborazione | In caso di eccezioni durante il polling |
| **Error** | Errori di connessione database | Problemi di connessione ai database |

### Visualizzazione Log

Per visualizzare i log del servizio:
1. Aprire **Event Viewer** (Visualizzatore eventi)
2. Navigare in **Windows Logs → Application**
3. Filtrare per Source: **EliosBrokerService**

## 🛠️ Tecnologie Utilizzate

- **.NET 9.0**
- **C# 13.0**
- **Entity Framework Core** (per l'accesso ai database)
- **Microsoft.Extensions.Hosting** (per il servizio Windows)
- **Microsoft.Extensions.Logging** (per il logging)
- **Microsoft.Extensions.Configuration** (per la configurazione)

## 📝 Note Tecniche

- **Intervallo di polling**: 30 secondi (configurabile tramite `POLLING_SECONDS`)
- **Pausa tra operazioni**: 1 secondo tra batch di invio/aggiornamento
- **Finestra temporale feedback**: Ultimi 1 giorno (accettazioni con aggiornamenti recenti)
- **Gestione transazioni**: Tutte le operazioni sul database utilizzano transazioni per garantire l'integrità dei dati
- **Gestione errori**: Le eccezioni vengono catturate e registrate, il servizio continua l'esecuzione
- **Cancellation Token**: Supporto per l'arresto graceful del servizio

## 🔍 Troubleshooting

### Il servizio non si avvia

**Causa possibile**: Configurazione mancante o errata
- Verificare che il file `appsettings.json` sia presente nella directory del servizio
- Controllare che le stringhe di connessione siano corrette
- Verificare i permessi di accesso ai database

**Soluzione**:
```powershell
# Verificare i log di Windows Event Viewer
eventvwr.msc
```

### Errori di connessione al database

**Causa possibile**: Credenziali errate o firewall
- Verificare username e password nelle connection strings
- Controllare che i server database siano raggiungibili
- Verificare le regole del firewall

**Soluzione**:
```powershell
# Test connessione SQL Server
Test-NetConnection -ComputerName server_name -Port 1433
```

### Il servizio si arresta inaspettatamente

**Causa possibile**: Eccezioni non gestite o problemi di risorse
- Verificare i log nel Event Viewer
- Controllare l'uso di memoria e CPU
- Verificare la disponibilità dei database

## 🔒 Requisiti

- Windows Server 2016 o superiore
- .NET 9.0 Runtime
- Accesso ai database Jibria e Elios PACS
- Permessi per l'esecuzione come servizio Windows
- Connettività di rete ai server database

## 📄 Licenza

[Specificare la licenza del progetto]

## 👥 Autori

[Specificare gli autori del progetto]

## 🤝 Contribuire

[Specificare le linee guida per contribuire al progetto]

## 📞 Supporto

[Specificare i contatti per il supporto]
