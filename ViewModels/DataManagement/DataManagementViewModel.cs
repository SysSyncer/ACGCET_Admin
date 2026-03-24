using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ACGCET_Admin.Models;

namespace ACGCET_Admin.ViewModels.DataManagement
{
    public partial class DataManagementViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _db;

        [ObservableProperty]
        private object _currentDataView;

        public DataManagementViewModel(AcgcetDbContext db)
        {
            _db = db;
            CurrentDataView = new ManageDegreesViewModel(_db);
        }

        public DataManagementViewModel()
        {
            _db = null!;
            _currentDataView = new object();
        }

        [RelayCommand]
        private void Navigate(string destination)
        {
            CurrentDataView = destination switch
            {
                // Lookup Tables
                "Degrees" => new ManageDegreesViewModel(_db),
                "Programs" => new ManageProgramsViewModel(_db),
                "Regulations" => new ManageRegulationsViewModel(_db),
                "ExamTypes" => new ManageExamTypesViewModel(_db),
                "TestTypes" => new ManageTestTypesViewModel(_db),
                "Schemes" => new ManageSchemesViewModel(_db),
                "ExamSessions" => new ManageExamSessionsViewModel(_db),
                "Blocks" => new ManageBlocksViewModel(_db),
                // Academic Structure
                "Courses" => new ManageCoursesViewModel(_db),
                "Batches" => new ManageBatchesViewModel(_db),
                "Sections" => new ManageSectionsViewModel(_db),
                "Papers" => new ManagePapersViewModel(_db),
                "PaperFees" => new ManagePaperFeesViewModel(_db),
                "PassingCriteria" => new ManagePassingCriteriaViewModel(_db),
                // Students
                "Students" => new ManageStudentsViewModel(_db),
                // Examinations
                "Examinations" => new ManageExaminationsViewModel(_db),
                "ExamApplications" => new ManageExamApplicationsViewModel(_db),
                "ExamSchedule" => new ManageExamScheduleViewModel(_db),
                // Infrastructure
                "Rooms" => new ManageRoomsViewModel(_db),
                "SeatAllocations" => new ManageSeatAllocationsViewModel(_db),
                // Marks & Results
                "InternalMarks" => new ManageInternalMarksViewModel(_db),
                "ExternalMarks" => new ManageExternalMarksViewModel(_db),
                "ExamResults" => new ManageExamResultsViewModel(_db),
                _ => CurrentDataView
            };
        }
    }
}
