{{~ d = ndate.to_zoned_time model.Fixture.PlannedStart ~}}
Hallo VolleyballerInnen,
hier ist Euer nächster Spieltag in der {{ org_ctx.ShortName }}:

Begegnung:       {{ model.Fixture.HomeTeamNameForRound }} - {{ model.Fixture.GuestTeamNameForRound }}
Datum:           {{ d | ndate.format "g" }} {{ d | ndate.tz_abbr }}  
{{~ if model.Venue ~}}
Austragungsort:  {{ model.Venue.Name }}
                 {{ model.Venue.Street }}, {{ model.Venue.PostalCode }} {{ model.Venue.City }}  
{{~ else ~}}
Austragungsort:  Noch nicht festgelegt!
{{~ end ~}}

Die Angaben entsprechen dem Stand zum Zeitpunkt des Versands dieser Mail. 
Aktuelle Angaben sind dem Spielplan auf der Website zu entnehmen.

Den Spielberichtsbogen könnt Ihr über die Website herunterladen.
Spieltag als Termin im ics-Format in alle gängigen Kalenderprogramme importieren:
{{ model.IcsCalendarUrl }}

Sollte es ausnahmsweise notwendig werden, den Spieltag zu verlegen, bitte sofort Kontakt mit 
der gegnerischen Mannschaft aufnehmen und den vereinbarten Ersatztermin auf der Website eintragen.

Ein interessantes Spiel wünscht
{{ org_ctx.Name }}
