using System;
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
    public partial class AnomalyManagementViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _db;

        [ObservableProperty] private ObservableCollection<AnomalyDetectionLog> _incidents = new();
        [ObservableProperty] private AnomalyDetectionLog? _selectedIncident;
        [ObservableProperty] private string _investigationNotes = "";
        [ObservableProperty] private string _actionTaken = "";
        [ObservableProperty] private bool _isLoading;

        [ObservableProperty]
        private ObservableCollection<string> _statusFilters = new()
            { "All", "Uninvestigated", "Investigated" };
        [ObservableProperty] private string _selectedStatusFilter = "Uninvestigated";

        [ObservableProperty]
        private ObservableCollection<string> _severityFilters = new()
            { "All", "High", "Medium", "Low" };
        [ObservableProperty] private string _selectedSeverityFilter = "All";

        public AnomalyManagementViewModel(AcgcetDbContext db)
        {
            _db = db;
            _ = LoadAsync();
        }

        public AnomalyManagementViewModel() { _db = null!; }

        partial void OnSelectedStatusFilterChanged(string value) => _ = LoadAsync();
        partial void OnSelectedSeverityFilterChanged(string value) => _ = LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            if (_db == null) return;
            IsLoading = true;
            try
            {
                var query = _db.AnomalyDetectionLogs
                    .Include(m => m.SuspiciousUser)
                    .Include(m => m.InvestigatedByNavigation)
                    .AsQueryable();

                if (SelectedStatusFilter == "Uninvestigated")
                    query = query.Where(m => m.IsInvestigated != true);
                else if (SelectedStatusFilter == "Investigated")
                    query = query.Where(m => m.IsInvestigated == true);

                if (SelectedSeverityFilter != "All")
                    query = query.Where(m => m.SeverityLevel == SelectedSeverityFilter);

                var items = await query
                    .OrderByDescending(m => m.DetectionDateTime)
                    .Take(200)
                    .ToListAsync();

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    Incidents = new ObservableCollection<AnomalyDetectionLog>(items);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading incidents: {ex.Message}");
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private async Task MarkInvestigated()
        {
            if (SelectedIncident == null) return;
            if (string.IsNullOrWhiteSpace(InvestigationNotes))
            {
                MessageBox.Show("Please enter investigation notes.", "Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedIncident.IsInvestigated = true;
            SelectedIncident.InvestigationNotes = InvestigationNotes;
            SelectedIncident.ActionTaken = string.IsNullOrWhiteSpace(ActionTaken) ? "Reviewed — no action needed" : ActionTaken;
            SelectedIncident.InvestigationDateTime = DateTime.Now;

            await _db.SaveChangesAsync();
            MessageBox.Show("Incident marked as investigated.");
            InvestigationNotes = "";
            ActionTaken = "";
            await LoadAsync();
        }

        [RelayCommand]
        private async Task DismissIncident()
        {
            if (SelectedIncident == null) return;

            SelectedIncident.IsInvestigated = true;
            SelectedIncident.InvestigationNotes = "Dismissed — false positive";
            SelectedIncident.ActionTaken = "Dismissed";
            SelectedIncident.InvestigationDateTime = DateTime.Now;

            await _db.SaveChangesAsync();
            await LoadAsync();
        }
    }
}

