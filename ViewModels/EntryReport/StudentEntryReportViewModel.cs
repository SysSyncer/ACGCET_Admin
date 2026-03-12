using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ACGCET_Admin.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Generic;

namespace ACGCET_Admin.ViewModels.EntryReport
{
    // Shared Base for Filtering
    public abstract partial class BaseEntryReportViewModel : ObservableObject
    {
        protected readonly AcgcetDbContext _dbContext;

        [ObservableProperty] private ObservableCollection<string> _courseLevels = new() { "UG", "PG" };
        [ObservableProperty] private string _selectedCourseLevel = "";
        
        [ObservableProperty] private ObservableCollection<Program> _programs = new();
        [ObservableProperty] private Program? _selectedProgram;

        [ObservableProperty] private ObservableCollection<Batch> _batches = new();
        [ObservableProperty] private Batch? _selectedBatch;

        [ObservableProperty] private ObservableCollection<Section> _sections = new();
        [ObservableProperty] private Section? _selectedSection;

        public BaseEntryReportViewModel(AcgcetDbContext dbContext)
        {
            _dbContext = dbContext;
            // Do NOT call async void LoadPrograms() here. It causes concurrency crashes when multiple instances share context.
        }

        public async Task LoadMasterData()
        {
            if (Programs.Count > 0) return; // Already loaded

            var progs = await _dbContext.Programs.Include(p => p.Degree).ToListAsync();
            Programs.Clear();
            foreach(var p in progs) Programs.Add(p);
        }

        async partial void OnSelectedCourseLevelChanged(string value)
        {
            if(string.IsNullOrEmpty(value)) return;
            var programs = await _dbContext.Programs.Include(p => p.Degree)
                .Where(p => p.Degree != null && p.Degree.GraduationLevel == value).ToListAsync();
             Programs.Clear();
             foreach(var p in programs) Programs.Add(p);
        }

        async partial void OnSelectedProgramChanged(Program? value)
        {
            if (value == null) return;
            var batches = await _dbContext.Batches.Where(b => b.Course!.ProgramId == value.ProgramId).ToListAsync();
            Batches.Clear();
            foreach(var b in batches) Batches.Add(b);
        }

        async partial void OnSelectedBatchChanged(Batch? value)
        {
             if (value == null) return;
             var sections = await _dbContext.Sections.Where(s => s.BatchId == value.BatchId).ToListAsync();
             Sections.Clear();
             foreach(var s in sections) Sections.Add(s);
        }

        [RelayCommand]
        public abstract Task View();

        [RelayCommand]
        public void Clear()
        {
            SelectedCourseLevel = string.Empty;
            SelectedProgram = null; 
            SelectedBatch = null;
            SelectedSection = null;
            ClearData();
        }

        protected abstract void ClearData();
    }

    public partial class StudentEntryItem : ObservableObject
    {
        public string? AdmissionNo { get; set; }
        public string? RollNo { get; set; }
        public string? RegNo { get; set; }
        public string? StudentName { get; set; }
        public string? RegulationName { get; set; }
        public string? EntryPerson { get; set; }
    }

    public partial class StudentEntryReportViewModel : BaseEntryReportViewModel
    {
        [ObservableProperty] private ObservableCollection<StudentEntryItem> _reportData = new();

        public StudentEntryReportViewModel(AcgcetDbContext dbContext) : base(dbContext) { }

        public override async Task View()
        {
            if(SelectedBatch == null) { MessageBox.Show("Select Batch"); return; }
            
            var query = _dbContext.Students.Include(s => s.Regulation).AsQueryable();
            if (SelectedBatch != null) query = query.Where(s => s.BatchId == SelectedBatch.BatchId);
            if (SelectedSection != null) query = query.Where(s => s.SectionId == SelectedSection.SectionId);
            if (SelectedProgram != null) query = query.Where(s => s.Course!.ProgramId == SelectedProgram.ProgramId); 
            
            var students = await query.ToListAsync();
            ReportData.Clear();
            foreach(var s in students)
            {
                ReportData.Add(new StudentEntryItem 
                {
                    AdmissionNo = s.AdmissionNumber,
                    RollNo = s.RollNumber,
                    RegNo = s.RegistrationNumber,
                    StudentName = s.FullName,
                    RegulationName = s.Regulation?.RegulationName ?? "N/A",
                    EntryPerson = s.CreatedBy ?? "Unknown"
                });
            }
        }

        [RelayCommand]
        public void Preview()
        {
            if (ReportData.Count == 0) { MessageBox.Show("No data to display"); return; }
            if (SelectedBatch == null) { MessageBox.Show("Select Batch"); return; }

            var printService = new Services.PrintService();
            printService.GenerateStudentReport(
                ReportData, 
                SelectedProgram?.Degree?.DegreeName ?? "Degree", 
                SelectedProgram?.ProgramName ?? "Program", 
                SelectedBatch.BatchName, 
                SelectedSection?.SectionName ?? "All"
            );
        }

        [RelayCommand]
        public void Print()
        {
            // With QuestPDF, Preview and Print both generate the PDF and open it.
            Preview();
        }

        protected override void ClearData() => ReportData.Clear();

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await View();
        }
    }
}
