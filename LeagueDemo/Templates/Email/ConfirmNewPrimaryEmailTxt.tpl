
{{ L "Change your primary email address" }}

{{ L "Please confirm the new email address by clicking this link" }}:

{{ model.CallbackUrl }}
{{ L "The confirmation link can be used once until" }} {{ d = ndate.to_zoned_time model.Deadline }}{{ d | ndate.format "g" }} {{ d | ndate.tz_abbr }}.

{{ org_ctx.Name }}


xxxxx