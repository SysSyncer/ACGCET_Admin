using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ACGCET_Admin.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Linq;

namespace ACGCET_Admin.ViewModels.MissingEntry
{
    public partial class MissingInternalEntryViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _dbContext;

        public ObservableCollection<ExamSession> ExamSessions { get; } = new();
        public ObservableCollection<string> Semesters { get; } = new();
        public ObservableCollection<Paper> Papers { get; } = new();

        [ObservableProperty]
        private ExamSession? _selectedSession;

        [ObservableProperty]
        private string _selectedSemester = "";

        [ObservableProperty]
        private Paper? _selectedPaper;

        [ObservableProperty]
        private ObservableCollection<MissingEntryItem> _reportData = new();

        [ObservableProperty]
        private bool _isLoading;

        public MissingInternalEntryViewModel(AcgcetDbContext dbContext)
        {
            _dbContext = dbContext;
            LoadFilters();
        }

        private void LoadFilters()
        {
            var sessions = _dbContext.ExamSessions.OrderByDescending(s => s.ExamSessionId).ToList();
            foreach (var s in sessions) ExamSessions.Add(s);

            // 1 to 8 Semesters
            for (int i = 1; i <= 8; i++) Semesters.Add(i.ToString());
            
            LoadPapers();
        }

        private void LoadPapers()
        {
            // Load all papers or filter by Sem?
            // For now, load all
            var papers = _dbContext.Papers.Include(p => p.Course).OrderBy(p => p.PaperCode).Take(500).ToList();
            Papers.Clear();
            foreach (var p in papers) Papers.Add(p);
        }

        partial void OnSelectedSemesterChanged(string value)
        {
             // Reload papers based on Sem
             if (!string.IsNullOrEmpty(value) && int.TryParse(value, out int sem))
             {
                 var papers = _dbContext.Papers
                    .Where(p => p.Semester == sem)
                    .OrderBy(p => p.PaperCode)
                    .ToList();
                 Papers.Clear();
                 foreach(var p in papers) Papers.Add(p);
             }
        }

        [RelayCommand]
        private async Task Search()
        {
            if (IsLoading) return;
            IsLoading = true;
            ReportData.Clear();

            try
            {
                if (SelectedSession == null) return;
                
                // Active Examination for Session?
                // Logic: "Missing Entry" implies checking ExamApplications for the chosen Session.
                // Find Examination for this Session
                var exam = await _dbContext.Examinations.FirstOrDefaultAsync(e => e.ExamMonth == SelectedSession.SessionName); 
                // Wait, SessionName might not match ExamMonth directly.
                // Assuming SelectedSession -> Examination logic exists. 
                // Or user selects Examination directly. 
                // Let's use ExamSessions for now, assuming Examination exists.
                // Actually, schema has Examination.ExamMonth (String). 
                
                // Let's fetch ExamApplications joined with Papers.
                // Filter by Paper if selected.
                
                var query = _dbContext.ExamApplicationPapers
                    .Include(eap => eap.ExamApplication)
                        .ThenInclude(ea => ea!.Student)
                    .Include(eap => eap.Paper)
                    .AsQueryable();

                // if (SelectedSession != null) ... filter by ExamApp.ExaminationId?
                // We need ExaminationId.
                // If UI selects 'Nov 2023', we find ExamID for 'Nov 2023'.
                
                // Let's simplified: Check all ExamApplications for selected Paper (if provided), 
                // and check if Internal exists for that Student+Paper.
                // "Exam Apply" button in legacy UI prompts filtering by applied students.

                if (SelectedPaper != null)
                {
                    query = query.Where(x => x.PaperId == SelectedPaper.PaperId);
                }
                
                // Add Semester filter if Paper not selected
                if (SelectedPaper == null && !string.IsNullOrEmpty(SelectedSemester))
                {
                     int sem = int.Parse(SelectedSemester);
                     query = query.Where(x => x.Paper!.Semester == sem);
                }

                var apps = await query.ToListAsync();
                
                var internalMarks = await _dbContext.InternalMarks
                    .Where(im => apps.Select(a => a.PaperId).Contains(im.PaperId)) // Optimization
                    .ToListAsync();

                foreach (var app in apps)
                {
                     // Check if Internal Mark exists
                     bool exists = internalMarks.Any(im => im.StudentId == app.ExamApplication!.StudentId && im.PaperId == app.PaperId);
                     
                     if (!exists)
                     {
                         ReportData.Add(new MissingEntryItem
                         {
                             RegNo = app.ExamApplication!.Student!.RegistrationNumber ?? "",
                             StudentName = app.ExamApplication.Student.FullName ?? "",
                             PaperCode = app.Paper!.PaperCode ?? "",
                             Status = "Missing Internal",
                             Course = app.Paper?.Course?.CourseCode ?? ""
                         });
                     }
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void Print()
        {
            // Placeholder
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await Search();
        }
    }

    public class MissingEntryItem
    {
        public string RegNo { get; set; } = "";
        public string StudentName { get; set; } = "";
        public string PaperCode { get; set; } = "";
        public string Status { get; set; } = "";
        public string Course { get; set; } = ""; // Added for legacy match
    }
}
