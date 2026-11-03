# Guide des fichiers importants

## Gateway

### `Gateway.Api/Program.cs`

Ce fichier configure les services ASP.NET Core de la gateway : authentification JWT, CORS, YARP, Swagger et santé. Il appartient à `Gateway.Api` car ces responsabilités concernent le point d'entrée HTTP. `Main` construit `WebApplication`, puis le pipeline appelle `UseCors`, `UseAuthentication`, `UseAuthorization`, `MapHealthChecks` et `MapReverseProxy`.

### `Gateway.Api/appsettings.json`

Ce fichier configure les routes YARP, les adresses des services, le JWT et les origines Angular. La gateway lit ces valeurs au démarrage et n'a besoin que du fichier pour découvrir les destinations.

### `Gateway.Api/CorrelationMiddleware.cs`

Le middleware lit ou crée `X-Correlation-ID`, l'ajoute à la réponse et remplace `TraceIdentifier`. Il appartient à la gateway car elle crée l'identifiant de bout en bout. Exemple : une requête sans en-tête reçoit `X-Correlation-ID: a...`, puis les appels YARP peuvent être associés à cet identifiant.

## Persistance JSON

### `IJsonRepository.cs`

L'interface décrit les opérations CRUD de lecture et d'écriture sans imposer ASP.NET Core à un appelant. Elle permet à une application future de remplacer l'implémentation JSON par un autre dépôt.

### `JsonRepositoryOptions.cs`

Cette classe contient le nom du fichier, le dossier et le choix du seed. Elle est destinée à être liée depuis la configuration `appsettings.json` dans les services.

### `JsonRepository.cs`

La classe lit et écrit le fichier de façon asynchrone, utilise `SemaphoreSlim` pour la concurrence locale, crée les fichiers absents et remplace le fichier final via un temporaire. Exemple d'exécution : `AddAsync` acquiert le sémaphore, recharge le JSON, ajoute l'objet, écrit un fichier temporaire puis déplace ce fichier vers le fichier cible.

### `JsonRepositoryHealthCheck.cs`

Ce check vérifie que le dossier JSON configuré existe. La gateway et les services utilisent ce mécanisme HTTP/Santé pour détecter rapidement un problème de configuration.

## API des microservices

Chaque `Program.cs` appartient à son API indépendante. Il configure Swagger, les contrôles de santé et le point minimal `/api/service-info`. Le projet reste autonome, possède son propre lancement HTTP et peut référencer `SharedKernel` sans dépendre des autres services.

## Tests

`JsonRepositoryTests.cs` vérifie deux obligations : créer un fichier seed manquant et ne perdre aucun élément pendant des écritures concurrentes. Un test crée un dossier temporaire, appelle le dépôt réel puis supprime uniquement ce dossier à la fin.
