<!-- source: https://buttons.cm/ -->
<div>
    <h3{{ L "Change your primary email address" }}</h3>
    <p>{{ L "Please confirm the new email address by clicking this link" }}</p>
    <!--[if mso]>
        <v:roundrect xmlns:v="urn:schemas-microsoft-com:vml" xmlns:w="urn:schemas-microsoft-com:office:word" href="{{ model.CallbackUrl }}" style="height: 38px; v-text-anchor: middle; width: 200px;" arcsize="27%" strokecolor="#018dff" fillcolor="#018dff">
            <w:anchorlock/>
            <center style="color: #ffffff; font-family: Arial, sans-serif; font-size: 13px; font-weight: bold;">{{ L "Confirm New Email"}}</center>
        </v:roundrect>
    <![endif]-->
    <!--[if !mso]><!-- -->
    <a href="{{ model.CallbackUrl }}" style="background-color: #018dff; border: 1px solid #018dff; border-radius: 10px; color: #ffffff; display: inline-block; font-family: Arial, sans-serif; font-size: 13px; font-weight: bold; line-height: 38px; text-align: center; text-decoration: none; width: 200px; -webkit-text-size-adjust: none; mso-hide: all;">{{ L "Confirm New Email" }}</a>
    <!--<![endif]-->
    <p>
        {{ L "The confirmation link can be used once until" }} {{ d = ndate.to_zoned_time model.Deadline }}{{ d | ndate.format "g" }} {{ d | ndate.tz_abbr }}.
    </p>
    <p>
        {{ org_ctx.Name }}
    </p>
</div>
