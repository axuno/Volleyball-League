{{ func gender ~}}
    {{~ if $0 == "f" }}{{ ret "Mrs."}}{{else if $0 == "m"}}{{ret "Mr."}}{{ else }}{{ret ""}}{{ end ~}}
{{ end ~}}

{{ L "Contact form message to" }} {{ if tenant_ctx.IsDefault }}volleyball-liga.de{{ else }}{{ org_ctx.Name }}{{ end}}

{{ L "Sender" }}
    {{ (gender model.Form.Gender) | L }} {{ model.Form.FirstName }} {{ model.Form.LastName }}
    {{ L "Email" }}: {{ model.Form.Email }}
{{ if model.Form.PhoneNumber != "" ~}}
    {{ L "Phone" }}: {{ model.Form.PhoneNumber }}
{{ end ~}}

{{ L "Subject" }}
    {{ model.Form.Subject }}

{{ if model.Form.Message == "" ~}}
    {{~ L "No message included" }}
{{ else ~}}
    {{~ L "Message" }}
------------------------------
    {{~ model.Form.Message}}
------------------------------
{{ end }}
