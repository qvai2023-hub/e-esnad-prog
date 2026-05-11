/* ========================================================================
   إسناد / Telesak — Chat widget (full mode in app)
   ======================================================================== */

(function () {
  'use strict';

  const QA = [
    { triggers: ['قبول مهمة', 'كيف أقبل'], a: 'افتح المهمة من قسم "جديدة"، اقرأ التفاصيل ثم اضغط زر <b>"قبول" ✔</b>. ستنتقل المهمة تلقائياً إلى قسم "جاري العمل".' },
    { triggers: ['إنهاء مهمة', 'كيف أنهي'], a: 'افتح المهمة (جاري العمل)، أضف تعليق وأدخل الوقت المستغرق، ثم اضغط زر <b>"إنهاء" ✔</b>. ستنتقل المهمة إلى قسم "منتهية".' },
    { triggers: ['رفع ملف', 'مرفق', 'ارفق'], a: 'افتح المهمة → اضغط "المرفقات" → "ارفع الملف". الحد الأقصى: <b>4 ميغابايت</b>. الأنواع المسموحة: PDF, Word, Excel, PowerPoint, صور، MP4.' },
    { triggers: ['تأخير', 'متأخرة'], a: 'المهام التي تتجاوز تاريخ التسليم تنتقل تلقائياً إلى قسم "متأخرة" بلون أحمر. لا يزال بإمكانك إنهاؤها لكن سيُحسب التأخير في التقرير.' },
    { triggers: ['تقرير', 'استخراج'], a: 'افتح "التقارير" من القائمة الجانبية، اختر النوع، حدّد الفلاتر (الفترة، الموظف، الحالة)، ثم اضغط "استخراج التقرير". يمكن طباعته بصيغة PDF.' },
    { triggers: ['نسيت كلمة المرور', 'استعادة'], a: 'يمكنك استعادة كلمة المرور من رابط <b>"نسيت كلمة المرور"</b> في صفحة الدخول، أو التواصل مع مدير الشركة.' },
    { triggers: ['حضور', 'انصراف'], a: 'يتم تسجيل الحضور تلقائياً عند تسجيل الدخول، والانصراف عند الضغط على "تسجيل الخروج" أو إغلاق المتصفح.' },
    { triggers: ['تقويم', 'هجري', 'ميلادي'], a: 'يمكنك التبديل بين التقويم الهجري والميلادي من الزر في الشريط العلوي بجانب أيقونة الإشعارات.' }
  ];

  const QUICK = [
    { c: 'tasks', t: '✓ كيف أقبل مهمة؟', q: 'قبول مهمة' },
    { c: 'tasks', t: '🏁 كيف أنهي مهمة؟', q: 'إنهاء مهمة' },
    { c: 'files', t: '📎 رفع ملف', q: 'رفع ملف' },
    { c: 'files', t: '⏰ التأخير', q: 'تأخير' },
    { c: 'reports', t: '📊 استخراج تقرير', q: 'تقرير' },
    { c: 'login', t: '🔑 نسيت كلمة المرور', q: 'نسيت كلمة المرور' },
    { c: 'attend', t: '🕐 الحضور والانصراف', q: 'حضور' },
    { c: 'reports', t: '📅 التقويم الهجري', q: 'هجري' }
  ];

  const fab = document.getElementById('chatFab');
  const pop = document.getElementById('chatPop');
  const close = document.getElementById('chatClose');
  const body = document.getElementById('chatBody');
  if (!fab) return;

  let opened = false;

  fab.addEventListener('click', () => {
    pop.hidden = !pop.hidden;
    if (!opened) initChat();
    opened = true;
  });
  close.addEventListener('click', () => { pop.hidden = true; });

  function initChat() {
    body.innerHTML = `
      <div class="chat-msg chat-msg--bot">
        مرحباً 👋 أنا المساعد الذكي لإسناد. كيف يمكنني مساعدتك؟
      </div>
      <div class="chat-msg chat-msg--bot" style="background: transparent; border: 0; padding: 0;">
        <div style="font-size: 12px; color: var(--text-muted); margin-bottom: 8px;">أسئلة شائعة:</div>
        <div class="chat-pop__chips">
          ${QUICK.map(q => `<button class="chip" data-q="${q.q}">${q.t}</button>`).join('')}
        </div>
      </div>
    `;
  }

  document.addEventListener('click', (e) => {
    const chip = e.target.closest('.chat-pop__chips .chip');
    if (!chip || !chip.dataset.q) return;
    const userText = chip.textContent.replace(/^[^؀-ۿ]+/, '').trim();
    const ans = match(chip.dataset.q);
    addMsg(userText, 'user');
    setTimeout(() => addMsg(ans, 'bot'), 350);
  });

  function match(q) {
    const ql = q.toLowerCase();
    const found = QA.find(item => item.triggers.some(t => ql.includes(t.toLowerCase())));
    return found ? found.a : 'سؤال جيد! يمكنك التواصل مع الدعم الفني للحصول على إجابة تفصيلية.';
  }

  function addMsg(text, who) {
    const msg = document.createElement('div');
    msg.className = `chat-msg chat-msg--${who}`;
    msg.innerHTML = text;
    body.appendChild(msg);
    body.scrollTop = body.scrollHeight;
  }
})();
