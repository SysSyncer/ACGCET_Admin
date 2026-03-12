using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ACGCET_Admin.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;

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
                LoadLocks();
        }

        public DataInputLockViewModel()
        {
            _dbContext = null!;
        }

        private void LoadLocks()
        {
            // Join Modules with ModuleLocks (Global Lock: ExaminationId IS NULL)
            var query = from m in _dbContext.Modules
                        join l in _dbContext.ModuleLocks.Where(x => x.ExaminationId == null)
                        on m.ModuleId equals l.ModuleId into locks
                        from ml in locks.DefaultIfEmpty()
                        select new { Module = m, Lock = ml };

            var result = query.ToList();

            LockItems.Clear();
            foreach (var item in result)
            {
                bool isLocked = item.Lock != null && (item.Lock.IsLocked ?? false);

                LockItems.Add(new ModuleLockItem 
                { 
                    ModuleId = item.Module.ModuleId, 
                    ModuleName = item.Module.ModuleName, 
                    // Init the property based on the DB Lock status
                    IsLocked = isLocked 
                });
            }
        }

        [RelayCommand]
        private void UpdateLocks()
        {
            try 
            {
                foreach (var item in LockItems)
                {
                    // Find existing global lock for this module
                    var existingLock = _dbContext.ModuleLocks
                        .FirstOrDefault(l => l.ModuleId == item.ModuleId && l.ExaminationId == null);

                    if (existingLock != null)
                    {
                        // Update existing
                        existingLock.IsLocked = item.IsLocked;
                        existingLock.LockedDateTime = item.IsLocked ? System.DateTime.Now : existingLock.LockedDateTime;
                        existingLock.LockedBy = item.IsLocked ? "Admin" : existingLock.LockedBy;
                    }
                    else if (item.IsLocked)
                    {
                        // Create new lock ONLY if we are locking (don't clutter DB with unlocked records)
                        var newLock = new ModuleLock
                        {
                            ModuleId = item.ModuleId,
                            ExaminationId = null, // Global Lock
                            IsLocked = true,
                            LockedDateTime = System.DateTime.Now,
                            LockedBy = "Admin",
                            LockReason = "Manual Lock via Admin UI"
                        };
                        _dbContext.ModuleLocks.Add(newLock);
                    }
                }
                _dbContext.SaveChanges();
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
        private void Refresh()
        {
            if (_dbContext != null)
                LoadLocks();
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
