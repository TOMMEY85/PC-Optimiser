# PC Optimizer v7

PC Optimizer v7 — application WPF .NET 8 dédiée à l'optimisation Windows et au gaming.

## Fonctionnalités v7

- Dashboard Gaming modernisé et score de performance
- Analyse CPU, GPU, RAM et stockage
- Profil Gaming / Productivité
- Optimisation Windows réversible avec sauvegarde préalable
- Mode Performances élevées
- Nettoyage des fichiers temporaires
- Diagnostic réseau : purge DNS + test de latence
- Optimisation Fortnite
- Optimisation Call of Duty
- Booster temporaire des jeux en cours
- Désactivation de l'accélération souris avec restauration
- Sauvegarde / restauration du dernier état système

## Sécurité des changements

Les optimisations v7 enregistrent avant modification le plan d'alimentation actif et les valeurs souris utilisées pour la restauration. Les priorités de jeu sont temporaires et ne modifient pas durablement les fichiers des jeux.

## Build

Le projet cible net8.0-windows et utilise WPF.

PowerShell :

dotnet restore PCOptimizer.App/PCOptimizer.App.csproj
dotnet build PCOptimizer.App/PCOptimizer.App.csproj -c Release

Pour l'installateur Inno Setup :

.\Build-Installer.bat

Le script cherche automatiquement Inno Setup 6 ou 7.

## Arborescence

- PCOptimizer.App/ : application WPF
- Installer/ : script Inno Setup
- Build-Installer.bat : génération de l'installateur
- Build-Installer.ps1 : publication/build