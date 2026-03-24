using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using ACGCET_Admin.Models;

namespace ACGCET_Admin.ViewModels.AdminControl
{
    public partial class AdminControlViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _dbContext;

        [ObservableProperty]
        private object _currentAdminView;

        public AdminControlViewModel(AcgcetDbContext dbContext)
        {
            _dbContext = dbContext;
            // Default View
            CurrentAdminView = new NewUserCreationViewModel(_dbContext);
        }

        public AdminControlViewModel()
        {
            _dbContext = null!;
            _currentAdminView = new NewUserCreationViewModel(null!);
        }

        [RelayCommand]
        private void Navigate(string destination)
        {
            switch (destination)
            {
                case "NewUserCreation":
                    CurrentAdminView = new NewUserCreationViewModel(_dbContext);
                    break;
                case "ExtMarkEntryBarcode":
                    CurrentAdminView = new ExtMarkEntryBarcodeViewModel(_dbContext);
                    break;
                case "StudentWiseBarcodeView":
                    CurrentAdminView = new StudentWiseBarcodeViewModel(_dbContext);
                    break;
                case "DataInputLock":
                    CurrentAdminView = new DataInputLockViewModel(_dbContext);
                    break;
                case "DataCorrection":
                    CurrentAdminView = new DataCorrectionManagementViewModel(_dbContext);
                    break;
                case "LockOverride":
                    CurrentAdminView = new LockOverrideManagementViewModel(_dbContext);
                    break;
                case "AnomalyDetection":
                    CurrentAdminView = new AnomalyManagementViewModel(_dbContext);
                    break;
                case "EntryProgress":
                    CurrentAdminView = new EntryProgressViewModel(_dbContext);
                    break;
                case "Close":
                    break;
            }
        }
    }
}
