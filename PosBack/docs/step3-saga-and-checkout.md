# Checkout, saga et idempotence

## Séparation des responsabilités

Sales possède la vente et son reçu. Payments possède l'état financier simulé. Inventory reste responsable des quantités et mouvements. Products reste la source de vérité des prix et taxes.

## Séquence

1. Sales valide le panier et lit chaque produit via le service Products.
2. Sales lit le stock dans Inventory.
3. Sales calcule les totaux en centimes sur le serveur.
4. Sales crée une vente `Pending`.
5. Payments traite le paiement avec la même clé d'idempotence que l'opération.
6. Sales décrémente chaque produit dans Inventory avec une raison `checkout:<key>`.
7. Sales passe la vente à `Completed`.

```text
Frontend -> Gateway -> Sales -> Products
                     Sales -> Inventory
                     Sales -> Payments
                     Sales -> Inventory
```

Cette séquence est une saga orchestrée par Sales. Chaque service possède sa persistance et publie son résultat. Ce n'est pas une transaction ACID distribuée.

## Compensation

Si Payments échoue, aucune vente n'est marquée Completed et Inventory n'est pas appelé. Si Inventory échoue après un paiement réussi, la vente passe à Failed. Une nouvelle tentative peut rejouer le paiement idempotent, puis les mouvements de stock. La compensation complète réelle serait un remboursement du paiement; elle doit être ajoutée avec un provider avant production.

## Idempotence

Une clé `Idempotency-Key` identifie le checkout. Payments possède également sa propre clé dérivée de l'opération. Sales renvoie une vente complétée existante pour éviter une seconde vente. Inventory ignore un mouvement dont la raison `checkout:<key>` existe déjà pour le même produit. Chaque JSON est néanmoins protégé seulement localement : deux processus simultanés peuvent contourner le verrou mémoire.

## Limites JSON

Les écritures sont atomiques dans un fichier, mais plusieurs services et plusieurs fichiers ne peuvent pas être liés dans une transaction. Un crash après le paiement et avant le stock peut laisser une vente Processing ou Failed. L'opération doit donc être rejouée et les mouvements compensés manuellement ou via un mécanisme durable avant une mise en production.
