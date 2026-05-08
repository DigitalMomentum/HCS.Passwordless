export function initLoginForm(formEl) {
    if (!formEl) return;

    const base = formEl.dataset.pwlBase || '/auth';
    const getEmail = () => formEl.querySelector('#pwl-email')?.value?.trim() ?? '';
    const getReturnUrl = () => formEl.dataset.returnUrl ?? '';
    const msgEl = formEl.querySelector('#pwl-message');

    function showMessage(text, isError) {
        if (!msgEl) return;
        msgEl.textContent = text;
        msgEl.style.color = isError ? '#c00' : '#080';
    }

    function antiforgeryToken(scopeEl) {
        return (scopeEl ?? formEl).querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';
    }

    async function postJson(url, body, scopeEl) {
        return fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': antiforgeryToken(scopeEl),
            },
            body: JSON.stringify(body),
        });
    }

    // Magic link
    formEl.querySelector('#pwl-btn-magic-link')?.addEventListener('click', async function () {
        const email = getEmail();
        if (!email) { showMessage('Please enter your email address.', true); return; }
        showMessage('');
        this.disabled = true;
        try {
            const resp = await postJson(`${base}/magic-link/request`, { email, returnUrl: getReturnUrl() });
            showMessage(resp.ok ? 'Check your email for a sign-in link.' : 'Something went wrong. Please try again.', !resp.ok);
        } catch {
            showMessage('Network error. Please try again.', true);
        } finally {
            this.disabled = false;
        }
    });

    // OTP request — section lives outside the login form to avoid invalid nested <form> elements
    const otpSection = document.getElementById('pwl-otp-section');
    formEl.querySelector('#pwl-btn-otp')?.addEventListener('click', async function () {
        const email = getEmail();
        if (!email) { showMessage('Please enter your email address.', true); return; }
        showMessage('');
        this.disabled = true;
        try {
            const resp = await postJson(`${base}/otp/request`, { email, returnUrl: getReturnUrl() });
            if (resp.ok) {
                if (otpSection) {
                    otpSection.style.display = '';
                    const otpForm = otpSection.querySelector('#pwl-otp-form');
                    if (otpForm) otpForm.style.display = '';
                }
                showMessage('A one-time code has been sent to your email.');
            } else {
                showMessage('Something went wrong. Please try again.', true);
            }
        } catch {
            showMessage('Network error. Please try again.', true);
        } finally {
            this.disabled = false;
        }
    });

    // OTP verify
    otpSection?.querySelector('#pwl-otp-form')?.addEventListener('submit', async function (e) {
        e.preventDefault();
        const code = this.querySelector('#pwl-otp-code')?.value?.trim() ?? '';
        const otpMsgEl = this.querySelector('#pwl-otp-message');
        const showOtpMsg = (text, isError) => {
            if (!otpMsgEl) return;
            otpMsgEl.textContent = text;
            otpMsgEl.style.color = isError ? '#c00' : '#080';
        };
        if (!code) { showOtpMsg('Please enter the code.', true); return; }
        const submitBtn = this.querySelector('[type="submit"]');
        if (submitBtn) submitBtn.disabled = true;
        try {
            const resp = await postJson(`${base}/otp/verify`,
                { email: getEmail(), code, returnUrl: getReturnUrl() }, this);
            const data = await resp.json();
            if (data.success) {
                window.location.href = data.redirectTo || getReturnUrl() || '/';
            } else {
                showOtpMsg(data.error || 'Invalid code. Please try again.', true);
            }
        } catch {
            showOtpMsg('Network error. Please try again.', true);
        } finally {
            if (submitBtn) submitBtn.disabled = false;
        }
    });

    // Passkey / WebAuthn
    formEl.querySelector('#pwl-btn-passkey')?.addEventListener('click', async function () {
        if (!window.PublicKeyCredential) {
            showMessage('Your browser does not support passkeys.', true);
            return;
        }
        showMessage('');
        this.disabled = true;
        try {
            const optResp = await fetch(`${base}/webauthn/signin/options`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email: getEmail() || null }),
            });
            if (!optResp.ok) { showMessage('Could not start passkey sign-in.', true); return; }

            const { ceremonyId, options } = await optResp.json();
            const publicKey = {
                ...options,
                challenge: b64uToBuffer(options.challenge),
                allowCredentials: (options.allowCredentials ?? []).map(c => ({
                    ...c,
                    id: b64uToBuffer(c.id),
                })),
            };

            const credential = await navigator.credentials.get({ publicKey });
            if (!credential) { showMessage('Passkey sign-in was cancelled.', true); return; }

            const completeResp = await fetch(`${base}/webauthn/signin/complete`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    ceremonyId,
                    assertion: {
                        id: bufferToB64u(credential.rawId),
                        rawId: bufferToB64u(credential.rawId),
                        type: credential.type,
                        response: {
                            authenticatorData: bufferToB64u(credential.response.authenticatorData),
                            clientDataJson: bufferToB64u(credential.response.clientDataJSON),
                            signature: bufferToB64u(credential.response.signature),
                            userHandle: credential.response.userHandle
                                ? bufferToB64u(credential.response.userHandle)
                                : null,
                        },
                        extensions: credential.getClientExtensionResults?.() ?? {},
                    },
                }),
            });

            if (completeResp.ok) {
                window.location.href = getReturnUrl() || '/';
            } else {
                showMessage('Passkey verification failed. Please try again.', true);
            }
        } catch (err) {
            showMessage(
                err.name === 'NotAllowedError'
                    ? 'Passkey sign-in was cancelled.'
                    : 'Passkey sign-in failed. Please try another method.',
                true);
        } finally {
            this.disabled = false;
        }
    });
}

function bufferToB64u(buf) {
    const bytes = new Uint8Array(buf);
    let s = '';
    for (const b of bytes) s += String.fromCharCode(b);
    return btoa(s).replace(/\+/g, '-').replace(/\//g, '_').replace(/=/g, '');
}

function b64uToBuffer(b64u) {
    const b64 = b64u.replace(/-/g, '+').replace(/_/g, '/');
    const bin = atob(b64);
    const buf = new Uint8Array(bin.length);
    for (let i = 0; i < bin.length; i++) buf[i] = bin.charCodeAt(i);
    return buf.buffer;
}
