{{~ d = ndate.to_zoned_time model.Fixture.PlannedStart ~}}
Hallo VolleyballerInnen,

für die Begegnung 
{{ model.Fixture.HomeTeamNameForRound }} - {{ model.Fixture.GuestTeamNameForRound }}
vom {{ d | ndate.format "g" }} {{ d | ndate.tz_abbr }}
liegt noch kein Spielergebnis vor.

Wenn der Spieltag verlegt wurde, bitte das neue Datum melden.
Ansonsten bitte das Ergebnis eintragen. Beides übernimmt die 
Heimmannschaft. 

Die Gastmannschaft erhält diese Mail lediglich zur Information.

Herzlichen Dank für Eure Unterstützung!
{{ org_ctx.Name }}
