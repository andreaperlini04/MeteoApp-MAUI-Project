# MeteoApp

Autori: **Christian Selva**, **Andrea Perlini**

---

## 1. Introduzione

MeteoApp è un'applicazione mobile Android sviluppata in .NET MAUI che consente all'utente di consultare le condizioni meteo di una o più località scelte. L'applicazione mostra in cima alla lista la posizione corrente rilevata via GPS e, sotto, l'elenco delle città salvate dall'utente; ogni città può essere aperta per visualizzarne il dettaglio meteo (temperatura, minima, massima, descrizione, icona) e l'andamento delle prossime ore in forma di grafico.

L'obiettivo del progetto è coprire i requisiti definiti dal corso — pattern *list-detail*, gestione delle città (aggiunta tramite ricerca testuale o selezione su mappa, eliminazione), persistenza locale, sincronizzazione cloud, notifiche push e una pagina Blazor — combinando questi blocchi in un'app coerente, localizzata e tematizzabile.

## 2. Tecnologie e metodologie

### Stack principale

- **.NET MAUI** (`net10.0-android`) per l'applicazione cross-platform; il target di consegna è Android.
- **Architettura MVVM** con `BaseViewModel` che implementa `INotifyPropertyChanged` e `[CallerMemberName]`. I ViewModel sono separati dalle View e iniettati tramite Dependency Injection.
- **Dependency Injection** nativa di MAUI: i `Service` e i `Database` sono registrati come `Singleton`, le `Page` e i `ViewModel` come `Transient` in `MauiProgram.cs`.
- **Shell** con `TabBar` come root della navigazione; `Routing.RegisterRoute` per la rotta del dettaglio (`entrydetails`) e per la pagina del grafico (`forecast`); `AddLocationPage` aperta come modale con `PushModalAsync`.

### Persistenza

- **SQLite** locale tramite `sqlite-net-pcl` con accesso `SQLiteAsyncConnection`. Le località sono modellate dalla classe `MeteoLocation` con chiave primaria auto-incrementata.
- **Appwrite Cloud** (`fra.cloud.appwrite.io`) come database remoto. Il documento Appwrite usa come ID il nome della città normalizzato (`lowercase`, senza spazi), così da garantire de-duplicazione cross-device per nome.
- **Sincronizzazione bidirezionale** locale↔cloud: a ogni caricamento della lista vengono aggiunte localmente le città presenti su cloud ma assenti, e rimosse localmente quelle non più presenti su cloud. Le scritture verso Appwrite (`CreateDocument`, `DeleteDocument`) sono awaited per garantire consistenza tra le due fonti di verità.

### API e servizi

- **OpenWeatherMap** come fonte dati (`api.openweathermap.org/data/2.5/weather` per il meteo corrente, `/forecast` per le previsioni). Le chiamate avvengono in `WeatherService` (in `MeteoApp.Core`) tramite `HttpClient` e `GetFromJsonAsync`. Le unità sono sempre richieste in `metric`; la conversione in °F avviene nel `ViewModel`.
- **Firebase Cloud Messaging** via `Plugin.Firebase 4.2.1` per la ricezione delle notifiche push lato client.
- **Server Node.js** in `MauiServer/` (`firebase-admin` + `serviceAccountKey.json`) per l'invio delle notifiche on-demand a un device identificato dal suo FCM token.

### UI / UX

- **Localizzazione IT/EN** tramite `AppResources.resx` + `LanguageService`. La lingua si cambia al volo dalla toolbar; al cambio, l'`AppShell` viene ricreata per forzare il refresh dei binding `{x:Static}`.
- **Tema chiaro/scuro** via `AppThemeBinding` su tutte le risorse cromatiche; il tema è persistito nelle preferenze utente e applicato all'avvio.
- **Unità di temperatura** (°C/°F) selezionabile dall'utente e persistita.
- **Mappa Google Maps** in `AddLocationPage` (tramite `Microsoft.Maui.Controls.Maps`) per la selezione della località tramite tap; il nome viene poi risolto via reverse geocoding sull'API di OpenWeatherMap.
- **Pagina Blazor Hybrid** (`ForecastPage` + `ForecastChart.razor`) con `BlazorWebView` e Chart.js per visualizzare il grafico delle temperature future.

### Gestione delle preferenze e delle key

- Le preferenze utente (lingua, tema, unità) sono serializzate in JSON e salvate via `Preferences` API di MAUI (`SettingsService`).
- Le chiavi API e gli identificativi cloud sono confinati in `MeteoApp.Core/Config.cs`, escluso dal versionamento; la repo include un `Config.template.cs` per facilitarne la ricreazione. Lo stesso pattern è applicato a `google-services.json`, alla `strings.xml` di Android (per la chiave Google Maps) e al `serviceAccountKey.json` lato server.

### Metodologia di sviluppo

Il lavoro è stato organizzato per **feature branch** su Git, con merge su `dev` man mano che le funzionalità venivano completate.

## 3. Implementazione e sfide principali

L'implementazione è avvenuta in modo incrementale: prima la lista-dettaglio con dati statici e SQLite locale, poi l'integrazione delle API meteo, poi l'aggiunta della mappa, della sincronizzazione Appwrite, della localizzazione, delle notifiche e infine della pagina Blazor.

Le sfide più significative incontrate durante lo sviluppo:

- **Setup di Firebase Cloud Messaging end-to-end.** Il flusso completo — registrazione del device, ottenimento del token FCM, invio dal server Node tramite `firebase-admin`, gestione del notification channel su Android — ha richiesto particolare attenzione nella configurazione (`google-services.json`, `MainActivity.OnCreate` con `CreateNotificationChannel`, `OnNewIntent` per gestire i messaggi a app aperta).

- **Sincronizzazione bidirezionale SQLite ↔ Appwrite.** Le due fonti di verità devono restare allineate non solo in scrittura ma anche in cancellazione. Una prima versione del metodo di sync gestiva soltanto il caso *cloud → locale* in aggiunta, lasciando scoperto il caso in cui un altro dispositivo avesse cancellato una città: l'utente continuava a vederla in lista. È stato aggiunto un secondo passaggio che rimuove dal SQLite locale le città non più presenti su cloud. Successivamente è emerso un secondo bug: aggiungendo una città questa non appariva, e all'aggiunta successiva compariva la *precedente*. La causa era una *race condition* tra la `SaveLocationAsync`, che pubblicava su Appwrite in `Task.Run` fire-and-forget, e la `SyncWithAppwriteAsync` che partiva subito dopo, leggendo da cloud uno stato non ancora aggiornato e cancellando dal locale la città appena salvata. Il fix è stato rendere awaited la scrittura su Appwrite, eliminando la race.

- **Blazor Hybrid dentro MAUI.** Integrare un `BlazorWebView` con Chart.js ha richiesto di passare i dati al component Razor tramite un servizio singleton condiviso (`ForecastStateService`), gestire l'invocazione JavaScript via `IJSRuntime` per il rendering del grafico e curare correttamente il ciclo di vita del component (`OnAfterRenderAsync`, `Dispose`).

- **Localizzazione runtime.** Cambiare lingua mentre l'app è in esecuzione richiede il refresh effettivo dei binding `{x:Static}`, che sono compile-time. La soluzione adottata, in linea con il pattern del corso, è ricreare l'`AppShell` su evento `LanguageChanged` esposto dal `LanguageService`, lasciando che il framework rifaccia il bind con la nuova `CultureInfo`.

- **Gestione dei permessi GPS.** È stato implementato un doppio livello di richiesta: una nativa Android in `MainActivity.OnCreate` (per `ACCESS_FINE_LOCATION` e `POST_NOTIFICATIONS`) e una tramite l'API MAUI `Permissions` in `OnAppearing` della lista, che gestisce sia il check sia la richiesta runtime ed evita di accendere il GPS prima che l'utente abbia concesso il permesso.

## 4. Valutazione finale e possibili miglioramenti

Il risultato finale soddisfa tutti i requisiti *must-have* indicati nelle specifiche del corso: gestione delle località (aggiunta da ricerca o da mappa, eliminazione con swipe), dettaglio meteo, GPS, persistenza SQLite, sincronizzazione Appwrite, pagina Blazor e notifiche push. L'app è inoltre localizzata IT/EN, supporta tema chiaro/scuro e doppia unità di temperatura. La sincronizzazione bidirezionale tra dispositivi diversi funziona correttamente sia per gli inserimenti sia per le rimozioni. Il codice è organizzato secondo il pattern MVVM con una netta separazione tra la libreria `MeteoApp.Core` (logica di dominio, riusabile) e il progetto MAUI (UI e servizi specifici della piattaforma).

Possibili miglioramenti per evoluzioni future:

- **Notifiche server-side complete.** Estendere il server Node.js in modo che memorizzi le preferenze di notifica per utente, controlli periodicamente le temperature delle località salvate e invii la notifica al superamento di una soglia configurata, anziché limitarsi all'invio on-demand.

- **Robustezza offline.** Introdurre una coda di operazioni pendenti (`pendingUpload`) per gestire correttamente le città create senza connessione: oggi una scrittura fallita verso Appwrite viene ignorata silenziosamente, e al primo sync online la città potrebbe essere cancellata localmente perché non risulta su cloud.

- **Cache delle previsioni.** Salvare in locale la risposta di `/forecast` per ogni città con un TTL, riducendo il numero di chiamate all'API e mantenendo il grafico funzionante anche senza rete.
