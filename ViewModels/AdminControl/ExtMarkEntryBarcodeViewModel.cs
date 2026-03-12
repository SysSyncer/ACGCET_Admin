using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ACGCET_Admin.Models;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
        private void ProcessBarcode()
        {
            StatusMessage = "";
            if (string.IsNullOrWhiteSpace(Barcode)) return;

            // Find ExamApplicationPaper by Barcode
            // Needs Include?
            var paperApp = _dbContext.ExamApplicationPapers
                .Include(p => p.ExamApplication)
                .ThenInclude(ea => ea!.Student)
                .Include(p => p.Paper)
                .Include(p => p.ExamApplication)
                .ThenInclude(ea => ea!.Examination) 
                .FirstOrDefault(p => p.Barcode == Barcode);

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
            
            // Check if mark exists
            var existingMark = _dbContext.ExternalMarks.FirstOrDefault(m => 
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
        private void SaveMark()
        {
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

            var existingMark = _dbContext.ExternalMarks.FirstOrDefault(m => 
                m.StudentId == _currentStudentId && 
                m.PaperId == _currentPaperId && 
                m.ExaminationId == _currentExamId);

            if (existingMark != null)
            {
                existingMark.TotalMark = markVal;
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
                    TheoryMark = 0, // Default,
                    EnteredBy = "Admin",
                    EnteredDate = DateTime.Now
                    // Theory/Lab separation? Accessing TotalMark for now as per schema
                });
            }

            _dbContext.SaveChanges();
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
