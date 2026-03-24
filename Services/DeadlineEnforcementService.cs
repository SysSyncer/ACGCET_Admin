using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using ACGCET_Admin.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ACGCET_Admin.Services
{
    /// <summary>
    /// Periodically enforces deadline-based module locks and re-locks expired overrides.
    /// Runs on app startup and every 5 minutes via DispatcherTimer.
    /// Implements the "temporal logic constraints" described in the abstract.
    /// </summary>
    public class DeadlineEnforcementService
    {
        private readonly IServiceProvider _serviceProvider;
        private DispatcherTimer? _timer;

        public DeadlineEnforcementService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Start()
        {
            _ = EnforceAsync();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(5)
            };
            _timer.Tick += async (_, _) => await EnforceAsync();
            _timer.Start();
        }

        public void Stop()
        {
            _timer?.Stop();
        }

        public async Task EnforceAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AcgcetDbContext>();

                // Push any expired deadlines 2 years into the future so the SP won't
                // keep re-locking modules that the admin has already manually unlocked.
                var expiredDeadlines = await db.DeadlineConfigurations
                    .Where(dc => dc.DeadlineDateTime < DateTime.Now)
                    .ToListAsync();

                if (expiredDeadlines.Count > 0)
                {
                    foreach (var dc in expiredDeadlines)
                        dc.DeadlineDateTime = dc.DeadlineDateTime.AddYears(2);
                    await db.SaveChangesAsync();
                }

                // 1. Execute SP_AutoLockModulesByDeadline — locks modules past their deadline
                await db.Database.ExecuteSqlRawAsync("EXEC SP_AutoLockModulesByDeadline");

                // 2. Re-lock modules with expired temporary overrides
                var expiredOverrides = await db.LockOverrideRequests
                    .Include(lor => lor.ModuleLock)
                    .Where(lor => lor.ApprovalStatus == "Approved"
                               && lor.TemporaryUnlockExpiry != null
                               && lor.TemporaryUnlockExpiry < DateTime.Now
                               && lor.ModuleLock != null
                               && lor.ModuleLock.IsLocked == false)
                    .ToListAsync();

                foreach (var ovr in expiredOverrides)
                {
                    if (ovr.ModuleLock != null)
                    {
                        ovr.ModuleLock.IsLocked = true;
                        ovr.ModuleLock.LockedDateTime = DateTime.Now;
                        ovr.ModuleLock.LockedBy = "SYSTEM";
                        ovr.ModuleLock.LockReason = "Temporary override expired — auto re-locked";
                        ovr.ModuleLock.AutoLocked = true;
                        ovr.ApprovalStatus = "Expired";
                    }
                }

                if (expiredOverrides.Count > 0)
                {
                    db.AuditLogs.Add(new AuditLog
                    {
                        ActionType = "OVERRIDE_EXPIRED",
                        Description = $"{expiredOverrides.Count} temporary override(s) expired and modules re-locked",
                        ActionDate = DateTime.Now
                    });
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DeadlineEnforcement error: {ex.Message}");
            }
        }
    }
}
