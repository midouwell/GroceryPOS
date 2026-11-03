# Concepts à comprendre avant l'étape 3

- L'injection de dépendances relie contrôleur, service et repository.
- Le contrôleur ne doit contenir que le transport HTTP; les règles métier appartiennent au service.
- DTO et entité sont séparés pour éviter d'exposer les données internes.
- Les exceptions métier doivent être traduites en status HTTP cohérents.
- JWT contient des claims telles que le sujet et le rôle; les services appliquent `[Authorize(Roles = ...)]`.
- `/api/v1/...` est le contrat stable utilisé par Angular via la gateway.
- La pagination évite de charger et transférer une grande liste complète.
- `IJsonRepository<T>` fournit l'asynchronisme et la protection locale d'accès aux fichiers.
- Les dossiers JSON de chaque service constituent des sources de données indépendantes, sans base SQL partagée.
- Une vente devra être coordonnée entre Sales, Payments, Inventory et éventuellement Products; ces échanges seront étudiés à l'étape 3.
