# PC Optimiser

PC Optimizer - optimisation Windows et gaming.

Version 6.1 — application WPF .NET 8 pour analyser et optimiser Windows avec des réglages réversibles et des sauvegardes.

## Fonctionnalités

- Analyse système et score d’optimisation
- Optimisations Windows et gaming
- Game Mode / Game DVR
- Plan d’alimentation hautes performances
- Nettoyage temporaire et optimisation SSD/TRIM
- Réglages USB et réseau
- Gestion des programmes au démarrage
- Optimisations gaming avancées et priorité temporaire des jeux
- Optimisation Fortnite avec sauvegarde/restauration
- Sauvegarde/restauration des réglages
- Guide XMP/EXPO et informations GPU/pilotes

## Build

Le projet cible `net8.0-windows` et utilise WPF.

Le déploiement recommandé est publié en mode self-contained multi-fichiers (`PublishSingleFile=false`) puis empaqueté avec Inno Setup.

## Projet

- `PCOptimizer.App/` : application WPF
- `Installer/` : script Inno Setup
- `Build-Installer.bat` : génération de l’installateur
- `Build-Installer.ps1` : publication/build
