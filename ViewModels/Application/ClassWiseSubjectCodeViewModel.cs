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
    public partial class ClassWiseSubjectCodeViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _dbContext;

        [ObservableProperty]
        private ObservableCollection<string> _regulations = new() { "Select" };

        [ObservableProperty]
        private ObservableCollection<string> _programs = new() { "Select" };

        [ObservableProperty]
        private string _selectedRegulation = "Select";

        [ObservableProperty]
        private string _selectedProgram = "Select";

        [ObservableProperty]
        private ObservableCollection<SubjectReportItem> _subjectsList = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotLoading))]
        private bool _isLoading;

        public bool IsNotLoading => !IsLoading;

        public ClassWiseSubjectCodeViewModel(AcgcetDbContext dbContext)
        {
            _dbContext = dbContext;
            Task.Run(InitializeAsync);
        }

        public ClassWiseSubjectCodeViewModel()
        {
            _dbContext = null!;
        }

        private async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                List<string> regulations = new List<string> { "Select" };
                List<string> programs = new List<string> { "Select" };

                if (_dbContext != null)
                {
                    // Load Regulations
                    var dbRegulations = await _dbContext.Regulations
                                          .Select(r => r.RegulationName)
                                          .Where(r => r != null)
                                          .ToListAsync();
                    regulations.AddRange(dbRegulations!);

                    // Load Programs
                    var dbPrograms = await _dbContext.Programs
                                         .Select(p => p.ProgramName)
                                         .Where(p => p != null)
                                         .ToListAsync();
                    programs.AddRange(dbPrograms!);
                }

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    Regulations = new ObservableCollection<string>(regulations);
                    Programs = new ObservableCollection<string>(programs);
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
                // Query Papers based on Regulation and Program
                // Note: Schema might need Navigation Property checks
                var query = _dbContext.Papers
                    .Include(p => p.PaperMarkDistribution)
                    .Include(p => p.PaperType)
                    .Include(p => p.Course) // Assuming Course links to Program/Regulation
                    .AsQueryable();

                if (SelectedRegulation != "Select")
                {
                    query = query.Where(p => p.Course!.Regulation!.RegulationName == SelectedRegulation);
                }

                if (SelectedProgram != "Select")
                {
                    // Program is linked via Course?
                    query = query.Where(p => p.Course!.Program!.ProgramName == SelectedProgram);
                }

                query = query.OrderBy(p => p.Semester).ThenBy(p => p.PaperCode);

                var papers = await query.ToListAsync();

                var reportItems = papers.Select(p => new SubjectReportItem
                {
                    Semester = p.Semester.ToString(),
                    PaperCode = p.PaperCode ?? "",
                    PaperName = p.PaperName ?? "",
                    Credits = (p.Credits ?? 0).ToString("0.0"),
                    Type = p.PaperType?.TypeCode ?? "",
                    
                    // Marks (Handle Nullable Decimals safely)
                    IntMax = p.PaperMarkDistribution != null ? ((p.PaperMarkDistribution.InternalTheoryMax ?? 0) + (p.PaperMarkDistribution.InternalLabMax ?? 0)).ToString("0") : "",
                    ExtMax = p.PaperMarkDistribution != null ? ((p.PaperMarkDistribution.ExternalTheoryMax ?? 0) + (p.PaperMarkDistribution.ExternalLabMax ?? 0)).ToString("0") : "",
                    TotalMax = p.PaperMarkDistribution != null ? (p.PaperMarkDistribution.TotalMax ?? 0).ToString("0") : "",

                    IntMin = p.PaperMarkDistribution != null ? ((p.PaperMarkDistribution.InternalTheoryMin ?? 0) + (p.PaperMarkDistribution.InternalLabMin ?? 0)).ToString("0") : "",
                    ExtMin = p.PaperMarkDistribution != null ? ((p.PaperMarkDistribution.ExternalTheoryMin ?? 0) + (p.PaperMarkDistribution.ExternalLabMin ?? 0)).ToString("0") : "",
                    TotalMin = p.PaperMarkDistribution != null ? (p.PaperMarkDistribution.TotalMin ?? 0).ToString("0") : ""
                });

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    SubjectsList = new ObservableCollection<SubjectReportItem>(reportItems);
                });
            }
            catch { }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private void ClearFilters()
        {
            SelectedRegulation = "Select";
            SelectedProgram = "Select";
            SubjectsList.Clear();
        }
        [RelayCommand]
        public void Preview()
        {
            if (SubjectsList.Count == 0) { Services.CustomMessageBox.Show("No data to display"); return; }
            if (SelectedRegulation == "Select") { Services.CustomMessageBox.Show("Select Regulation"); return; }

            var printService = new Services.PrintService();
            printService.GenerateSubjectReport(SubjectsList, SelectedRegulation, SelectedProgram);
        }

        [RelayCommand]
        public void Print()
        {
            Preview();
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await ViewReport();
        }
    }

    public class SubjectReportItem
    {
        public string Semester { get; set; } = "";
        public string PaperCode { get; set; } = "";
        public string PaperName { get; set; } = "";
        public string Credits { get; set; } = "";
        public string Type { get; set; } = "";
        public string IntMax { get; set; } = "";
        public string ExtMax { get; set; } = "";
        public string TotalMax { get; set; } = "";
        public string IntMin { get; set; } = "";
        public string ExtMin { get; set; } = "";
        public string TotalMin { get; set; } = "";
        // Fees not implemented yet
    }
}
