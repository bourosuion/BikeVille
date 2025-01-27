# BikeVille

## Introduzione
**BikeVille** è una web app di e-commerce basata sul database AdventureWorksLT2019, progettata per gestire in modo completo la vendita online di prodotti. L'applicazione garantisce compatibilità con i dati storici degli utenti e degli ordini già presenti nel database, e adotta pratiche avanzate di sicurezza per la gestione delle credenziali e delle informazioni sensibili.
Lo sviluppo del progetto prevede l’utilizzo di:
-**Frontend**: Angular
-**Backend**: C# con ASP.NET Core
-**Database**:
-SQL Server (SSMS) per la gestione di prodotti, ordini e storico clienti.
-MongoDB per la gestione dei dati sensibili (password, dati personali).

## Obiettivi del Progetto
-**Gestione Completa dell'E-commerce**:
Pieno controllo su utenti, ordini e catalogo prodotti tramite un’interfaccia amministrativa.
-**Compatibilità Storica**:
Mantenere la continuità dei dati con il database esistente, incluso lo storico degli ordini per i clienti già registrati.
-**Sicurezza dei Dati**:
Implementazione di tecniche di protezione avanzate per le credenziali e le informazioni personali.

## Requisiti Funzionali

1. ### Gestione Utenti
   -**Utenti Registrati**:
   Possono:
   Navigare nel catalogo prodotti.
   Aggiungere prodotti al carrello ed effettuare ordini.
   Visualizzare e gestire il proprio storico ordini.
   Accesso tramite credenziali cifrate con algoritmi sicuri (es. SHA256 con Salt).
   -**Utenti Non Registrati**:
   Possono:
   Esplorare il catalogo prodotti.
   Verificare disponibilità e prezzi.
   Devono registrarsi per procedere agli acquisti.

2. ### Gestione Ordini
   -**Funzionalità del Carrello**:
   Aggiunta, modifica e rimozione di prodotti.
   Salvataggio dello stato del carrello per completare l’ordine su dispositivi diversi.
   -**Conferma Ordine**:
   Visualizzazione di un popup riepilogativo.
   Possibilità di stampa del riepilogo.
   Invio della conferma d’ordine via email (opzionale).

## Requisiti di Sicurezza

1. ### Autenticazione
   Login sicuro tramite:
   **Token JWT** (JSON Web Token) per gestire l’autenticazione e le sessioni.
   Meccanismi di logout e gestione delle sessioni attive su dispositivi multipli.
2. ### Crittografia
   Le password saranno cifrate utilizzando algoritmi sicuri:
   SHA256 o SHA512, con l'aggiunta di Salt per aumentare la sicurezza.
3. ### Gestione dei Dati Sensibili
   I dati sensibili degli utenti, come password e informazioni personali, saranno memorizzati su **MongoDB**.
   Tutti gli altri dati (prodotti, ordini, storico clienti) saranno memorizzati su SQL Server (SSMS).

## Compatibilità Storica

Integrazione con il database **AdventureWorksLT2019** per:
Mantenere la continuità dello storico ordini.
Permettere agli utenti già registrati di accedere ai dati storici.
Nuova gestione delle password per gli utenti esistenti, in quanto non è possibile recuperare le credenziali precedenti.

## Architettura del Sistema

1. ### Frontend (Angular)
   Componenti principali:
   Homepage: Visualizzazione del catalogo prodotti.
   Pagina prodotto: Dettagli dei singoli prodotti e disponibilità.
   Carrello: Gestione degli ordini prima della conferma.
   Gestione del Routing:
   Navigazione tra le diverse pagine (catalogo, carrello, storico ordini).

2. ### Backend (C# con ASP.NET Core)
   Struttura MVC:
   Modelli per la rappresentazione dei dati.
   Controller per la gestione delle API REST.
   Servizi per la logica di business e la connessione ai database.

3. ### Database
   SQL Server (SSMS):
   Tabelle per prodotti, ordini, e storico clienti.
   MongoDB:
   Collezioni per utenti, credenziali e dati sensibili.

## Test e Validazione

1. ### Test Funzionali
   Verifica delle operazioni CRUD su:
   Prodotti.
   Utenti.
   Ordini.
2. ### Test di Sicurezza
   Verifica della crittografia delle password.
   Test di penetrazione per garantire la protezione dei dati sensibili.
3. ### Test di Performance
   Valutazione della reattività del sistema con un alto numero di accessi simultanei.
