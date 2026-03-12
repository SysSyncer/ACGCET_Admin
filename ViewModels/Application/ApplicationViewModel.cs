using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using ACGCET_Admin.Models;

namespace ACGCET_Admin.ViewModels.Application
{
    public class ApplicationViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _dbContext;
        private object _currentReportView = new object();

        public object CurrentReportView
        {
            get => _currentReportView;
            set => SetProperty(ref _currentReportView, value);
        }

        public ICommand NavigateReportCommand { get; }

        public ApplicationViewModel(AcgcetDbContext dbContext)
        {
            _dbContext = dbContext;
            NavigateReportCommand = new RelayCommand<string>(NavigateReport);
            
            // Default View
            NavigateReport("ClassWiseStudents");
        }

        private void NavigateReport(string? reportName)
        {
            if (string.IsNullOrEmpty(reportName)) return;
            switch (reportName)
            {
                case "ClassWiseStudents":
                    CurrentReportView = new ClassWiseStudentsViewModel(_dbContext);
                    break;
                case "ClassWiseSubjectCode":
                    CurrentReportView = new ClassWiseSubjectCodeViewModel(_dbContext);
                    break;
                case "ClassInternalMarkReport":
                    CurrentReportView = new ClassInternalMarkReportViewModel(_dbContext);
                    break;
                case "ClassWiseHallTicket":
                    CurrentReportView = new ClassWiseHallTicketViewModel(_dbContext);
                    break;
                case "TimetablePrint":
                    CurrentReportView = new TimetablePrintViewModel(_dbContext);
                    break;
                case "YearWiseIntExtReport":
                    CurrentReportView = new YearWiseIntExtReportViewModel(_dbContext);
                    break;
                case "ClassWiseIntExtReport":
                    CurrentReportView = new ClassWiseIntExtReportViewModel(_dbContext);
                    break;
                case "A4ResultPrint":
                    CurrentReportView = new A4ResultPrintViewModel(_dbContext);
                    break;
                case "RevaluationReport":
                    CurrentReportView = new RevaluationReportViewModel(_dbContext);
                    break;
                case "PassingCreditReport":
                    CurrentReportView = new PassingCreditReportViewModel(_dbContext);
                    break;
                // Add other reports here
            }
        }
    }
}
