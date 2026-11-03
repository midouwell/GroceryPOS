# Architecture cible

## Principes

GroceryPOS utilise une architecture orientée services. Le navigateur communique uniquement avec la gateway YARP. Chaque service possède son propre espace de données JSON et expose une API indépendante.

```text
Angular -> Gateway.Api -> Identity.Api
                       -> Products.Api
                       -> Inventory.Api
                       -> Sales.Api
                       -> Payments.Api
                       -> Customers.Api
                       -> Reports.Api
```

## Découpage des projets

- `Gateway.Api` est le point d'entrée HTTP et gère le reverse proxy, JWT, CORS, la santé, Swagger et la correlation.
- Les API métier sont indépendantes et démarrables séparément.
- `SharedKernel` fournit les fondations communes qui ne représentent pas un état partagé.
- `Messaging` prépare les échanges futurs entre services.
- `Observability` prépare la configuration commune de traçabilité et de journaux.

## Responsabilités

- API : contrôleurs, DTO d'entrée/sortie, mapping HTTP et codes de retour.
- Application : cas d'utilisation, validation et orchestration.
- Domaine : entités, règles métier et exceptions.
- Infrastructure : fichiers JSON, options et implémentations techniques.

Cette étape ne crée pas encore les entités métier. Chaque service obtient toutefois la même fondation de stockage JSON, afin que les étapes suivantes puissent implémenter leurs domaines sans dupliquer le mécanisme de fichier.

## Persistance JSON

Chaque microservice possède un fichier sous son dossier `data`, non partagé avec les autres services. `JsonRepository<T>` :

1. résout le chemin configuré ;
2. initialise le fichier manquant ;
3. charge les éléments de façon asynchrone ;
4. protège les opérations concurrentes par un `SemaphoreSlim` ;
5. écrit dans un fichier temporaire puis déplace atomiquement le fichier final.

Un seed optionnel permet uniquement de peupler une nouvelle installation en environnement Development.

## Communication inter-services

Les services ne partagent aucune base JSON. Les prochains échanges devront passer par des appels HTTP explicites ou des messages. Le monolithisme de données est volontairement évité pour préserver l'autonomie des services.

## Sécurité et observabilité

La gateway valide JWT. Les services peuvent être exposés localement pour le développement mais restent conceptuellement protégés derrière la gateway. Chaque requête reçoit un `X-Correlation-ID`, propagé dans les appels sortants et les journaux.
