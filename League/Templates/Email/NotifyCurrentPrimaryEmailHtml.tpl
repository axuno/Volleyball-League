<div>
    <h3>{{ L "Your primary email" }}</h3>
    <p>
        {{ L "Your primary email is about to be changed to" }}<br />
        <strong>{{ model.Email }}</strong>
    </p>
    <p>
        {{ L "We have sent a confirmation code to the new email address" }}.
    </p>
    <p>
        {{ org_ctx.Name }}
    </p>
</div>
