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
using System.Windows.Data;
using CommunityToolkit.Mvvm.Messaging;

namespace ACGCET_Admin.ViewModels.Application
{
    public partial class TimetablePrintViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _dbContext;

        // Filters
        [ObservableProperty]
        private ObservableCollection<string> _examinations = new() { "Select" };

        [ObservableProperty]
        private ObservableCollection<string> _levels = new() { "Select", "UG", "PG" };

        [ObservableProperty]
        private ObservableCollection<string> _programs = new() { "Select" };

        [ObservableProperty]
        private ObservableCollection<string> _regulations = new() { "Select" };

        // Selected Values
        [ObservableProperty]
        private string _selectedExamination = "Select";

        [ObservableProperty]
        private string _selectedLevel = "Select";

        [ObservableProperty]
        private string _selectedProgram = "Select";

        [ObservableProperty]
        private string _selectedRegulation = "Select";

        // Data
        [ObservableProperty]
        private ObservableCollection<TimetableItem> _timetableData = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotLoading))]
        private bool _isLoading;

        public bool IsNotLoading => !IsLoading;

        public TimetablePrintViewModel(AcgcetDbContext dbContext)
        {
            _dbContext = dbContext;
            Task.Run(InitializeAsync);
        }

        public TimetablePrintViewModel()
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
                    // Load Exams
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
        private async Task ViewTimetable()
        {
            if (_dbContext == null) return;
            IsLoading = true;
            try
            {
                var query = _dbContext.ExamSchedules
                    .Include(es => es.Paper)
                    .ThenInclude(p => p!.Course)
                    .ThenInclude(c => c!.Program)
                    .Include(es => es.Examination)
                    .AsQueryable();

                // Filters
                if (SelectedExamination != "Select")
                    query = query.Where(es => es.Examination!.ExamMonth == SelectedExamination);
                
                if (SelectedProgram != "Select")
                    query = query.Where(es => es.Paper!.Course!.Program!.ProgramName == SelectedProgram);

                if (SelectedRegulation != "Select")
                    query = query.Where(es => es.Paper!.Course!.Regulation!.RegulationName == SelectedRegulation);

                // Level Filter
                // if (SelectedLevel != "Select") ... (Assuming Program handles Level, or separate CourseLevel logic)

                var schedules = await query.OrderBy(es => es.ExamDate).ToListAsync();

                var items = schedules.Select(es => new TimetableItem
                {
                    Date = es.ExamDate.ToString("dd/MM/yyyy"),
                    Session = es.ExamSession != null ? es.ExamSession.SessionName ?? "" : "",
                    CourseCode = es.Paper!.PaperCode ?? "",
                    CourseTitle = es.Paper.PaperName ?? "",
                    Semester = es.Paper.Semester
                });

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    TimetableData = new ObservableCollection<TimetableItem>(items);
                });
            }
            catch { }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private void PrintPreview() => TriggerPrint(true);

        [RelayCommand]
        private void Print() => TriggerPrint(false);

        private void TriggerPrint(bool isPreview)
        {
            if (!TimetableData.Any()) return;

            var msg = new PrintTimetableMessage
            {
                IsPreview = isPreview,
                Items = TimetableData.ToList(),
                ExamName = SelectedExamination,
                Program = SelectedProgram,
                Regulation = SelectedRegulation
            };
            WeakReferenceMessenger.Default.Send(msg);
        }

        [RelayCommand]
        private void ClearFilters()
        {
            SelectedExamination = "Select";
            SelectedLevel = "Select";
            SelectedProgram = "Select";
            SelectedRegulation = "Select";
            TimetableData.Clear();
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await ViewTimetable();
        }
    }

    public class TimetableItem
    {
        public string Date { get; set; } = "";
        public string Session { get; set; } = "";
        public string CourseCode { get; set; } = "";
        public string CourseTitle { get; set; } = "";
        public int Semester { get; set; }
        public string SemesterDisplay => $"Sem {Semester}";
    }

    public class PrintTimetableMessage
    {
        public bool IsPreview { get; set; }
        public List<TimetableItem> Items { get; set; } = new();
        public string ExamName { get; set; } = "";
        public string Program { get; set; } = "";
        public string Regulation { get; set; } = "";
    }
}
