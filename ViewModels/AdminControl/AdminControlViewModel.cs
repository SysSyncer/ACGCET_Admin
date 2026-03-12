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
                case "Close":
                    // Handled by Dashboard Navigation or specific close logic
                    // Usually "Back" goes to Dashboard Home?
                    // We can send a message to Dashboard?
                    // Or just let User use Dashboard Sidebar.
                    break;
            }
        }
    }
}
