using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ACGCET_Admin.Models;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ACGCET_Admin.ViewModels.DeleteEntry
{
    public partial class DeleteEntryViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _dbContext;
        // private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private object _currentDeleteView;

        public DeleteEntryViewModel(AcgcetDbContext dbContext)
        {
            _dbContext = dbContext;
            // Default view
            CurrentDeleteView = new DeleteExamApplyViewModel(_dbContext);
        }

        [RelayCommand]
        private void Navigate(string destination)
        {
            switch (destination)
            {
                case "ExamApply":
                    CurrentDeleteView = new DeleteExamApplyViewModel(_dbContext);
                    break;
                case "InternalMark":
                    CurrentDeleteView = new DeleteInternalMarkViewModel(_dbContext);
                    break;
                case "ExternalMark":
                    CurrentDeleteView = new DeleteExternalMarkViewModel(_dbContext);
                    break;
                case "ConvertMark":
                    CurrentDeleteView = new DeleteConvertMarkViewModel(_dbContext);
                    break;
                case "FinalResult":
                    CurrentDeleteView = new DeleteFinalResultViewModel(_dbContext);
                    break;
                case "StudentMaster":
                     CurrentDeleteView = new DeleteStudentMasterViewModel(_dbContext);
                     break;
                case "AuditCourse":
                     CurrentDeleteView = new DeleteAuditCourseViewModel(_dbContext);
                     break;
                case "IntexnCourse":
                     CurrentDeleteView = new DeleteIntexnCourseViewModel(_dbContext);
                     break;
                case "NccCourse":
                     CurrentDeleteView = new DeleteNccCourseViewModel(_dbContext);
                     break;
                case "NaanMudhalvan":
                     CurrentDeleteView = new DeleteNaanMudhalvanViewModel(_dbContext);
                     break;
                case "MandatoryCourse":
                     CurrentDeleteView = new DeleteMandatoryCourseViewModel(_dbContext);
                     break;
            }
        }
    }
}
