/* ========================================================================
   إسناد / Telesak — Login + auth
   ======================================================================== */

(function () {
  'use strict';

  const form = document.getElementById('loginForm');
  const errorBox = document.getElementById('loginError');
  const togglePwd = document.getElementById('togglePwd');
  const passwordInput = document.getElementById('password');
  const loginBtn = document.getElementById('loginBtn');

  // Auto-fill demo creds on click
  document.querySelectorAll('.demo-cred').forEach(btn => {
    btn.addEventListener('click', () => {
      document.getElementById('domain').value = btn.dataset.d;
      document.getElementById('username').value = btn.dataset.u;
      document.getElementById('password').value = btn.dataset.p;
      passwordInput.focus();
    });
  });

  // Show/hide password
  togglePwd.addEventListener('click', () => {
    passwordInput.type = passwordInput.type === 'password' ? 'text' : 'password';
    togglePwd.textContent = passwordInput.type === 'password' ? '👁' : '🙈';
  });

  // Submit
  form.addEventListener('submit', (e) => {
    e.preventDefault();
    errorBox.hidden = true;

    const domain = document.getElementById('domain').value.trim().toLowerCase();
    const username = document.getElementById('username').value.trim().toLowerCase();
    const password = document.getElementById('password').value;

    if (!domain || !username || !password) {
      showError('يرجى تعبئة جميع الحقول');
      return;
    }

    loginBtn.disabled = true;
    loginBtn.querySelector('.btn__label').textContent = 'جاري التحقق...';

    setTimeout(() => {
      const state = Store.load();
      const account = state.accounts.find(a =>
        a.domain.toLowerCase() === domain &&
        a.username.toLowerCase() === username &&
        a.password === password
      );

      if (!account) {
        showError('بيانات الدخول غير صحيحة. تحقق من الدومين واسم المستخدم وكلمة المرور.');
        loginBtn.disabled = false;
        loginBtn.querySelector('.btn__label').textContent = 'دخول';
        return;
      }

      if (!account.active) {
        // Stopped account
        document.body.innerHTML = `
          <div class="stop-page">
            <div class="stop-card">
              <div class="stop-card__icon">⊘</div>
              <h2>تم إيقاف حسابك</h2>
              <p>عذراً، تم إيقاف حسابك من قبل المشرف.<br/>يرجى التواصل مع مدير الشركة لإعادة التفعيل.</p>
              <button class="btn btn--ghost" onclick="location.href='index.html'">العودة لتسجيل الدخول</button>
            </div>
          </div>
        `;
        return;
      }

      // Success → store session, redirect
      Session.set(account);
      loginBtn.querySelector('.btn__label').textContent = 'تم بنجاح ✓';
      loginBtn.style.background = 'var(--success)';
      setTimeout(() => { location.href = 'app.html'; }, 400);
    }, 600);
  });

  function showError(msg) {
    errorBox.textContent = msg;
    errorBox.hidden = false;
  }

  /* ----------------- Login chat widget ----------------- */
  const fab = document.getElementById('chatFab');
  const pop = document.getElementById('chatPop');
  const close = document.getElementById('chatClose');
  const body = document.getElementById('chatBody');

  fab.addEventListener('click', () => {
    pop.hidden = !pop.hidden;
    fab.style.transform = pop.hidden ? '' : 'rotate(45deg)';
  });
  close.addEventListener('click', () => {
    pop.hidden = true;
    fab.style.transform = '';
  });

  document.addEventListener('click', (e) => {
    const chip = e.target.closest('.chat-pop__chips .chip');
    if (!chip) return;
    const q = chip.dataset.q;
    const userMsg = chip.textContent;
    const answer = q === 'login'
      ? 'تأكد من إدخال <b>الدومين</b> و<b>اسم المستخدم</b> و<b>كلمة المرور</b> بشكل صحيح. ولاحظ أن الحقول حساسة للحروف الإنجليزية.<br/><br/>إذا استمرت المشكلة، تواصل مع مدير الشركة لإعادة تعيين كلمة المرور.'
      : 'لاستعادة كلمة المرور، اضغط على رابط <b>"نسيت كلمة المرور؟"</b> أسفل النموذج. سيتم إرسال رابط الاستعادة إلى بريدك المسجّل في النظام.';

    // Append user msg + bot reply
    chip.parentElement.remove();
    addMsg(userMsg, 'user');
    setTimeout(() => addMsg(answer, 'bot'), 400);
  });

  function addMsg(text, who) {
    const msg = document.createElement('div');
    msg.className = `chat-msg chat-msg--${who}`;
    msg.innerHTML = text;
    body.appendChild(msg);
    body.scrollTop = body.scrollHeight;
  }
})();
