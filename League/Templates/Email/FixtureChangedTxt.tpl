
{{ org_ctx.Name }} ({{ model.Fixture.RoundDescription }}) 
{{ model.Fixture.HomeTeamNameForRound }} : {{ model.Fixture.GuestTeamNameForRound }}
{{~ padright = [ L "Original fixture date", L "Replacement fixture date" ] | array.map "size" | array.sort | array.last ~}}
{{ if model.Fixture.OrigPlannedStart }}
    {{~ L "Original fixture date" | string.pad_right padright }}: {{ d = ndate.to_zoned_time model.Fixture.OrigPlannedStart }}{{ if d }}{{ d | ndate.format "g" }} {{ d | ndate.tz_abbr }}{{ else }}{{ L "unknown" }}{{ end }}
    {{~ L "Replacement fixture date" | string.pad_right padright }}: {{ d = ndate.to_zoned_time model.Fixture.PlannedStart }}{{ if d }}{{ d | ndate.format "g" }} {{ d | ndate.tz_abbr }}{{ else }}{{ L "unknown" }}{{ end }}
{{ else }}
    {{~ L "Original fixture date" | string.pad_right padright }}: {{ d = ndate.to_zoned_time model.Fixture.PlannedStart }}{{ if d }}{{ d | ndate.format "g" }} {{ d | ndate.tz_abbr }}{{ else }}{{ L "unknown" }}{{ end }}
{{ end }}
{{~ padright = [ L "Original venue", L "New venue" ] | array.map "size" | array.sort | array.last ~}}
{{ if model.Fixture.OrigVenueId ~}}
    {{~ L "Original venue" | string.pad_right padright }}: {{ if model.Fixture.OrigVenueName }}{{ model.Fixture.OrigVenueName }}{{ else }}{{ L "unknown" }}{{ end }}
    {{~ L "New venue" | string.pad_right padright }}: {{ if model.Fixture.VenueName }}{{ model.Fixture.VenueName }}{{ else }}{{ L "unknown" }}{{ end }}
{{ else ~}}
    {{~ L "Original venue" }}: {{ if model.Fixture.VenueName }}{{ model.Fixture.VenueName }}{{ else }}{{ L "unknown" }}{{ end }}
{{ end }}
{{ L "Changes were submitted by" }} {{ model.Username ? model.Username : "?" }}.
{{ L "This notification has been sent to the contact persons of the teams" }}.

{{ org_ctx.Name }}

{{ L "ID" }} #{{ model.Fixture.Id }} - {{ L "Number of changes" }}: {{ model.Fixture.ChangeSerial }}
