
{{ L "Thank you for creating an account" }}

{{ L "Please continue with your registration by clicking this link" }}:
{{ model.CallbackUrl }}

{{ L "The confirmation link can be used until" }} {{ d = ndate.to_zoned_time model.Deadline }}{{ d | ndate.format "g" }} {{ d | ndate.tz_abbr }}.

{{ org_ctx.Name }}
