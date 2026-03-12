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
    public partial class PassingCreditReportViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _dbContext;

        [ObservableProperty]
        private ObservableCollection<string> _examinations = new() { "Select" };

        [ObservableProperty]
        private ObservableCollection<string> _programs = new() { "Select" };

        [ObservableProperty]
        private ObservableCollection<string> _regulations = new() { "Select" };

        [ObservableProperty]
        private string _selectedExamination = "Select";
        [ObservableProperty]
        private string _selectedProgram = "Select";
        [ObservableProperty]
        private string _selectedRegulation = "Select";

        [ObservableProperty]
        private ObservableCollection<PassingCreditItem> _creditData = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotLoading))]
        private bool _isLoading;

        public bool IsNotLoading => !IsLoading;

        public PassingCreditReportViewModel(AcgcetDbContext dbContext)
        {
            _dbContext = dbContext;
            Task.Run(InitializeAsync);
        }

        public PassingCreditReportViewModel()
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
        private async Task ViewReport()
        {
            if (_dbContext == null) return;
            IsLoading = true;
            try
            {
                var examination = SelectedExamination != "Select"
                    ? await _dbContext.Examinations
                        .FirstOrDefaultAsync(e => e.ExamMonth == SelectedExamination)
                    : null;

                // Build student query
                var studentQuery = _dbContext.Students.AsQueryable();
                if (SelectedProgram != "Select")
                    studentQuery = studentQuery.Where(s => s.Batch!.Course!.Program!.ProgramName == SelectedProgram);
                if (SelectedRegulation != "Select")
                    studentQuery = studentQuery.Where(s => s.Regulation!.RegulationName == SelectedRegulation);

                var students = await studentQuery.OrderBy(s => s.RegistrationNumber).ToListAsync();
                if (!students.Any()) { CreditData.Clear(); return; }

                var studentIds = students.Select(s => (int?)s.StudentId).ToList();

                // Load ExamResults (with Papers for credits) filtered by examination if set
                var resultsQuery = _dbContext.ExamResults
                    .Include(r => r.Paper)
                    .Include(r => r.ResultStatus)
                    .Where(r => studentIds.Contains(r.StudentId));

                if (examination != null)
                    resultsQuery = resultsQuery.Where(r => r.ExaminationId == examination.ExaminationId);

                var allResults = await resultsQuery.ToListAsync();

                var items = new List<PassingCreditItem>();
                int sno = 1;
                foreach (var student in students)
                {
                    var studentResults = allResults.Where(r => r.StudentId == student.StudentId).ToList();
                    decimal total   = studentResults.Sum(r => r.Paper?.Credits ?? 0);
                    decimal passed  = studentResults
                        .Where(r => r.ResultStatus?.StatusCode == "P" || r.ResultStatus?.StatusName?.Contains("Pass", StringComparison.OrdinalIgnoreCase) == true)
                        .Sum(r => r.Paper?.Credits ?? 0);
                    decimal failed  = total - passed;

                    items.Add(new PassingCreditItem
                    {
                        SNo = sno++,
                        RegistrationNumber = student.RegistrationNumber ?? "",
                        StudentName = student.FullName ?? "",
                        TotalCredits  = (int)total,
                        PassedCredits = (int)passed,
                        FailedCredits = (int)failed
                    });
                }

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    CreditData = new ObservableCollection<PassingCreditItem>(items);
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
            CreditData.Clear();
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await ViewReport();
        }
    }

    public class PassingCreditItem
    {
        public int SNo { get; set; }
        public string RegistrationNumber { get; set; } = "";
        public string StudentName { get; set; } = "";
        public int TotalCredits { get; set; }
        public int PassedCredits { get; set; }
        public int FailedCredits { get; set; }
    }
}
