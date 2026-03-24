using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ACGCET_Admin.Models;
using Microsoft.EntityFrameworkCore;

namespace ACGCET_Admin.ViewModels.AdminControl
{
    public partial class EntryProgressViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _db;

        [ObservableProperty] private ObservableCollection<DepartmentProgress> _departmentStats = new();
        [ObservableProperty] private ObservableCollection<PaperProgress> _paperStats = new();
        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private string _examLabel = "—";

        private List<DepartmentProgress> _allDeptStats = new();
        private List<PaperProgress> _allPaperStats = new();

        [ObservableProperty] private string _searchText = string.Empty;
        partial void OnSearchTextChanged(string value) => ApplyFilter();

        // Summary KPIs
        [ObservableProperty] private int _totalApplied;
        [ObservableProperty] private int _internalEntered;
        [ObservableProperty] private int _externalEntered;
        [ObservableProperty] private int _resultsPublished;

        public EntryProgressViewModel(AcgcetDbContext db)
        {
            _db = db;
            _ = LoadAsync();
        }

        public EntryProgressViewModel() { _db = null!; }

        [RelayCommand]
        private async Task LoadAsync()
        {
            if (_db == null) return;
            IsLoading = true;
            try
            {
                var latestExam = await _db.Examinations
                    .OrderByDescending(e => e.ExaminationId)
                    .FirstOrDefaultAsync();

                if (latestExam == null)
                {
                    ExamLabel = "No examination found";
                    IsLoading = false;
                    return;
                }

                ExamLabel = latestExam.ExamMonth ?? "N/A";
                int examId = latestExam.ExaminationId;

                // Get all applied student-paper combos for this exam
                var applications = await _db.ExamApplicationPapers
                    .Include(eap => eap.ExamApplication).ThenInclude(ea => ea!.Student)
                        .ThenInclude(s => s!.Batch).ThenInclude(b => b!.Course).ThenInclude(c => c!.Program)
                    .Include(eap => eap.Paper)
                    .Where(eap => eap.ExamApplication!.ExaminationId == examId)
                    .Select(eap => new
                    {
                        eap.ExamApplication!.StudentId,
                        Department = eap.ExamApplication.Student!.Batch!.Course!.Program!.ProgramName ?? "Unknown",
                        eap.PaperId,
                        PaperCode = eap.Paper!.PaperCode ?? ""
                    })
                    .ToListAsync();

                var studentIds = applications.Select(a => a.StudentId).Distinct().ToList();
                var paperIds = applications.Select(a => a.PaperId).Distinct().ToList();

                // Fetch existing entries
                var internalStudentPapers = await _db.InternalMarks
                    .Where(m => studentIds.Contains(m.StudentId) && paperIds.Contains(m.PaperId))
                    .Select(m => new { m.StudentId, m.PaperId })
                    .Distinct()
                    .ToListAsync();
                var intSet = internalStudentPapers.Select(x => (x.StudentId, x.PaperId)).ToHashSet();

                var externalStudentPapers = await _db.ExternalMarks
                    .Where(m => m.ExaminationId == examId && studentIds.Contains(m.StudentId) && paperIds.Contains(m.PaperId))
                    .Select(m => new { m.StudentId, m.PaperId })
                    .Distinct()
                    .ToListAsync();
                var extSet = externalStudentPapers.Select(x => (x.StudentId, x.PaperId)).ToHashSet();

                var resultStudentPapers = await _db.ExamResults
                    .Where(r => r.ExaminationId == examId && studentIds.Contains(r.StudentId) && paperIds.Contains(r.PaperId))
                    .Select(r => new { r.StudentId, r.PaperId })
                    .Distinct()
                    .ToListAsync();
                var resSet = resultStudentPapers.Select(x => (x.StudentId, x.PaperId)).ToHashSet();

                // Department-level stats
                var deptGroups = applications.GroupBy(a => a.Department);
                var deptList = deptGroups.Select(g =>
                {
                    int total = g.Count();
                    int intDone = g.Count(a => intSet.Contains((a.StudentId, a.PaperId)));
                    int extDone = g.Count(a => extSet.Contains((a.StudentId, a.PaperId)));
                    int resDone = g.Count(a => resSet.Contains((a.StudentId, a.PaperId)));
                    return new DepartmentProgress
                    {
                        Department = g.Key,
                        TotalEntries = total,
                        InternalDone = intDone,
                        ExternalDone = extDone,
                        ResultsDone = resDone
                    };
                }).OrderBy(d => d.Department).ToList();

                // Paper-level stats
                var paperGroups = applications.GroupBy(a => new { a.PaperId, a.PaperCode });
                var paperList = paperGroups.Select(g =>
                {
                    int total = g.Count();
                    int intDone = g.Count(a => intSet.Contains((a.StudentId, a.PaperId)));
                    int extDone = g.Count(a => extSet.Contains((a.StudentId, a.PaperId)));
                    int resDone = g.Count(a => resSet.Contains((a.StudentId, a.PaperId)));
                    return new PaperProgress
                    {
                        PaperCode = g.Key.PaperCode,
                        TotalStudents = total,
                        InternalDone = intDone,
                        ExternalDone = extDone,
                        ResultsDone = resDone
                    };
                }).OrderBy(p => p.PaperCode).ToList();

                int totalAll = applications.Count;
                int intAll = applications.Count(a => intSet.Contains((a.StudentId, a.PaperId)));
                int extAll = applications.Count(a => extSet.Contains((a.StudentId, a.PaperId)));
                int resAll = applications.Count(a => resSet.Contains((a.StudentId, a.PaperId)));

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    _allDeptStats = deptList;
                    _allPaperStats = paperList;
                    TotalApplied = totalAll;
                    InternalEntered = intAll;
                    ExternalEntered = extAll;
                    ResultsPublished = resAll;
                    SearchText = string.Empty;
                    ApplyFilter();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading progress: {ex.Message}");
            }
            finally { IsLoading = false; }
        }

        private void ApplyFilter()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                DepartmentStats = new ObservableCollection<DepartmentProgress>(_allDeptStats);
                PaperStats = new ObservableCollection<PaperProgress>(_allPaperStats);
            }
            else
            {
                DepartmentStats = new ObservableCollection<DepartmentProgress>(
                    _allDeptStats.Where(d => d.Department.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
                PaperStats = new ObservableCollection<PaperProgress>(
                    _allPaperStats.Where(p => p.PaperCode.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
            }
        }
    }

    public class DepartmentProgress
    {
        public string Department { get; set; } = "";
        public int TotalEntries { get; set; }
        public int InternalDone { get; set; }
        public int ExternalDone { get; set; }
        public int ResultsDone { get; set; }
        public string InternalPercent => TotalEntries > 0 ? $"{InternalDone * 100 / TotalEntries}%" : "—";
        public string ExternalPercent => TotalEntries > 0 ? $"{ExternalDone * 100 / TotalEntries}%" : "—";
        public string ResultsPercent => TotalEntries > 0 ? $"{ResultsDone * 100 / TotalEntries}%" : "—";
    }

    public class PaperProgress
    {
        public string PaperCode { get; set; } = "";
        public int TotalStudents { get; set; }
        public int InternalDone { get; set; }
        public int ExternalDone { get; set; }
        public int ResultsDone { get; set; }
        public int InternalPending => TotalStudents - InternalDone;
        public int ExternalPending => TotalStudents - ExternalDone;
    }
}
