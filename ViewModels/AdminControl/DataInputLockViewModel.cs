using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ACGCET_Admin.Models;
using ACGCET_Admin.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ACGCET_Admin.ViewModels.AdminControl
{
    public partial class DataInputLockViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _dbContext = null!;

        [ObservableProperty]
        private ObservableCollection<ModuleLockItem> _lockItems = new();

        public DataInputLockViewModel(AcgcetDbContext dbContext)
        {
            _dbContext = dbContext;
            if (_dbContext != null)
                _ = LoadLocksAsync();
        }

        public DataInputLockViewModel()
        {
            _dbContext = null!;
        }

        private async Task LoadLocksAsync()
        {
            // Determine the latest examination for exam-scoped locks
            var latestExam = await _dbContext.Examinations
                .OrderByDescending(e => e.ExaminationId)
                .FirstOrDefaultAsync();

            int? examId = latestExam?.ExaminationId;

            // Load all modules, joining with both global (ExaminationId=null) and exam-scoped locks
            var modules = await _dbContext.Modules.ToListAsync();
            var globalLocks = await _dbContext.ModuleLocks
                .Where(x => x.ExaminationId == null)
                .ToListAsync();
            var examLocks = examId != null
                ? await _dbContext.ModuleLocks
                    .Where(x => x.ExaminationId == examId)
                    .ToListAsync()
                : new List<ModuleLock>();

            LockItems.Clear();
            foreach (var mod in modules)
            {
                var globalLock = globalLocks.FirstOrDefault(l => l.ModuleId == mod.ModuleId);
                var examLock = examLocks.FirstOrDefault(l => l.ModuleId == mod.ModuleId);

                // Module is locked if EITHER global or exam-scoped lock is active
                bool isLocked = (globalLock != null && (globalLock.IsLocked ?? false))
                             || (examLock != null && (examLock.IsLocked ?? false));

                LockItems.Add(new ModuleLockItem
                {
                    ModuleId = mod.ModuleId,
                    ModuleName = mod.ModuleName,
                    IsLocked = isLocked
                });
            }
        }

        [RelayCommand]
        private async Task UpdateLocks()
        {
            if (!UserPermissionService.Current.CanUpdate("LOCK_MGMT"))
            {
                MessageBox.Show("You do not have permission to modify locks.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var latestExam = await _dbContext.Examinations
                    .OrderByDescending(e => e.ExaminationId)
                    .FirstOrDefaultAsync();

                foreach (var item in LockItems)
                {
                    // Update or create global lock (ExaminationId = null)
                    var globalLock = await _dbContext.ModuleLocks
                        .FirstOrDefaultAsync(l => l.ModuleId == item.ModuleId && l.ExaminationId == null);

                    if (globalLock != null)
                    {
                        globalLock.IsLocked = item.IsLocked;
                        globalLock.LockedDateTime = item.IsLocked ? System.DateTime.Now : globalLock.LockedDateTime;
                        globalLock.LockedBy = item.IsLocked ? "Admin" : globalLock.LockedBy;
                    }
                    else if (item.IsLocked)
                    {
                        _dbContext.ModuleLocks.Add(new ModuleLock
                        {
                            ModuleId = item.ModuleId,
                            ExaminationId = null,
                            IsLocked = true,
                            LockedDateTime = System.DateTime.Now,
                            LockedBy = "Admin",
                            LockReason = "Manual Lock via Admin UI"
                        });
                    }

                    // Sync ALL exam-scoped locks for this module so old exam locks don't cause stale LOCKED state
                    var allExamLocks = await _dbContext.ModuleLocks
                        .Where(l => l.ModuleId == item.ModuleId && l.ExaminationId != null)
                        .ToListAsync();

                    foreach (var el in allExamLocks)
                    {
                        el.IsLocked = item.IsLocked;
                        el.LockedDateTime = item.IsLocked ? System.DateTime.Now : el.LockedDateTime;
                        el.LockedBy = item.IsLocked ? "Admin" : el.LockedBy;
                    }

                    // If locking and the latest exam has no lock record yet, create one
                    if (item.IsLocked && latestExam != null && !allExamLocks.Any(l => l.ExaminationId == latestExam.ExaminationId))
                    {
                        _dbContext.ModuleLocks.Add(new ModuleLock
                        {
                            ModuleId = item.ModuleId,
                            ExaminationId = latestExam.ExaminationId,
                            IsLocked = true,
                            LockedDateTime = System.DateTime.Now,
                            LockedBy = "Admin",
                            LockReason = "Manual Lock via Admin UI"
                        });
                    }
                }
                await _dbContext.SaveChangesAsync();
                MessageBox.Show("Input Locks Updated Successfully (SQL Security Enforced)");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error updating locks: {ex.Message}");
            }
        }

        [RelayCommand]
        private void Clear()
        {
            foreach (var item in LockItems) item.IsLocked = false;
        }

        [RelayCommand]
        private async Task Refresh()
        {
            if (_dbContext != null)
                await LoadLocksAsync();
        }
    }

    public partial class ModuleLockItem : ObservableObject
    {
        public int ModuleId { get; set; }
        public string ModuleName { get; set; } = "";

        private bool _isLocked;
        public bool IsLocked
        {
            get => _isLocked;
            set => SetProperty(ref _isLocked, value);
        }
    }
}
