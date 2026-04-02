/**
 * Telesak Chat Widget
 * Floating AI chat assistant for the Telesak task management system.
 * Self-contained: all styles injected via <style> tag.
 */
(function () {
    'use strict';

    // ─── Inject CSS ───
    var css =
        /* Floating button */
        '#tlsk-chat-btn{position:fixed;bottom:24px;left:24px;width:56px;height:56px;border-radius:50%;' +
        'background:#3D85C6;color:#fff;border:none;cursor:pointer;z-index:10000;' +
        'box-shadow:0 4px 12px rgba(61,133,198,.4);display:flex;align-items:center;justify-content:center;' +
        'transition:transform .2s,background .2s}' +
        '#tlsk-chat-btn:hover{transform:scale(1.1);background:#2d6ea8}' +
        '#tlsk-chat-btn svg{width:28px;height:28px;fill:#fff}' +

        /* Panel */
        '#tlsk-chat-panel{position:fixed;bottom:90px;left:24px;width:400px;height:540px;max-height:82vh;' +
        'background:#fff;border-radius:14px;box-shadow:0 8px 30px rgba(0,0,0,.2);z-index:10001;' +
        'display:none;flex-direction:column;overflow:hidden;direction:rtl;' +
        'font-family:"Segoe UI",Tahoma,"janna LT",sans-serif}' +
        '#tlsk-chat-panel.tlsk-open{display:flex}' +

        /* Header */
        '#tlsk-chat-header{background:#3D85C6;color:#fff;padding:12px 16px;display:flex;align-items:center;' +
        'justify-content:space-between;flex-shrink:0}' +
        '#tlsk-chat-header .tlsk-hdr-right{display:flex;align-items:center;gap:10px}' +
        '#tlsk-chat-header .tlsk-hdr-avatar{width:36px;height:36px;border-radius:50%;background:rgba(255,255,255,.25);' +
        'display:flex;align-items:center;justify-content:center;font-weight:700;font-size:16px}' +
        '#tlsk-chat-header .tlsk-hdr-info{display:flex;flex-direction:column}' +
        '#tlsk-chat-header .tlsk-title{font-size:15px;font-weight:700}' +
        '#tlsk-chat-header .tlsk-status{font-size:11px;opacity:.85;display:flex;align-items:center;gap:4px}' +
        '#tlsk-chat-header .tlsk-status-dot{width:7px;height:7px;border-radius:50%;background:#4ade80;display:inline-block}' +
        '#tlsk-chat-header .tlsk-hdr-actions{display:flex;align-items:center;gap:4px}' +
        '#tlsk-chat-header .tlsk-hdr-btn{background:none;border:none;color:#fff;cursor:pointer;padding:4px;' +
        'display:flex;align-items:center;justify-content:center;border-radius:4px;transition:background .15s}' +
        '#tlsk-chat-header .tlsk-hdr-btn:hover{background:rgba(255,255,255,.15)}' +
        '#tlsk-chat-header .tlsk-hdr-btn svg{width:18px;height:18px;fill:#fff}' +

        /* Messages area */
        '#tlsk-chat-messages{flex:1;overflow-y:auto;padding:12px 14px;display:flex;flex-direction:column;gap:10px;' +
        'background:#f5f5f5}' +

        /* Message rows */
        '.tlsk-msg-row{display:flex;gap:8px;align-items:flex-end}' +
        '.tlsk-msg-row-user{flex-direction:row-reverse}' +
        '.tlsk-msg-row-assistant{flex-direction:row}' +

        /* Bot avatar in messages */
        '.tlsk-bot-avatar{width:28px;height:28px;border-radius:50%;background:#3D85C6;color:#fff;' +
        'display:flex;align-items:center;justify-content:center;font-size:13px;font-weight:700;flex-shrink:0}' +

        /* Message bubbles */
        '.tlsk-msg-wrap{display:flex;flex-direction:column;max-width:80%}' +
        '.tlsk-msg{padding:10px 14px;border-radius:12px;font-size:13.5px;line-height:1.7;' +
        'word-wrap:break-word;white-space:pre-wrap}' +
        '.tlsk-msg-user{background:#3D85C6;color:#fff;border-bottom-right-radius:4px}' +
        '.tlsk-msg-assistant{background:#fff;color:#222;border:1px solid #e5e5e5;border-bottom-left-radius:4px}' +
        '.tlsk-msg-error{background:#f8d7da;color:#721c24;border-bottom-left-radius:4px}' +

        /* Welcome card */
        '.tlsk-welcome-card{background:#fff;border:1px solid #e5e5e5;border-radius:12px;padding:16px;' +
        'font-size:13.5px;line-height:1.8;text-align:right}' +

        /* Support button */
        '.tlsk-support-btn{display:inline-flex;align-items:center;gap:6px;border:1px solid #a3c4e8;' +
        'border-radius:20px;padding:6px 16px;background:#fff;color:#2d6ea8;font-size:13px;cursor:pointer;' +
        'margin-top:8px;transition:background .15s}' +
        '.tlsk-support-btn:hover{background:#e8f2fb}' +
        '.tlsk-support-btn svg{width:18px;height:18px;fill:#D07311}' +

        /* Timestamp */
        '.tlsk-time{font-size:11px;color:#999;margin-top:2px;text-align:left;direction:ltr}' +

        /* Category tabs */
        '#tlsk-chat-tabs{display:flex;gap:6px;padding:8px 14px;overflow-x:auto;flex-shrink:0;' +
        'background:#f5f5f5;border-top:1px solid #eee;direction:rtl}' +
        '#tlsk-chat-tabs::-webkit-scrollbar{display:none}' +
        '.tlsk-tab{background:#fff;border:1px solid #ddd;color:#555;border-radius:16px;padding:5px 14px;' +
        'font-size:12px;cursor:pointer;white-space:nowrap;transition:background .15s,color .15s,border-color .15s}' +
        '.tlsk-tab:hover{border-color:#a3c4e8;color:#3D85C6}' +
        '.tlsk-tab.active{background:#3D85C6;color:#fff;border-color:#3D85C6}' +

        /* Quick buttons (chips) */
        '#tlsk-chat-chips{padding:8px 14px;display:flex;flex-wrap:wrap;gap:6px;background:#f5f5f5;' +
        'max-height:140px;overflow-y:auto}' +
        '.tlsk-chip{background:#fff;border:1px solid #a3c4e8;color:#333;border-radius:18px;padding:6px 14px;' +
        'font-size:12.5px;cursor:pointer;transition:background .15s,border-color .15s;white-space:nowrap;' +
        'display:inline-flex;align-items:center;gap:4px}' +
        '.tlsk-chip:hover{background:#e8f2fb;border-color:#3D85C6}' +
        '.tlsk-chip .tlsk-chip-icon{font-size:14px}' +

        /* Input area */
        '#tlsk-chat-input-area{display:flex;align-items:center;padding:10px 12px;border-top:1px solid #e0e0e0;' +
        'background:#fff;gap:8px;flex-shrink:0}' +
        '#tlsk-chat-input{flex:1;border:1px solid #ccc;border-radius:20px;padding:8px 14px;font-size:13.5px;' +
        'outline:none;direction:rtl;text-align:right;resize:none;max-height:72px;line-height:1.4;font-family:inherit}' +
        '#tlsk-chat-input:focus{border-color:#3D85C6}' +
        '#tlsk-chat-send{background:#3D85C6;color:#fff;border:none;border-radius:50%;width:38px;height:38px;' +
        'cursor:pointer;display:flex;align-items:center;justify-content:center;flex-shrink:0;' +
        'transition:opacity .15s,background .15s}' +
        '#tlsk-chat-send:hover{background:#2d6ea8}' +
        '#tlsk-chat-send:disabled{opacity:.5;cursor:default}' +
        '#tlsk-chat-send svg{width:18px;height:18px;fill:#fff;transform:scaleX(-1)}' +

        /* Typing indicator */
        '.tlsk-typing{display:flex;gap:4px;align-items:center;padding:10px 14px;' +
        'background:#fff;border:1px solid #e5e5e5;border-radius:12px;border-bottom-left-radius:4px}' +
        '.tlsk-typing span{width:7px;height:7px;background:#bbb;border-radius:50%;animation:tlsk-bounce .6s infinite alternate}' +
        '.tlsk-typing span:nth-child(2){animation-delay:.2s}' +
        '.tlsk-typing span:nth-child(3){animation-delay:.4s}' +
        '@keyframes tlsk-bounce{to{opacity:.3;transform:translateY(-4px)}}' +

        /* Responsive */
        '@media(max-width:480px){' +
        '#tlsk-chat-panel{width:calc(100% - 20px);left:10px;right:10px;bottom:84px;height:70vh}' +
        '#tlsk-chat-btn{bottom:16px;left:16px;width:50px;height:50px}' +
        '#tlsk-chat-btn svg{width:24px;height:24px}}';

    var styleEl = document.createElement('style');
    styleEl.textContent = css;
    document.head.appendChild(styleEl);

    // ─── Mode Detection ───
    var MODE = window.tlskChatMode || 'login'; // 'login' = limited, 'full' = all features
    var isLoginMode = MODE === 'login';

    // ─── Login mode: hardcoded Q&A ───
    var loginQA = {
        'مشكلة في تسجيل الدخول': 'تأكد من عدم وجود مسافات باسم المستخدم أو كلمة المرور، أو أدخل بياناتك يدوياً',
        'نسيت كلمة المرور؟': 'تواصل معنا على الواتساب لإعادة إرسال بيانات الدخول'
    };

    var loginButtons = [
        { text: 'مشكلة في تسجيل الدخول' },
        { text: 'نسيت كلمة المرور؟' }
    ];

    // ─── Icons ───
    var chatIconSvg = '<svg viewBox="0 0 24 24"><path d="M20 2H4c-1.1 0-2 .9-2 2v18l4-4h14c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2z"/></svg>';
    var closeIconSvg = '<svg viewBox="0 0 24 24"><path d="M19 6.41L17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z"/></svg>';
    var sendIconSvg = '<svg viewBox="0 0 24 24"><path d="M2.01 21L23 12 2.01 3 2 10l15 2-15 2z"/></svg>';
    var resetIconSvg = '<svg viewBox="0 0 24 24"><path d="M17.65 6.35A7.958 7.958 0 0012 4c-4.42 0-7.99 3.58-7.99 8s3.57 8 7.99 8c3.73 0 6.84-2.55 7.73-6h-2.08A5.99 5.99 0 0112 18c-3.31 0-6-2.69-6-6s2.69-6 6-6c1.66 0 3.14.69 4.22 1.78L13 11h7V4l-2.35 2.35z"/></svg>';
    var supportIconSvg = '<svg viewBox="0 0 24 24"><path d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347z"/><path d="M12 2C6.477 2 2 6.477 2 12c0 1.89.525 3.66 1.438 5.168L2 22l4.832-1.438A9.955 9.955 0 0012 22c5.523 0 10-4.477 10-10S17.523 2 12 2z"/></svg>';

    // ─── Quick buttons data ───
    var quickButtons = [
        { text: 'قبول المهمة', cat: 'tasks' },
        { text: 'إنهاء المهمة', cat: 'tasks' },
        { text: 'ما المطلوب مني؟', cat: 'tasks' },
        { text: 'الوقت المستغرق', cat: 'tasks' },
        { text: 'لماذا تظهر المهمة متأخرة؟', cat: 'tasks' },
        { text: 'مهمة لا تظهر', cat: 'tasks' },
        { text: 'الاستبيان', cat: 'tasks' },
        { text: 'رفع ملف', cat: 'files' },
        { text: 'ملف داخل المهمة', cat: 'files' },
        { text: 'إنشاء ملف Word', cat: 'files' },
        { text: 'تسجيل الحضور', cat: 'attendance' },
        { text: 'تسجيل الانصراف', cat: 'attendance' },
        { text: 'تقرير المهام', cat: 'reports' },
        { text: 'فيديو الشرح', cat: 'reports' }
    ];

    var categories = [
        { id: 'all', label: 'الكل' },
        { id: 'tasks', label: 'المهام' },
        { id: 'files', label: 'الملفات' },
        { id: 'attendance', label: 'الحضور' },
        { id: 'reports', label: 'التقارير' }
    ];

    // ─── State ───
    var history = [];
    var activeCategory = 'all';
    var isOpen = false;

    // ─── Build HTML ───
    var btn = document.createElement('button');
    btn.id = 'tlsk-chat-btn';
    btn.title = 'المساعد الذكي';
    btn.innerHTML = chatIconSvg;

    var panel = document.createElement('div');
    panel.id = 'tlsk-chat-panel';
    panel.innerHTML =
        '<div id="tlsk-chat-header">' +
            '<div class="tlsk-hdr-right">' +
                '<div class="tlsk-hdr-avatar">ت</div>' +
                '<div class="tlsk-hdr-info">' +
                    '<span class="tlsk-title">المساعد الذكي</span>' +
                    '<span class="tlsk-status"><span class="tlsk-status-dot"></span> متاح الآن · يرد فوراً</span>' +
                '</div>' +
            '</div>' +
            '<div class="tlsk-hdr-actions">' +
                '<button class="tlsk-hdr-btn tlsk-reset" title="محادثة جديدة">' + resetIconSvg + '</button>' +
                '<button class="tlsk-hdr-btn tlsk-close" title="إغلاق">' + closeIconSvg + '</button>' +
            '</div>' +
        '</div>' +
        '<div id="tlsk-chat-messages"></div>' +
        '<div id="tlsk-chat-tabs"></div>' +
        '<div id="tlsk-chat-chips"></div>' +
        '<div id="tlsk-chat-input-area">' +
            '<textarea id="tlsk-chat-input" rows="1" placeholder="اكتب سؤالك هنا..."></textarea>' +
            '<button id="tlsk-chat-send" title="إرسال">' + sendIconSvg + '</button>' +
        '</div>';

    document.body.appendChild(btn);
    document.body.appendChild(panel);

    // ─── jQuery references ───
    var $panel = $('#tlsk-chat-panel');
    var $messages = $('#tlsk-chat-messages');
    var $tabs = $('#tlsk-chat-tabs');
    var $chips = $('#tlsk-chat-chips');
    var $input = $('#tlsk-chat-input');
    var $sendBtn = $('#tlsk-chat-send');

    // ─── Helpers ───
    function getTimeStr() {
        var now = new Date();
        var h = now.getHours();
        var m = now.getMinutes();
        return (h < 10 ? '0' : '') + h + ':' + (m < 10 ? '0' : '') + m;
    }

    function scrollToBottom() {
        var el = $messages[0];
        el.scrollTop = el.scrollHeight;
    }

    function trimHistory() {
        while (history.length > 6) {
            history.shift();
        }
    }

    // ─── Render: Welcome message ───
    function showWelcome() {
        $messages.empty();
        var time = getTimeStr();
        var welcomeText = isLoginMode
            ? 'مرحباً!<br><br>هل تواجه مشكلة في تسجيل الدخول؟ اختر من الأسئلة أدناه.'
            : 'مرحباً! أنا مساعدك الذكي في تلي ساك<br><br>يمكنني مساعدتك في متابعة مهامك، تسجيل حضورك،<br>والاطلاع على تقاريرك وغير ذلك.<br>اختر من الأسئلة الشائعة أدناه أو تواصل مع الدعم.';
        var supportBtn = '<button class="tlsk-support-btn">' + supportIconSvg + ' تواصل مع الدعم</button>';
        var html =
            '<div class="tlsk-msg-row tlsk-msg-row-assistant">' +
                '<div class="tlsk-bot-avatar">ت</div>' +
                '<div class="tlsk-msg-wrap">' +
                    '<div class="tlsk-welcome-card">' + welcomeText + '</div>' +
                    supportBtn +
                    '<div class="tlsk-time">' + time + '</div>' +
                '</div>' +
            '</div>';
        $messages.append(html);
    }

    // ─── Render: Category tabs ───
    function renderTabs() {
        var html = '';
        for (var i = 0; i < categories.length; i++) {
            var c = categories[i];
            var cls = c.id === activeCategory ? 'tlsk-tab active' : 'tlsk-tab';
            html += '<span class="' + cls + '" data-cat="' + c.id + '">' + c.label + '</span>';
        }
        $tabs.html(html);
    }

    // ─── Render: Quick buttons ───
    function renderChips() {
        var html = '';
        var buttons = isLoginMode ? loginButtons : quickButtons;
        for (var i = 0; i < buttons.length; i++) {
            var b = buttons[i];
            if (!isLoginMode && activeCategory !== 'all' && b.cat !== activeCategory) continue;
            html += '<span class="tlsk-chip" data-text="' + b.text + '">' + b.text + '</span>';
        }
        $chips.html(html);
    }

    // ─── Append message ───
    function appendMessage(role, text) {
        var time = getTimeStr();
        var html = '';

        if (role === 'user') {
            html = '<div class="tlsk-msg-row tlsk-msg-row-user">' +
                       '<div class="tlsk-msg-wrap">' +
                           '<div class="tlsk-msg tlsk-msg-user"></div>' +
                       '</div>' +
                   '</div>';
            var $row = $(html);
            $row.find('.tlsk-msg').text(text);
            $messages.append($row);
        } else {
            var msgClass = role === 'error' ? 'tlsk-msg-error' : 'tlsk-msg-assistant';
            html = '<div class="tlsk-msg-row tlsk-msg-row-assistant">' +
                       '<div class="tlsk-bot-avatar">ت</div>' +
                       '<div class="tlsk-msg-wrap">' +
                           '<div class="tlsk-msg ' + msgClass + '"></div>' +
                           '<div class="tlsk-time">' + time + '</div>' +
                       '</div>' +
                   '</div>';
            var $row2 = $(html);
            $row2.find('.tlsk-msg').text(text);
            $messages.append($row2);
        }
        scrollToBottom();
    }

    // ─── Reset chat ───
    function resetChat() {
        history = [];
        activeCategory = 'all';
        showWelcome();
        if (!isLoginMode) { renderTabs(); $tabs.show(); }
        renderChips();
        $chips.show();
        scrollToBottom();
    }

    // ─── Init ───
    showWelcome();
    if (isLoginMode) {
        $tabs.hide();
    } else {
        renderTabs();
    }
    $('#tlsk-chat-input-area').hide();
    renderChips();

    // ─── Events: Toggle panel ───
    btn.addEventListener('click', function () {
        isOpen = !isOpen;
        $panel.toggleClass('tlsk-open', isOpen);
        btn.innerHTML = isOpen ? closeIconSvg : chatIconSvg;
        if (isOpen) {
            $input.focus();
            scrollToBottom();
        }
    });

    $panel.on('click', '.tlsk-close', function () {
        isOpen = false;
        $panel.removeClass('tlsk-open');
        btn.innerHTML = chatIconSvg;
    });

    // ─── Events: Reset ───
    $panel.on('click', '.tlsk-reset', function () {
        resetChat();
    });

    // ─── Events: Tabs ───
    $tabs.on('click', '.tlsk-tab', function () {
        activeCategory = $(this).data('cat');
        renderTabs();
        renderChips();
    });

    // ─── Events: Quick buttons ───
    $chips.on('click', '.tlsk-chip', function () {
        var text = $(this).data('text');
        $input.val(text);
        sendMessage();
    });

    // ─── Events: Send ───
    $sendBtn.on('click', function () {
        sendMessage();
    });

    $input.on('keydown', function (e) {
        if (e.which === 13 && !e.shiftKey) {
            e.preventDefault();
            sendMessage();
        }
    });

    // Auto-resize textarea
    $input.on('input', function () {
        this.style.height = 'auto';
        this.style.height = Math.min(this.scrollHeight, 72) + 'px';
    });

    // ─── Send message ───
    function sendMessage() {
        var text = $.trim($input.val());
        if (!text) return;

        // Hide tabs and chips after first message
        $tabs.hide();
        $chips.hide();

        appendMessage('user', text);
        $input.val('').trigger('input');

        // ─── Login mode: local answers only ───
        if (isLoginMode) {
            if (text === 'مشكلة في تسجيل الدخول') {
                // Step 1: show intermediate question + [نعم] button
                appendMessage('assistant', 'مرحبا!\nهل تواجه مشكلة في تسجيل الدخول؟');
                var $yesBtn = $('<div style="margin-top:6px;"><span class="tlsk-chip tlsk-yes-btn" data-answer="تأكد من عدم وجود مسافات باسم المستخدم أو كلمة المرور، أو أدخل بياناتك يدوياً">نعم</span></div>');
                $messages.append($yesBtn);
                scrollToBottom();
                return;
            }
            // Other login Q&A
            var answer = loginQA[text];
            if (!answer) {
                for (var key in loginQA) {
                    if (text.indexOf(key) !== -1 || key.indexOf(text) !== -1) {
                        answer = loginQA[key];
                        break;
                    }
                }
            }
            if (answer) {
                appendMessage('assistant', answer);
            } else {
                appendMessage('assistant', 'للمساعدة في تسجيل الدخول، اختر أحد الأسئلة أعلاه أو تواصل مع الدعم.');
            }
            renderChips();
            $chips.show();
            return;
        }

        // ─── Full mode: server call ───
        $sendBtn.prop('disabled', true);

        history.push({ role: 'user', content: text });
        trimHistory();

        // Typing indicator
        var $typing = $(
            '<div class="tlsk-msg-row tlsk-msg-row-assistant">' +
                '<div class="tlsk-bot-avatar">ت</div>' +
                '<div class="tlsk-typing"><span></span><span></span><span></span></div>' +
            '</div>'
        );
        $messages.append($typing);
        scrollToBottom();

        // Headers
        var headers = { 'Content-Type': 'application/json' };
        var token = $('input[name="__RequestVerificationToken"]').val();
        if (token) {
            headers['RequestVerificationToken'] = token;
        }

        $.ajax({
            url: '/Chat/Send',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ message: text, history: history.slice(0, -1) }),
            headers: headers,
            dataType: 'json',
            success: function (resp) {
                $typing.remove();
                if (resp.success) {
                    appendMessage('assistant', resp.answer);
                    history.push({ role: 'assistant', content: resp.answer });
                    trimHistory();
                } else {
                    appendMessage('error', resp.answer || 'حدث خطأ غير متوقع.');
                }
            },
            error: function () {
                $typing.remove();
                appendMessage('error', 'تعذر الاتصال بالخادم. يرجى المحاولة لاحقاً.');
            },
            complete: function () {
                $sendBtn.prop('disabled', false);
                $input.focus();
            }
        });
    }

    // ─── Support button → WhatsApp (optional) ───
    $messages.on('click', '.tlsk-support-btn', function () {
        window.open('https://wa.me/966568786846', '_blank');
    });

    // ─── Login mode: [نعم] button handler ───
    $messages.on('click', '.tlsk-yes-btn', function () {
        var answer = $(this).data('answer');
        $(this).parent().remove();
        appendMessage('assistant', answer);
        renderChips();
        $chips.show();
    });

})();
