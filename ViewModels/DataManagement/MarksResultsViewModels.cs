using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ACGCET_Admin.Models;
using Microsoft.EntityFrameworkCore;

namespace ACGCET_Admin.ViewModels.DataManagement
{
    // ════════════════════════════════════════════════════
    //  INTERNAL MARKS
    // ════════════════════════════════════════════════════
    public partial class ManageInternalMarksViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _db;

        [ObservableProperty] private string _searchRegNo = "";
        [ObservableProperty] private Student? _foundStudent;
        [ObservableProperty] private Paper? _selectedPaper;
        [ObservableProperty] private TestType? _selectedTestType;
        [ObservableProperty] private int? _semester;
        [ObservableProperty] private decimal? _mark;
        [ObservableProperty] private decimal? _maxMark;

        [ObservableProperty] private ObservableCollection<Paper> _paperList = new();
        [ObservableProperty] private ObservableCollection<TestType> _testTypeList = new();
        [ObservableProperty] private ObservableCollection<InternalMark> _internalMarks = new();
        [ObservableProperty] private InternalMark? _selectedInternalMark;

        public ManageInternalMarksViewModel(AcgcetDbContext db) { _db = db; _ = LoadAsync(); }
        public ManageInternalMarksViewModel() { _db = null!; }

        [RelayCommand]
        private async Task LoadAsync()
        {
            TestTypeList = new ObservableCollection<TestType>(await _db.TestTypes.OrderBy(t => t.TestName).ToListAsync());
            InternalMarks = new ObservableCollection<InternalMark>(
                await _db.InternalMarks
                    .Include(m => m.Student).Include(m => m.Paper).Include(m => m.TestType)
                    .OrderByDescending(m => m.EnteredDate)
                    .Take(200).ToListAsync());
        }

        [RelayCommand]
        private async Task SearchStudent()
        {
            if (string.IsNullOrWhiteSpace(SearchRegNo)) return;
            FoundStudent = await _db.Students
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.RegistrationNumber == SearchRegNo.Trim());

            if (FoundStudent == null) { MessageBox.Show("Student not found.", "Search"); return; }

            // Load papers for this student's course
            if (FoundStudent.CourseId != null)
                PaperList = new ObservableCollection<Paper>(
                    await _db.Papers.Where(p => p.CourseId == FoundStudent.CourseId)
                        .OrderBy(p => p.Semester).ThenBy(p => p.PaperCode).ToListAsync());
        }

        [RelayCommand]
        private async Task Save()
        {
            if (FoundStudent == null) { MessageBox.Show("Search a student first.", "Validation"); return; }
            if (SelectedPaper == null || SelectedTestType == null || Semester == null || Mark == null)
            { MessageBox.Show("Paper, Test Type, Semester, and Mark required.", "Validation"); return; }

            // Check for duplicate
            if (await _db.InternalMarks.AnyAsync(m =>
                m.StudentId == FoundStudent.StudentId &&
                m.PaperId == SelectedPaper.PaperId &&
                m.TestTypeId == SelectedTestType.TestTypeId &&
                m.Semester == Semester))
            { MessageBox.Show("Internal mark already exists for this combination.", "Duplicate"); return; }

            _db.InternalMarks.Add(new InternalMark
            {
                StudentId = FoundStudent.StudentId,
                PaperId = SelectedPaper.PaperId,
                TestTypeId = SelectedTestType.TestTypeId,
                Semester = Semester,
                Mark = Mark,
                MaxMark = MaxMark ?? SelectedTestType.MaxMark,
                EnteredBy = "Admin",
                EnteredDate = DateTime.Now
            });

            try
            {
                await _db.SaveChangesAsync();
                MessageBox.Show("Internal Mark saved!", "Success");
                Clear();
                await LoadAsync();
            }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Save failed: {ex.InnerException?.Message ?? ex.Message}", "Error"); }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedInternalMark == null) return;
            if (MessageBox.Show("Delete this internal mark entry?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            _db.InternalMarks.Remove(SelectedInternalMark);
            try { await _db.SaveChangesAsync(); await LoadAsync(); }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Cannot delete.\n{ex.InnerException?.Message}", "Error"); }
        }

        [RelayCommand]
        private void Clear()
        {
            SearchRegNo = ""; FoundStudent = null; SelectedPaper = null;
            SelectedTestType = null; Semester = null; Mark = null; MaxMark = null;
            SelectedInternalMark = null;
        }
    }

    // ════════════════════════════════════════════════════
    //  EXTERNAL MARKS
    // ════════════════════════════════════════════════════
    public partial class ManageExternalMarksViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _db;

        [ObservableProperty] private string _searchRegNo = "";
        [ObservableProperty] private Student? _foundStudent;
        [ObservableProperty] private Paper? _selectedPaper;
        [ObservableProperty] private Examination? _selectedExamination;
        [ObservableProperty] private decimal? _theoryMark;
        [ObservableProperty] private decimal? _labMark;
        [ObservableProperty] private decimal? _totalMark;

        [ObservableProperty] private ObservableCollection<Paper> _paperList = new();
        [ObservableProperty] private ObservableCollection<Examination> _examinationList = new();
        [ObservableProperty] private ObservableCollection<ExternalMark> _externalMarks = new();
        [ObservableProperty] private ExternalMark? _selectedExternalMark;

        public ManageExternalMarksViewModel(AcgcetDbContext db) { _db = db; _ = LoadAsync(); }
        public ManageExternalMarksViewModel() { _db = null!; }

        [RelayCommand]
        private async Task LoadAsync()
        {
            ExaminationList = new ObservableCollection<Examination>(
                await _db.Examinations.OrderByDescending(e => e.ExamYear).ToListAsync());
            ExternalMarks = new ObservableCollection<ExternalMark>(
                await _db.ExternalMarks
                    .Include(m => m.Student).Include(m => m.Paper).Include(m => m.Examination)
                    .OrderByDescending(m => m.EnteredDate)
                    .Take(200).ToListAsync());
        }

        [RelayCommand]
        private async Task SearchStudent()
        {
            if (string.IsNullOrWhiteSpace(SearchRegNo)) return;
            FoundStudent = await _db.Students
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.RegistrationNumber == SearchRegNo.Trim());

            if (FoundStudent == null) { MessageBox.Show("Student not found.", "Search"); return; }

            if (FoundStudent.CourseId != null)
                PaperList = new ObservableCollection<Paper>(
                    await _db.Papers.Where(p => p.CourseId == FoundStudent.CourseId)
                        .OrderBy(p => p.Semester).ThenBy(p => p.PaperCode).ToListAsync());
        }

        partial void OnTheoryMarkChanged(decimal? value) => ComputeTotal();
        partial void OnLabMarkChanged(decimal? value) => ComputeTotal();

        private void ComputeTotal()
        {
            TotalMark = (TheoryMark ?? 0) + (LabMark ?? 0);
        }

        [RelayCommand]
        private async Task Save()
        {
            if (FoundStudent == null) { MessageBox.Show("Search a student first.", "Validation"); return; }
            if (SelectedPaper == null || SelectedExamination == null)
            { MessageBox.Show("Paper and Examination required.", "Validation"); return; }

            if (await _db.ExternalMarks.AnyAsync(m =>
                m.StudentId == FoundStudent.StudentId &&
                m.PaperId == SelectedPaper.PaperId &&
                m.ExaminationId == SelectedExamination.ExaminationId))
            { MessageBox.Show("External mark already exists for this combination.", "Duplicate"); return; }

            _db.ExternalMarks.Add(new ExternalMark
            {
                StudentId = FoundStudent.StudentId,
                PaperId = SelectedPaper.PaperId,
                ExaminationId = SelectedExamination.ExaminationId,
                TheoryMark = TheoryMark,
                LabMark = LabMark,
                TotalMark = TotalMark,
                EnteredBy = "Admin",
                EnteredDate = DateTime.Now
            });

            try
            {
                await _db.SaveChangesAsync();
                MessageBox.Show("External Mark saved!", "Success");
                Clear();
                await LoadAsync();
            }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Save failed: {ex.InnerException?.Message ?? ex.Message}", "Error"); }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedExternalMark == null) return;
            if (MessageBox.Show("Delete this external mark entry?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            _db.ExternalMarks.Remove(SelectedExternalMark);
            try { await _db.SaveChangesAsync(); await LoadAsync(); }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Cannot delete.\n{ex.InnerException?.Message}", "Error"); }
        }

        [RelayCommand]
        private void Clear()
        {
            SearchRegNo = ""; FoundStudent = null; SelectedPaper = null;
            SelectedExamination = null; TheoryMark = null; LabMark = null;
            TotalMark = null; SelectedExternalMark = null;
        }
    }

    // ════════════════════════════════════════════════════
    //  EXAM RESULTS
    // ════════════════════════════════════════════════════
    public partial class ManageExamResultsViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _db;

        [ObservableProperty] private string _searchRegNo = "";
        [ObservableProperty] private Student? _foundStudent;
        [ObservableProperty] private Paper? _selectedPaper;
        [ObservableProperty] private Examination? _selectedExamination;
        [ObservableProperty] private decimal? _internalTotal;
        [ObservableProperty] private decimal? _externalTotal;
        [ObservableProperty] private decimal? _grandTotal;
        [ObservableProperty] private string _grade = "";
        [ObservableProperty] private ResultStatus? _selectedResultStatus;

        [ObservableProperty] private ObservableCollection<Paper> _paperList = new();
        [ObservableProperty] private ObservableCollection<Examination> _examinationList = new();
        [ObservableProperty] private ObservableCollection<ResultStatus> _resultStatusList = new();
        [ObservableProperty] private ObservableCollection<ExamResult> _examResults = new();
        [ObservableProperty] private ExamResult? _selectedExamResult;

        public ManageExamResultsViewModel(AcgcetDbContext db) { _db = db; _ = LoadAsync(); }
        public ManageExamResultsViewModel() { _db = null!; }

        [RelayCommand]
        private async Task LoadAsync()
        {
            ExaminationList = new ObservableCollection<Examination>(
                await _db.Examinations.OrderByDescending(e => e.ExamYear).ToListAsync());
            ResultStatusList = new ObservableCollection<ResultStatus>(
                await _db.ResultStatuses.OrderBy(r => r.StatusName).ToListAsync());
            ExamResults = new ObservableCollection<ExamResult>(
                await _db.ExamResults
                    .Include(r => r.Student).Include(r => r.Paper)
                    .Include(r => r.Examination).Include(r => r.ResultStatus)
                    .OrderByDescending(r => r.ProcessedDate)
                    .Take(200).ToListAsync());
        }

        [RelayCommand]
        private async Task SearchStudent()
        {
            if (string.IsNullOrWhiteSpace(SearchRegNo)) return;
            FoundStudent = await _db.Students
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.RegistrationNumber == SearchRegNo.Trim());

            if (FoundStudent == null) { MessageBox.Show("Student not found.", "Search"); return; }

            if (FoundStudent.CourseId != null)
                PaperList = new ObservableCollection<Paper>(
                    await _db.Papers.Where(p => p.CourseId == FoundStudent.CourseId)
                        .OrderBy(p => p.Semester).ThenBy(p => p.PaperCode).ToListAsync());
        }

        partial void OnInternalTotalChanged(decimal? value) => GrandTotal = (value ?? 0) + (ExternalTotal ?? 0);
        partial void OnExternalTotalChanged(decimal? value) => GrandTotal = (InternalTotal ?? 0) + (value ?? 0);

        [RelayCommand]
        private async Task Save()
        {
            if (FoundStudent == null) { MessageBox.Show("Search a student first.", "Validation"); return; }
            if (SelectedPaper == null || SelectedExamination == null)
            { MessageBox.Show("Paper and Examination required.", "Validation"); return; }

            if (await _db.ExamResults.AnyAsync(r =>
                r.StudentId == FoundStudent.StudentId &&
                r.PaperId == SelectedPaper.PaperId &&
                r.ExaminationId == SelectedExamination.ExaminationId))
            { MessageBox.Show("Result already exists for this combination.", "Duplicate"); return; }

            _db.ExamResults.Add(new ExamResult
            {
                StudentId = FoundStudent.StudentId,
                PaperId = SelectedPaper.PaperId,
                ExaminationId = SelectedExamination.ExaminationId,
                InternalTotal = InternalTotal,
                ExternalTotal = ExternalTotal,
                GrandTotal = GrandTotal,
                Grade = string.IsNullOrWhiteSpace(Grade) ? null : Grade.Trim().ToUpper(),
                ResultStatusId = SelectedResultStatus?.ResultStatusId,
                ProcessedDate = DateTime.Now,
                CreatedBy = "Admin"
            });

            try
            {
                await _db.SaveChangesAsync();
                MessageBox.Show("Result saved!", "Success");
                Clear();
                await LoadAsync();
            }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Save failed: {ex.InnerException?.Message ?? ex.Message}", "Error"); }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedExamResult == null) return;
            if (MessageBox.Show("Delete this result entry?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            _db.ExamResults.Remove(SelectedExamResult);
            try { await _db.SaveChangesAsync(); await LoadAsync(); }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Cannot delete — revaluation requests may exist.\n{ex.InnerException?.Message}", "Error"); }
        }

        [RelayCommand]
        private void Clear()
        {
            SearchRegNo = ""; FoundStudent = null; SelectedPaper = null;
            SelectedExamination = null; InternalTotal = null; ExternalTotal = null;
            GrandTotal = null; Grade = ""; SelectedResultStatus = null; SelectedExamResult = null;
        }
    }
}
