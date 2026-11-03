# JSON data roots

Chaque microservice possède ses propres fichiers sous son projet ou sous ce dossier de données. Aucune API ne lit ou écrit le fichier JSON d'un autre service.

Les chemins sont configurables via `JsonRepositoryOptions.DataPath`. Le fichier peut être initialisé depuis les données de seed uniquement pendant le développement.
