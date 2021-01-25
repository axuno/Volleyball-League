{{~ d = ndate.to_zoned_time model.Fixture.PlannedStart ~}}
Hello volleyball players,
here is your next matchday in {{ org_ctx.ShortName }}:

Match:  {{ model.Fixture.HomeTeamNameForRound }} - {{ model.Fixture.GuestTeamNameForRound }}
Date:   {{ d | ndate.format "g" }} {{ d | ndate.tz_abbr }}  
{{~ if model.Venue ~}}
Venue:  {{ model.Venue.Name }}
        {{ model.Venue.Street }}, {{ model.Venue.PostalCode }} {{ model.Venue.City }}  
{{~ else ~}}
Venue:  Yet unknown!
{{~ end ~}}

The information is correct at the time of sending this mail. 
For current details, please refer to the fixture on the website.

Download the match report sheet using from the website.
Import the fixure as an appointment in ics format into all common calendar programs
{{ model.IcsCalendarUrl }}

Should it exceptionally become necessary to reschedule the match day, please contact 
the opposing team immediately and enter the agreed alternative date on the website.

We wish you an interesting match
{{ org_ctx.Name }}
