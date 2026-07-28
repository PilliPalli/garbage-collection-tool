# CleanUP

> Windows-Desktopanwendung zur kontrollierten Bereinigung temporärer, veralteter und doppelter Dateien.
> Suchpfade, Dateimuster und Löschregeln lassen sich konfigurieren, Bereinigungen manuell oder zeitgesteuert ausführen und Ergebnisse anschließend auswerten. Umgesetzt ist das Projekt als WPF-Anwendung mit .NET 8.

---

## ✨ Features

- Dateien anhand von Wildcards oder regulären Ausdrücken suchen
- Dateien nach Alter filtern und Unterverzeichnisse optional einbeziehen
- Temporäre Junk-Dateien erkennen und entfernen
- Doppelte Dateien über Inhaltsvergleiche identifizieren und löschen
- Dateien wahlweise in den Papierkorb verschieben oder direkt löschen
- Automatische Bereinigung in konfigurierbaren Intervallen
- Countdown bis zum nächsten geplanten Bereinigungslauf
- Bereinigungen mit Anzahl und freigegebenem Speicherplatz protokollieren
- Protokolle als CSV-Datei exportieren
- Benutzerregistrierung, Anmeldung und Rollenverwaltung
- Passwörter mit Argon2id und individuellem Salt hashen
- Einstellungen und Datenbankverbindung lokal in `config.json` speichern

---

## 🛠 Tech Stack

| Kategorie | Technologie |
|---|---|
| Framework | WPF auf .NET 8 |
| Sprache | C# |
| UI | XAML und Material Design in XAML |
| Architektur | MVVM |
| Datenzugriff | Entity Framework Core 8 |
| Datenbank | Microsoft SQL Server |
| Sicherheit | Argon2id-Passwort-Hashing |
| Konfiguration | JSON mit Newtonsoft.Json |

---

## 🏗 Architektur

Die WPF-Oberfläche ist in Views für Anmeldung, Startseite, Bereinigung, Einstellungen, Informationen und Administration gegliedert. ViewModels enthalten Navigation und Anwendungslogik. Der `CleanupVM` durchsucht das Dateisystem, führt die gewählte Bereinigung aus und schreibt Protokolle über Entity Framework Core in SQL Server. Die gemeinsam verwendete `AppConfig` persistiert Such- und Löschoptionen sowie die Datenbankverbindung in einer lokalen JSON-Datei.

```text
WPF-Oberfläche
├── NavigationVM
├── CleanupVM
│   ├── Dateisystem
│   ├── Scheduler
│   └── CleanupLogs
├── SettingsVM
├── LoginVM und RegisterVM
├── AdministrationVM
└── GarbageCollectorDbContext
    └── SQL Server
```

---

## 📂 Projektstruktur

```text
garbage-collection-tool/
├── Database/
│   └── GarbageCollectorDB.sql       # Tabellen und Rollen
├── Garbage-Collector/
│   ├── Garbage-Collector/
│   │   ├── Images/                  # Logos und UI-Icons
│   │   ├── Model/                   # Konfiguration, Entitäten und DbContext
│   │   ├── Styles/                  # Wiederverwendbare WPF-Styles
│   │   ├── Utilities/               # Commands, Converter und Basisklassen
│   │   ├── View/                    # Anwendungsansichten
│   │   ├── ViewModel/               # Navigation und Anwendungslogik
│   │   └── Garbage-Collector.csproj
│   └── Garbage-Collector.sln
├── config.example.json              # Neutrale Beispielkonfiguration
└── README.md
```

---

## 🚀 Installation

### Voraussetzungen

- Windows 10 oder neuer
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Microsoft SQL Server 2022, lokal oder als Container
- Git

### Repository klonen

```bash
git clone https://github.com/PilliPalli/garbage-collection-tool.git
cd garbage-collection-tool
```

### Datenbank vorbereiten

1. Lege in SQL Server eine Datenbank mit dem Namen `GarbageCollectorDB` an.
2. Führe `Database/GarbageCollectorDB.sql` gegen diese Datenbank aus.
3. Starte die Anwendung einmal, damit eine lokale `config.json` erzeugt wird.
4. Ersetze dort den Platzhalter `CHANGE_ME` durch ein eigenes Entwicklungspasswort und prüfe die übrigen Verbindungsdaten.

### Anwendung starten

```bash
dotnet restore Garbage-Collector/Garbage-Collector.sln
dotnet run --project Garbage-Collector/Garbage-Collector/Garbage-Collector.csproj
```

### Build

```bash
dotnet build Garbage-Collector/Garbage-Collector.sln
```

---

## ⚙ Konfiguration

Die lokale `config.json` wird nicht versioniert, da sie Zugangsdaten und persönliche Pfade enthalten kann. `config.example.json` zeigt die erwartete Struktur:

| Einstellung | Bedeutung |
|---|---|
| `SearchPath` | Zu durchsuchendes Verzeichnis |
| `FilePatterns` | Wildcards oder reguläre Ausdrücke für die Dateisuche |
| `OlderThanDays` | Mindestalter der zu bereinigenden Dateien |
| `DeleteDirectly` | Dateien direkt statt über den Papierkorb löschen |
| `DeleteRecursively` | Unterverzeichnisse in die Suche einbeziehen |
| `ConnectionString` | Verbindung zur Datenbank `GarbageCollectorDB` |

---
