using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ACGCET_Admin.Models;
using ACGCET_Admin.Services;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ACGCET_Admin.ViewModels.AdminControl
{
    public partial class ExtMarkEntryBarcodeViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _dbContext;

        [ObservableProperty]
        private string _barcode = "";

        // Mark input
        [ObservableProperty]
        private string _markInput = "";

        [ObservableProperty]
        private decimal _maxMark = 100;

        // Display Info
        [ObservableProperty]
        private string _studentRegNo = "";

        [ObservableProperty]
        private string _paperCode = "";

        [ObservableProperty]
        private string _paperName = "";

        [ObservableProperty]
        private string _statusMessage = "";

        private int _currentStudentId;
        private int _currentPaperId;
        private int _currentExamId; // Should filter by Active Exam?

        public ExtMarkEntryBarcodeViewModel(AcgcetDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public ExtMarkEntryBarcodeViewModel() { _dbContext = null!; }

        [RelayCommand]
        private async Task ProcessBarcode()
        {
            StatusMessage = "";
            if (string.IsNullOrWhiteSpace(Barcode)) return;

            var paperApp = await _dbContext.ExamApplicationPapers
                .Include(p => p.ExamApplication)
                .ThenInclude(ea => ea!.Student)
                .Include(p => p.Paper)
                .Include(p => p.ExamApplication)
                .ThenInclude(ea => ea!.Examination)
                .FirstOrDefaultAsync(p => p.Barcode == Barcode);

            if (paperApp == null)
            {
                StatusMessage = "Barcode Not Found";
                StudentRegNo = "";
                PaperCode = "";
                PaperName = "";
                return;
            }

            _currentStudentId = paperApp.ExamApplication!.StudentId ?? 0;
            _currentPaperId = paperApp.PaperId ?? 0;
            _currentExamId = paperApp.ExamApplication!.ExaminationId ?? 0;

            StudentRegNo = paperApp.ExamApplication!.Student!.RegistrationNumber ?? "";
            PaperCode = paperApp.Paper!.PaperCode;
            PaperName = paperApp.Paper!.PaperName;

            var existingMark = await _dbContext.ExternalMarks.FirstOrDefaultAsync(m =>
                m.StudentId == _currentStudentId &&
                m.PaperId == _currentPaperId &&
                m.ExaminationId == _currentExamId);

            if (existingMark != null)
            {
                MarkInput = existingMark.TotalMark?.ToString("0.##") ?? "";
                StatusMessage = "Existing Mark Loaded";
            }
            else
            {
                MarkInput = "";
                StatusMessage = "Enter Mark";
            }
        }

        [RelayCommand]
        private async Task SaveMark()
        {
            if (!UserPermissionService.Current.CanCreate("EXT_MARKS"))
            {
                StatusMessage = "Access Denied — insufficient permissions";
                return;
            }

            if (_currentStudentId == 0 || _currentPaperId == 0)
            {
                StatusMessage = "Scan Barcode First";
                return;
            }

            if (!decimal.TryParse(MarkInput, out decimal markVal))
            {
                StatusMessage = "Invalid Mark";
                return;
            }

            if (markVal > MaxMark)
            {
                StatusMessage = "Mark exceeds Max Mark";
                return;
            }

            var existingMark = await _dbContext.ExternalMarks.FirstOrDefaultAsync(m =>
                m.StudentId == _currentStudentId &&
                m.PaperId == _currentPaperId &&
                m.ExaminationId == _currentExamId);

            if (existingMark != null)
            {
                existingMark.TotalMark = markVal;
                existingMark.TheoryMark = markVal;
                existingMark.ModifiedDate = DateTime.Now;
                existingMark.ModifiedBy = "Admin";
            }
            else
            {
                _dbContext.ExternalMarks.Add(new ExternalMark
                {
                    StudentId = _currentStudentId,
                    PaperId = _currentPaperId,
                    ExaminationId = _currentExamId,
                    TotalMark = markVal,
                    TheoryMark = markVal,
                    EnteredBy = "Admin",
                    EnteredDate = DateTime.Now
                });
            }

            await _dbContext.SaveChangesAsync();
            StatusMessage = "Mark Saved";

            // Reset for next
            Barcode = "";
            MarkInput = "";
            StudentRegNo = "";
            // Focus back to Barcode (View should handle)
        }

        [RelayCommand]
        private void Refresh()
        {
            Barcode = "";
            MarkInput = "";
            StudentRegNo = "";
            PaperCode = "";
            PaperName = "";
            StatusMessage = "";
            _currentStudentId = 0;
            _currentPaperId = 0;
            _currentExamId = 0;
        }
    }
}
