/**
 * Telesak Chat Widget
 * Floating AI chat assistant for the Telesak task management system.
 * Self-contained: all styles injected via <style> tag.
 */
(function () {
    'use strict';

    // ─── Inject CSS ───
    var css =
        '#tlsk-chat-btn{position:fixed;bottom:24px;left:24px;width:56px;height:56px;border-radius:50%;' +
        'background:#5c9b30;color:#fff;border:none;cursor:pointer;z-index:10000;box-shadow:0 4px 12px rgba(0,0,0,.25);' +
        'display:flex;align-items:center;justify-content:center;transition:transform .2s}' +
        '#tlsk-chat-btn:hover{transform:scale(1.1)}' +
        '#tlsk-chat-btn svg{width:28px;height:28px;fill:#fff}' +

        '#tlsk-chat-panel{position:fixed;bottom:90px;left:24px;width:380px;height:500px;max-height:80vh;' +
        'background:#fff;border-radius:12px;box-shadow:0 8px 30px rgba(0,0,0,.2);z-index:10001;' +
        'display:none;flex-direction:column;overflow:hidden;direction:rtl;font-family:"Segoe UI",Tahoma,"janna LT",sans-serif}' +
        '#tlsk-chat-panel.tlsk-open{display:flex}' +

        '#tlsk-chat-header{background:#5c9b30;color:#fff;padding:12px 16px;display:flex;align-items:center;' +
        'justify-content:space-between;flex-shrink:0}' +
        '#tlsk-chat-header .tlsk-title{font-size:15px;font-weight:700}' +
        '#tlsk-chat-header .tlsk-close{background:none;border:none;color:#fff;font-size:22px;cursor:pointer;' +
        'line-height:1;padding:0 4px}' +

        '#tlsk-chat-messages{flex:1;overflow-y:auto;padding:12px 14px;display:flex;flex-direction:column;gap:8px;' +
        'background:#f5f5f5}' +

        '.tlsk-msg{max-width:82%;padding:10px 14px;border-radius:12px;font-size:13.5px;line-height:1.6;' +
        'word-wrap:break-word;white-space:pre-wrap}' +
        '.tlsk-msg-user{background:#5c9b30;color:#fff;align-self:flex-start;border-bottom-right-radius:4px}' +
        '.tlsk-msg-assistant{background:#e9e9e9;color:#222;align-self:flex-end;border-bottom-left-radius:4px}' +
        '.tlsk-msg-error{background:#f8d7da;color:#721c24;align-self:flex-end;border-bottom-left-radius:4px}' +

        '#tlsk-chat-chips{padding:8px 14px;display:flex;flex-wrap:wrap;gap:6px;background:#f5f5f5}' +
        '.tlsk-chip{background:#fff;border:1px solid #5c9b30;color:#5c9b30;border-radius:16px;padding:5px 14px;' +
        'font-size:12.5px;cursor:pointer;transition:background .15s,color .15s;white-space:nowrap}' +
        '.tlsk-chip:hover{background:#5c9b30;color:#fff}' +

        '#tlsk-chat-input-area{display:flex;align-items:center;padding:10px 12px;border-top:1px solid #e0e0e0;' +
        'background:#fff;gap:8px;flex-shrink:0}' +
        '#tlsk-chat-input{flex:1;border:1px solid #ccc;border-radius:20px;padding:8px 14px;font-size:13.5px;' +
        'outline:none;direction:rtl;text-align:right;resize:none;max-height:72px;line-height:1.4;' +
        'font-family:inherit}' +
        '#tlsk-chat-input:focus{border-color:#5c9b30}' +
        '#tlsk-chat-send{background:#5c9b30;color:#fff;border:none;border-radius:50%;width:36px;height:36px;' +
        'cursor:pointer;display:flex;align-items:center;justify-content:center;flex-shrink:0;transition:opacity .15s}' +
        '#tlsk-chat-send:disabled{opacity:.5;cursor:default}' +
        '#tlsk-chat-send svg{width:18px;height:18px;fill:#fff;transform:scaleX(-1)}' +

        '.tlsk-typing{display:flex;gap:4px;align-items:center;align-self:flex-end;padding:8px 14px;' +
        'background:#e9e9e9;border-radius:12px;border-bottom-left-radius:4px}' +
        '.tlsk-typing span{width:7px;height:7px;background:#999;border-radius:50%;animation:tlsk-bounce .6s infinite alternate}' +
        '.tlsk-typing span:nth-child(2){animation-delay:.2s}' +
        '.tlsk-typing span:nth-child(3){animation-delay:.4s}' +
        '@keyframes tlsk-bounce{to{opacity:.3;transform:translateY(-4px)}}' +

        '@media(max-width:480px){' +
        '#tlsk-chat-panel{width:calc(100% - 20px);left:10px;right:10px;bottom:84px;height:60vh}' +
        '#tlsk-chat-btn{bottom:16px;left:16px;width:50px;height:50px}' +
        '#tlsk-chat-btn svg{width:24px;height:24px}}';

    var styleEl = document.createElement('style');
    styleEl.textContent = css;
    document.head.appendChild(styleEl);

    // ─── Build HTML ───
    var chatIcon = '<svg viewBox="0 0 24 24"><path d="M20 2H4c-1.1 0-2 .9-2 2v18l4-4h14c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2z"/></svg>';
    var sendIcon = '<svg viewBox="0 0 24 24"><path d="M2.01 21L23 12 2.01 3 2 10l15 2-15 2z"/></svg>';

    var btn = document.createElement('button');
    btn.id = 'tlsk-chat-btn';
    btn.title = 'المساعد الذكي';
    btn.innerHTML = chatIcon;

    var panel = document.createElement('div');
    panel.id = 'tlsk-chat-panel';
    panel.innerHTML =
        '<div id="tlsk-chat-header">' +
            '<span class="tlsk-title">المساعد الذكي</span>' +
            '<button class="tlsk-close" title="إغلاق">&times;</button>' +
        '</div>' +
        '<div id="tlsk-chat-messages"></div>' +
        '<div id="tlsk-chat-chips"></div>' +
        '<div id="tlsk-chat-input-area">' +
            '<textarea id="tlsk-chat-input" rows="1" placeholder="اكتب سؤالك هنا..."></textarea>' +
            '<button id="tlsk-chat-send" title="إرسال">' + sendIcon + '</button>' +
        '</div>';

    document.body.appendChild(btn);
    document.body.appendChild(panel);

    // ─── References ───
    var $panel = $('#tlsk-chat-panel');
    var $messages = $('#tlsk-chat-messages');
    var $chips = $('#tlsk-chat-chips');
    var $input = $('#tlsk-chat-input');
    var $sendBtn = $('#tlsk-chat-send');

    // ─── State ───
    var history = []; // { role, content } — keep last 6

    // ─── Suggestion chips ───
    var suggestions = [
        'كيف أقبل المهمة؟',
        'كيف أنهي المهمة؟',
        'أين أرفع الملف؟',
        'نسيت كلمة المرور'
    ];

    function renderChips() {
        var html = '';
        for (var i = 0; i < suggestions.length; i++) {
            html += '<span class="tlsk-chip" data-text="' + suggestions[i] + '">' + suggestions[i] + '</span>';
        }
        $chips.html(html);
    }
    renderChips();

    $chips.on('click', '.tlsk-chip', function () {
        var text = $(this).data('text');
        $input.val(text);
        sendMessage();
    });

    // ─── Toggle panel ───
    btn.addEventListener('click', function () {
        $panel.toggleClass('tlsk-open');
        if ($panel.hasClass('tlsk-open')) {
            $input.focus();
        }
    });

    $panel.on('click', '.tlsk-close', function () {
        $panel.removeClass('tlsk-open');
    });

    // ─── Send ───
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

    function sendMessage() {
        var text = $.trim($input.val());
        if (!text) return;

        // Hide chips after first message
        $chips.hide();

        // Show user message
        appendMessage('user', text);
        $input.val('').trigger('input');
        $sendBtn.prop('disabled', true);

        // Add to history
        history.push({ role: 'user', content: text });
        trimHistory();

        // Show typing indicator
        var $typing = $('<div class="tlsk-typing"><span></span><span></span><span></span></div>');
        $messages.append($typing);
        scrollToBottom();

        // Build headers
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

    function appendMessage(role, text) {
        var cls = role === 'user' ? 'tlsk-msg-user' : (role === 'error' ? 'tlsk-msg-error' : 'tlsk-msg-assistant');
        var $msg = $('<div class="tlsk-msg ' + cls + '"></div>').text(text);
        $messages.append($msg);
        scrollToBottom();
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

})();
