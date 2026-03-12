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
    public partial class DeleteExternalMarkViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _dbContext;

        [ObservableProperty] private ObservableCollection<ExamSession> _examSessions = new();
        [ObservableProperty] private ExamSession? _selectedSession;
        [ObservableProperty] private string _regNo = "";
        [ObservableProperty] private string _studentName = "";
        [ObservableProperty] private ObservableCollection<DeleteExternalMarkItem> _markList = new();

        public DeleteExternalMarkViewModel(AcgcetDbContext dbContext)
        {
            _dbContext = dbContext;
            _ = LoadSessionsAsync();
        }

        public DeleteExternalMarkViewModel() { _dbContext = null!; }

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
            await LoadMarks(student.StudentId);
        }

        private async Task LoadMarks(int studentId)
        {
            MarkList.Clear();
            if (SelectedSession == null) { MessageBox.Show("Select Exam Session"); return; }
            var exam = await _dbContext.Examinations.FirstOrDefaultAsync(e => e.ExamMonth == SelectedSession.SessionName);
            if (exam == null) return;

            var marks = await _dbContext.ExternalMarks
                .Include(m => m.Paper)
                .Where(m => m.StudentId == studentId && m.ExaminationId == exam.ExaminationId)
                .ToListAsync();

            foreach(var m in marks)
            {
                MarkList.Add(new DeleteExternalMarkItem
                {
                    PaperCode = m.Paper!.PaperCode ?? "",
                    Mark = m.TotalMark?.ToString() ?? "",
                    ExternalMarkId = m.ExternalMarkId
                });
            }
        }

        [RelayCommand]
        private async Task Delete()
        {
             var selected = MarkList.Where(m => m.IsSelected).ToList();
             if(!selected.Any()) { MessageBox.Show("Select marks"); return; }
             if (MessageBox.Show($"Delete {selected.Count} marks?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
             {
                 var ids = selected.Select(m => m.ExternalMarkId).ToList();
                 var toDelete = await _dbContext.ExternalMarks.Where(m => ids.Contains(m.ExternalMarkId)).ToListAsync();
                 _dbContext.ExternalMarks.RemoveRange(toDelete);
                 await _dbContext.SaveChangesAsync();
                 await Search();
                 MessageBox.Show("Deleted");
             }
        }

        [RelayCommand] private void Clear() { RegNo = ""; StudentName = ""; MarkList.Clear(); }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await Search();
        }
    }

    public partial class DeleteExternalMarkItem : ObservableObject
    {
        public string PaperCode { get; set; } = "";
        public string Mark { get; set; } = "";
        public long ExternalMarkId { get; set; }
        [ObservableProperty] private bool _isSelected;
    }
}
