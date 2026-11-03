# Guide pratique du checkout - Step 3

## Démarrer les services

Dans des terminaux séparés :

```powershell
cd C:\Dev\GroceryPOS\PosBack
dotnet run --project src\Services\Identity\Identity.Api
dotnet run --project src\Services\Products\Products.Api
dotnet run --project src\Services\Inventory\Inventory.Api
dotnet run --project src\Services\Payments\Payments.Api
dotnet run --project src\Services\Sales\Sales.Api
dotnet run --project src\Gateway\Gateway.Api --urls http://localhost:5100
```

Swagger : `http://localhost:5104/swagger`, `http://localhost:5105/swagger` et Gateway `http://localhost:5100/swagger`.

## Obtenir un token

```powershell
$login = Invoke-RestMethod -Method Post `
  -Uri http://localhost:5100/api/v1/auth/login `
  -ContentType application/json `
  -Body '{"username":"cashier","password":"Cashier123!"}'
$headers = @{ Authorization = "Bearer $($login.token)" }
```

## Exemple de checkout

Le montant est exprimé en centimes. Remplacer `PRODUCT_ID` par l'identifiant réel :

```powershell
$checkout = @{
  items = @(@{ productId = "PRODUCT_ID"; quantity = 1 })
  paymentMethod = "cash"
  tenderedCents = 1000
}
$headers["Idempotency-Key"] = "checkout-demo-001"
Invoke-RestMethod -Method Post `
  -Uri http://localhost:5100/api/v1/sales/checkout `
  -Headers $headers -ContentType application/json `
  -Body ($checkout | ConvertTo-Json -Depth 5)
```

Relancer exactement la même requête avec le même header. La vente existante doit être renvoyée sans décrémenter le stock une seconde fois.

## Carte simulée

```json
{
  "items": [{ "productId": "PRODUCT_ID", "quantity": 1 }],
  "paymentMethod": "card",
  "tenderedCents": 0
}
```

Le paiement contient `MOCK_CARD_TERMINAL_NOT_AUTHORIZED`. Aucun numéro de carte, CVV ou PIN n'est transmis.

## Vérifications

- Historique : `GET /api/v1/sales`
- Détail : `GET /api/v1/sales/{id}`
- Paiement : `GET /api/v1/payments/{id}`
- Stock : `GET /api/v1/inventory/{productId}`
- Tests : `dotnet test PosBack.sln`

Les fichiers persistants attendus sont `data/sales/sales.json`, `data/payments/payments.json` et les fichiers de stock Inventory.
