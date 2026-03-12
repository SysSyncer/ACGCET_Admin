using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ACGCET_Admin.Models;

namespace ACGCET_Admin.ViewModels.MissingEntry
{
    public partial class MissingEntryViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _dbContext;

        [ObservableProperty]
        private object _currentView;

        public MissingEntryViewModel(AcgcetDbContext dbContext)
        {
            _dbContext = dbContext;
            // Default view? Maybe Internal Entry
            CurrentView = new MissingInternalEntryViewModel(_dbContext);
        }

        [RelayCommand]
        private void Navigate(string destination)
        {
            switch (destination)
            {
                case "MissingInternalEntry":
                    CurrentView = new MissingInternalEntryViewModel(_dbContext);
                    break;
                case "MissingExternalEntry":
                    CurrentView = new MissingExternalEntryViewModel(_dbContext);
                    break;
                case "MissingConvertEntry":
                    CurrentView = new MissingConvertEntryViewModel(_dbContext);
                    break;
                case "MissingResultEntry":
                    CurrentView = new MissingResultEntryViewModel(_dbContext);
                    break;
            }
        }
    }
}
