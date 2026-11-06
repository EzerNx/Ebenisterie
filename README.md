# Collabox Import

Application Windows de l'Ébénisterie ERN qui prépare les données de Cabinet Vision pour Collabox.

- **Restaurer la base et exporter** : supprime la base `Report`, la restaure depuis un fichier `.bak` choisi par l'utilisateur, puis exporte ses tables dans `import_AAAA-MM-JJ.xlsx`.
- **Exporter le catalogue** : exporte 6 tables de la base `CVData` (catalogue des matériaux) dans `catalogue_AAAA-MM-JJ.xlsx`, sans restauration.
- **Paramètres** : choix du serveur SQL de ce PC.

Les fichiers Excel sont créés dans `Téléchargements\importDataCollabox\`.

## Prérequis sur chaque PC

1. **Windows 10 ou 11, 64 bits.** Le .NET n'a pas besoin d'être installé : il est intégré au `.exe`.
2. **SQL Server installé sur le PC** (SQL Server Express suffit). Chaque PC a son propre serveur ; il n'y a pas de serveur partagé.
3. **Droits SQL du compte Windows de l'utilisateur** : rôle `dbcreator` (ou `sysadmin`), pour pouvoir supprimer et restaurer la base `Report`. La connexion utilise le compte Windows (`Trusted_Connection`), sans mot de passe.
4. **Base `CVData` attachée** au même serveur, pour l'export du catalogue.
5. **Fichier `.bak` lisible par le service SQL Server** : c'est le service qui lit le fichier, pas l'application. Placer le `.bak` dans un dossier comme `C:\Backups` plutôt que dans Téléchargements ou le Bureau.

## Serveur SQL de chaque PC

Au démarrage, l'application cherche un serveur qui répond, dans cet ordre :

1. celui enregistré sur ce PC (`%LocalAppData%\CollaboxImport\parametres.json`) ;
2. celui de `appsettings.json` (valeur par défaut : `localhost\SQLEXPRESS`) ;
3. les instances SQL Server installées sur le PC (lues dans le registre Windows).

Le premier serveur qui répond est enregistré pour les prochains démarrages. Si aucun ne répond, le bouton **Paramètres** permet de l'indiquer et de tester la connexion.

## Produire l'exécutable

Dans Visual Studio : clic droit sur le projet, **Publier…**, profil **CollaboxImport**, puis **Publier**.

En ligne de commande :

```
dotnet publish -p:PublishProfile=CollaboxImport
```

Le résultat est dans `bin\Publish\CollaboxImport\` :

- `CollaboxImport.exe` : l'application ;
- `appsettings.json` : le serveur par défaut, à garder à côté du `.exe`.

Copier ces deux fichiers dans un dossier sur chaque PC (par exemple `C:\Program Files\Collabox Import\`) et créer un raccourci vers `CollaboxImport.exe`.

Pour une nouvelle version, augmenter `<Version>` dans `Ebenisterie.csproj` avant de publier.

## Structure du code

| Dossier / fichier | Rôle |
|---|---|
| `Form1.cs` | fenêtre principale, boutons d'export |
| `ParametresForm.cs` | fenêtre de choix du serveur SQL |
| `Services/ParametresApp.cs` | détection et enregistrement du serveur de ce PC |
| `Services/DatabaseRestorer.cs` | suppression et restauration de la base `Report` |
| `Services/SqlServerService.cs` | exécution des requêtes SQL |
| `Services/ExcelService.cs`, `Helpers/ExcelWriter.cs` | création du fichier Excel (EPPlus) |
| `Exporters/` | une classe par table exportée ; `CatalogueExporter.cs` pour `CVData` |
