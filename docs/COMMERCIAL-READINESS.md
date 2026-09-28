# Verkauf, Lizenzen und offene Freigaben

Stand: 28. September 2026. Technische und lizenzbezogene Vorprüfung, keine verbindliche Rechtsberatung.

## Einschätzung

Ein kommerzieller Vertrieb einer eigenständigen Quest-App zur Steuerung von Home Assistant ist grundsätzlich denkbar. **Diese Entwicklungsversion ist noch kein verkaufsfertiges Produkt.** Insbesondere sind reale Mehrraum-Lokalisierung, die wiederholt gemeldeten Handtracking-Probleme, Performance, Systemtastatur und Kameradarstellung auf Quest noch nicht abgenommen. Die automatisierten Prüfungen ersetzen diese Abnahme nicht.

## Unity: vor einem Verkauf schriftlich klären

Die App dient der Haussteuerung und damit nicht ausschließlich Spielen oder Unterhaltung. Nach der aktuellen Definition kann sie unter „Industry Customer“ fallen; Unity entscheidet über die Einordnung. Die Bedingungen zur Runtime-Verteilung enthalten für Industry-Kunden eine zusätzliche ausdrückliche Genehmigung. Deshalb nicht allein auf die Personal-Umsatzgrenze vertrauen. Mit Unity die Einordnung, den tatsächlich verwendeten Tarif, dessen für den Nutzer geltende Vertragsfassung und die geplante kostenpflichtige APK-Verteilung schriftlich klären. Die allgemeinen Unity-6-Regeln sehen bei Einhaltung der Bedingungen keine Runtime Fee vor; das ersetzt keine passende Lizenz. Quellen: [Editor Software Terms, Abschnitte 2.2/2.2.1 und Definitionen](https://unity.com/legal/editor-terms-of-service/software), [Lizenzberechtigung](https://unity.com/pages/license-compliance), [FAQ](https://unity.com/legal/editor-terms-of-service/software/faq).

## Meta SDK und Store

Die installierten Meta XR Core- und MRUK-Pakete verweisen auf die Meta/Oculus-SDK-Lizenz. Diese erlaubt die Entwicklung und Verteilung von Anwendungen unter Bedingungen, beschränkt den Einsatz auf zugelassene Plattformen und verlangt passende Lizenz-/Copyright-Hinweise. Das ist keine pauschale Freigabe für alle Meta-Beispiele: Manche separat lizenzierten Beispiele sind nur zur nichtkommerziellen Evaluation bestimmt. Diese App verwendet eigene UI-Grafiken und Sounds; es wurden keine Filmoberflächen oder separat heruntergeladenen Meta-UI-Mockups übernommen.

Ein Sideload-Build ist keine Store-Zulassung. Vor Veröffentlichung sind Entwicklerorganisation, Store-Vorgaben/VRCs, Datenschutzangaben und gegebenenfalls die Prüfung angeforderter Kameraberechtigungen erforderlich. Boundaryless bleibt eine offizielle Plattformfunktion; Systemschutz und Systemgesten werden nicht umgangen. Quellen: [SDK-Lizenz](https://developers.meta.com/horizon/licenses/oculussdk/), [separate Examples-Lizenz](https://developers.meta.com/horizon/licenses/examples-license-1.0/), [VRCs](https://developers.meta.com/horizon/resources/publish-common-vrc-failures/), [prüfpflichtige Berechtigungen](https://developers.meta.com/horizon/resources/permissions-review-required/).

## Home Assistant und Marken

Die App kommuniziert über die dokumentierte HTTP-API mit einem vom Nutzer betriebenen Home Assistant. Es wird kein Home-Assistant-Servercode in die APK eingebaut. Der separate API-Client muss deshalb nicht allein wegen der Verbindung unter die Lizenz des Servers gestellt werden. Die Lizenz eines Servers gibt aber keine Markenrechte. „Home Assistant“, das Logo, „Works with Home Assistant“, „Meta“ und „Quest“ nicht als eigene Marke oder als offizielle Partnerschaft darstellen. Den Produktnamen „HA goes Quest“ vor kommerzieller Veröffentlichung markenrechtlich prüfen; eine sachliche Kompatibilitätsangabe klar von einem Zertifizierungsbadge unterscheiden. Quellen: [HA REST API](https://developers.home-assistant.io/docs/api/rest/), [Markenhinweise](https://www.home-assistant.io/tos/), [Works-with-Guide](https://works-with.home-assistant.io/resources/wwha-compliance-guide/).

## Eingebundene Bestandteile und eigene Inhalte

- ZXing.Net: Apache-2.0, Lizenz und vorhandene Hinweise erhalten. Newtonsoft.Json: MIT; Copyright und Lizenz erhalten. Unity-Pakete haben eigene Unity-Lizenzbedingungen und teilweise Drittanbieterhinweise.
- Die lokalen Paket-Lizenzdateien und Hinweise werden mitgeliefert. Vor Verkauf die tatsächliche Release-APK einschließlich nativer Bibliotheken nochmals mit einer Komponentenliste abgleichen; diese Vorprüfung ist keine vollständige transitive Rechtsprüfung.
- Die neuen geometrischen Icons und Klickgeräusche entstehen aus eigenem App-Code. Der bestehende Feuerball wurde generiert; Herkunft und Prompt stehen in `ART-MAGIC.md`. OpenAIs Bedingungen übertragen Output-Rechte im Verhältnis zum Anbieter soweit rechtlich zulässig, versprechen aber keine Einzigartigkeit oder Freiheit von Rechten Dritter. [OpenAI-Bedingungen](https://openai.com/policies/row-terms-of-use/).
- Es wird keine Marvel-/Iron-Man-Marke, Filmmusik oder kopierte Filmoberfläche ausgeliefert. Die Uhr öffnet nach Metas Gestaltungsempfehlung ein ortsfestes Panel.
- Eine eigene öffentliche Quellcodelizenz ist weiterhin eine Entscheidung des Rechteinhabers. Drittanbieterdateien dürfen nicht pauschal als eigener Code neu lizenziert werden.

## Vor dem Verkauf noch erforderlich

1. Unity-Einordnung und passende kommerzielle Verteilungsrechte klären; Markenprüfung und vollständige Komponenten-/Lizenzprüfung abschließen.
2. Getrennte Produktionssignierung mit sicherem Schlüsselmanagement planen. Die heutige APK nutzt bewusst dieselbe Entwicklungssignatur wie die vorherigen Testversionen, damit deren Token erhalten bleibt. Ein Schlüsselwechsel erlaubt kein normales Update derselben Installation; Migration/Store-Signierung vor Erstverkauf entscheiden.
3. Auf echter Quest Kaltstarts in mehreren Räumen, Rekalibrierung, Raumänderung, verlorenes Tracking, Gesten, Systemtastatur, lange Sitzungen und Offline-Fälle prüfen. Messbare Stabilität und Performance nachweisen.
4. Datenschutzinformation und Löschkonzept für Zugangstoken, Raumanker, Kamera- und Gerätedaten sowie tatsächlich eingebundene SDK-Verarbeitung prüfen. Kein pauschales „keine Daten verlassen das Gerät“ versprechen: Home-Assistant-Anfragen gehen bewusst an den konfigurierten Server, und Plattformdienste haben eigene Datenverarbeitung.
5. Preise, Support, Updatepolitik, Store-Unterlagen und für den Vertriebsmarkt erforderliche Verbraucher-/Datenschutzinformationen rechtlich prüfen lassen.

**Ergebnis:** als fortgeschrittener Prototyp weiter testen. Ein kostenpflichtiges Endkundenprodukt erst nach diesen Freigaben und erfolgreicher Geräteabnahme anbieten.
