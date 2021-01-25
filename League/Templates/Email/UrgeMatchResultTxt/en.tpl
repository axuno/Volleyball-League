{{~ d = ndate.to_zoned_time model.Fixture.PlannedStart ~}}
Hello volleyball players,

for the match 
{{ model.Fixture.HomeTeamNameForRound }} - {{ model.Fixture.GuestTeamNameForRound }} 
scheduled for {{ d | ndate.format "g" }} {{ d | ndate.tz_abbr }}
there is no match result submitted, and also an alternate match day has not been determined.

Once again you have the chance to enter a new date for the match, or to submit the result now.  
Otherwise, the match will be considered as canceled for both teams according to the rules of game.

Sporting greetings,
{{ org_ctx.Name }}
