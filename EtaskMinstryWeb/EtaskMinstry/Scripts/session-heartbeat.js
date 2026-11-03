/**
 * Sign-in session heartbeat (Company and Employee).
 *
 * Every open tab reports that the session is still in use; the server
 * (UserSessionTracker / AutoCheckoutJob) closes the session and writes the
 * Logout only after no tab has reported for the timeout. So closing one tab
 * while another is open does not sign the user out.
 *
 * pagehide + sendBeacon only refreshes the last-seen time as the tab goes
 * away - it never signs out by itself (F5, Back and links fire it too).
 */
(function () {
    'use strict';

    var HEARTBEAT_INTERVAL = 60000; // 1 minute
    var URL = '/Security/SessionHeartbeat';
    var signingOut = false;

    function beat() {
        if (signingOut) return;
        var xhr = new XMLHttpRequest();
        xhr.open('POST', URL, true);
        xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
        xhr.onreadystatechange = function () {
            if (xhr.readyState !== 4 || xhr.status !== 200) return;
            var data;
            try { data = JSON.parse(xhr.responseText); } catch (e) { return; }
            // The server already closed this session (timeout or Logout in another tab)
            if (data && data.active === false && !signingOut) {
                signingOut = true;
                window.location.href = '/Security/Logout';
            }
        };
        xhr.send();
    }

    beat();
    setInterval(beat, HEARTBEAT_INTERVAL);

    // Background tabs throttle timers: report as soon as the tab is visible again
    document.addEventListener('visibilitychange', function () {
        if (document.visibilityState === 'visible') beat();
    });

    window.addEventListener('pagehide', function () {
        if (!signingOut && navigator.sendBeacon) navigator.sendBeacon(URL);
    });
})();
