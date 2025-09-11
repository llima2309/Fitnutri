window.blazorSubmitCookie = (token, returnUrl) => {
    const f = document.createElement('form');
    f.method = 'POST';
    f.action = '/auth/cb';

    const i1 = document.createElement('input');
    i1.type = 'hidden'; i1.name = 'token'; i1.value = token;

    const i2 = document.createElement('input');
    i2.type = 'hidden'; i2.name = 'returnUrl'; i2.value = returnUrl || '/';

    f.appendChild(i1);
    f.appendChild(i2);
    document.body.appendChild(f);
    f.submit();
};
