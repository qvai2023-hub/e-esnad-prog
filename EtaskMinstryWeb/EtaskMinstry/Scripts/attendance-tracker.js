/**
 * Attendance Tracker - Automatic Check-out System
 * Handles browser close, tab close, and inactivity detection
 *
 * Features:
 * - Layer 1: sendBeacon for browser/tab close (~80% coverage)
 * - Layer 2: Inactivity detection with modal warning (~10% coverage)
 * - Layer 3: Server-side job handles remaining cases (~5%)
 */

(function () {
    'use strict';

    // Configuration
    var CONFIG = {
        HEARTBEAT_INTERVAL: 60000,        // 1 minute - send heartbeat
        INACTIVITY_WARNING: 30 * 60000,   // 30 minutes - show warning
        INACTIVITY_TIMEOUT: 15 * 60000,   // 15 minutes after warning - auto checkout
        ACTIVITY_EVENTS: ['click', 'keypress', 'mousemove', 'scroll', 'touchstart']
    };

    // State
    var state = {
        attendanceId: null,
        lastActivityTime: new Date(),
        heartbeatTimer: null,
        inactivityTimer: null,
        timeoutTimer: null,
        countdownInterval: null,
        isModalShown: false,
        isInitialized: false,
        isCheckingOut: false
    };

    // URLs
    var URLS = {
        logActivity: '/Attendance/LogActivity',
        beaconCheckout: '/Attendance/BeaconCheckout',
        inactivityCheckout: '/Attendance/InactivityCheckout',
        getCurrentAttendance: '/Attendance/GetCurrentAttendance'
    };

    /**
     * Initialize the tracker
     */
    function init() {
        if (state.isInitialized) return;

        // Check if user has active attendance
        checkCurrentAttendance(function (hasAttendance) {
            if (!hasAttendance) {
                console.log('[AttendanceTracker] No active attendance, tracker disabled');
                return;
            }

            console.log('[AttendanceTracker] Initialized with attendance ID:', state.attendanceId);
            state.isInitialized = true;

            // Setup event listeners
            setupActivityTracking();
            setupBeaconCheckout();
            setupHeartbeat();
            setupInactivityDetection();
        });
    }

    /**
     * Check if user has current active attendance
     */
    function checkCurrentAttendance(callback) {
        $.ajax({
            url: URLS.getCurrentAttendance,
            type: 'GET',
            dataType: 'json',
            success: function (response) {
                if (response.hasAttendance) {
                    state.attendanceId = response.attendanceId;
                    callback(true);
                } else {
                    callback(false);
                }
            },
            error: function () {
                callback(false);
            }
        });
    }

    /**
     * Setup activity tracking (click, keypress, etc.)
     */
    function setupActivityTracking() {
        var throttledUpdate = throttle(updateLastActivity, 5000); // Max once per 5 seconds

        CONFIG.ACTIVITY_EVENTS.forEach(function (eventType) {
            document.addEventListener(eventType, throttledUpdate, { passive: true });
        });
    }

    /**
     * Update last activity time
     */
    function updateLastActivity() {
        state.lastActivityTime = new Date();

        // Reset inactivity timer
        resetInactivityTimer();

        // Hide modal if shown
        if (state.isModalShown) {
            hideInactivityModal();
        }
    }

    /**
     * Setup sendBeacon for browser/tab close
     * This is the most reliable method for catching closes
     */
    function setupBeaconCheckout() {
        // beforeunload - fires when page is about to unload
        window.addEventListener('beforeunload', function () {
            sendBeaconCheckout();
        });

        // pagehide - more reliable on mobile
        window.addEventListener('pagehide', function () {
            sendBeaconCheckout();
        });
    }

    /**
     * Send beacon checkout request
     * Works even during page unload
     */
    function sendBeaconCheckout() {
        if (!state.attendanceId) return;

        var data = new FormData();
        data.append('attendanceId', state.attendanceId);
        data.append('lastActivityTime', state.lastActivityTime.toISOString());

        // navigator.sendBeacon is fire-and-forget, works during unload
        if (navigator.sendBeacon) {
            navigator.sendBeacon(URLS.beaconCheckout, data);
        } else {
            // Fallback for older browsers - synchronous XHR
            var xhr = new XMLHttpRequest();
            xhr.open('POST', URLS.beaconCheckout, false); // false = synchronous
            xhr.send(data);
        }
    }

    /**
     * Setup heartbeat to log activity periodically
     */
    function setupHeartbeat() {
        state.heartbeatTimer = setInterval(function () {
            logActivity('heartbeat');
        }, CONFIG.HEARTBEAT_INTERVAL);
    }

    /**
     * Log activity to server
     */
    function logActivity(activityType) {
        $.ajax({
            url: URLS.logActivity,
            type: 'POST',
            data: { activityType: activityType },
            dataType: 'json',
            success: function (response) {
                if (response.success && response.attendanceId) {
                    state.attendanceId = response.attendanceId;
                }
            }
        });
    }

    /**
     * Setup inactivity detection
     */
    function setupInactivityDetection() {
        resetInactivityTimer();
    }

    /**
     * Reset inactivity timer
     */
    function resetInactivityTimer() {
        // Clear existing timers
        if (state.inactivityTimer) clearTimeout(state.inactivityTimer);
        if (state.timeoutTimer) clearTimeout(state.timeoutTimer);

        // Set new inactivity timer
        state.inactivityTimer = setTimeout(function () {
            showInactivityModal();
        }, CONFIG.INACTIVITY_WARNING);
    }

    /**
     * Show inactivity warning modal
     */
    function showInactivityModal() {
        if (state.isModalShown) return;

        // First check if attendance is still active (might have been checked out by beacon)
        checkCurrentAttendance(function (hasAttendance) {
            if (!hasAttendance) {
                console.log('[AttendanceTracker] No active attendance, stopping tracker');
                cleanup();
                return;
            }

            state.isModalShown = true;

            // Create modal if not exists
            var modal = document.getElementById('inactivityModal');
            if (!modal) {
                modal = createInactivityModal();
                document.body.appendChild(modal);
            }

            // Show modal
            $(modal).modal('show');

            // Start countdown (handles checkout when it reaches 0)
            startCountdown();
        });
    }

    /**
     * Create inactivity modal HTML
     */
    function createInactivityModal() {
        var modal = document.createElement('div');
        modal.id = 'inactivityModal';
        modal.className = 'modal fade';
        modal.setAttribute('tabindex', '-1');
        modal.setAttribute('role', 'dialog');
        modal.setAttribute('data-backdrop', 'static');
        modal.setAttribute('data-keyboard', 'false');

        modal.innerHTML =
            '<div class="modal-dialog modal-dialog-centered" role="document">' +
                '<div class="modal-content">' +
                    '<div class="modal-header bg-warning">' +
                        '<h5 class="modal-title">' +
                            '<i class="fa fa-clock-o"></i> ' +
                            'تنبيه عدم النشاط' +
                        '</h5>' +
                    '</div>' +
                    '<div class="modal-body text-center">' +
                        '<p class="lead">هل أنهيت عملك لليوم؟</p>' +
                        '<p>لم يتم اكتشاف أي نشاط منذ فترة.</p>' +
                        '<p>سيتم تسجيل خروجك تلقائياً خلال: <strong id="inactivityCountdown">15:00</strong></p>' +
                    '</div>' +
                    '<div class="modal-footer justify-content-center">' +
                        '<button type="button" class="btn btn-success btn-lg" id="btnContinueWork">' +
                            '<i class="fa fa-check"></i> استمر في العمل' +
                        '</button>' +
                        '<button type="button" class="btn btn-danger btn-lg" id="btnEndWork">' +
                            '<i class="fa fa-sign-out"></i> أنهِ العمل' +
                        '</button>' +
                    '</div>' +
                '</div>' +
            '</div>';

        // Event handlers
        modal.querySelector('#btnContinueWork').addEventListener('click', function () {
            hideInactivityModal();
            updateLastActivity();
            logActivity('continue_work');
        });

        modal.querySelector('#btnEndWork').addEventListener('click', function () {
            performInactivityCheckout();
        });

        return modal;
    }

    /**
     * Start countdown timer in modal
     */
    function startCountdown() {
        var remaining = CONFIG.INACTIVITY_TIMEOUT / 1000; // Convert to seconds
        var countdownEl = document.getElementById('inactivityCountdown');

        // Store interval in state so it can be cleared
        if (state.countdownInterval) clearInterval(state.countdownInterval);

        state.countdownInterval = setInterval(function () {
            remaining--;

            // Update display first
            var minutes = Math.floor(remaining / 60);
            var seconds = remaining % 60;
            countdownEl.textContent =
                (minutes < 10 ? '0' : '') + minutes + ':' +
                (seconds < 10 ? '0' : '') + seconds;

            // Then check if we should stop
            if (remaining <= 0) {
                clearInterval(state.countdownInterval);
                // Trigger checkout when countdown reaches 0
                console.log('[AttendanceTracker] Countdown reached 0, performing checkout');
                performInactivityCheckout();
                return;
            }

            if (!state.isModalShown) {
                clearInterval(state.countdownInterval);
                return;
            }
        }, 1000);
    }

    /**
     * Hide inactivity modal
     */
    function hideInactivityModal() {
        state.isModalShown = false;
        if (state.timeoutTimer) clearTimeout(state.timeoutTimer);

        var modal = document.getElementById('inactivityModal');
        if (modal) {
            $(modal).modal('hide');
        }
    }

    /**
     * Perform checkout due to inactivity
     */
    function performInactivityCheckout() {
        // Prevent multiple checkout attempts
        if (state.isCheckingOut) return;
        state.isCheckingOut = true;

        console.log('[AttendanceTracker] Performing checkout, attendanceId:', state.attendanceId);
        $.ajax({
            url: URLS.inactivityCheckout,
            type: 'POST',
            data: { attendanceId: state.attendanceId },
            dataType: 'json',
            success: function (response) {
                console.log('[AttendanceTracker] Checkout response:', response);
                if (response.success) {
                    hideInactivityModal();
                    showCheckoutNotification(response.checkoutTime);
                    cleanup();
                } else {
                    console.error('[AttendanceTracker] Checkout failed:', response.message || 'Unknown error');
                    // If already checked out, stop the tracker and redirect to logout
                    if (response.message && response.message.indexOf('تم تسجيل الخروج مسبقاً') >= 0) {
                        hideInactivityModal();
                        cleanup();
                        alert('ملاحظة: ' + response.message);
                        // Still redirect to logout since user has no active attendance
                        setTimeout(function () {
                            window.location.href = '/Security/Logout';
                        }, 1000);
                    } else {
                        alert('فشل تسجيل الخروج: ' + (response.message || 'خطأ غير معروف'));
                    }
                }
            },
            error: function (xhr, status, error) {
                console.error('[AttendanceTracker] Checkout AJAX error:', status, error);
                alert('فشل الاتصال بالخادم: ' + error);
            }
        });
    }

    /**
     * Show checkout notification and redirect to logout
     */
    function showCheckoutNotification(checkoutTime) {
        console.log('[AttendanceTracker] showCheckoutNotification called, will redirect in 3 seconds');
        var notification = document.createElement('div');
        notification.className = 'alert alert-info alert-dismissible fade show';
        notification.style.cssText = 'position: fixed; top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
        notification.innerHTML =
            '<strong><i class="fa fa-info-circle"></i> تم تسجيل الخروج</strong><br>' +
            'تم تسجيل خروجك في الساعة ' + checkoutTime + ' بسبب عدم النشاط.<br>' +
            '<small>سيتم تحويلك لصفحة تسجيل الدخول خلال 3 ثواني...</small>';

        document.body.appendChild(notification);

        // Redirect to logout page after 3 seconds
        setTimeout(function () {
            console.log('[AttendanceTracker] Redirecting to logout page now...');
            window.location.href = '/Security/Logout';
        }, 3000);
    }

    /**
     * Cleanup - stop all timers
     */
    function cleanup() {
        if (state.heartbeatTimer) clearInterval(state.heartbeatTimer);
        if (state.inactivityTimer) clearTimeout(state.inactivityTimer);
        if (state.timeoutTimer) clearTimeout(state.timeoutTimer);
        if (state.countdownInterval) clearInterval(state.countdownInterval);
        state.isInitialized = false;
    }

    /**
     * Throttle function - limits how often a function can be called
     */
    function throttle(func, limit) {
        var lastCall = 0;
        return function () {
            var now = Date.now();
            if (now - lastCall >= limit) {
                lastCall = now;
                func.apply(this, arguments);
            }
        };
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    // Expose for manual control if needed
    window.AttendanceTracker = {
        init: init,
        logActivity: logActivity,
        checkout: sendBeaconCheckout,
        config: CONFIG,
        // For testing: update config and restart timers
        setConfig: function(newConfig) {
            if (newConfig.INACTIVITY_WARNING) CONFIG.INACTIVITY_WARNING = newConfig.INACTIVITY_WARNING;
            if (newConfig.INACTIVITY_TIMEOUT) CONFIG.INACTIVITY_TIMEOUT = newConfig.INACTIVITY_TIMEOUT;
            console.log('[AttendanceTracker] Config updated:', CONFIG);
            // Restart inactivity timer with new values
            resetInactivityTimer();
            console.log('[AttendanceTracker] Timer restarted. Warning in ' + (CONFIG.INACTIVITY_WARNING/1000) + 's');
        }
    };

})();
