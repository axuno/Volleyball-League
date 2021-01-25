{{~ d = ndate.to_zoned_time model.Fixture.PlannedStart ~}}
Hello volleyball players,

for the match 
{{ model.Fixture.HomeTeamNameForRound }} - {{ model.Fixture.GuestTeamNameForRound }}
scheduled for {{ d | ndate.format "g" }} {{ d | ndate.tz_abbr }}
there is no match result yet.

If the match day has been rescheduled, please report the new date.
Otherwise please enter the result. Both will be done by the 
home team. 

The visiting team will receive this message for information only.

Thank you very much for your support!
{{ org_ctx.Name }}
