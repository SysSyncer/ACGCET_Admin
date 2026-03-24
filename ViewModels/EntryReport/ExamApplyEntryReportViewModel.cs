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
    public partial class ExamApplyEntryItem : ObservableObject
    {
        public string? RegNo { get; set; }
        public string? StudentName { get; set; }
        public string? ExamCode { get; set; }
        public string? Status { get; set; }
        public string? EntryPerson { get; set; } // CreatedBy or ApprovedBy? I added CreatedBy.
    }

    public partial class ExamApplyEntryReportViewModel : BaseEntryReportViewModel
    {
        [ObservableProperty] private ObservableCollection<ExamApplyEntryItem> _reportData = new();
        private List<ExamApplyEntryItem> _allReportData = new();

        public ExamApplyEntryReportViewModel(AcgcetDbContext dbContext) : base(dbContext) { }

        public override async Task View()
        {
            if (SelectedBatch == null) { MessageBox.Show("Select Batch"); return; }

            var studentQuery = _dbContext.Students.AsQueryable();
            if (SelectedBatch != null) studentQuery = studentQuery.Where(s => s.BatchId == SelectedBatch.BatchId);
            if (SelectedSection != null) studentQuery = studentQuery.Where(s => s.SectionId == SelectedSection.SectionId);
            var studentIds = await studentQuery.Select(s => s.StudentId).ToListAsync();

            var apps = await _dbContext.ExamApplications
                .Include(a => a.Student)
                .Include(a => a.Examination)
                .Where(a => a.StudentId.HasValue && studentIds.Contains(a.StudentId.Value))
                .OrderBy(a => a.Student!.RegistrationNumber)
                .ToListAsync();

            _allReportData = apps.Select(a => new ExamApplyEntryItem
            {
                RegNo = a.Student!.RegistrationNumber,
                StudentName = a.Student.FullName,
                ExamCode = a.Examination!.ExamCode,
                Status = a.ApprovalStatus,
                EntryPerson = a.CreatedBy ?? a.ApprovedBy ?? "Unknown"
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
                    (x.ExamCode?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
            ReportData = new ObservableCollection<ExamApplyEntryItem>(filtered);
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
