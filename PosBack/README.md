# PosBack

Socle backend ASP.NET Core 10 de GroceryPOS. La solution est une architecture de microservices pilotée par une gateway YARP.

## Prérequis

- .NET SDK 10.0
- Angular reste indépendant dans `../PosFront`

## Construire

```powershell
cd C:\Dev\GroceryPOS\PosBack
dotnet restore PosBack.sln
dotnet build PosBack.sln
```

## Lancer la gateway

```powershell
dotnet run --project src/Gateway/Gateway.Api/Gateway.Api.csproj --launch-profile http
```

- Gateway : `http://localhost:5100`
- Santé : `http://localhost:5100/health`
- Swagger : `http://localhost:5100/swagger`

## Lancer les services

Chaque service est indépendant :

```powershell
dotnet run --project src/Services/Identity/Identity.Api/Identity.Api.csproj --launch-profile http
dotnet run --project src/Services/Products/Products.Api/Products.Api.csproj --launch-profile http
dotnet run --project src/Services/Inventory/Inventory.Api/Inventory.Api.csproj --launch-profile http
dotnet run --project src/Services/Sales/Sales.Api/Sales.Api.csproj --launch-profile http
dotnet run --project src/Services/Payments/Payments.Api/Payments.Api.csproj --launch-profile http
dotnet run --project src/Services/Customers/Customers.Api/Customers.Api.csproj --launch-profile http
dotnet run --project src/Services/Reports/Reports.Api/Reports.Api.csproj --launch-profile http
```

Ports HTTP : 5101 à 5107. Chaque service expose `/health`, `/swagger` et `/api/service-info` dans cette étape.

## Tester la persistance

```powershell
dotnet test tests/SharedKernel.Tests/SharedKernel.Tests.csproj
```

Les tests vérifient l'initialisation d'un fichier absent et l'absence de perte lors de plusieurs écritures concurrentes.

## Docker Compose

Les Dockerfiles doivent être présents avant `docker compose up --build`. Ils ne sont pas nécessaires pour lancer les projets localement avec .NET.

## Step 4

Rapports, RabbitMQ, Docker Compose, observabilité et intégration Angular sont documentés dans :

- `docs/step4-docker-and-messaging.md`
- `docs/angular-integration.md`
- `docs/sql-server-migration.md`
- `docs/step3-saga-and-checkout.md`

Demarrer le backend complet :

```powershell
Copy-Item .env.example .env
docker compose up --build
```

Gateway : `http://localhost:5100`
RabbitMQ management : `http://localhost:15672`

Lancer les tests :

```powershell
dotnet test PosBack.sln
docker compose config
```

## Ce qu'il faut comprendre avant l'étape 4

1. La gateway est le seul point d'entrée Angular; les ports internes ne sont pas publiés au navigateur.
2. YARP utilise la section `ReverseProxy` de `appsettings.json`.
3. Chaque service possède son propre stockage JSON et ne partage pas de base.
4. DI crée les dépendances des classes; les interfaces permettent de changer d'implémentation sans modifier l'appelant.
5. `JsonRepository<T>` protège les lectures et écritures avec un sémaphore et écrit d'abord un fichier temporaire.
6. Les futurs entités métier doivent rester dans le domaine de leur service.
