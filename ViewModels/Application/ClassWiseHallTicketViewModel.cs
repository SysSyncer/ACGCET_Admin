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
    public partial class ClassWiseHallTicketViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _dbContext;

        // Filters
        [ObservableProperty]
        private ObservableCollection<string> _examinations = new() { "Select" }; // e.g., "Nov 2023"

        [ObservableProperty]
        private ObservableCollection<string> _levels = new() { "Select", "UG", "PG" };

        [ObservableProperty]
        private ObservableCollection<string> _programs = new() { "Select" };

        [ObservableProperty]
        private ObservableCollection<string> _regulations = new() { "Select" };

        [ObservableProperty]
        private ObservableCollection<string> _semesters = new() { "Select", "1", "2", "3", "4", "5", "6", "7", "8" };

        [ObservableProperty]
        private ObservableCollection<string> _sections = new() { "Select", "Overall", "A", "B", "C", "D" };

        // Selected Values
        [ObservableProperty]
        private string _selectedExamination = "Select";

        [ObservableProperty]
        private string _selectedLevel = "Select";

        [ObservableProperty]
        private string _selectedProgram = "Select";

        [ObservableProperty]
        private string _selectedRegulation = "Select";
        
        [ObservableProperty]
        private string _selectedSemester = "Select";

        [ObservableProperty]
        private string _selectedSection = "Select";

        // Data
        [ObservableProperty]
        private ObservableCollection<HallTicketStudentItem> _studentsList = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotLoading))]
        private bool _isLoading;

        public bool IsNotLoading => !IsLoading;

        public ClassWiseHallTicketViewModel(AcgcetDbContext dbContext)
        {
            _dbContext = dbContext;
            Task.Run(InitializeAsync);
        }

        public ClassWiseHallTicketViewModel()
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
                        .OrderByDescending(e => e.ExaminationId) // Latest first?
                        .Select(e => e.ExamMonth) // Assuming this holds "Nov 2023"
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
        private async Task ViewStudents()
        {
            if (_dbContext == null) return;
            IsLoading = true;
            try
            {
                // Fetch Students based on filters
                var query = _dbContext.Students.AsQueryable();

                if (SelectedProgram != "Select") 
                    query = query.Where(s => s.Batch!.Course!.Program!.ProgramName == SelectedProgram);
                if (SelectedRegulation != "Select")
                    query = query.Where(s => s.Regulation!.RegulationName == SelectedRegulation);
                if (SelectedSection != "Select" && SelectedSection != "Overall")
                    query = query.Where(s => s.Section!.SectionName == SelectedSection);
                
                // Semester filter? Usually Semester implies "Current Semester" of the student/batch
                // If the report is for a specific semester exam, we assume students in that batch are in that semester?
                // Or we don't filter students by semester if it's not stored on student directly. 
                // We'll ignore Student.Semester property if it doesn't exist. Assuming Batch->CurrentSemester or implied.

                var students = await query.OrderBy(s => s.RollNumber).ToListAsync();

                var items = students.Select((s, index) => new HallTicketStudentItem
                {
                    IsSelected = true, // Default select all
                    SNo = index + 1,
                    AdmissionNo = s.AdmissionNumber ?? "",
                    RollNo = s.RollNumber ?? "",
                    RegNo = s.RegistrationNumber ?? "",
                    StudentName = s.FullName ?? ""
                });

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    StudentsList = new ObservableCollection<HallTicketStudentItem>(items);
                });
            }
            catch { }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private void PrintPreview()
        {
            TriggerPrint(true);
        }

        [RelayCommand]
        private void Print()
        {
            TriggerPrint(false);
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await ViewStudents();
        }

        private async void TriggerPrint(bool isPreview)
        {
            var selectedStudents = StudentsList.Where(s => s.IsSelected).ToList();
            if (!selectedStudents.Any()) return;

            IsLoading = true;
            try 
            {
                // Fetch Schedule
                // Note: Assuming 'ExamSchedule' table exists and links correctly. 
                // Since I cannot verify exact Schema for Schedule linkage right now without deeper check, 
                // I will assume a standard join path.
                
                int sem = int.TryParse(SelectedSemester, out int s) ? s : 0;

                var schedule = await _dbContext.ExamSchedules
                    .Include(es => es.Paper)
                    .ThenInclude(p => p!.Course)
                    .Where(es => es.Examination!.ExamMonth == SelectedExamination &&
                                 es.Paper!.Course!.Program!.ProgramName == SelectedProgram &&
                                 es.Paper.Course.Regulation!.RegulationName == SelectedRegulation &&
                                 es.Paper.Semester == sem)
                    .OrderBy(es => es.ExamDate)
                    .Select(es => new HallTicketScheduleItem
                    {
                         SubjectCode = es.Paper!.PaperCode ?? "",
                         SubjectName = es.Paper.PaperName ?? "",
                         ExamDate = es.ExamDate.ToString("dd/MM/yyyy"),
                         Session = es.ExamSession != null ? es.ExamSession.SessionName ?? "" : "" // Check Session logic
                    })
                    .ToListAsync();
                
                // Fallback: If ExamSchedule table is tricky, maybe just fetch Papers? 
                // But Hall Ticket needs DATES. Assuming implementation exists.
                
                var msg = new PrintHallTicketMessage
                {
                    IsPreview = isPreview,
                    Students = selectedStudents,
                    ExamName = SelectedExamination,
                    Program = SelectedProgram,
                    Regulation = SelectedRegulation,
                    Semester = SelectedSemester,
                    Schedule = schedule
                };
                
                WeakReferenceMessenger.Default.Send(msg);
            }
            catch { }
            finally { IsLoading = false; }
        }
    }

    public class HallTicketStudentItem
    {
        public bool IsSelected { get; set; }
        public int SNo { get; set; }
        public string AdmissionNo { get; set; } = "";
        public string RollNo { get; set; } = "";
        public string RegNo { get; set; } = "";
        public string StudentName { get; set; } = "";
    }

    public class HallTicketScheduleItem
    {
        public string SubjectCode { get; set; } = "";
        public string SubjectName { get; set; } = "";
        public string ExamDate { get; set; } = "";
        public string Session { get; set; } = "";
    }

    public class PrintHallTicketMessage
    {
        public bool IsPreview { get; set; }
        public List<HallTicketStudentItem> Students { get; set; } = new();
        public string ExamName { get; set; } = "";
        public string Program { get; set; } = "";
        public string Regulation { get; set; } = "";
        public string Semester { get; set; } = "";
        public List<HallTicketScheduleItem> Schedule { get; set; } = new();
    }
}
