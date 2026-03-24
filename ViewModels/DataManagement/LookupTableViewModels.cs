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
    //  DEGREES
    // ════════════════════════════════════════════════════
    public partial class ManageDegreesViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _db;

        [ObservableProperty] private string _degreeName = "";
        [ObservableProperty] private string _degreeCode = "";
        [ObservableProperty] private string _graduationLevel = "";
        [ObservableProperty] private ObservableCollection<Degree> _degrees = new();
        [ObservableProperty] private Degree? _selectedDegree;

        public ObservableCollection<string> GraduationLevels { get; } = new() { "UG", "PG", "Diploma" };

        public ManageDegreesViewModel(AcgcetDbContext db) { _db = db; _ = LoadAsync(); }
        public ManageDegreesViewModel() { _db = null!; }

        [RelayCommand]
        private async Task LoadAsync()
        {
            var list = await _db.Degrees.OrderBy(d => d.DegreeName).ToListAsync();
            Degrees = new ObservableCollection<Degree>(list);
        }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(DegreeName) || string.IsNullOrWhiteSpace(DegreeCode))
            { MessageBox.Show("Degree Name and Code are required.", "Validation"); return; }

            if (await _db.Degrees.AnyAsync(d => d.DegreeCode == DegreeCode))
            { MessageBox.Show("Degree Code already exists.", "Duplicate"); return; }

            _db.Degrees.Add(new Degree
            {
                DegreeName = DegreeName.Trim(),
                DegreeCode = DegreeCode.Trim().ToUpper(),
                GraduationLevel = string.IsNullOrWhiteSpace(GraduationLevel) ? null : GraduationLevel
            });

            try
            {
                await _db.SaveChangesAsync();
                MessageBox.Show("Degree added successfully!", "Success");
                Clear();
                await LoadAsync();
            }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Save failed: {ex.InnerException?.Message ?? ex.Message}", "Error"); }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedDegree == null) { MessageBox.Show("Select a degree first.", "Validation"); return; }
            if (MessageBox.Show($"Delete '{SelectedDegree.DegreeName}'? This will fail if programs or courses use it.",
                "Confirm Delete", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            _db.Degrees.Remove(SelectedDegree);
            try
            {
                await _db.SaveChangesAsync();
                MessageBox.Show("Deleted.", "Success");
                await LoadAsync();
            }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Cannot delete — it is referenced by other records.\n{ex.InnerException?.Message}", "Error"); }
        }

        [RelayCommand]
        private void Clear() { DegreeName = ""; DegreeCode = ""; GraduationLevel = ""; SelectedDegree = null; }
    }

    // ════════════════════════════════════════════════════
    //  PROGRAMS
    // ════════════════════════════════════════════════════
    public partial class ManageProgramsViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _db;

        [ObservableProperty] private string _programName = "";
        [ObservableProperty] private string _programCode = "";
        [ObservableProperty] private Degree? _selectedDegree;
        [ObservableProperty] private ObservableCollection<Degree> _degreeList = new();
        [ObservableProperty] private ObservableCollection<Program> _programs = new();
        [ObservableProperty] private Program? _selectedProgram;

        public ManageProgramsViewModel(AcgcetDbContext db) { _db = db; _ = LoadAsync(); }
        public ManageProgramsViewModel() { _db = null!; }

        [RelayCommand]
        private async Task LoadAsync()
        {
            DegreeList = new ObservableCollection<Degree>(await _db.Degrees.OrderBy(d => d.DegreeName).ToListAsync());
            Programs = new ObservableCollection<Program>(
                await _db.Programs.Include(p => p.Degree).OrderBy(p => p.ProgramName).ToListAsync());
        }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(ProgramName) || string.IsNullOrWhiteSpace(ProgramCode))
            { MessageBox.Show("Program Name and Code are required.", "Validation"); return; }
            if (SelectedDegree == null) { MessageBox.Show("Select a Degree.", "Validation"); return; }

            if (await _db.Programs.AnyAsync(p => p.ProgramCode == ProgramCode && p.DegreeId == SelectedDegree.DegreeId))
            { MessageBox.Show("Program Code already exists for this degree.", "Duplicate"); return; }

            _db.Programs.Add(new Program
            {
                ProgramName = ProgramName.Trim(),
                ProgramCode = ProgramCode.Trim().ToUpper(),
                DegreeId = SelectedDegree.DegreeId
            });

            try
            {
                await _db.SaveChangesAsync();
                MessageBox.Show("Program added!", "Success");
                Clear();
                await LoadAsync();
            }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Save failed: {ex.InnerException?.Message ?? ex.Message}", "Error"); }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedProgram == null) return;
            if (MessageBox.Show($"Delete '{SelectedProgram.ProgramName}'?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            _db.Programs.Remove(SelectedProgram);
            try { await _db.SaveChangesAsync(); await LoadAsync(); }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Cannot delete — referenced by other records.\n{ex.InnerException?.Message}", "Error"); }
        }

        [RelayCommand]
        private void Clear() { ProgramName = ""; ProgramCode = ""; SelectedDegree = null; SelectedProgram = null; }
    }

    // ════════════════════════════════════════════════════
    //  REGULATIONS
    // ════════════════════════════════════════════════════
    public partial class ManageRegulationsViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _db;

        [ObservableProperty] private int _regulationYear;
        [ObservableProperty] private string _regulationName = "";
        [ObservableProperty] private DateTime? _effectiveFrom;
        [ObservableProperty] private ObservableCollection<Regulation> _regulations = new();
        [ObservableProperty] private Regulation? _selectedRegulation;

        public ManageRegulationsViewModel(AcgcetDbContext db) { _db = db; _ = LoadAsync(); }
        public ManageRegulationsViewModel() { _db = null!; }

        [RelayCommand]
        private async Task LoadAsync()
        {
            Regulations = new ObservableCollection<Regulation>(
                await _db.Regulations.OrderByDescending(r => r.RegulationYear).ToListAsync());
        }

        [RelayCommand]
        private async Task Save()
        {
            if (RegulationYear < 2000 || string.IsNullOrWhiteSpace(RegulationName))
            { MessageBox.Show("Valid year (≥2000) and name required.", "Validation"); return; }

            if (await _db.Regulations.AnyAsync(r => r.RegulationYear == RegulationYear))
            { MessageBox.Show("Regulation year already exists.", "Duplicate"); return; }

            _db.Regulations.Add(new Regulation
            {
                RegulationYear = RegulationYear,
                RegulationName = RegulationName.Trim(),
                EffectiveFrom = EffectiveFrom.HasValue ? DateOnly.FromDateTime(EffectiveFrom.Value) : null
            });

            try
            {
                await _db.SaveChangesAsync();
                MessageBox.Show("Regulation added!", "Success");
                Clear();
                await LoadAsync();
            }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Save failed: {ex.InnerException?.Message ?? ex.Message}", "Error"); }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedRegulation == null) return;
            if (MessageBox.Show($"Delete '{SelectedRegulation.RegulationName}'?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            _db.Regulations.Remove(SelectedRegulation);
            try { await _db.SaveChangesAsync(); await LoadAsync(); }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Cannot delete — referenced.\n{ex.InnerException?.Message}", "Error"); }
        }

        [RelayCommand]
        private void Clear() { RegulationYear = 0; RegulationName = ""; EffectiveFrom = null; SelectedRegulation = null; }
    }

    // ════════════════════════════════════════════════════
    //  EXAM TYPES
    // ════════════════════════════════════════════════════
    public partial class ManageExamTypesViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _db;

        [ObservableProperty] private string _typeName = "";
        [ObservableProperty] private string _typeCode = "";
        [ObservableProperty] private ObservableCollection<ExamType> _examTypes = new();
        [ObservableProperty] private ExamType? _selectedExamType;

        public ManageExamTypesViewModel(AcgcetDbContext db) { _db = db; _ = LoadAsync(); }
        public ManageExamTypesViewModel() { _db = null!; }

        [RelayCommand]
        private async Task LoadAsync()
        {
            ExamTypes = new ObservableCollection<ExamType>(await _db.ExamTypes.OrderBy(e => e.TypeName).ToListAsync());
        }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(TypeName) || string.IsNullOrWhiteSpace(TypeCode))
            { MessageBox.Show("Name and Code required.", "Validation"); return; }

            if (await _db.ExamTypes.AnyAsync(e => e.TypeCode == TypeCode))
            { MessageBox.Show("Type Code already exists.", "Duplicate"); return; }

            _db.ExamTypes.Add(new ExamType { TypeName = TypeName.Trim(), TypeCode = TypeCode.Trim().ToUpper() });
            try
            {
                await _db.SaveChangesAsync();
                MessageBox.Show("Exam Type added!", "Success");
                Clear();
                await LoadAsync();
            }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Save failed: {ex.InnerException?.Message ?? ex.Message}", "Error"); }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedExamType == null) return;
            if (MessageBox.Show($"Delete '{SelectedExamType.TypeName}'?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            _db.ExamTypes.Remove(SelectedExamType);
            try { await _db.SaveChangesAsync(); await LoadAsync(); }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Cannot delete — referenced.\n{ex.InnerException?.Message}", "Error"); }
        }

        [RelayCommand]
        private void Clear() { TypeName = ""; TypeCode = ""; SelectedExamType = null; }
    }

    // ════════════════════════════════════════════════════
    //  TEST TYPES
    // ════════════════════════════════════════════════════
    public partial class ManageTestTypesViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _db;

        [ObservableProperty] private string _testName = "";
        [ObservableProperty] private string _testCode = "";
        [ObservableProperty] private decimal? _maxMark;
        [ObservableProperty] private ObservableCollection<TestType> _testTypes = new();
        [ObservableProperty] private TestType? _selectedTestType;

        public ManageTestTypesViewModel(AcgcetDbContext db) { _db = db; _ = LoadAsync(); }
        public ManageTestTypesViewModel() { _db = null!; }

        [RelayCommand]
        private async Task LoadAsync()
        {
            TestTypes = new ObservableCollection<TestType>(await _db.TestTypes.OrderBy(t => t.TestName).ToListAsync());
        }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(TestName) || string.IsNullOrWhiteSpace(TestCode))
            { MessageBox.Show("Name and Code required.", "Validation"); return; }

            if (await _db.TestTypes.AnyAsync(t => t.TestCode == TestCode))
            { MessageBox.Show("Test Code already exists.", "Duplicate"); return; }

            _db.TestTypes.Add(new TestType { TestName = TestName.Trim(), TestCode = TestCode.Trim().ToUpper(), MaxMark = MaxMark });
            try
            {
                await _db.SaveChangesAsync();
                MessageBox.Show("Test Type added!", "Success");
                Clear();
                await LoadAsync();
            }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Save failed: {ex.InnerException?.Message ?? ex.Message}", "Error"); }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedTestType == null) return;
            if (MessageBox.Show($"Delete '{SelectedTestType.TestName}'?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            _db.TestTypes.Remove(SelectedTestType);
            try { await _db.SaveChangesAsync(); await LoadAsync(); }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Cannot delete — referenced.\n{ex.InnerException?.Message}", "Error"); }
        }

        [RelayCommand]
        private void Clear() { TestName = ""; TestCode = ""; MaxMark = null; SelectedTestType = null; }
    }

    // ════════════════════════════════════════════════════
    //  SCHEMES
    // ════════════════════════════════════════════════════
    public partial class ManageSchemesViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _db;

        [ObservableProperty] private string _schemeName = "";
        [ObservableProperty] private int _schemeYear;
        [ObservableProperty] private ObservableCollection<Scheme> _schemes = new();
        [ObservableProperty] private Scheme? _selectedScheme;

        public ManageSchemesViewModel(AcgcetDbContext db) { _db = db; _ = LoadAsync(); }
        public ManageSchemesViewModel() { _db = null!; }

        [RelayCommand]
        private async Task LoadAsync()
        {
            Schemes = new ObservableCollection<Scheme>(await _db.Schemes.OrderByDescending(s => s.SchemeYear).ToListAsync());
        }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(SchemeName) || SchemeYear < 2000)
            { MessageBox.Show("Scheme Name and valid Year required.", "Validation"); return; }

            if (await _db.Schemes.AnyAsync(s => s.SchemeName == SchemeName && s.SchemeYear == SchemeYear))
            { MessageBox.Show("Scheme already exists.", "Duplicate"); return; }

            _db.Schemes.Add(new Scheme { SchemeName = SchemeName.Trim(), SchemeYear = SchemeYear });
            try
            {
                await _db.SaveChangesAsync();
                MessageBox.Show("Scheme added!", "Success");
                Clear();
                await LoadAsync();
            }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Save failed: {ex.InnerException?.Message ?? ex.Message}", "Error"); }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedScheme == null) return;
            if (MessageBox.Show($"Delete '{SelectedScheme.SchemeName}'?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            _db.Schemes.Remove(SelectedScheme);
            try { await _db.SaveChangesAsync(); await LoadAsync(); }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Cannot delete — referenced.\n{ex.InnerException?.Message}", "Error"); }
        }

        [RelayCommand]
        private void Clear() { SchemeName = ""; SchemeYear = 0; SelectedScheme = null; }
    }

    // ════════════════════════════════════════════════════
    //  EXAM SESSIONS
    // ════════════════════════════════════════════════════
    public partial class ManageExamSessionsViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _db;

        [ObservableProperty] private string _sessionName = "";
        [ObservableProperty] private string _sessionCode = "";
        [ObservableProperty] private string _startTime = "";
        [ObservableProperty] private string _endTime = "";
        [ObservableProperty] private ObservableCollection<ExamSession> _examSessions = new();
        [ObservableProperty] private ExamSession? _selectedExamSession;

        public ManageExamSessionsViewModel(AcgcetDbContext db) { _db = db; _ = LoadAsync(); }
        public ManageExamSessionsViewModel() { _db = null!; }

        [RelayCommand]
        private async Task LoadAsync()
        {
            ExamSessions = new ObservableCollection<ExamSession>(await _db.ExamSessions.OrderBy(e => e.SessionName).ToListAsync());
        }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(SessionName) || string.IsNullOrWhiteSpace(SessionCode))
            { MessageBox.Show("Name and Code required.", "Validation"); return; }

            if (await _db.ExamSessions.AnyAsync(e => e.SessionCode == SessionCode))
            { MessageBox.Show("Session Code already exists.", "Duplicate"); return; }

            TimeOnly? start = TimeOnly.TryParse(StartTime, out var s) ? s : null;
            TimeOnly? end = TimeOnly.TryParse(EndTime, out var e2) ? e2 : null;

            _db.ExamSessions.Add(new ExamSession
            {
                SessionName = SessionName.Trim(),
                SessionCode = SessionCode.Trim().ToUpper(),
                StartTime = start,
                EndTime = end
            });

            try
            {
                await _db.SaveChangesAsync();
                MessageBox.Show("Exam Session added!", "Success");
                Clear();
                await LoadAsync();
            }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Save failed: {ex.InnerException?.Message ?? ex.Message}", "Error"); }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedExamSession == null) return;
            if (MessageBox.Show($"Delete '{SelectedExamSession.SessionName}'?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            _db.ExamSessions.Remove(SelectedExamSession);
            try { await _db.SaveChangesAsync(); await LoadAsync(); }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Cannot delete — referenced.\n{ex.InnerException?.Message}", "Error"); }
        }

        [RelayCommand]
        private void Clear() { SessionName = ""; SessionCode = ""; StartTime = ""; EndTime = ""; SelectedExamSession = null; }
    }

    // ════════════════════════════════════════════════════
    //  BLOCKS
    // ════════════════════════════════════════════════════
    public partial class ManageBlocksViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _db;

        [ObservableProperty] private string _blockName = "";
        [ObservableProperty] private string _buildingCode = "";
        [ObservableProperty] private ObservableCollection<Block> _blocks = new();
        [ObservableProperty] private Block? _selectedBlock;

        public ManageBlocksViewModel(AcgcetDbContext db) { _db = db; _ = LoadAsync(); }
        public ManageBlocksViewModel() { _db = null!; }

        [RelayCommand]
        private async Task LoadAsync()
        {
            Blocks = new ObservableCollection<Block>(await _db.Blocks.OrderBy(b => b.BlockName).ToListAsync());
        }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(BlockName))
            { MessageBox.Show("Block Name required.", "Validation"); return; }

            if (await _db.Blocks.AnyAsync(b => b.BlockName == BlockName))
            { MessageBox.Show("Block Name already exists.", "Duplicate"); return; }

            _db.Blocks.Add(new Block
            {
                BlockName = BlockName.Trim(),
                BuildingCode = string.IsNullOrWhiteSpace(BuildingCode) ? null : BuildingCode.Trim()
            });

            try
            {
                await _db.SaveChangesAsync();
                MessageBox.Show("Block added!", "Success");
                Clear();
                await LoadAsync();
            }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Save failed: {ex.InnerException?.Message ?? ex.Message}", "Error"); }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedBlock == null) return;
            if (MessageBox.Show($"Delete '{SelectedBlock.BlockName}'?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            _db.Blocks.Remove(SelectedBlock);
            try { await _db.SaveChangesAsync(); await LoadAsync(); }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Cannot delete — referenced.\n{ex.InnerException?.Message}", "Error"); }
        }

        [RelayCommand]
        private void Clear() { BlockName = ""; BuildingCode = ""; SelectedBlock = null; }
    }
}
