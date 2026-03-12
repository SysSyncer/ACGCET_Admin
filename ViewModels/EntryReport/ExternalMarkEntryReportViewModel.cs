using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ACGCET_Admin.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

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
        
        public ExternalMarkEntryReportViewModel(AcgcetDbContext dbContext) : base(dbContext) { }

        public override async Task View()
        {
             if(SelectedBatch == null) { MessageBox.Show("Select Batch"); return; }
             
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

             ReportData.Clear();
             foreach(var m in marks)
             {
                 ReportData.Add(new ExternalMarkEntryItem
                 {
                     RegNo = m.Student!.RegistrationNumber,
                     StudentName = m.Student.FullName,
                     PaperCode = m.Paper!.PaperCode,
                     TotalMark = m.TotalMark?.ToString(),
                     EntryPerson = m.EnteredBy ?? "Unknown"
                 });
             }
        }

        protected override void ClearData() => ReportData.Clear();

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await View();
        }
    }
}
