# Entwicklung-eines-Garbage-Collection-Tool-zur-Loeschung-temporaerer-Dateien

Dieses Projekt beinhaltet ein Garbage-Collection-Tool zur automatisierten Bereinigung temporärer und überflüssiger Dateien. Ziel ist die Optimierung der Systemressourcen und Steigerung der Effizienz der IT-Infrastruktur.

## Funktionen
- Automatische Erkennung und Löschung temporärer Dateien
- Manuelle Auswahl und Bereinigung von Dateien
- Berichterstellung über Bereinigungsaktionen
- Sicherheitsfunktionen zum Schutz vor unbeabsichtigtem Löschen
- Konfigurierbare Einstellungen über JSON-Datei
- Duplikaterkennung und Entfernung
- Geplanter Bereinigungsintervall (Scheduler)
- Benuterverwaltung inklusive Rollen

## Installationsanleitung

### 1. Systemvoraussetzungen
- Windows 10 oder neuer
- .NET Runtime 8.0 oder höher
- Microsoft SQL Server (lokal oder im Netzwerk)
- Microsoft SQL Server Management Studio (SSMS) zur Verwaltung der Datenbank
- Optional: Git, falls der Quellcode über ein Repository geladen wird

### 2. Vorbereitung der Datenbank
- Eine neue Datenbank mit dem Namen `GarbageCollectorDB` anlegen.
- Das mitgelieferte SQL-Skript `GarbageCollectorDB.sql` ausführen. Dieses erstellt alle notwendigen Tabellen und Beziehungen.
- Es werden keine Testdaten benötigt, da sensible Informationen (z. B. Passwörter) in der Anwendung verschlüsselt gespeichert werden.

### 3. Konfiguration und Verbindung
- Der Connection String wird in der Datei `config.json` gespeichert.
- Diese Datei befindet sich im gleichen Verzeichnis wie die ausführbare Datei der Anwendung.
- Das Programm kann nun ausgeführt werden, entweder über Visual Studio oder über eine Release-Build Version.


## Hinweis
Dieses Repository enthält den finalen Projektstand inklusive Git-Historie zur Dokumentation im Rahmen der Hausarbeit.

