﻿<!-- source: https://buttons.cm/ -->
<div>
    <h3>{{ L "Here is your password recovery code" }}.</h3>
    <p>{{ L "In order to choose a new password, please click this link" }}:</p>
    <!--[if mso]>
        <v:roundrect xmlns:v="urn:schemas-microsoft-com:vml" xmlns:w="urn:schemas-microsoft-com:office:word" href="{{ model.CallbackUrl }}" style="height: 38px; v-text-anchor: middle; width: 200px;" arcsize="27%" strokecolor="#018dff" fillcolor="#018dff">
            <w:anchorlock/>
            <center style="color: #ffffff; font-family: Arial, sans-serif; font-size: 13px; font-weight: bold;">@buttonText</center>
        </v:roundrect>
    <![endif]-->
    <!--[if !mso]><!-- -->
    <a href="{{ model.CallbackUrl }}" style="background-color: #018dff; border: 1px solid #018dff; border-radius: 10px; color: #ffffff; display: inline-block; font-family: Arial, sans-serif; font-size: 13px; font-weight: bold; line-height: 38px; text-align: center; text-decoration: none; width: 200px; -webkit-text-size-adjust: none; mso-hide: all;">{{ L "Change Password" }}</a>
    <!--<![endif]-->
    <p>
        {{ L "The recovery code can be used once until" }}  {{ d = ndate.to_zoned_time model.Deadline }}{{ d | ndate.format "g" }} {{ d | ndate.tz_abbr }}.
    </p>
    <p>
        {{ org_ctx.Name }}
    </p>
</div>
