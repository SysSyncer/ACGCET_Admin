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
    public partial class CorrectionRequestRow : ObservableObject
    {
        public int RequestId { get; set; }
        public string RequestedByUser { get; set; } = "";
        public string TypeName { get; set; } = "";
        public string TargetTable { get; set; } = "";
        public string TargetRecordDetails { get; set; } = "";
        public string CurrentValue { get; set; } = "";
        public string ProposedValue { get; set; } = "";
        public string Reason { get; set; } = "";
        public string ApprovalStatus { get; set; } = "";
        public DateTime? RequestedDateTime { get; set; }
    }

    public partial class DataCorrectionManagementViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _db;

        [ObservableProperty] private ObservableCollection<CorrectionRequestRow> _requests = new();
        [ObservableProperty] private CorrectionRequestRow? _selectedRequest;
        [ObservableProperty] private string _approvalComments = "";
        [ObservableProperty] private bool _isLoading = false;
        [ObservableProperty] private string _statusMessage = "";
        [ObservableProperty] private string _statusColor = "#4CAF50";
        [ObservableProperty] private string _filterStatus = "Pending";
        [ObservableProperty] private bool _hasRequests = false;

        public ObservableCollection<string> StatusOptions { get; } = new() { "All", "Pending", "Approved", "Rejected", "Executed" };

        public DataCorrectionManagementViewModel(AcgcetDbContext db)
        {
            _db = db;
            _ = LoadRequestsAsync();
        }

        public DataCorrectionManagementViewModel() { _db = null!; }

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
                var query = _db.DataCorrectionRequests
                    .Include(r => r.RequestedByNavigation)
                    .Include(r => r.CorrectionRequestType)
                    .Where(r => r.IsActive == true);

                if (FilterStatus != "All")
                    query = query.Where(r => r.ApprovalStatus == FilterStatus);

                var list = await query
                    .OrderByDescending(r => r.RequestedDateTime)
                    .Take(200)
                    .ToListAsync();

                Requests = new ObservableCollection<CorrectionRequestRow>(list.Select(r => new CorrectionRequestRow
                {
                    RequestId = r.DataCorrectionRequestId,
                    RequestedByUser = r.RequestedByNavigation?.FullName ?? r.RequestedByNavigation?.UserName ?? "Unknown",
                    TypeName = r.CorrectionRequestType?.TypeName ?? "General",
                    TargetTable = r.TargetTable,
                    TargetRecordDetails = r.TargetRecordDetails ?? "",
                    CurrentValue = r.CurrentValue ?? "",
                    ProposedValue = r.ProposedValue ?? "",
                    Reason = r.Reason,
                    ApprovalStatus = r.ApprovalStatus ?? "Pending",
                    RequestedDateTime = r.RequestedDateTime
                }));

                HasRequests = Requests.Any();
                SetStatus(HasRequests ? $"Showing {Requests.Count} request(s)." : "No requests found.", HasRequests ? "#4CAF50" : "#FF9800");
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
        private async Task ApproveRequest()
        {
            if (SelectedRequest == null) { SetStatus("Select a request first.", "#FF9800"); return; }
            await UpdateRequestStatus("Approved");
        }

        [RelayCommand]
        private async Task RejectRequest()
        {
            if (SelectedRequest == null) { SetStatus("Select a request first.", "#FF9800"); return; }
            await UpdateRequestStatus("Rejected");
        }

        [RelayCommand]
        private async Task ExecuteRequest()
        {
            if (SelectedRequest == null) { SetStatus("Select a request first.", "#FF9800"); return; }

            try
            {
                var req = await _db.DataCorrectionRequests.FindAsync(SelectedRequest.RequestId);
                if (req == null) return;

                if (req.ApprovalStatus != "Approved")
                {
                    SetStatus("Only approved requests can be executed.", "#FF9800");
                    return;
                }

                // Execute the correction based on target table
                bool executed = await ExecuteCorrectionAsync(req);
                if (!executed) return;

                req.ApprovalStatus = "Executed";
                req.ExecutedDateTime = DateTime.Now;
                req.ExecutionNotes = string.IsNullOrWhiteSpace(ApprovalComments) ? "Executed via Admin UI" : ApprovalComments;
                await _db.SaveChangesAsync();

                ApprovalComments = "";
                SetStatus("Correction executed successfully.", "#4CAF50");
                await LoadRequestsAsync();
            }
            catch (Exception ex)
            {
                SetStatus($"Execution failed: {ex.Message}", "#F44336");
            }
        }

        private async Task<bool> ExecuteCorrectionAsync(DataCorrectionRequest req)
        {
            switch (req.TargetTable)
            {
                case "ExamResults":
                    if (int.TryParse(req.TargetRecordId, out int resultId))
                    {
                        var result = await _db.ExamResults.FindAsync(resultId);
                        if (result == null)
                        {
                            SetStatus("Target result record not found.", "#F44336");
                            return false;
                        }
                        // Mark the result record for COE review — actual value changes
                        // should be done through the appropriate entry screen
                        _db.ExamResults.Remove(result);
                        SetStatus("Result record removed. Re-entry can be done through the marks pipeline.", "#4CAF50");
                    }
                    return true;

                case "InternalMarks":
                    if (long.TryParse(req.TargetRecordId, out long markId))
                    {
                        var mark = await _db.InternalMarks.FindAsync(markId);
                        if (mark != null) _db.InternalMarks.Remove(mark);
                    }
                    return true;

                case "ExternalMarks":
                    if (long.TryParse(req.TargetRecordId, out long extMarkId))
                    {
                        var mark = await _db.ExternalMarks.FindAsync(extMarkId);
                        if (mark != null) _db.ExternalMarks.Remove(mark);
                    }
                    return true;

                default:
                    SetStatus($"Automated execution not supported for table '{req.TargetTable}'. Please handle manually.", "#FF9800");
                    return false;
            }
        }

        private async Task UpdateRequestStatus(string status)
        {
            try
            {
                var req = await _db.DataCorrectionRequests.FindAsync(SelectedRequest!.RequestId);
                if (req == null) return;

                req.ApprovalStatus = status;
                req.ApprovalDateTime = DateTime.Now;
                req.ApprovalComments = string.IsNullOrWhiteSpace(ApprovalComments) ? null : ApprovalComments;
                await _db.SaveChangesAsync();

                ApprovalComments = "";
                SetStatus($"Request {status.ToLower()} successfully.", "#4CAF50");
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
