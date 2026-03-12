using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using ACGCET_Admin.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Threading.Tasks;
using System;

namespace ACGCET_Admin.ViewModels.Application
{
    public partial class RevaluationReportViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _dbContext;

        [ObservableProperty]
        private ObservableCollection<string> _examinations = new() { "Select" };

        [ObservableProperty]
        private ObservableCollection<string> _programs = new() { "Select" };

        [ObservableProperty]
        private ObservableCollection<string> _regulations = new() { "Select" };

        [ObservableProperty]
        private ObservableCollection<string> _paperCodes = new() { "Select" };

        [ObservableProperty]
        private string _selectedExamination = "Select";
        [ObservableProperty]
        private string _selectedProgram = "Select";
        [ObservableProperty]
        private string _selectedRegulation = "Select";
        [ObservableProperty]
        private string _selectedPaperCode = "Select";

        [ObservableProperty]
        private ObservableCollection<RevaluationItem> _revaluations = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotLoading))]
        private bool _isLoading;

        public bool IsNotLoading => !IsLoading;

        public RevaluationReportViewModel(AcgcetDbContext dbContext)
        {
            _dbContext = dbContext;
            Task.Run(InitializeAsync);
        }

        public RevaluationReportViewModel()
        {
            _dbContext = null!;
        }

        private async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                List<string> exams = new List<string> { "Select" };
                List<string> programs = new List<string> { "Select" };
                List<string> regulations = new List<string> { "Select" };

                if (_dbContext != null)
                {
                    var dbExams = await _dbContext.Examinations
                        .OrderByDescending(e => e.ExaminationId)
                        .Select(e => e.ExamMonth)
                        .Distinct()
                        .Where(e => e != null)
                        .ToListAsync();
                    exams.AddRange(dbExams!);

                    var dbPrograms = await _dbContext.Programs.Select(p => p.ProgramName).Where(p => p != null).ToListAsync();
                    programs.AddRange(dbPrograms!);

                    var dbRegs = await _dbContext.Regulations.Select(r => r.RegulationName).Where(r => r != null).ToListAsync();
                    regulations.AddRange(dbRegs!);
                }

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    Examinations = new ObservableCollection<string>(exams);
                    Programs = new ObservableCollection<string>(programs);
                    Regulations = new ObservableCollection<string>(regulations);
                });
            }
            catch { }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private async Task ViewRevaluations()
        {
            if (_dbContext == null) return;
            IsLoading = true;
            try
            {
                var query = _dbContext.RevaluationRequests
                    .Include(r => r.Student)
                    .Include(r => r.ExamResult).ThenInclude(er => er!.Paper)
                    .Include(r => r.RevaluationStatus)
                    .AsQueryable();

                if (SelectedExamination != "Select")
                {
                    var exam = await _dbContext.Examinations
                        .FirstOrDefaultAsync(e => e.ExamMonth == SelectedExamination);
                    if (exam != null)
                        query = query.Where(r => r.ExamResult!.ExaminationId == exam.ExaminationId);
                }

                if (SelectedProgram != "Select")
                    query = query.Where(r => r.Student!.Batch!.Course!.Program!.ProgramName == SelectedProgram);

                if (SelectedRegulation != "Select")
                    query = query.Where(r => r.Student!.Regulation!.RegulationName == SelectedRegulation);

                if (SelectedPaperCode != "Select" && !string.IsNullOrEmpty(SelectedPaperCode))
                    query = query.Where(r => r.ExamResult!.Paper!.PaperCode == SelectedPaperCode);

                var results = await query.OrderBy(r => r.RequestDate).ToListAsync();

                var items = results.Select((r, idx) => new RevaluationItem
                {
                    SNo = idx + 1,
                    RegistrationNumber = r.Student?.RegistrationNumber ?? "",
                    StudentName = r.Student?.FullName ?? "",
                    PaperCode = r.ExamResult?.Paper?.PaperCode ?? "",
                    PaperName = r.ExamResult?.Paper?.PaperName ?? "",
                    OriginalMark = r.OriginalMark?.ToString("0") ?? "",
                    RevaluatedMark = r.RevaluatedMark?.ToString("0") ?? "",
                    Status = r.RevaluationStatus?.StatusName ?? "Pending"
                }).ToList();

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    Revaluations = new ObservableCollection<RevaluationItem>(items);
                });
            }
            catch { }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private void ClearFilters()
        {
            SelectedExamination = "Select";
            SelectedProgram = "Select";
            SelectedRegulation = "Select";
            SelectedPaperCode = "Select";
            Revaluations.Clear();
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await ViewRevaluations();
        }
    }

    public class RevaluationItem
    {
        public int SNo { get; set; }
        public string RegistrationNumber { get; set; } = "";
        public string StudentName { get; set; } = "";
        public string PaperCode { get; set; } = "";
        public string PaperName { get; set; } = "";
        public string OriginalMark { get; set; } = "";
        public string RevaluatedMark { get; set; } = "";
        public string Status { get; set; } = "";
    }
}
