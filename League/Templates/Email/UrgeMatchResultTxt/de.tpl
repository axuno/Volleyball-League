{{~ d = ndate.to_zoned_time model.Fixture.PlannedStart ~}}
Hallo VolleyballerInnen,

für die Begegnung {{ model.Fixture.HomeTeamNameForRound }} - {{ model.Fixture.GuestTeamNameForRound }} 
vom {{ d | ndate.format "g" }} {{ d | ndate.tz_abbr }} liegt kein Spielergebnis vor,
und auch ein Ersatzspieltag ist nicht bestimmt.

Noch einmal habt Ihr die Chance, ein neues Datum für das Spiel einzutragen, oder das Ergebnis nachzumelden. 

Ansonsten wird das Spiel lt. Spielordnung für beide Mannschaften als ausgefallen gewertet.

Sportliche Grüße,
{{ org_ctx.Name }}
