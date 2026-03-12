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
    public abstract partial class DeleteCourseBaseViewModel : ObservableObject
    {
        protected readonly AcgcetDbContext _dbContext;
        protected readonly string _targetPaperType;

        [ObservableProperty] private string _regNo = "";
        [ObservableProperty] private string _studentName = "";
        [ObservableProperty] private ObservableCollection<DeleteBookItem> _paperList = new();

        public DeleteCourseBaseViewModel(AcgcetDbContext dbContext, string targetPaperType)
        {
            _dbContext = dbContext;
            _targetPaperType = targetPaperType;
        }

        [RelayCommand]
        protected async Task Search()
        {
            if (string.IsNullOrEmpty(RegNo)) return;
            var student = await _dbContext.Students.FirstOrDefaultAsync(s => s.RegistrationNumber == RegNo);
            if (student == null) { MessageBox.Show("Student Not Found"); return; }
            StudentName = student.FullName ?? "";
            await LoadPapers(student.StudentId);
        }

        private async Task LoadPapers(int studentId)
        {
            PaperList.Clear();
            var papers = await _dbContext.ExamApplicationPapers
                .Include(p => p.Paper).ThenInclude(pt => pt!.PaperType)
                .Where(p => p.ExamApplication!.StudentId == studentId && p.Paper!.PaperType!.TypeName == _targetPaperType)
                .ToListAsync();

            foreach(var p in papers)
            {
                PaperList.Add(new DeleteBookItem
                {
                    PaperCode = p.Paper!.PaperCode ?? "",
                    PaperName = p.Paper.PaperName ?? "",
                    Id = p.ExamApplicationPaperId
                });
            }
        }

        [RelayCommand]
        protected async Task Delete()
        {
             var selected = PaperList.Where(p => p.IsSelected).ToList();
             if (!selected.Any()) { MessageBox.Show("Select papers"); return; }
             if (MessageBox.Show($"Delete {selected.Count} entries?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
             {
                 var ids = selected.Select(p => p.Id).ToList();
                 var toDelete = await _dbContext.ExamApplicationPapers.Where(x => ids.Contains(x.ExamApplicationPaperId)).ToListAsync();
                 _dbContext.ExamApplicationPapers.RemoveRange(toDelete);
                 await _dbContext.SaveChangesAsync();
                 await Search();
                 MessageBox.Show("Deleted");
             }
        }

        [RelayCommand] protected void Clear() { RegNo = ""; StudentName = ""; PaperList.Clear(); }
    }

    public partial class DeleteBookItem : ObservableObject
    {
        public string PaperCode { get; set; } = "";
        public string PaperName { get; set; } = "";
        public long Id { get; set; }
        [ObservableProperty] private bool _isSelected;
    }
}
