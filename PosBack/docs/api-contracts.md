# Contrats API prévus

Les URL ci-dessous sont des contrats cibles exposés par la gateway. Les routes finales restent à sécuriser et à valider dans les étapes métier.

## Convention commune

- Préfixe gateway : `/api`
- Authentification : JWT Bearer après activation de l'identité
- Format : JSON
- Nombres monétaires : centimes entiers
- Correlation : en-tête `X-Correlation-ID`
- Erreurs : réponses HTTP standard ASP.NET Core avec détails structurés

## Identity

| Méthode | Route | Rôle |
|---|---|---|
| POST | `/api/auth/login` | authentifier un utilisateur démonstration |
| POST | `/api/auth/refresh` | renouveler un jeton |
| GET | `/api/users/me` | lire l'utilisateur courant |

## Products

| Méthode | Route | Rôle |
|---|---|---|
| GET | `/api/products` | lister les produits |
| GET | `/api/products/{id}` | lire un produit |
| POST | `/api/products` | créer un produit |
| PUT | `/api/products/{id}` | modifier un produit |
| PATCH | `/api/products/{id}/active` | activer/désactiver un produit |
| GET | `/api/products/barcode/{barcode}` | rechercher par code-barres |

## Inventory

| Méthode | Route | Rôle |
|---|---|---|
| GET | `/api/inventory/{productId}` | lire le niveau de stock |
| POST | `/api/inventory/{productId}/adjustments` | appliquer un ajustement |
| GET | `/api/inventory/low-stock` | lister les produits sous seuil |

## Sales

| Méthode | Route | Rôle |
|---|---|---|
| POST | `/api/sales/checkout` | soumettre une vente |
| GET | `/api/sales` | lister les ventes |
| GET | `/api/sales/{id}` | lire une vente |

Payload de vente attendu :

```json
{
  "lines": [
    {
      "productId": "p1",
      "quantity": 1
    }
  ],
  "paymentMethod": "cash",
  "cashTenderedCents": 1000
}
```

## Payments

| Méthode | Route | Rôle |
|---|---|---|
| POST | `/api/payments/cash` | enregistrer un paiement en espèces |
| POST | `/api/payments/cards/authorize` | préparer l'autorisation carte |
| GET | `/api/payments/{id}` | lire un paiement |

## Customers

| Méthode | Route | Rôle |
|---|---|---|
| GET | `/api/customers` | lister les clients |
| GET | `/api/customers/{id}` | lire un client |
| POST | `/api/customers` | créer un client |
| PUT | `/api/customers/{id}` | modifier un client |

## Reports

| Méthode | Route | Rôle |
|---|---|---|
| GET | `/api/reports/summary` | synthèse des revenus |
| GET | `/api/reports/sales-by-day` | agrégats journaliers |

## Compatibilité frontend

Les champs de produit, client, ligne de vente et vente restent alignés avec les interfaces Angular. Le prix doit être rendu en cents côté API pour éviter les erreurs d'arrondi. Les montants reçus depuis JavaScript doivent être des entiers.
