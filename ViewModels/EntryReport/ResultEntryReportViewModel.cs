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
    public partial class ResultEntryItem : ObservableObject
    {
        public string? RegNo { get; set; }
        public string? StudentName { get; set; }
        public string? PaperCode { get; set; }
        public string? Grade { get; set; }
        public string? EntryPerson { get; set; }
    }

    public partial class ResultEntryReportViewModel : BaseEntryReportViewModel
    {
        [ObservableProperty] private ObservableCollection<ResultEntryItem> _reportData = new();
        private List<ResultEntryItem> _allReportData = new();

        public ResultEntryReportViewModel(AcgcetDbContext dbContext) : base(dbContext) { }

        public override async Task View()
        {
            if (SelectedBatch == null) { MessageBox.Show("Select Batch"); return; }

            var studentQuery = _dbContext.Students.AsQueryable();
            if (SelectedBatch != null) studentQuery = studentQuery.Where(s => s.BatchId == SelectedBatch.BatchId);
            if (SelectedSection != null) studentQuery = studentQuery.Where(s => s.SectionId == SelectedSection.SectionId);
            var studentIds = await studentQuery.Select(s => s.StudentId).ToListAsync();

            var results = await _dbContext.ExamResults
                .Include(r => r.Student)
                .Include(r => r.Paper)
                .Where(r => r.StudentId.HasValue && studentIds.Contains(r.StudentId.Value))
                .OrderBy(r => r.Student!.RegistrationNumber)
                .ToListAsync();

            _allReportData = results.Select(r => new ResultEntryItem
            {
                RegNo = r.Student!.RegistrationNumber,
                StudentName = r.Student.FullName,
                PaperCode = r.Paper!.PaperCode,
                Grade = r.Grade,
                EntryPerson = r.CreatedBy ?? "System"
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
                    (x.PaperCode?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (x.Grade?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
            ReportData = new ObservableCollection<ResultEntryItem>(filtered);
        }

        [RelayCommand]
        public void Preview()
        {
            if (ReportData.Count == 0) { MessageBox.Show("No data to display"); return; }
            if (SelectedBatch == null) { MessageBox.Show("Select Batch"); return; }

            var printService = new Services.PrintService();
            printService.GenerateResultReport(
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
