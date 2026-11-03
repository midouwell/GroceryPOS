# Guide de test - étape 2

## Démarrer les services

Dans quatre terminaux :

```powershell
cd C:\Dev\GroceryPOS\PosBack
dotnet run --project src\Services\Identity\Identity.Api\Identity.Api.csproj --launch-profile http
dotnet run --project src\Services\Products\Products.Api\Products.Api.csproj --launch-profile http
dotnet run --project src\Services\Inventory\Inventory.Api\Inventory.Api.csproj --launch-profile http
dotnet run --project src\Services\Customers\Customers.Api\Customers.Api.csproj --launch-profile http
```

Démarrer ensuite la gateway dans un cinquième terminal :

```powershell
dotnet run --project src\Gateway\Gateway.Api\Gateway.Api.csproj --launch-profile http
```

## Comptes de démonstration

Mot de passe de développement uniquement :

| Utilisateur | Mot de passe | Rôle |
|---|---|---|
| admin | Admin123! | Administrator |
| manager | Manager123! | Manager |
| cashier | Cashier123! | Cashier |

Les secrets JWT des fichiers `appsettings.json` sont exclusivement de développement. En production, fournir `Jwt__SigningKey`, `Jwt__Issuer` et `Jwt__Audience` par variables d'environnement ou coffre de secrets.

## Obtenir un jeton

```powershell
$login = @{ userName = "admin"; password = "Admin123!" } | ConvertTo-Json
$result = Invoke-RestMethod http://localhost:5100/api/v1/auth/login -Method Post -ContentType "application/json" -Body $login
$token = $result.token
$headers = @{ Authorization = "Bearer $token" }
```

## Tester les produits

```powershell
$product = @{
  sku = "PRD-TEST-1"; barcode = "628200000001"; name = "Milk"; description = "Test product"
  category = "Dairy"; unit = "2 L"; price = 449; taxRate = 0; isActive = $true
} | ConvertTo-Json

Invoke-RestMethod http://localhost:5100/api/v1/products -Method Post -Headers $headers -ContentType "application/json" -Body $product
Invoke-RestMethod http://localhost:5100/api/v1/products?search=milk -Headers $headers
```

## Tester l'inventaire

```powershell
$adjustment = @{ quantityDelta = 12; type = "InitialStock"; reason = "Opening stock"; reorderLevel = 5 } | ConvertTo-Json
Invoke-RestMethod http://localhost:5100/api/v1/inventory/p1/adjustments -Method Post -Headers $headers -ContentType "application/json" -Body $adjustment
Invoke-RestMethod http://localhost:5100/api/v1/inventory/p1 -Headers $headers
Invoke-RestMethod http://localhost:5100/api/v1/inventory/low-stock -Headers $headers
```

## Tester les clients

```powershell
$customer = @{ name = "Amelia Chen"; email = "amelia@example.test"; phone = "555-0100" } | ConvertTo-Json
$created = Invoke-RestMethod http://localhost:5100/api/v1/customers -Method Post -Headers $headers -ContentType "application/json" -Body $customer
Invoke-RestMethod "http://localhost:5100/api/v1/customers/$($created.id)" -Headers $headers
```

## Tests automatisés

```powershell
dotnet test tests/CoreBusiness.Tests/CoreBusiness.Tests.csproj
dotnet test tests/SharedKernel.Tests/SharedKernel.Tests.csproj
```

Les tests utilisent des dossiers temporaires sous le dossier temporaire de l'utilisateur et ne modifient pas les données JSON de démonstration.
