# Dreamstreaming Subtitle Tracker

Een beheerpagina voor Jellyfin Server 12.4 die laat zien welke films en afleveringen al een ondertiteltrack hebben. Handig om na de dagelijkse Wyzie Subs-taak te zien welke titels nog ondertiteling missen.

## Installeren via de Jellyfin-catalogus

1. Open als beheerder **Dashboard → Plugins → Repositories** en voeg een repository toe.
2. Geef deze de naam **Dreamstreaming** en gebruik deze manifest-URL:

   `https://github.com/certified-dumbass/subtitle-checker/releases/latest/download/manifest.json`

3. Sla op, open **Catalogus**, zoek **Dreamstreaming Subtitle Tracker** en kies **Installeren**.
4. Herstart Jellyfin op een geschikt moment. Open de plugin via het dashboard.

De manifest-URL wordt actief zodra de eerste release succesvol is gepubliceerd. Je hebt bij deze installatiemethode geen API-sleutel of ontwikkeltools nodig.

## Functies en bediening

- **Ondertiteling → Nee** toont ontbrekende ondertitels en is de standaardweergave. Kies **Ja** voor titels met minstens één door Jellyfin herkende ondertiteltrack.
- **Type** filtert op films, afleveringen of beide. Bij afleveringen zie je ook de serie en de beschikbare seizoen- en afleveringsnummers.
- De lijst is op sorteertitel geordend. Je ziet het totaal voor je selectie en bladert met **Vorige** en **Volgende** door maximaal 50 titels per pagina.
- **Vernieuwen** haalt de huidige status op binnen de bibliotheken van het ingelogde account.

Elke taal telt mee, ook een herkende externe SRT of ingebouwde ondertiteltrack. Ingebrande tekst in het beeld heeft geen aparte track. De plugin leest de Jellyfin-index en bewaart geen kijkgeschiedenis. Downloads lopen via je bestaande ondertitelplugin. Verschijnt een nieuwe track nog niet, controleer dan eerst de media-informatie van de titel en laat Jellyfin indien nodig de bibliotheek opnieuw scannen.

## Handmatig installeren of bouwen

Download de plugin-zip van **Releases**, pak de DLL uit in een eigen map onder de plugins-map van je Jellyfin-datamap en herstart Jellyfin. Verwijderen kan via het plugindashboard.

Voor een eigen Windows-build gebruik je de broncode en de .NET SDK voor het framework uit `jellyfin.runtimeconfig.json`. Open PowerShell in de uitgepakte bronmap:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Build.ps1
```

Bij een afwijkend installatiepad voeg je `-ServerDir "D:\Jellyfin\Server"` toe. De DLL staat daarna in `output`.

## Releasebouw

De GitHub Actions-workflow bouwt tegen de assemblies uit de officiële Jellyfin 12.4-container. De release bevat de plugin-zip en een Jellyfin-manifest met de MD5 van diezelfde zip. Voor een nieuwe versie verhoog je de projectversie voordat je naar main pusht.
