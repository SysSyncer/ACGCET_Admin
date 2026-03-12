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
    public partial class DeleteInternalMarkViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _dbContext;

        [ObservableProperty] private ObservableCollection<ExamSession> _examSessions = new();
        [ObservableProperty] private ExamSession _selectedSession = null!;
        
        [ObservableProperty] private string _regNo = "";
        [ObservableProperty] private string _studentName = "";

        [ObservableProperty] 
        private ObservableCollection<DeleteInternalMarkItem> _markList = new();

        public DeleteInternalMarkViewModel(AcgcetDbContext dbContext)
        {
            _dbContext = dbContext;
            _ = LoadSessionsAsync();
        }

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
            if (student == null)
            {
                MessageBox.Show("Student Not Found");
                return;
            }
            StudentName = student.FullName;

            await LoadMarks(student.StudentId);
        }

        private async Task LoadMarks(int studentId)
        {
             MarkList.Clear();
             // Internal Marks likely not tied strictly to ExamSession in DB schema (no examId in InternalMarks), 
             // but filtered by Papers registered in that Session?
             // Or just show all Internal Marks for that student?
             // Usually Internal Marks are per semester.
             // If User selects Session, maybe filter by Papers in that session?
             
             // Schema: InternalMarks (StudentId, PaperId, TestTypeId, Semester...)
             // It doesn't have ExamId.
             // But we can filter by papers registered in selected Exam Session?
             // Or just show all.
             // I'll show all for now, or filter if Session is selected.
             
             var query = _dbContext.InternalMarks
                 .Include(m => m.Paper)
                 .Include(m => m.TestType)
                 .Where(m => m.StudentId == studentId);
                 
             var marks = await query.ToListAsync();
             foreach(var m in marks)
             {
                 MarkList.Add(new DeleteInternalMarkItem
                 {
                     PaperCode = m.Paper!.PaperCode ?? "",
                     TestName = m.TestType!.TestName ?? "",
                     Mark = m.Mark?.ToString() ?? "Abs",
                     InternalMarkId = m.InternalMarkId
                 });
             }
        }

        [RelayCommand]
        private async Task Delete()
        {
            var selected = MarkList.Where(m => m.IsSelected).ToList();
            if(!selected.Any()) { MessageBox.Show("Select marks to delete"); return; }

            if (MessageBox.Show($"Delete {selected.Count} marks?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var ids = selected.Select(m => m.InternalMarkId).ToList();
                var toDelete = await _dbContext.InternalMarks.Where(m => ids.Contains(m.InternalMarkId)).ToListAsync();
                _dbContext.InternalMarks.RemoveRange(toDelete);
                await _dbContext.SaveChangesAsync();
                
                await Search(); // Reload
                MessageBox.Show("Deleted");
            }
        }

        [RelayCommand]
        private void Clear()
        {
            RegNo = "";
            StudentName = "";
            MarkList.Clear();
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await Search();
        }
    }

    public partial class DeleteInternalMarkItem : ObservableObject
    {
        public string PaperCode { get; set; } = "";
        public string TestName { get; set; } = "";
        public string Mark { get; set; } = "";
        public long InternalMarkId { get; set; }
        
        [ObservableProperty] private bool _isSelected;
    }
}
