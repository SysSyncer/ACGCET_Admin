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
    public partial class ExternalMarkEntryItem : ObservableObject
    {
        public string? RegNo { get; set; }
        public string? StudentName { get; set; }
        public string? PaperCode { get; set; }
        public string? TotalMark { get; set; }
        public string? EntryPerson { get; set; }
    }

    public partial class ExternalMarkEntryReportViewModel : BaseEntryReportViewModel
    {
        [ObservableProperty] private ObservableCollection<ExternalMarkEntryItem> _reportData = new();
        private List<ExternalMarkEntryItem> _allReportData = new();

        public ExternalMarkEntryReportViewModel(AcgcetDbContext dbContext) : base(dbContext) { }

        public override async Task View()
        {
            if (SelectedBatch == null) { MessageBox.Show("Select Batch"); return; }

            var studentQuery = _dbContext.Students.AsQueryable();
            if (SelectedBatch != null) studentQuery = studentQuery.Where(s => s.BatchId == SelectedBatch.BatchId);
            if (SelectedSection != null) studentQuery = studentQuery.Where(s => s.SectionId == SelectedSection.SectionId);
            var studentIds = await studentQuery.Select(s => s.StudentId).ToListAsync();

            var marks = await _dbContext.ExternalMarks
                .Include(m => m.Student)
                .Include(m => m.Paper)
                .Where(m => m.StudentId.HasValue && studentIds.Contains(m.StudentId.Value))
                .OrderBy(m => m.Student!.RegistrationNumber)
                .ToListAsync();

            _allReportData = marks.Select(m => new ExternalMarkEntryItem
            {
                RegNo = m.Student!.RegistrationNumber,
                StudentName = m.Student.FullName,
                PaperCode = m.Paper!.PaperCode,
                TotalMark = m.TotalMark?.ToString(),
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
            ReportData = new ObservableCollection<ExternalMarkEntryItem>(filtered);
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
