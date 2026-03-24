using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ACGCET_Admin.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace ACGCET_Admin.ViewModels.AdminControl
{
    public partial class LockOverrideRow : ObservableObject
    {
        public int RequestId { get; set; }
        public string RequestedByUser { get; set; } = "";
        public string ModuleName { get; set; } = "";
        public string RequestReason { get; set; } = "";
        public string ApprovalStatus { get; set; } = "";
        public DateTime? RequestedDateTime { get; set; }
        public int? DurationMinutes { get; set; }
    }

    public partial class LockOverrideManagementViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _db;

        [ObservableProperty] private ObservableCollection<LockOverrideRow> _requests = new();
        [ObservableProperty] private LockOverrideRow? _selectedRequest;
        [ObservableProperty] private string _approvalComments = "";
        [ObservableProperty] private int _unlockDurationMinutes = 60;
        [ObservableProperty] private bool _isLoading = false;
        [ObservableProperty] private string _statusMessage = "";
        [ObservableProperty] private string _statusColor = "#4CAF50";
        [ObservableProperty] private string _filterStatus = "Pending";
        [ObservableProperty] private bool _hasRequests = false;

        public ObservableCollection<string> StatusOptions { get; } = new() { "All", "Pending", "Approved", "Rejected" };

        public LockOverrideManagementViewModel(AcgcetDbContext db)
        {
            _db = db;
            _ = LoadRequestsAsync();
        }

        public LockOverrideManagementViewModel() { _db = null!; }

        [RelayCommand]
        private async Task LoadRequests()
        {
            await LoadRequestsAsync();
        }

        private async Task LoadRequestsAsync()
        {
            IsLoading = true;
            try
            {
                var query = _db.LockOverrideRequests
                    .Include(r => r.RequestedByNavigation)
                    .Include(r => r.ModuleLock)
                        .ThenInclude(ml => ml!.Module)
                    .Where(r => r.IsActive == true);

                if (FilterStatus != "All")
                    query = query.Where(r => r.ApprovalStatus == FilterStatus);

                var list = await query
                    .OrderByDescending(r => r.RequestedDateTime)
                    .Take(200)
                    .ToListAsync();

                Requests = new ObservableCollection<LockOverrideRow>(list.Select(r => new LockOverrideRow
                {
                    RequestId = r.LockOverrideRequestId,
                    RequestedByUser = r.RequestedByNavigation?.FullName ?? r.RequestedByNavigation?.UserName ?? "Unknown",
                    ModuleName = r.ModuleLock?.Module?.ModuleName ?? "Unknown Module",
                    RequestReason = r.RequestReason,
                    ApprovalStatus = r.ApprovalStatus ?? "Pending",
                    RequestedDateTime = r.RequestedDateTime,
                    DurationMinutes = r.TemporaryUnlockDuration
                }));

                HasRequests = Requests.Any();
                SetStatus(HasRequests ? $"Showing {Requests.Count} request(s)." : "No requests found.",
                          HasRequests ? "#4CAF50" : "#FF9800");
            }
            catch (Exception ex)
            {
                SetStatus($"Error: {ex.Message}", "#F44336");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ApproveAndUnlock()
        {
            if (SelectedRequest == null) { SetStatus("Select a request first.", "#FF9800"); return; }

            try
            {
                var req = await _db.LockOverrideRequests
                    .Include(r => r.ModuleLock)
                    .FirstOrDefaultAsync(r => r.LockOverrideRequestId == SelectedRequest.RequestId);
                if (req == null) return;

                // Approve the request
                req.ApprovalStatus = "Approved";
                req.ApprovalDateTime = DateTime.Now;
                req.ApprovalComments = string.IsNullOrWhiteSpace(ApprovalComments) ? "Approved via Admin UI" : ApprovalComments;
                req.TemporaryUnlockDuration = UnlockDurationMinutes;
                req.TemporaryUnlockExpiry = DateTime.Now.AddMinutes(UnlockDurationMinutes);

                // Temporarily unlock the module
                if (req.ModuleLock != null)
                {
                    req.ModuleLock.IsLocked = false;
                    req.ModuleLock.UnlockedDateTime = DateTime.Now;
                    req.ModuleLock.UnlockedBy = "Admin (Override)";
                    req.ModuleLock.UnlockReason = $"Temporary override for {UnlockDurationMinutes} min — {req.RequestReason}";
                }

                await _db.SaveChangesAsync();

                ApprovalComments = "";
                SetStatus($"Request approved. Module temporarily unlocked for {UnlockDurationMinutes} minutes.", "#4CAF50");
                await LoadRequestsAsync();
            }
            catch (Exception ex)
            {
                SetStatus($"Error: {ex.Message}", "#F44336");
            }
        }

        [RelayCommand]
        private async Task RejectRequest()
        {
            if (SelectedRequest == null) { SetStatus("Select a request first.", "#FF9800"); return; }

            try
            {
                var req = await _db.LockOverrideRequests.FindAsync(SelectedRequest.RequestId);
                if (req == null) return;

                req.ApprovalStatus = "Rejected";
                req.ApprovalDateTime = DateTime.Now;
                req.ApprovalComments = string.IsNullOrWhiteSpace(ApprovalComments) ? "Rejected" : ApprovalComments;
                await _db.SaveChangesAsync();

                ApprovalComments = "";
                SetStatus("Request rejected.", "#4CAF50");
                await LoadRequestsAsync();
            }
            catch (Exception ex)
            {
                SetStatus($"Error: {ex.Message}", "#F44336");
            }
        }

        private void SetStatus(string msg, string color)
        {
            StatusMessage = msg;
            StatusColor = color;
        }
    }
}
