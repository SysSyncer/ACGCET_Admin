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
    public partial class DeleteFinalResultViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _dbContext;

        [ObservableProperty] private ObservableCollection<ExamSession> _examSessions = new();
        [ObservableProperty] private ExamSession? _selectedSession;
        [ObservableProperty] private string _regNo = "";
        [ObservableProperty] private string _studentName = "";
        [ObservableProperty] private ObservableCollection<DeleteResultItem> _resultList = new();

        public DeleteFinalResultViewModel(AcgcetDbContext dbContext)
        {
            _dbContext = dbContext;
            _ = LoadSessionsAsync();
        }

        public DeleteFinalResultViewModel() { _dbContext = null!; }

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
            if (string.IsNullOrEmpty(RegNo)) return;
            var student = await _dbContext.Students.FirstOrDefaultAsync(s => s.RegistrationNumber == RegNo);
            if (student == null) { MessageBox.Show("Student Not Found"); return; }
            StudentName = student.FullName ?? "";
            await LoadResults(student.StudentId);
        }

        private async Task LoadResults(int studentId)
        {
            ResultList.Clear();
            if (SelectedSession == null) { MessageBox.Show("Select Session"); return; }
            var exam = await _dbContext.Examinations.FirstOrDefaultAsync(e => e.ExamMonth == SelectedSession.SessionName);
            if (exam == null) return;

            var results = await _dbContext.ExamResults
                .Include(r => r.Paper)
                .Where(r => r.StudentId == studentId && r.ExaminationId == exam.ExaminationId)
                .ToListAsync();

            foreach(var r in results)
            {
                ResultList.Add(new DeleteResultItem
                {
                    PaperCode = r.Paper!.PaperCode ?? "",
                    Result = r.Grade ?? "", 
                    ExamResultId = r.ExamResultId
                });
            }
        }

        [RelayCommand]
        private async Task Delete()
        {
            var selected = ResultList.Where(r => r.IsSelected).ToList();
            if (!selected.Any()) { MessageBox.Show("Select results"); return; }
            if (MessageBox.Show($"Delete {selected.Count} final results?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var ids = selected.Select(r => r.ExamResultId).ToList();
                var toDelete = await _dbContext.ExamResults.Where(r => ids.Contains(r.ExamResultId)).ToListAsync();
                _dbContext.ExamResults.RemoveRange(toDelete);
                await _dbContext.SaveChangesAsync();
                await Search();
                MessageBox.Show("Deleted");
            }
        }

        [RelayCommand] private void Clear() { RegNo = ""; StudentName = ""; ResultList.Clear(); }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await Search();
        }
    }
}
