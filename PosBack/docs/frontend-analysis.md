# Analyse du frontend GroceryPOS

## Vue d'ensemble

Le dépôt `PosFront` est une application Angular autonome destinée au point de vente. Il utilise Angular 22.2, Angular CLI 22.2.4, TypeScript 6.0, NgRx Signals 22 et Vitest pour les tests unitaires. La navigation est découpée en routes paresseuses et couvre le tableau de bord, le checkout, les produits, l'inventaire, les ventes, les clients et les rapports.

Le frontend confirme actuellement ses données en mémoire. Il ne doit pas être modifié pendant cette étape.

## Structure

- `src/main.ts` démarre l'application.
- `src/app/app.config.ts` déclare les fournisseurs Angular, HTTP et les adaptateurs simulés.
- `src/app/app.routes.ts` déclare les routes fonctionnelles.
- `src/app/core/` contient la configuration API, les gardes, les interceptors, les modèles, les services et les stores transverses.
- `src/app/features/` contient les fonctionnalités métier et leurs stores.
- `src/app/shared/` contient les éléments réutilisables de présentation.

Le base URL configurée est `https://localhost:7001/api`, mais aucun service métier ne l'appelle encore.

## Modèles existants

### Produit

`Product` contient : identifiant, SKU, code-barres, nom, description, catégorie, unité, prix, taux de taxe, quantité en stock, seuil de réapprovisionnement et état actif.

### Ligne de panier et vente

`CartLine` contient l'identifiant du produit, son libellé, son SKU, son unité, son prix unitaire en centimes, son taux de taxe et sa quantité. `Sale` contient les lignes, les montants en centimes, la méthode de paiement et le statut.

Le frontend évite les décimales monétaires dans les calculs métier : les prix et soldes sont exprimés en centimes. Le taux de taxe reste représenté comme une fraction décimale.

### Client et utilisateur

`Customer` contient les coordonnées et les indicateurs de visite/dépense. `MockUser` contient l'identifiant, le nom affiché et le rôle. L'identité frontend est actuellement simulée et ne fournit pas de jeton JWT.

## NgRx SignalStores

Les stores principaux sont :

- `ProductsStore` : catalogue, produit sélectionné, produits actifs, faible stock et rupture.
- `InventoryStore` : ajustements de stock et indicateurs calculés depuis `ProductsStore`.
- `CartStore` : lignes, quantités, sous-total, taxe et total.
- `CheckoutStore` : état de paiement, montant rendu, reçu et soumission.
- `SalesStore` : historique des ventes et ajout de vente.
- `CustomersStore` : liste et ajout de clients.
- `ReportsStore` : revenus, nombre de transactions, moyenne et ventes en espèces.
- `AuthStore` : utilisateur, authentification et jeton.
- `DashboardStore` et `AppSettingsStore` : synthèse et préférences.

Les stores dépendent de services via injection Angular, ce qui permettra plus tard de remplacer les adaptateurs simulés par des clients HTTP vers la gateway.

## Services simulés et opérations attendues

- `MockProductDataService` prépare les sauvegardes et activations de produits.
- `MockInventoryDataService` applique localement les ajustements de stock.
- `MockCheckoutService` valide et crée une vente au comptant.
- `MockPaymentGateway` annonce qu'aucun terminal de carte n'est configuré.
- `OfflineSyncService` contient un mécanisme préparatoire hors ligne.
- `ApiService` fournit déjà les primitives HTTP GET, POST, PUT et DELETE.

Le backend devra donc offrir, au minimum dans les étapes suivantes, des opérations pour le catalogue, l'inventaire, le panier/checkout côté serveur, les ventes, les paiements, les clients, les rapports et l'identité.
