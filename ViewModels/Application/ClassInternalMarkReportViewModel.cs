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
    public partial class ClassInternalMarkReportViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _dbContext;

        // Filters
        [ObservableProperty]
        private ObservableCollection<string> _levels = new() { "Select", "UG", "PG" };

        [ObservableProperty]
        private ObservableCollection<string> _programs = new() { "Select" };

        [ObservableProperty]
        private ObservableCollection<string> _regulations = new() { "Select" }; // Batch in legacy

        [ObservableProperty]
        private ObservableCollection<string> _semesters = new() { "Select", "1", "2", "3", "4", "5", "6", "7", "8" };

        [ObservableProperty]
        private ObservableCollection<string> _sections = new() { "Select", "Overall", "A", "B", "C", "D" };

        [ObservableProperty]
        private ObservableCollection<PaperDto> _papersList = new();

        // Selected Values
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

        [ObservableProperty]
        private PaperDto? _selectedPaper;

        // Report Data
        [ObservableProperty]
        private ObservableCollection<StudentMarkItem> _reportData = new();
        
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotLoading))]
        private bool _isLoading;

        public bool IsNotLoading => !IsLoading;

        public ClassInternalMarkReportViewModel(AcgcetDbContext dbContext)
        {
            _dbContext = dbContext;
            Task.Run(InitializeAsync);
        }

        public ClassInternalMarkReportViewModel()
        {
             _dbContext = null!;
        }

        private async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                List<string> programs = new List<string> { "Select" };
                List<string> regulations = new List<string> { "Select" };

                if (_dbContext != null)
                {
                     var dbPrograms = await _dbContext.Programs.Select(p => p.ProgramName).Where(p => p != null).ToListAsync();
                     programs.AddRange(dbPrograms!);

                     var dbRegs = await _dbContext.Regulations.Select(r => r.RegulationName).Where(r => r != null).ToListAsync();
                     regulations.AddRange(dbRegs!);
                }

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    Programs = new ObservableCollection<string>(programs);
                    Regulations = new ObservableCollection<string>(regulations);
                });
            }
            catch { }
            finally { IsLoading = false; }
        }

        // Trigger loading papers when dependencies change
        partial void OnSelectedProgramChanged(string value) => LoadPapers();
        partial void OnSelectedRegulationChanged(string value) => LoadPapers();
        partial void OnSelectedSemesterChanged(string value) => LoadPapers();

        private void LoadPapers()
        {
            if (SelectedProgram == "Select" || SelectedRegulation == "Select" || SelectedSemester == "Select")
            {
                PapersList.Clear();
                return;
            }

            Task.Run(async () =>
            {
                try
                {
                    IsLoading = true;
                    int sem = int.Parse(SelectedSemester);
                    
                    var papers = await _dbContext.Papers
                        .Include(p => p.Course)
                        .Where(p => p.Course!.Program!.ProgramName == SelectedProgram &&
                                    p.Course.Regulation!.RegulationName == SelectedRegulation &&
                                    p.Semester == sem)
                        .Select(p => new PaperDto 
                        { 
                            PaperId = p.PaperId, 
                            PaperCode = p.PaperCode, 
                            PaperName = p.PaperName 
                        })
                        .ToListAsync();

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        PapersList = new ObservableCollection<PaperDto>(papers);
                    });
                }
                catch { }
                finally { IsLoading = false; }
            });
        }

        [RelayCommand]
        private async Task ViewReport()
        {
            if (_dbContext == null || SelectedPaper == null) return;

            IsLoading = true;
            try
            {
                var query = _dbContext.Students
                    .Where(s => s.Batch!.Course!.Program!.ProgramName == SelectedProgram &&
                                s.Regulation!.RegulationName == SelectedRegulation);

                if (SelectedSection != "Select" && SelectedSection != "Overall")
                {
                    query = query.Where(s => s.Section!.SectionName == SelectedSection);
                }

                var students = await query.OrderBy(s => s.RegistrationNumber).ToListAsync();
                var paperId = SelectedPaper.PaperId;

                // Load Marks
                var marks = await _dbContext.InternalMarks
                    .Where(m => m.PaperId == paperId)
                    .ToListAsync();

                var report = new List<StudentMarkItem>();
                int i = 1;

                foreach (var s in students)
                {
                    var markEntry = marks.FirstOrDefault(m => m.StudentId == s.StudentId);
                    report.Add(new StudentMarkItem
                    {
                        SNo = i++,
                        RegNo = s.RegistrationNumber,
                        RollNo = s.RollNumber,
                        StudentName = s.FullName,
                        InternalMark = markEntry?.Mark?.ToString("0") ?? "ABS" // Assuming 0 or null is Absent/Empty
                    });
                }

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    ReportData = new ObservableCollection<StudentMarkItem>(report);
                });
            }
            catch { }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private void ClearFilters()
        {
            SelectedLevel = "Select";
            SelectedProgram = "Select";
            SelectedRegulation = "Select";
            SelectedSemester = "Select";
            SelectedSection = "Select";
            SelectedPaper = null;
            ReportData.Clear();
            PapersList.Clear();
        }

        // Print Logic will be handled by View or Service, usually Command on VM triggers View Service
        // For now, we will add a PlaceHolder Command
        [RelayCommand]
        private void PrintPreview()
        {
            // Logic to be implemented (passed to View code-behind or Service)
            WeakReferenceMessenger.Default.Send(new PrintReportMessage { IsPreview = true, ReportItems = ReportData, Title = $"Internal Mark Report - {SelectedPaper?.PaperName ?? ""}" });
        }
        
        [RelayCommand]
        private void Print()
        {
             WeakReferenceMessenger.Default.Send(new PrintReportMessage { IsPreview = false, ReportItems = ReportData, Title = $"Internal Mark Report - {SelectedPaper?.PaperName ?? ""}" });
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await ViewReport();
        }
    }

    public class PaperDto
    {
        public int PaperId { get; set; }
        public string? PaperCode { get; set; }
        public string? PaperName { get; set; }
        public string DisplayName => $"{PaperCode} - {PaperName}";
    }

    public class StudentMarkItem
    {
        public int SNo { get; set; }
        public string? RegNo { get; set; }
        public string? RollNo { get; set; }
        public string? StudentName { get; set; }
        public string? InternalMark { get; set; }
    }
    
    // Message for Printing
    public class PrintReportMessage
    {
        public bool IsPreview { get; set; }
        public IEnumerable<StudentMarkItem> ReportItems { get; set; } = new List<StudentMarkItem>();
        public string Title { get; set; } = string.Empty;
    }
}
