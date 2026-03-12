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
using CommunityToolkit.Mvvm.Messaging;

namespace ACGCET_Admin.ViewModels.Application
{
    public partial class A4ResultPrintViewModel : ObservableObject
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
        private string _selectedSection = "Select";

        // Data
        [ObservableProperty]
        private ObservableCollection<A4ResultStudentItem> _students = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotLoading))]
        private bool _isLoading;

        public bool IsNotLoading => !IsLoading;

        public A4ResultPrintViewModel(AcgcetDbContext dbContext)
        {
            _dbContext = dbContext;
            Task.Run(InitializeAsync);
        }

        public A4ResultPrintViewModel()
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
        private async Task ViewStudents()
        {
            if (_dbContext == null) return;
            IsLoading = true;
            try
            {
                var query = _dbContext.Students.AsQueryable();

                if (SelectedProgram != "Select")
                    query = query.Where(s => s.Batch!.Course!.Program!.ProgramName == SelectedProgram);
                if (SelectedRegulation != "Select")
                    query = query.Where(s => s.Regulation!.RegulationName == SelectedRegulation);
                if (SelectedSection != "Select" && SelectedSection != "Overall")
                    query = query.Where(s => s.Section!.SectionName == SelectedSection);

                var students = await query
                    .OrderBy(s => s.RollNumber)
                    .Select(s => new A4ResultStudentItem
                    {
                        SNo = 0,
                        RegistrationNumber = s.RegistrationNumber ?? "",
                        RollNumber = s.RollNumber ?? "",
                        StudentName = s.FullName ?? ""
                    })
                    .ToListAsync();

                int sno = 1;
                foreach (var student in students)
                {
                    student.SNo = sno++;
                }

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    Students = new ObservableCollection<A4ResultStudentItem>(students);
                });
            }
            catch { }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private void PrintResults(bool isPreview)
        {
            if (Students == null || !Students.Any()) return;
            
            var msg = new PrintA4ResultMessage
            {
                IsPreview = isPreview,
                Students = Students.ToList(),
                ExamName = SelectedExamination,
                Program = SelectedProgram,
                Regulation = SelectedRegulation,
                Section = SelectedSection
            };
            
            WeakReferenceMessenger.Default.Send(msg);
        }

        [RelayCommand]
        private void PrintPreview() => PrintResults(true);

        [RelayCommand]
        private void Print() => PrintResults(false);

        [RelayCommand]
        private void ClearFilters()
        {
            SelectedExamination = "Select";
            SelectedLevel = "Select";
            SelectedProgram = "Select";
            SelectedRegulation = "Select";
            SelectedSection = "Select";
            Students.Clear();
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await ViewStudents();
        }
    }

    public class A4ResultStudentItem
    {
        public int SNo { get; set; }
        public string RegistrationNumber { get; set; } = "";
        public string RollNumber { get; set; } = "";
        public string StudentName { get; set; } = "";
    }

    public class PrintA4ResultMessage
    {
        public bool IsPreview { get; set; }
        public List<A4ResultStudentItem> Students { get; set; } = new();
        public string ExamName { get; set; } = "";
        public string Program { get; set; } = "";
        public string Regulation { get; set; } = "";
        public string Section { get; set; } = "";
    }
}
