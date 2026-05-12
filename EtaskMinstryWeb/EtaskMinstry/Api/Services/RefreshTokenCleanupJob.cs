using System;
using System.Threading;

namespace EtaskMinstry.Api.Services
{
    /// <summary>
    /// Server-side job that periodically purges expired refresh tokens.
    /// Runs once a day. Mirrors the lifecycle of AutoCheckoutJob so wiring
    /// stays predictable: Start() from Application_Start, Stop() from
    /// Application_End in Global.asax.cs.
    /// </summary>
    public static class RefreshTokenCleanupJob
    {
        private static Timer _timer;
        private static readonly object _lock = new object();
        private static bool _isRunning = false;

        // Run once a day. Tokens older than 30 days past expiry are deleted.
        private const int JOB_INTERVAL_HOURS = 24;
        private const int RETAIN_DAYS_AFTER_EXPIRY = 30;

        public static void Start()
        {
            if (_timer != null) return;

            _timer = new Timer(
                Tick,
                null,
                TimeSpan.FromMinutes(10),
                TimeSpan.FromHours(JOB_INTERVAL_HOURS)
            );

            System.Diagnostics.Debug.WriteLine(
                "[RefreshTokenCleanupJob] Started - every {0}h, retain {1} days past expiry",
                JOB_INTERVAL_HOURS, RETAIN_DAYS_AFTER_EXPIRY);
        }

        public static void Stop()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        private static void Tick(object state)
        {
            lock (_lock)
            {
                if (_isRunning) return;
                _isRunning = true;
            }

            try
            {
                int deleted = new RefreshTokenService().DeleteExpiredOlderThan(RETAIN_DAYS_AFTER_EXPIRY);
                if (deleted > 0)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "[RefreshTokenCleanupJob] Deleted {0} expired refresh tokens at {1}",
                        deleted, DateTime.Now);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[RefreshTokenCleanupJob] Error: {0}", ex.Message);
            }
            finally
            {
                _isRunning = false;
            }
        }
    }
}
