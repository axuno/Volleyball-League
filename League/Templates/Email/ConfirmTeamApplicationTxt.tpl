{{ if model.IsNewApplication }}{{ L "Confirmation of the team registration" }}{{ else }}{{ L "Update of team registration"}}{{ end }}

{{ org_ctx.Name }}
{{ model.TournamentName }}
{{ model.RoundDescription }} {{ model.RoundTypeDescription }}

{{ padright = [ L "Team name", L "Registered by" ] | array.map "size" | array.sort | array.last ~}}
{{ L "Team name" | string.pad_right padright }}: {{ model.TeamName }}
{{ L "Registered by" | string.pad_right padright }}: {{ model.RegisteredByName }} <{{ model.RegisteredByEmail }}>

{{ L "Thanks for registering your team." }}
{{ L "We wish you much success and good luck." ~}}
{{ if org_ctx.Bank.ShowBankDetailsInConfirmationEmail && model.IsRegisteringUser }}

    {{~ padright = [ L "Amount", L "Recipient", L "IBAN", L "BIC", L "Bank name", L "Reference ID" ] | array.map "size" | array.sort | array.last ~}}
    {{~ L "Please transfer the participation fee to the following bank account:" }}
    {{~ L "Amount" | string.pad_right padright }}: {{ org_ctx.Bank.Amount }} {{ org_ctx.Bank.Currency }}
    {{~ L "Recipient" | string.pad_right padright }}: {{ org_ctx.Bank.Recipient }}
    {{~ L "IBAN" | string.pad_right padright }}: {{ org_ctx.Bank.Iban }}
    {{~ L "BIC" | string.pad_right padright }}: {{ org_ctx.Bank.Bic }}
    {{~ L "Bank name" | string.pad_right padright }}: {{org_ctx.Bank.BankName }}
    {{~ L "Reference ID" }}: {{ model.BankTransferId }}

    {{~ L "Thank you." }}
{{ end }}
{{ if model.IsRegisteringUser }}
    {{~ L "The registration can be edited using this link:" }}
    {{~ model.UrlToEditApplication }}
{{ end }}
{{ L "Sporting greetings" }}
{{ org_ctx.ShortName }}
