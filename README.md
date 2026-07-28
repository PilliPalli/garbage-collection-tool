# CleanUP – Garbage Collector für Windows

CleanUP ist eine WPF-Desktopanwendung zur kontrollierten Bereinigung temporärer,
veralteter und doppelter Dateien. Das Projekt entstand als Abschlussprojekt und
zeigt unter anderem MVVM, asynchrone Dateiverarbeitung, rollenbasierte
Benutzerverwaltung und die Anbindung eines SQL Servers mit Entity Framework Core.

> **Hinweis:** Die Anwendung kann Dateien dauerhaft löschen. Verwende zum Testen
> ausschließlich ein eigens dafür angelegtes Verzeichnis und prüfe die
> Konfiguration vor jedem Bereinigungslauf.

## Funktionen

- Dateisuche über Wildcards oder reguläre Ausdrücke
- Filterung nach Dateialter und optionale rekursive Suche
- Verschieben in den Papierkorb oder bewusst aktivierbares direktes Löschen
- Erkennung und Entfernung von Duplikaten
- Zeitgesteuerte Bereinigung mit Countdown
- Protokollierung gelöschter Dateien und freigegebenen Speicherplatzes
- Registrierung, Anmeldung und Rollenverwaltung
- Passwort-Hashing mit Argon2id und individuellem Salt

## Tech-Stack

- C# und .NET 8
- Windows Presentation Foundation (WPF)
- MVVM
- Entity Framework Core 8
- Microsoft SQL Server
- Material Design in XAML
- Argon2id

## Projektstruktur

```text
Garbage-Collector/
├── Database/                         # SQL-Schema
└── Garbage-Collector/
    ├── Garbage-Collector.sln
    └── Garbage-Collector/
        ├── Model/
        ├── View/
        ├── ViewModel/
        ├── Utilities/
        └── Styles/
```

## Voraussetzungen

- Windows 10 oder neuer
- .NET 8 SDK
- Microsoft SQL Server 2022, lokal oder als Container
- Visual Studio 2022 mit dem Workload „.NET-Desktopentwicklung“ (empfohlen)

## Lokale Einrichtung

1. Repository klonen und die Solution
   `Garbage-Collector/Garbage-Collector.sln` öffnen.
2. Eine SQL-Server-Datenbank namens `GarbageCollectorDB` anlegen.
3. `Database/GarbageCollectorDB.sql` gegen diese Datenbank ausführen.
4. Die Anwendung einmal starten. Dadurch wird neben der ausführbaren Datei eine
   lokale `config.json` erzeugt.
5. In `config.json` den Platzhalter `CHANGE_ME` durch ein eigenes
   Entwicklungs-Passwort ersetzen und die übrigen Verbindungsdaten prüfen.
6. Ein separates Testverzeichnis anlegen und dieses als `SearchPath` auswählen.

`config.json` wird absichtlich nicht versioniert, weil sie lokale Pfade und
Zugangsdaten enthalten kann.

## Sicher testen

- `DeleteDirectly` zunächst auf `false` lassen; Dateien landen dann im Papierkorb.
- Für Tests nur Kopien und niemals persönliche Ordner verwenden.
- Mit einem kleinen Dateimuster wie `*.tmp` beginnen.
- Das Ergebnis und das Bereinigungsprotokoll prüfen, bevor rekursive Suche oder
  direktes Löschen aktiviert werden.

## Bekannte Grenzen

- Die Anwendung ist derzeit ausschließlich für Windows ausgelegt.
- Für Anmeldung und Protokollierung ist ein erreichbarer SQL Server erforderlich.
- Automatisierte Tests und eine CI-Pipeline sind noch nicht Teil dieses
  Projektstands.
- Vor einem produktiven Einsatz wären zusätzliche Schutzmechanismen wie
  Pfad-Allowlisting, Dry-Run-Vorschau und eine Bestätigung pro Löschlauf sinnvoll.

## Datenschutz und Repository-Hygiene

Dieses öffentliche Projekt soll ausschließlich Quellcode und neutrale
Beispielkonfiguration enthalten. Lokale Konfigurationen, Zugangsdaten,
personenbezogene Projektdokumentation und Build-Artefakte gehören nicht in das
Repository.

## Lizenz

Der Quellcode ist derzeit nicht unter einer Open-Source-Lizenz veröffentlicht.
Ohne separate Lizenz bleiben alle Rechte vorbehalten.
