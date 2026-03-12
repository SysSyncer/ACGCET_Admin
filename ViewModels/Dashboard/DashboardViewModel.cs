using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ACGCET_Admin.Models;
using Microsoft.EntityFrameworkCore;
using ACGCET_Admin.ViewModels.Application;
using ACGCET_Admin.ViewModels.AdminControl;
using ACGCET_Admin.ViewModels.MissingEntry;
using ACGCET_Admin.ViewModels.EntryReport;
using ACGCET_Admin.ViewModels.DeleteEntry;

namespace ACGCET_Admin.ViewModels.Dashboard
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _db;
        private readonly AdminUser?      _loggedInUser;

        [ObservableProperty] private object _currentView    = new object();
        [ObservableProperty] private string _welcomeMessage = "Welcome to ACGCET Admin Dashboard";
        [ObservableProperty] private bool   _isDrawerOpen   = true;

        /// <summary>Only COE role users can manage admin/faculty accounts.</summary>
        public bool CanManageUsers { get; }

        /// <summary>Full name or username for display in the top bar.</summary>
        public string UserDisplayName { get; }
        /// <summary>1-2 letter initials shown in the avatar circle.</summary>
        public string UserInitials { get; }
        /// <summary>Human-readable role label shown below the user's name in the sidebar.</summary>
        public string UserRoleLabel { get; }

        public DashboardViewModel(AcgcetDbContext db, AdminUser? loggedInUser,
                                  bool isSuperAdmin, bool isCOE)
        {
            _db            = db;
            _loggedInUser  = loggedInUser;
            CanManageUsers = isCOE;
            UserRoleLabel  = isCOE ? "COE" : "Admin Staff";

            string name    = loggedInUser?.FullName ?? loggedInUser?.UserName ?? "User";
            WelcomeMessage = $"Welcome, {name}";
            UserDisplayName = name;
            UserInitials    = BuildInitials(name);
            IsDrawerOpen    = true;

            CurrentView = new HomeViewModel(_db);
        }

        public DashboardViewModel() { _db = null!; UserDisplayName = "User"; UserInitials = "U"; UserRoleLabel = "Admin"; }

        private static string BuildInitials(string name)
        {
            var parts = name.Trim().Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 2
                ? $"{parts[0][0]}{parts[^1][0]}".ToUpper()
                : name.Length > 0 ? name[0].ToString().ToUpper() : "U";
        }

        [RelayCommand]
        private void Navigate(string destination)
        {
            switch (destination)
            {
                case "Home":
                    CurrentView    = new HomeViewModel(_db);
                    WelcomeMessage = $"Welcome, {_loggedInUser?.FullName ?? "User"}";
                    break;
                case "Application":
                    CurrentView    = new ApplicationViewModel(_db);
                    WelcomeMessage = "Exam Applications";
                    break;
                case "AdminControl":
                    CurrentView    = new AdminControlViewModel(_db);
                    WelcomeMessage = "Admin Control";
                    break;
                case "MissingEntry":
                    CurrentView    = new MissingEntryViewModel(_db);
                    WelcomeMessage = "Missing Entries";
                    break;
                case "DeleteEntry":
                    CurrentView    = new DeleteEntryViewModel(_db);
                    WelcomeMessage = "Delete Entries";
                    break;
                case "EntryReport":
                    CurrentView = new EntryReportViewModel(
                        new StudentEntryReportViewModel(_db),
                        new InternalMarkEntryReportViewModel(_db),
                        new ExamApplyEntryReportViewModel(_db),
                        new ExternalMarkEntryReportViewModel(_db),
                        new ResultEntryReportViewModel(_db));
                    WelcomeMessage = "Entry Reports";
                    break;
                case "UserManagement":
                    if (CanManageUsers)
                    {
                        CurrentView    = new UserManagementViewModel(_db, _loggedInUser);
                        WelcomeMessage = "User Management";
                    }
                    break;
            }
        }

        [RelayCommand]
        private static void Logout() =>
            WeakReferenceMessenger.Default.Send(new LogoutMessage());
    }

    // ── Logout Message ─────────────────────────────────────────
    public class LogoutMessage { }

    // ── Home ViewModel (inline) ────────────────────────────────
    public partial class HomeViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _db;
        private const int LogsPageSize = 10;
        private int _totalLogCount;

        // KPI stats
        [ObservableProperty] private int    _totalStudents;
        [ObservableProperty] private int    _totalApplications;
        [ObservableProperty] private int    _pendingRevaluations;
        [ObservableProperty] private int    _totalPassedResults;
        [ObservableProperty] private int    _totalFailedResults;
        [ObservableProperty] private string _latestExamLabel = "—";
        [ObservableProperty] private int    _missingInternalCount;
        [ObservableProperty] private int    _missingExternalCount;
        [ObservableProperty] private int    _pendingApplications;

        // Pagination
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanGoPrevLogs))]
        [NotifyPropertyChangedFor(nameof(LogsPageInfo))]
        private int _logsCurrentPage = 1;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanGoNextLogs))]
        [NotifyPropertyChangedFor(nameof(LogsPageInfo))]
        private int _logsTotalPages = 1;

        public bool   CanGoPrevLogs => LogsCurrentPage > 1;
        public bool   CanGoNextLogs => LogsCurrentPage < LogsTotalPages;
        public string LogsPageInfo  => $"Page {LogsCurrentPage} of {LogsTotalPages}";

        [ObservableProperty] private ObservableCollection<ModuleLockInfo> _moduleLockStatuses = new();
        [ObservableProperty] private ObservableCollection<AuditLog>       _recentLogs         = new();
        [ObservableProperty] private ObservableCollection<SystemAlert>    _recentAlerts       = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotLoading))]
        private bool _isLoading = true;
        public bool IsNotLoading => !IsLoading;

        public HomeViewModel(AcgcetDbContext db) { _db = db; _ = LoadAsync(); }
        public HomeViewModel() { _db = null!; IsLoading = false; }

        [RelayCommand]
        private async Task Refresh()
        {
            IsLoading = true;
            LogsCurrentPage = 1;
            await LoadAsync();
        }

        [RelayCommand(CanExecute = nameof(CanGoNextLogs))]
        private async Task NextLogsPage()
        {
            LogsCurrentPage++;
            await FetchLogsPageAsync();
        }

        [RelayCommand(CanExecute = nameof(CanGoPrevLogs))]
        private async Task PrevLogsPage()
        {
            LogsCurrentPage--;
            await FetchLogsPageAsync();
        }

        private async Task FetchLogsPageAsync()
        {
            if (_db == null) return;
            try
            {
                var logs = await _db.AuditLogs
                    .OrderByDescending(l => l.ActionDate)
                    .Skip((LogsCurrentPage - 1) * LogsPageSize)
                    .Take(LogsPageSize)
                    .ToListAsync();
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    RecentLogs = new ObservableCollection<AuditLog>(logs));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logs page error: {ex.Message}");
            }
        }

        private async Task LoadAsync()
        {
            if (_db == null) { IsLoading = false; return; }
            try
            {
                var totalStudents = await _db.Students.CountAsync();

                var latestExam = await _db.Examinations
                    .OrderByDescending(e => e.ExaminationId)
                    .FirstOrDefaultAsync();

                int totalApps       = 0;
                int pendingApps     = 0;
                int passed          = 0;
                int failed          = 0;
                int missingInternal = 0;
                int missingExternal = 0;
                var moduleLockInfos = new System.Collections.Generic.List<ModuleLockInfo>();

                if (latestExam != null)
                {
                    totalApps   = await _db.ExamApplications
                        .Where(a => a.ExaminationId == latestExam.ExaminationId).CountAsync();
                    pendingApps = await _db.ExamApplications
                        .Where(a => a.ExaminationId == latestExam.ExaminationId
                                    && (a.ApprovalStatus == null || a.ApprovalStatus == "Pending")).CountAsync();

                    var results = await _db.ExamResults
                        .Include(r => r.ResultStatus)
                        .Where(r => r.ExaminationId == latestExam.ExaminationId)
                        .ToListAsync();
                    passed = results.Count(r =>
                        r.ResultStatus?.StatusCode == "P" ||
                        r.ResultStatus?.StatusName?.Contains("Pass", StringComparison.OrdinalIgnoreCase) == true);
                    failed = results.Count - passed;

                    var appStudentIds = await _db.ExamApplications
                        .Where(a => a.ExaminationId == latestExam.ExaminationId)
                        .Select(a => a.StudentId).Distinct().ToListAsync();
                    var studentsWithInt = await _db.InternalMarks
                        .Where(m => appStudentIds.Contains(m.StudentId))
                        .Select(m => m.StudentId).Distinct().ToListAsync();
                    missingInternal = appStudentIds.Count - studentsWithInt.Count;

                    var studentsWithExt = await _db.ExternalMarks
                        .Where(m => m.ExaminationId == latestExam.ExaminationId && appStudentIds.Contains(m.StudentId))
                        .Select(m => m.StudentId).Distinct().ToListAsync();
                    missingExternal = appStudentIds.Count - studentsWithExt.Count;

                    var locks = await _db.ModuleLocks
                        .Include(ml => ml.Module)
                        .Where(ml => ml.ExaminationId == latestExam.ExaminationId)
                        .ToListAsync();
                    moduleLockInfos = locks.Select(ml => new ModuleLockInfo
                    {
                        ModuleName = ml.Module?.ModuleName ?? ml.Module?.ModuleCode ?? "Unknown",
                        IsLocked   = ml.IsLocked ?? false,
                        LockedBy   = ml.LockedBy ?? ""
                    }).ToList();
                }

                int pendingRevs = await _db.RevaluationRequests
                    .Where(r => r.RevaluationStatusId == null || r.CompletedDate == null)
                    .CountAsync();

                // Server-side pagination — only load first page (10 records)
                _totalLogCount = await _db.AuditLogs.CountAsync();
                var firstPageLogs = await _db.AuditLogs
                    .OrderByDescending(l => l.ActionDate)
                    .Take(LogsPageSize)
                    .ToListAsync();

                var alerts = await _db.SystemAlerts
                    .OrderByDescending(a => a.AlertDateTime)
                    .Take(10)
                    .ToListAsync();

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    TotalStudents        = totalStudents;
                    TotalApplications    = totalApps;
                    PendingApplications  = pendingApps;
                    TotalPassedResults   = passed;
                    TotalFailedResults   = failed;
                    PendingRevaluations  = pendingRevs;
                    MissingInternalCount = missingInternal < 0 ? 0 : missingInternal;
                    MissingExternalCount = missingExternal < 0 ? 0 : missingExternal;
                    LatestExamLabel      = latestExam?.ExamMonth ?? "N/A";
                    ModuleLockStatuses   = new ObservableCollection<ModuleLockInfo>(moduleLockInfos);
                    RecentLogs           = new ObservableCollection<AuditLog>(firstPageLogs);
                    RecentAlerts         = new ObservableCollection<SystemAlert>(alerts);
                    LogsCurrentPage      = 1;
                    LogsTotalPages       = Math.Max(1, (int)Math.Ceiling((double)_totalLogCount / LogsPageSize));
                    IsLoading            = false;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Dashboard load error: {ex.Message}");
                System.Windows.Application.Current.Dispatcher.Invoke(() => IsLoading = false);
            }
        }
    }

    public class ModuleLockInfo
    {
        public string ModuleName { get; set; } = "";
        public bool   IsLocked   { get; set; }
        public string LockedBy   { get; set; } = "";
        public string StatusLabel => IsLocked ? "LOCKED" : "OPEN";
    }
}
