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
using System.Data;
using CommunityToolkit.Mvvm.Messaging;

namespace ACGCET_Admin.ViewModels.Application
{
    public partial class YearWiseIntExtReportViewModel : ObservableObject
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
        private DataView? _reportData;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotLoading))]
        private bool _isLoading;

        public bool IsNotLoading => !IsLoading;

        public YearWiseIntExtReportViewModel(AcgcetDbContext dbContext)
        {
            _dbContext = dbContext;
            Task.Run(InitializeAsync);
        }

        public YearWiseIntExtReportViewModel()
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
                // Fetch Students
                var query = _dbContext.Students.AsQueryable();

                if (SelectedProgram != "Select") 
                    query = query.Where(s => s.Batch!.Course!.Program!.ProgramName == SelectedProgram);
                if (SelectedRegulation != "Select")
                    query = query.Where(s => s.Regulation!.RegulationName == SelectedRegulation);
                if (SelectedSection != "Select" && SelectedSection != "Overall")
                    query = query.Where(s => s.Section!.SectionName == SelectedSection);
                
                // Semester filter? Assuming current batch matches semester or use user input.
                // We also need Papers for this semester to build columns.

                var students = await query.OrderBy(s => s.RollNumber).ToListAsync();
                if (!students.Any()) 
                {
                     ReportData = null;
                     return;
                }

                // Fetch Papers for the selected Semester/Regulation/Program
                // Need to parse SelectedSemester
                int sem = int.TryParse(SelectedSemester, out int s) ? s : 0;
                
                var papers = await _dbContext.Papers
                    .Include(p => p.Course)
                    .Where(p => p.Course!.Program!.ProgramName == SelectedProgram &&
                                p.Course.Regulation!.RegulationName == SelectedRegulation &&
                                p.Semester == sem)
                    .OrderBy(p => p.PaperCode)
                    .ToListAsync();

                // Fetch Marks
                // This is heavy. Optimized: Fetch all marks for these students and papers in one go if possible.
                // Assuming 'InternalMarks' table and maybe 'ExternalMarks' table (not defined in context but inferred from STU_REPORT6 query 'StuExtValidityConv').
                // Since I don't have 'ExternalMarks' mapped in valid DB Context derived from previous turns (only InternalMarks), 
                // I will placeholder the External Marks logic or assume a table exists.
                // Actually, I'll check `AcgcetDbContext` context if I can view it.
                // But for now, I'll assume I can access InternalMarks.
                // 'StuExtValidityConv' logical equivalent?
                
                // Create DataTable
                var dt = new DataTable();
                dt.Columns.Add("SNo");
                dt.Columns.Add("RegNo");
                dt.Columns.Add("Name");

                foreach (var paper in papers)
                {
                    dt.Columns.Add($"{paper.PaperCode}\nInt");
                    dt.Columns.Add($"{paper.PaperCode}\nExt");
                    dt.Columns.Add($"{paper.PaperCode}\nTot");
                    dt.Columns.Add($"{paper.PaperCode}\nRes");
                }

                // Pre-fetch all marks for efficiency
                var studentIds = students.Select(s => (int?)s.StudentId).ToList();
                var paperIds   = papers.Select(p => (int?)p.PaperId).ToList();

                var examination = SelectedExamination != "Select"
                    ? await _dbContext.Examinations
                        .FirstOrDefaultAsync(e => e.ExamMonth == SelectedExamination)
                    : null;

                var internalMarks = await _dbContext.InternalMarks
                    .Where(m => studentIds.Contains(m.StudentId) && paperIds.Contains(m.PaperId))
                    .ToListAsync();

                var externalMarks = examination != null
                    ? await _dbContext.ExternalMarks
                        .Where(m => studentIds.Contains(m.StudentId) && paperIds.Contains(m.PaperId)
                                    && m.ExaminationId == examination.ExaminationId)
                        .ToListAsync()
                    : new List<ExternalMark>();

                var examResults = examination != null
                    ? await _dbContext.ExamResults
                        .Include(r => r.ResultStatus)
                        .Where(r => studentIds.Contains(r.StudentId) && paperIds.Contains(r.PaperId)
                                    && r.ExaminationId == examination.ExaminationId)
                        .ToListAsync()
                    : new List<ExamResult>();

                int i = 1;
                foreach (var student in students)
                {
                    var row = dt.NewRow();
                    row["SNo"] = i++;
                    row["RegNo"] = student.RegistrationNumber ?? "";
                    row["Name"] = student.FullName ?? "";

                    foreach (var paper in papers)
                    {
                        string pCode = paper.PaperCode ?? "";
                        // Sum all internal marks for this student+paper
                        decimal intTotal = internalMarks
                            .Where(m => m.StudentId == student.StudentId && m.PaperId == paper.PaperId)
                            .Sum(m => m.Mark ?? 0);
                        decimal? extMark = externalMarks
                            .FirstOrDefault(m => m.StudentId == student.StudentId && m.PaperId == paper.PaperId)
                            ?.TotalMark;
                        var result = examResults
                            .FirstOrDefault(r => r.StudentId == student.StudentId && r.PaperId == paper.PaperId);

                        row[$"{pCode}\nInt"] = intTotal > 0 ? intTotal.ToString("0") : "";
                        row[$"{pCode}\nExt"] = extMark.HasValue ? extMark.Value.ToString("0") : "";
                        row[$"{pCode}\nTot"] = result?.GrandTotal.HasValue == true ? result.GrandTotal.Value.ToString("0") : "";
                        row[$"{pCode}\nRes"] = result?.Grade ?? "";
                    }
                    dt.Rows.Add(row);
                }

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    ReportData = dt.DefaultView;
                });
            }
            catch { }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private void PrintPreview() 
        {
            // Print Logic for DataTable
        }

        [RelayCommand]
        private void Print() 
        {
        }

        [RelayCommand]
        private void ClearFilters()
        {
            SelectedExamination = "Select";
            SelectedLevel = "Select";
            SelectedProgram = "Select";
            SelectedRegulation = "Select";
            SelectedSemester = "Select";
            SelectedSection = "Select";
            ReportData = null;
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await ViewReport();
        }
    }
}
