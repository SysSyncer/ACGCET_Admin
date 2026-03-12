using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ACGCET_Admin.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Linq;

namespace ACGCET_Admin.ViewModels.MissingEntry
{
    public partial class MissingConvertEntryViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _dbContext;
        public ObservableCollection<ExamSession> ExamSessions { get; } = new();
        public ObservableCollection<string> Semesters { get; } = new();
        public ObservableCollection<Paper> Papers { get; } = new();
        [ObservableProperty] private ExamSession? _selectedSession;
        [ObservableProperty] private string _selectedSemester = "";
        [ObservableProperty] private Paper? _selectedPaper;
        [ObservableProperty] private ObservableCollection<MissingEntryItem> _reportData = new();
        [ObservableProperty] private bool _isLoading;

        public MissingConvertEntryViewModel(AcgcetDbContext dbContext)
        {
            _dbContext = dbContext;
            LoadFilters();
        }

        private void LoadFilters()
        {
            var sessions = _dbContext.ExamSessions.OrderByDescending(s => s.ExamSessionId).ToList();
            foreach (var s in sessions) ExamSessions.Add(s);
            for (int i = 1; i <= 8; i++) Semesters.Add(i.ToString());
            LoadPapers();
        }
         private void LoadPapers()
        {
            var papers = _dbContext.Papers.Include(p => p.Course).OrderBy(p => p.PaperCode).Take(500).ToList();
            Papers.Clear();
            foreach (var p in papers) Papers.Add(p);
        }
         partial void OnSelectedSemesterChanged(string value) { LoadPapers(); }

        [RelayCommand]
        private async Task Search()
        {
            if (IsLoading) return;
            IsLoading = true;
            ReportData.Clear();
            try
            {
                if (SelectedSession == null) return;
                var exam = await _dbContext.Examinations.FirstOrDefaultAsync(e => e.ExamMonth == SelectedSession.SessionName);
                if (exam == null) return;

                var query = _dbContext.ExamApplicationPapers
                    .Include(eap => eap.ExamApplication).ThenInclude(ea => ea!.Student)
                    .Include(eap => eap.Paper)
                    .Where(eap => eap.ExamApplication!.ExaminationId == exam.ExaminationId)
                    .AsQueryable();

                if (SelectedPaper != null) query = query.Where(x => x.PaperId == SelectedPaper.PaperId);
                else if (!string.IsNullOrEmpty(SelectedSemester))
                {
                     int sem = int.Parse(SelectedSemester);
                     query = query.Where(x => x.Paper!.Semester == sem);
                }

                var apps = await query.ToListAsync();
                var externalMarks = await _dbContext.ExternalMarks.Where(em => em.ExaminationId == exam.ExaminationId).ToListAsync();
                var results = await _dbContext.ExamResults.Where(r => r.ExaminationId == exam.ExaminationId).ToListAsync();

                foreach (var app in apps)
                {
                     bool hasExt = externalMarks.Any(em => em.StudentId == app.ExamApplication!.StudentId && em.PaperId == app.PaperId);
                     bool hasResult = results.Any(r => r.StudentId == app.ExamApplication!.StudentId && r.PaperId == app.PaperId);
                     
                     // Missing Convert: Has External but missing Result (Calculation Pending)
                     if (hasExt && !hasResult)
                     {
                         ReportData.Add(new MissingEntryItem
                         {
                             RegNo = app.ExamApplication!.Student!.RegistrationNumber ?? "",
                             StudentName = app.ExamApplication.Student.FullName ?? "",
                             PaperCode = app.Paper!.PaperCode ?? "",
                             Status = "Missing Conversion",
                             Course = ""
                         });
                     }
                }
            }
            finally { IsLoading = false; }
        }
        [RelayCommand] private void Print() {}

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await Search();
        }
    }
}
