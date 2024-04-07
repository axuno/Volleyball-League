
{{ org_ctx.Name }} ({{ model.RoundDescription }})
{{ model.HomeTeamName }} : {{ model.GuestTeamName }}

{{ plannedstart = ndate.to_zoned_time (model.Match.OrigPlannedStart ? model.Match.OrigPlannedStart : model.Match.PlannedStart) ~}}
{{ realstart = ndate.to_zoned_time model.Match.RealStart ~}}
{{ realend = ndate.to_zoned_time model.Match.RealEnd ~}}
{{ if realstart == plannedstart ~}}
    {{~ L "Match day of {0}" (ndate.format realstart "d") }} {{ realstart | ndate.tz_abbr }}
{{ else ~}}
    {{~ L "Match day of {0}, played on {1}" (ndate.format plannedstart "d") (ndate.format realstart "d") }} {{ realstart | ndate.tz_abbr }}
{{ end ~}}

{{ L "Result" }}
{{ if model.Match.Sets.size > 0 ~}}
    {{~ L "Ball points" }} / {{ L "Set points" }}
{{ else ~}}

    {{~ L "No sets" ~}}
{{ end ~}}
{{ i = 0 }}
{{ points = { homeball: 0, guestball: 0, homeset: 0, guestset: 0 } ~}}
{{for set in model.Match.Sets ~}} 
    {{~ points.homeball = points.homeball + set.HomeBallPoints ~}}
    {{~ points.guestball = points.guestball + set.GuestBallPoints ~}}
    {{~ points.homeset = points.homeset + set.HomeSetPoints ~}}
    {{~ points.guestset = points.guestset + set.GuestSetPoints ~}}
    {{~ i = i + 1 ~}}
    {{~ L "Set #{0}" i }}:  {{ set.HomeBallPoints | string.pad_left 2 }} : {{ set.GuestBallPoints | string.pad_right 2 }}  / {{ set.HomeSetPoints | string.pad_left 2 }} : {{ set.GuestSetPoints | string.pad_right 2 }}
{{ end ~}}

{{ padright = [ L "Ball points", L "Set points", L "Match points" ] | array.map "size" | array.sort | array.last ~}}
{{ L "Ball points" | string.pad_right padright }}: {{ points.homeball | string.pad_left 3 }} : {{ points.guestball }}
{{ L "Set points" | string.pad_right padright }}: {{ points.homeset | string.pad_left 3 }} : {{ points.guestset }}
{{ L "Match points" | string.pad_right padright }}: {{ model.Match.HomePoints | string.pad_left 3 }} : {{ model.Match.GuestPoints }}

{{ padright = [ L "Start of match", L "End of match" ] | array.map "size" | array.sort | array.last ~}}
{{ L "Start of match" | string.pad_right padright }}: {{ ndate.format realstart "t" }} {{ realstart | ndate.tz_abbr }}
{{ L "End of match" | string.pad_right padright }}: {{ ndate.format realend "t" }} {{ realend | ndate.tz_abbr }}

{{ if (model.Match.Remarks | string.strip | string.size) > 2 ~}}
    {{~ L "Remarks" }}:
    {{~ model.Match.Remarks }}

{{ end ~}}
{{ L "Changes were submitted by" }} {{ model.Username ? model.Username : "?" }}.
{{ L "This notification has been sent to the contact persons and players of the teams" }}.

{{ org_ctx.Name }}


{{ L "ID" }} #{{ model.Match.Id }} - {{ L "Number of changes" }}: {{ model.Match.ChangeSerial }}
