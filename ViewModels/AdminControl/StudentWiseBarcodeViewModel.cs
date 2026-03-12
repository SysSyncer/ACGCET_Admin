using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ACGCET_Admin.Models;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ACGCET_Admin.ViewModels.AdminControl
{
    public partial class StudentWiseBarcodeViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _dbContext;

        [ObservableProperty]
        private string _selectedPaperCode = "";

        [ObservableProperty]
        private ObservableCollection<StudentBarcodeItem> _students = new();

        public StudentWiseBarcodeViewModel(AcgcetDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [RelayCommand]
        private void Search()
        {
            if (string.IsNullOrWhiteSpace(SelectedPaperCode)) return;

            // Find PaperId
            var paper = _dbContext.Papers.FirstOrDefault(p => p.PaperCode == SelectedPaperCode);
            if (paper == null) return;

            // Find ExamApps for this paper
            var apps = _dbContext.ExamApplicationPapers
                .Include(p => p.ExamApplication)
                .ThenInclude(ea => ea!.Student)
                .Where(p => p.PaperId == paper.PaperId)
                .ToList();

            Students.Clear();
            foreach (var app in apps)
            {
                Students.Add(new StudentBarcodeItem
                {
                    RegNo = app.ExamApplication!.Student!.RegistrationNumber ?? "N/A",
                    StudentName = app.ExamApplication.Student.FullName ?? "N/A",
                    Barcode = app.Barcode ?? "N/A",
                    DummyNumber = app.DummyNumber ?? "N/A"
                });
            }
        }

        [RelayCommand]
        private void Print()
        {
            // Placeholder for print logic
            System.Windows.MessageBox.Show("Printing Barcode Report... (Implementation Pending)");
        }

        [RelayCommand]
        private void Refresh()
        {
            Search();
        }

        public StudentWiseBarcodeViewModel() { _dbContext = null!; }
    }

    public class StudentBarcodeItem
    {
        public string RegNo { get; set; } = "";
        public string StudentName { get; set; } = "";
        public string Barcode { get; set; } = "";
        public string DummyNumber { get; set; } = "";
    }
}
