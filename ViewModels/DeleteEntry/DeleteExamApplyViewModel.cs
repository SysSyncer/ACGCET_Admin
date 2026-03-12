using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ACGCET_Admin.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ACGCET_Admin.ViewModels.DeleteEntry
{
    public partial class DeleteExamApplyViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _dbContext;

        [ObservableProperty] private ObservableCollection<ExamSession> _examSessions = new();
        [ObservableProperty] private ExamSession? _selectedSession;
        
        [ObservableProperty] private string _admissionNo = "";
        [ObservableProperty] private string _rollNo = "";
        [ObservableProperty] private string _regNo = "";
        [ObservableProperty] private string _studentName = "";
        [ObservableProperty] private string _programName = "";
        [ObservableProperty] private string _batch = "";
        [ObservableProperty] private string _section = "";

        [ObservableProperty] 
        private ObservableCollection<DeleteExamPaperItem> _paperList = new();

        public DeleteExamApplyViewModel(AcgcetDbContext dbContext)
        {
            _dbContext = dbContext;
            _ = LoadSessionsAsync();
        }

        public DeleteExamApplyViewModel() { _dbContext = null!; }

        private async Task LoadSessionsAsync()
        {
            try
            {
                var sessions = await _dbContext.ExamSessions.OrderByDescending(s => s.ExamSessionId).ToListAsync();
                ExamSessions.Clear();
                foreach(var s in sessions) ExamSessions.Add(s);
            }
            catch { /* ignore load errors */ }
        }

        [RelayCommand]
        private async Task Search()
        {
            if (string.IsNullOrEmpty(RegNo))
            {
                MessageBox.Show("Please Enter Registration Number");
                return;
            }

            var student = await _dbContext.Students
                .Include(s => s.Course).ThenInclude(c => c!.Program)
                .Include(s => s.Batch)
                .Include(s => s.Section)
                .FirstOrDefaultAsync(s => s.RegistrationNumber == RegNo);

            if (student == null)
            {
                MessageBox.Show("Student Not Found");
                return;
            }

            // Populate Fields
            AdmissionNo = student.AdmissionNumber ?? "";
            RollNo = student.RollNumber ?? "";
            StudentName = student.FullName ?? "";
            ProgramName = student.Course?.Program?.ProgramName ?? "";
            Batch = student.Batch?.BatchName ?? "";
            Section = student.Section?.SectionName ?? "";

            await LoadPapers(student.StudentId);
        }

        private async Task LoadPapers(int studentId)
        {
            PaperList.Clear();
            if (SelectedSession == null) return;
            
            var exam = await _dbContext.Examinations.FirstOrDefaultAsync(e => e.ExamMonth == SelectedSession.SessionName);
            if (exam == null) return; // Or handle differently

            var app = await _dbContext.ExamApplications
                .Include(a => a.ExamApplicationPapers).ThenInclude(p => p.Paper)
                .FirstOrDefaultAsync(a => a.StudentId == studentId && a.ExaminationId == exam.ExaminationId);

            if (app != null)
            {
                foreach(var p in app.ExamApplicationPapers)
                {
                    PaperList.Add(new DeleteExamPaperItem
                    {
                        PaperCode = p.Paper!.PaperCode ?? "",
                        PaperName = p.Paper.PaperName ?? "",
                        ExamApplicationPaperId = p.ExamApplicationPaperId,
                        IsSelected = false
                    });
                }
            }
        }

        [RelayCommand]
        private async Task Delete()
        {
            var selected = PaperList.Where(p => p.IsSelected).ToList();
            if (!selected.Any())
            {
                MessageBox.Show("Please select papers to delete");
                return;
            }

            if (MessageBox.Show($"Are you sure you want to delete {selected.Count} papers?", "Confirm Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var ids = selected.Select(p => p.ExamApplicationPaperId).ToList();
                var papersToDelete = await _dbContext.ExamApplicationPapers.Where(x => ids.Contains(x.ExamApplicationPaperId)).ToListAsync();
                
                _dbContext.ExamApplicationPapers.RemoveRange(papersToDelete);
                await _dbContext.SaveChangesAsync();

                // If Application has no papers left, delete Application? 
                // Let's check
                if (SelectedSession != null && !string.IsNullOrEmpty(RegNo))
                {
                     // Re-loading to check count or just check DB
                     // Logic: If ExamApplication has 0 papers, delete it?
                     // Implement simple reload for now
                     await Search();
                }
                MessageBox.Show("Deleted Successfully");
            }
        }

        [RelayCommand]
        private void Clear()
        {
            RegNo = "";
            AdmissionNo = "";
            RollNo = "";
            StudentName = "";
            ProgramName = "";
            Batch = "";
            Section = "";
            PaperList.Clear();
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await Search();
        }
    }

    public partial class DeleteExamPaperItem : ObservableObject
    {
         public string PaperCode { get; set; } = "";
         public string PaperName { get; set; } = "";
         public int ExamApplicationPaperId { get; set; }
         
         [ObservableProperty]
         private bool _isSelected;
    }
}
