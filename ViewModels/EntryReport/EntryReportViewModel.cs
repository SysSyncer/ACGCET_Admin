using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace ACGCET_Admin.ViewModels.EntryReport
{
    public partial class EntryReportViewModel : ObservableObject
    {
        [ObservableProperty] private object _currentView;

        private readonly StudentEntryReportViewModel _studentEntryVM;
        private readonly InternalMarkEntryReportViewModel _internalMarkEntryVM;
        private readonly ExamApplyEntryReportViewModel _examApplyEntryVM;
        private readonly ExternalMarkEntryReportViewModel _externalMarkEntryVM;
        private readonly ResultEntryReportViewModel _resultEntryVM;

        public EntryReportViewModel(
            StudentEntryReportViewModel studentEntryVM,
            InternalMarkEntryReportViewModel internalMarkEntryVM,
            ExamApplyEntryReportViewModel examApplyEntryVM,
            ExternalMarkEntryReportViewModel externalMarkEntryVM,
            ResultEntryReportViewModel resultEntryVM)
        {
            _studentEntryVM = studentEntryVM;
            _internalMarkEntryVM = internalMarkEntryVM;
            _examApplyEntryVM = examApplyEntryVM;
            _externalMarkEntryVM = externalMarkEntryVM;
            _resultEntryVM = resultEntryVM;

            // Default
            // Default
            CurrentView = _studentEntryVM;
            Initialize(); // Async void is safe here as it is top-level for this VM
        }

        private async void Initialize()
        {
             await _studentEntryVM.LoadMasterData();
        }

        [RelayCommand] 
        private async Task ShowStudentEntry() 
        { 
            CurrentView = _studentEntryVM; 
            await _studentEntryVM.LoadMasterData();
        }

        [RelayCommand] 
        private async Task ShowInternalMarkEntry() 
        { 
            CurrentView = _internalMarkEntryVM; 
            await _internalMarkEntryVM.LoadMasterData();
        }

        [RelayCommand] 
        private async Task ShowExamApplyEntry() 
        { 
            CurrentView = _examApplyEntryVM; 
            await _examApplyEntryVM.LoadMasterData();
        }

        [RelayCommand] 
        private async Task ShowExternalMarkEntry() 
        { 
             CurrentView = _externalMarkEntryVM;
             await _externalMarkEntryVM.LoadMasterData();
        }

        [RelayCommand] 
        private async Task ShowResultEntry() 
        { 
             CurrentView = _resultEntryVM;
             await _resultEntryVM.LoadMasterData();
        }
    }
}
