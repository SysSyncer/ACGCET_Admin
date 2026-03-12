using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ACGCET_Admin.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ACGCET_Admin.ViewModels.DeleteEntry
{
    public partial class DeleteStudentMasterViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _dbContext;

        [ObservableProperty] private string _regNo = "";
        [ObservableProperty] private string _studentName = "";
        [ObservableProperty] private string _programName = "";
        [ObservableProperty] private string _batch = "";

        public DeleteStudentMasterViewModel(AcgcetDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [RelayCommand]
        private async Task Search()
        {
            if (string.IsNullOrEmpty(RegNo)) return;
            var student = await _dbContext.Students
                .Include(s => s.Course).ThenInclude(c => c!.Program)
                .Include(s => s.Batch)
                .FirstOrDefaultAsync(s => s.RegistrationNumber == RegNo);

            if (student == null) { MessageBox.Show("Student Not Found"); return; }
            StudentName = student.FullName ?? "";
            ProgramName = student.Course?.Program?.ProgramName ?? "";
            Batch = student.Batch?.BatchName ?? "";
        }

        [RelayCommand]
        private async Task Delete()
        {
             if (string.IsNullOrEmpty(RegNo)) return;
             if (MessageBox.Show($"Are you sure you want to PERMANENTLY delete student {RegNo}? This might delete related records if supported, or fail if data exists.", "Confirm Delete Student", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
             {
                 var student = await _dbContext.Students.FirstOrDefaultAsync(s => s.RegistrationNumber == RegNo);
                 if (student != null)
                 {
                     _dbContext.Students.Remove(student);
                     try
                     {
                         await _dbContext.SaveChangesAsync();
                         MessageBox.Show("Student Deleted Successfully");
                         Clear();
                     }
                     catch (DbUpdateException ex)
                     {
                         MessageBox.Show($"Delete Failed. Likely due to existing records (Marks, Exam Applications). Please delete them first.\nError: {ex.InnerException?.Message ?? ex.Message}");
                     }
                 }
             }
        }

        [RelayCommand]
        private void Clear()
        {
            RegNo = ""; StudentName = ""; ProgramName = ""; Batch = "";
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await Search();
        }
    }
}
