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
    //  COURSES
    // ════════════════════════════════════════════════════
    public partial class ManageCoursesViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _db;

        [ObservableProperty] private string _courseName = "";
        [ObservableProperty] private string _courseCode = "";
        [ObservableProperty] private int? _durationYears;
        [ObservableProperty] private int? _totalSemesters;
        [ObservableProperty] private Degree? _selectedDegree;
        [ObservableProperty] private Program? _selectedProgram;
        [ObservableProperty] private Regulation? _selectedRegulation;
        [ObservableProperty] private ObservableCollection<Degree> _degreeList = new();
        [ObservableProperty] private ObservableCollection<Program> _programList = new();
        [ObservableProperty] private ObservableCollection<Regulation> _regulationList = new();
        [ObservableProperty] private ObservableCollection<Course> _courses = new();
        [ObservableProperty] private Course? _selectedCourse;

        public ManageCoursesViewModel(AcgcetDbContext db) { _db = db; _ = LoadAsync(); }
        public ManageCoursesViewModel() { _db = null!; }

        partial void OnSelectedDegreeChanged(Degree? value)
        {
            if (value != null)
                ProgramList = new ObservableCollection<Program>(
                    _db.Programs.Where(p => p.DegreeId == value.DegreeId).OrderBy(p => p.ProgramName).ToList());
            else
                ProgramList.Clear();
            SelectedProgram = null;
        }

        [RelayCommand]
        private async Task LoadAsync()
        {
            DegreeList = new ObservableCollection<Degree>(await _db.Degrees.OrderBy(d => d.DegreeName).ToListAsync());
            RegulationList = new ObservableCollection<Regulation>(await _db.Regulations.OrderByDescending(r => r.RegulationYear).ToListAsync());
            Courses = new ObservableCollection<Course>(
                await _db.Courses.Include(c => c.Degree).Include(c => c.Program).Include(c => c.Regulation)
                    .OrderBy(c => c.CourseName).ToListAsync());
        }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(CourseName) || string.IsNullOrWhiteSpace(CourseCode))
            { MessageBox.Show("Course Name and Code required.", "Validation"); return; }

            if (await _db.Courses.AnyAsync(c => c.CourseCode == CourseCode))
            { MessageBox.Show("Course Code already exists.", "Duplicate"); return; }

            _db.Courses.Add(new Course
            {
                CourseName = CourseName.Trim(),
                CourseCode = CourseCode.Trim().ToUpper(),
                DegreeId = SelectedDegree?.DegreeId,
                ProgramId = SelectedProgram?.ProgramId,
                RegulationId = SelectedRegulation?.RegulationId,
                DurationYears = DurationYears,
                TotalSemesters = TotalSemesters
            });

            try
            {
                await _db.SaveChangesAsync();
                MessageBox.Show("Course added!", "Success");
                Clear();
                await LoadAsync();
            }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Save failed: {ex.InnerException?.Message ?? ex.Message}", "Error"); }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedCourse == null) return;
            if (MessageBox.Show($"Delete '{SelectedCourse.CourseName}'?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            _db.Courses.Remove(SelectedCourse);
            try { await _db.SaveChangesAsync(); await LoadAsync(); }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Cannot delete — referenced.\n{ex.InnerException?.Message}", "Error"); }
        }

        [RelayCommand]
        private void Clear()
        {
            CourseName = ""; CourseCode = ""; DurationYears = null; TotalSemesters = null;
            SelectedDegree = null; SelectedProgram = null; SelectedRegulation = null; SelectedCourse = null;
        }
    }

    // ════════════════════════════════════════════════════
    //  BATCHES
    // ════════════════════════════════════════════════════
    public partial class ManageBatchesViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _db;

        [ObservableProperty] private int _batchYear;
        [ObservableProperty] private string _batchName = "";
        [ObservableProperty] private DateTime? _startDate;
        [ObservableProperty] private DateTime? _expectedEndDate;
        [ObservableProperty] private Course? _selectedCourse;
        [ObservableProperty] private ObservableCollection<Course> _courseList = new();
        [ObservableProperty] private ObservableCollection<Batch> _batches = new();
        [ObservableProperty] private Batch? _selectedBatch;

        public ManageBatchesViewModel(AcgcetDbContext db) { _db = db; _ = LoadAsync(); }
        public ManageBatchesViewModel() { _db = null!; }

        [RelayCommand]
        private async Task LoadAsync()
        {
            CourseList = new ObservableCollection<Course>(await _db.Courses.OrderBy(c => c.CourseName).ToListAsync());
            Batches = new ObservableCollection<Batch>(
                await _db.Batches.Include(b => b.Course).OrderByDescending(b => b.BatchYear).ToListAsync());
        }

        [RelayCommand]
        private async Task Save()
        {
            if (BatchYear < 2000 || string.IsNullOrWhiteSpace(BatchName))
            { MessageBox.Show("Valid Year and Batch Name required.", "Validation"); return; }
            if (SelectedCourse == null)
            { MessageBox.Show("Select a Course.", "Validation"); return; }

            if (await _db.Batches.AnyAsync(b => b.BatchYear == BatchYear && b.CourseId == SelectedCourse.CourseId))
            { MessageBox.Show("Batch already exists for this course and year.", "Duplicate"); return; }

            _db.Batches.Add(new Batch
            {
                BatchYear = BatchYear,
                BatchName = BatchName.Trim(),
                CourseId = SelectedCourse.CourseId,
                StartDate = StartDate.HasValue ? DateOnly.FromDateTime(StartDate.Value) : null,
                ExpectedEndDate = ExpectedEndDate.HasValue ? DateOnly.FromDateTime(ExpectedEndDate.Value) : null
            });

            try
            {
                await _db.SaveChangesAsync();
                MessageBox.Show("Batch added!", "Success");
                Clear();
                await LoadAsync();
            }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Save failed: {ex.InnerException?.Message ?? ex.Message}", "Error"); }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedBatch == null) return;
            if (MessageBox.Show($"Delete '{SelectedBatch.BatchName}'?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            _db.Batches.Remove(SelectedBatch);
            try { await _db.SaveChangesAsync(); await LoadAsync(); }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Cannot delete — referenced.\n{ex.InnerException?.Message}", "Error"); }
        }

        [RelayCommand]
        private void Clear()
        {
            BatchYear = 0; BatchName = ""; StartDate = null; ExpectedEndDate = null;
            SelectedCourse = null; SelectedBatch = null;
        }
    }

    // ════════════════════════════════════════════════════
    //  SECTIONS
    // ════════════════════════════════════════════════════
    public partial class ManageSectionsViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _db;

        [ObservableProperty] private string _sectionName = "";
        [ObservableProperty] private string _sectionCode = "";
        [ObservableProperty] private int? _maxStudents;
        [ObservableProperty] private Batch? _selectedBatch;
        [ObservableProperty] private ObservableCollection<Batch> _batchList = new();
        [ObservableProperty] private ObservableCollection<Section> _sections = new();
        [ObservableProperty] private Section? _selectedSection;

        public ManageSectionsViewModel(AcgcetDbContext db) { _db = db; _ = LoadAsync(); }
        public ManageSectionsViewModel() { _db = null!; }

        [RelayCommand]
        private async Task LoadAsync()
        {
            BatchList = new ObservableCollection<Batch>(
                await _db.Batches.Include(b => b.Course).OrderByDescending(b => b.BatchYear).ToListAsync());
            Sections = new ObservableCollection<Section>(
                await _db.Sections.Include(s => s.Batch).ThenInclude(b => b!.Course).OrderBy(s => s.SectionName).ToListAsync());
        }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(SectionName) || string.IsNullOrWhiteSpace(SectionCode))
            { MessageBox.Show("Section Name and Code required.", "Validation"); return; }
            if (SelectedBatch == null) { MessageBox.Show("Select a Batch.", "Validation"); return; }

            if (await _db.Sections.AnyAsync(s => s.SectionCode == SectionCode && s.BatchId == SelectedBatch.BatchId))
            { MessageBox.Show("Section Code exists for this batch.", "Duplicate"); return; }

            _db.Sections.Add(new Section
            {
                SectionName = SectionName.Trim(),
                SectionCode = SectionCode.Trim().ToUpper(),
                BatchId = SelectedBatch.BatchId,
                MaxStudents = MaxStudents
            });

            try
            {
                await _db.SaveChangesAsync();
                MessageBox.Show("Section added!", "Success");
                Clear();
                await LoadAsync();
            }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Save failed: {ex.InnerException?.Message ?? ex.Message}", "Error"); }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedSection == null) return;
            if (MessageBox.Show($"Delete '{SelectedSection.SectionName}'?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            _db.Sections.Remove(SelectedSection);
            try { await _db.SaveChangesAsync(); await LoadAsync(); }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Cannot delete — referenced.\n{ex.InnerException?.Message}", "Error"); }
        }

        [RelayCommand]
        private void Clear()
        {
            SectionName = ""; SectionCode = ""; MaxStudents = null;
            SelectedBatch = null; SelectedSection = null;
        }
    }

    // ════════════════════════════════════════════════════
    //  PAPERS
    // ════════════════════════════════════════════════════
    public partial class ManagePapersViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _db;

        [ObservableProperty] private string _paperCode = "";
        [ObservableProperty] private string _paperName = "";
        [ObservableProperty] private int _semester;
        [ObservableProperty] private decimal? _credits;
        [ObservableProperty] private Course? _selectedCourse;
        [ObservableProperty] private PaperType? _selectedPaperType;
        [ObservableProperty] private Scheme? _selectedScheme;
        [ObservableProperty] private ObservableCollection<Course> _courseList = new();
        [ObservableProperty] private ObservableCollection<PaperType> _paperTypeList = new();
        [ObservableProperty] private ObservableCollection<Scheme> _schemeList = new();
        [ObservableProperty] private ObservableCollection<Paper> _papers = new();
        [ObservableProperty] private Paper? _selectedPaper;

        public ManagePapersViewModel(AcgcetDbContext db) { _db = db; _ = LoadAsync(); }
        public ManagePapersViewModel() { _db = null!; }

        [RelayCommand]
        private async Task LoadAsync()
        {
            CourseList = new ObservableCollection<Course>(await _db.Courses.OrderBy(c => c.CourseName).ToListAsync());
            PaperTypeList = new ObservableCollection<PaperType>(await _db.PaperTypes.OrderBy(p => p.TypeName).ToListAsync());
            SchemeList = new ObservableCollection<Scheme>(await _db.Schemes.OrderByDescending(s => s.SchemeYear).ToListAsync());
            Papers = new ObservableCollection<Paper>(
                await _db.Papers.Include(p => p.Course).Include(p => p.PaperType).Include(p => p.Scheme)
                    .OrderBy(p => p.PaperCode).ToListAsync());
        }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(PaperCode) || string.IsNullOrWhiteSpace(PaperName) || Semester < 1)
            { MessageBox.Show("Paper Code, Name, and valid Semester required.", "Validation"); return; }
            if (SelectedCourse == null) { MessageBox.Show("Select a Course.", "Validation"); return; }

            if (await _db.Papers.AnyAsync(p => p.PaperCode == PaperCode && p.CourseId == SelectedCourse.CourseId && p.Semester == Semester))
            { MessageBox.Show("Paper already exists for this course and semester.", "Duplicate"); return; }

            _db.Papers.Add(new Paper
            {
                PaperCode = PaperCode.Trim().ToUpper(),
                PaperName = PaperName.Trim(),
                CourseId = SelectedCourse.CourseId,
                Semester = Semester,
                Credits = Credits,
                PaperTypeId = SelectedPaperType?.PaperTypeId,
                SchemeId = SelectedScheme?.SchemeId
            });

            try
            {
                await _db.SaveChangesAsync();
                MessageBox.Show("Paper added!", "Success");
                Clear();
                await LoadAsync();
            }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Save failed: {ex.InnerException?.Message ?? ex.Message}", "Error"); }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedPaper == null) return;
            if (MessageBox.Show($"Delete '{SelectedPaper.PaperName}'?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            _db.Papers.Remove(SelectedPaper);
            try { await _db.SaveChangesAsync(); await LoadAsync(); }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Cannot delete — referenced.\n{ex.InnerException?.Message}", "Error"); }
        }

        [RelayCommand]
        private void Clear()
        {
            PaperCode = ""; PaperName = ""; Semester = 0; Credits = null;
            SelectedCourse = null; SelectedPaperType = null; SelectedScheme = null; SelectedPaper = null;
        }
    }

    // ════════════════════════════════════════════════════
    //  PAPER FEES
    // ════════════════════════════════════════════════════
    public partial class ManagePaperFeesViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _db;

        [ObservableProperty] private Paper? _selectedPaper;
        [ObservableProperty] private ExamType? _selectedExamType;
        [ObservableProperty] private decimal _amount;
        [ObservableProperty] private DateTime? _effectiveFrom;
        [ObservableProperty] private ObservableCollection<Paper> _paperList = new();
        [ObservableProperty] private ObservableCollection<ExamType> _examTypeList = new();
        [ObservableProperty] private ObservableCollection<PaperFee> _paperFees = new();
        [ObservableProperty] private PaperFee? _selectedPaperFee;

        public ManagePaperFeesViewModel(AcgcetDbContext db) { _db = db; _ = LoadAsync(); }
        public ManagePaperFeesViewModel() { _db = null!; }

        [RelayCommand]
        private async Task LoadAsync()
        {
            PaperList = new ObservableCollection<Paper>(await _db.Papers.OrderBy(p => p.PaperCode).ToListAsync());
            ExamTypeList = new ObservableCollection<ExamType>(await _db.ExamTypes.OrderBy(e => e.TypeName).ToListAsync());
            PaperFees = new ObservableCollection<PaperFee>(
                await _db.PaperFees.Include(f => f.Paper).Include(f => f.ExamType).OrderBy(f => f.Paper!.PaperCode).ToListAsync());
        }

        [RelayCommand]
        private async Task Save()
        {
            if (SelectedPaper == null || SelectedExamType == null || Amount <= 0)
            { MessageBox.Show("Select Paper, Exam Type and enter valid Amount.", "Validation"); return; }

            DateOnly? eff = EffectiveFrom.HasValue ? DateOnly.FromDateTime(EffectiveFrom.Value) : null;

            if (await _db.PaperFees.AnyAsync(f => f.PaperId == SelectedPaper.PaperId
                && f.ExamTypeId == SelectedExamType.ExamTypeId && f.EffectiveFrom == eff))
            { MessageBox.Show("Fee already exists for this combination.", "Duplicate"); return; }

            _db.PaperFees.Add(new PaperFee
            {
                PaperId = SelectedPaper.PaperId,
                ExamTypeId = SelectedExamType.ExamTypeId,
                Amount = Amount,
                EffectiveFrom = eff
            });

            try
            {
                await _db.SaveChangesAsync();
                MessageBox.Show("Paper Fee added!", "Success");
                Clear();
                await LoadAsync();
            }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Save failed: {ex.InnerException?.Message ?? ex.Message}", "Error"); }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedPaperFee == null) return;
            if (MessageBox.Show("Delete this fee entry?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            _db.PaperFees.Remove(SelectedPaperFee);
            try { await _db.SaveChangesAsync(); await LoadAsync(); }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Cannot delete.\n{ex.InnerException?.Message}", "Error"); }
        }

        [RelayCommand]
        private void Clear()
        {
            SelectedPaper = null; SelectedExamType = null; Amount = 0;
            EffectiveFrom = null; SelectedPaperFee = null;
        }
    }

    // ════════════════════════════════════════════════════
    //  PASSING CRITERIA
    // ════════════════════════════════════════════════════
    public partial class ManagePassingCriteriaViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _db;

        [ObservableProperty] private Course? _selectedCourse;
        [ObservableProperty] private Batch? _selectedBatch;
        [ObservableProperty] private decimal? _minimumCredits;
        [ObservableProperty] private int? _minimumPapers;
        [ObservableProperty] private ObservableCollection<Course> _courseList = new();
        [ObservableProperty] private ObservableCollection<Batch> _batchList = new();
        [ObservableProperty] private ObservableCollection<PassingCriterion> _criteria = new();
        [ObservableProperty] private PassingCriterion? _selectedCriterion;

        public ManagePassingCriteriaViewModel(AcgcetDbContext db) { _db = db; _ = LoadAsync(); }
        public ManagePassingCriteriaViewModel() { _db = null!; }

        [RelayCommand]
        private async Task LoadAsync()
        {
            CourseList = new ObservableCollection<Course>(await _db.Courses.OrderBy(c => c.CourseName).ToListAsync());
            BatchList = new ObservableCollection<Batch>(
                await _db.Batches.Include(b => b.Course).OrderByDescending(b => b.BatchYear).ToListAsync());
            Criteria = new ObservableCollection<PassingCriterion>(
                await _db.PassingCriteria.Include(p => p.Course).Include(p => p.Batch).ToListAsync());
        }

        [RelayCommand]
        private async Task Save()
        {
            if (SelectedCourse == null || SelectedBatch == null)
            { MessageBox.Show("Select Course and Batch.", "Validation"); return; }

            if (await _db.PassingCriteria.AnyAsync(p => p.CourseId == SelectedCourse.CourseId && p.BatchId == SelectedBatch.BatchId))
            { MessageBox.Show("Criteria already exists for this combination.", "Duplicate"); return; }

            _db.PassingCriteria.Add(new PassingCriterion
            {
                CourseId = SelectedCourse.CourseId,
                BatchId = SelectedBatch.BatchId,
                MinimumCredits = MinimumCredits,
                MinimumPapers = MinimumPapers
            });

            try
            {
                await _db.SaveChangesAsync();
                MessageBox.Show("Passing Criteria added!", "Success");
                Clear();
                await LoadAsync();
            }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Save failed: {ex.InnerException?.Message ?? ex.Message}", "Error"); }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedCriterion == null) return;
            if (MessageBox.Show("Delete this criteria?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            _db.PassingCriteria.Remove(SelectedCriterion);
            try { await _db.SaveChangesAsync(); await LoadAsync(); }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Cannot delete.\n{ex.InnerException?.Message}", "Error"); }
        }

        [RelayCommand]
        private void Clear()
        {
            SelectedCourse = null; SelectedBatch = null;
            MinimumCredits = null; MinimumPapers = null; SelectedCriterion = null;
        }
    }
}
