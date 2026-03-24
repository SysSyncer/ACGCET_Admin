using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ACGCET_Admin.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System;
using System.Collections.Generic;

namespace ACGCET_Admin.ViewModels.EntryReport
{
    public partial class InternalMarkEntryItem : ObservableObject
    {
        public string? RegNo { get; set; }
        public string? StudentName { get; set; }
        public string? PaperCode { get; set; }
        public string? Test { get; set; }
        public string? Mark { get; set; }
        public string? EntryPerson { get; set; }
    }

    public partial class InternalMarkEntryReportViewModel : BaseEntryReportViewModel
    {
        [ObservableProperty] private ObservableCollection<InternalMarkEntryItem> _reportData = new();
        private List<InternalMarkEntryItem> _allReportData = new();

        public InternalMarkEntryReportViewModel(AcgcetDbContext dbContext) : base(dbContext) { }

        public override async Task View()
        {
            if (SelectedBatch == null) { MessageBox.Show("Select Batch"); return; }

            var studentQuery = _dbContext.Students.AsQueryable();
            if (SelectedBatch != null) studentQuery = studentQuery.Where(s => s.BatchId == SelectedBatch.BatchId);
            if (SelectedSection != null) studentQuery = studentQuery.Where(s => s.SectionId == SelectedSection.SectionId);

            var studentIds = await studentQuery.Select(s => s.StudentId).ToListAsync();

            var marks = await _dbContext.InternalMarks
                .Include(m => m.Student)
                .Include(m => m.Paper)
                .Include(m => m.TestType)
                .Where(m => m.StudentId.HasValue && studentIds.Contains(m.StudentId.Value))
                .OrderBy(m => m.Student!.RegistrationNumber)
                .ToListAsync();

            _allReportData = marks.Select(m => new InternalMarkEntryItem
            {
                RegNo = m.Student!.RegistrationNumber,
                StudentName = m.Student.FullName,
                PaperCode = m.Paper!.PaperCode,
                Test = m.TestType!.TestName,
                Mark = m.Mark?.ToString(),
                EntryPerson = m.EnteredBy ?? "Unknown"
            }).ToList();
            SearchText = string.Empty;
            ApplyFilter();
        }

        protected override void ApplyFilter()
        {
            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? _allReportData
                : _allReportData.Where(x =>
                    (x.RegNo?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (x.StudentName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (x.PaperCode?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
            ReportData = new ObservableCollection<InternalMarkEntryItem>(filtered);
        }

        [RelayCommand]
        public void Preview()
        {
            if (ReportData.Count == 0) { MessageBox.Show("No data to display"); return; }
            if (SelectedBatch == null) { MessageBox.Show("Select Batch"); return; }

            var printService = new Services.PrintService();
            printService.GenerateInternalMarkReport(
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
            Preview();
        }

        protected override void ClearData()
        {
            _allReportData.Clear();
            ReportData.Clear();
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await View();
        }
    }
}
