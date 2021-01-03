
{{ L "Here is your password recovery code" }}.

{{ L "In order to choose a new password, please click this link" }}:
{{ model.CallbackUrl }}

{{ L "The recovery code can be used once until" }} {{ d = ndate.to_zoned_time model.Deadline }}{{ d | ndate.format "g" }} {{ d | ndate.tz_abbr }}.

{{ org_ctx.Name }}
