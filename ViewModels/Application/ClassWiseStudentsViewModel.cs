using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using ACGCET_Admin.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Threading.Tasks;
using System;

namespace ACGCET_Admin.ViewModels.Application
{
    public partial class ClassWiseStudentsViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _dbContext;

        // Filter Collections
        [ObservableProperty]
        private ObservableCollection<string> _levels = new() { "Select" };

        [ObservableProperty]
        private ObservableCollection<string> _programs = new() { "Select" }; 

        [ObservableProperty]
        private ObservableCollection<string> _batches = new() { "Select" };

        [ObservableProperty]
        private ObservableCollection<string> _sections = new() { "Select", "Overall", "A", "B", "C", "D" };

        // Selected Values
        [ObservableProperty]
        private string _selectedLevel = "Select";

        [ObservableProperty]
        private string _selectedProgram = "Select";

        [ObservableProperty]
        private string _selectedBatch = "Select";

        [ObservableProperty]
        private string _selectedSection = "Select";

        // Sorting / Options
        [ObservableProperty]
        private bool _orderByRegNo = true;
        
        [ObservableProperty]
        private bool _orderByRollNo;

        [ObservableProperty]
        private bool _orderByGender;

        [ObservableProperty]
        private bool _showAllStudents = true;

        [ObservableProperty]
        private bool _showMissingRegNo;

        // DataGrid Source
        [ObservableProperty]
        private ObservableCollection<Student> _studentsList = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotLoading))]
        private bool _isLoading;

        public bool IsNotLoading => !IsLoading;

        // Pagination Properties
        private List<Student> _fullStudentList = new();

        [ObservableProperty]
        private ObservableCollection<int> _pageSizes = new() { 10, 20, 50, 100 };

        [ObservableProperty]
        private int _selectedPageSize = 30; // Default

        partial void OnSelectedPageSizeChanged(int value)
        {
            CurrentPage = 1;
            ApplyPagination();
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PageInfo))]
        [NotifyPropertyChangedFor(nameof(CanGoNext))]
        [NotifyPropertyChangedFor(nameof(CanGoPrev))]
        private int _currentPage = 1;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PageInfo))]
        [NotifyPropertyChangedFor(nameof(CanGoNext))]
        [NotifyPropertyChangedFor(nameof(CanGoPrev))]
        private int _totalPages = 1;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PageInfo))]
        private int _totalRecords = 0;

        public string PageInfo => $"Page {CurrentPage} of {TotalPages} ({TotalRecords} items)";

        public bool CanGoNext => CurrentPage < TotalPages;
        public bool CanGoPrev => CurrentPage > 1;


        public ClassWiseStudentsViewModel(AcgcetDbContext dbContext)
        {
            _dbContext = dbContext;
            Task.Run(InitializeAsync);
        }

        public ClassWiseStudentsViewModel() 
        {
             _dbContext = null!;
        } // Design-time

        private async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                
                var levels = new[] { "Select", "UG", "PG" };
                List<string> batches = new List<string> { "Select" };
                List<string> programs = new List<string> { "Select" };

                if (_dbContext != null)
                {
                   var dbBatches = await _dbContext.Batches
                                       .Select(b => b.BatchName)
                                       .Distinct()
                                       .ToListAsync();
                   batches.AddRange(dbBatches);
                   
                   var dbPrograms = await _dbContext.Programs
                                        .Select(p => p.ProgramName)
                                        .ToListAsync();
                   programs.AddRange(dbPrograms);
                }

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    Levels = new ObservableCollection<string>(levels);
                    Batches = new ObservableCollection<string>(batches);
                    Programs = new ObservableCollection<string>(programs);
                    SelectedProgram = "Select";
                    SelectedLevel = "Select";
                });
            }
            catch (Exception) { }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadProgramsAsync(string level)
        {
             if (_dbContext == null) return;
             IsLoading = true;
             try
             {
                 List<string> programs = new List<string> { "Select" };
                 
                 if (level == "Select" || string.IsNullOrEmpty(level))
                 {
                     var all = await _dbContext.Programs.Select(p => p.ProgramName).ToListAsync();
                     programs.AddRange(all);
                 }
                 else
                 {
                     // Simplify for now, assuming Degree logic exists or strict string matching
                     var filtered = await _dbContext.Programs
                                          .Where(p => p.Degree != null && p.Degree.GraduationLevel == level)
                                          .Select(p => p.ProgramName)
                                          .ToListAsync();
                     programs.AddRange(filtered);
                 }
                 
                 System.Windows.Application.Current.Dispatcher.Invoke(() =>
                 {
                     Programs = new ObservableCollection<string>(programs);
                     SelectedProgram = "Select";
                 });
             }
             catch { }
             finally { IsLoading = false; }
        }

        partial void OnSelectedLevelChanged(string value)
        {
             Task.Run(() => LoadProgramsAsync(value));
        }

        [RelayCommand]
        private async Task ViewReport()
        {
            if (_dbContext == null) return;

            IsLoading = true;
            try
            {
                var query = _dbContext.Students.Include(s => s.Regulation).AsQueryable();

                if (SelectedProgram != "Select")
                {
                      query = query.Where(s => s.Batch != null && s.Batch.Course != null && s.Batch.Course.Program != null && s.Batch.Course.Program.ProgramName == SelectedProgram);
                }

                if (SelectedBatch != "Select")
                {
                     query = query.Where(s => s.Batch != null && s.Batch.BatchName == SelectedBatch);
                }

                if (SelectedSection != "Select" && SelectedSection != "Overall")
                {
                     query = query.Where(s => s.Section != null && s.Section.SectionName == SelectedSection);
                }

                if (ShowMissingRegNo)
                {
                    query = query.Where(s => string.IsNullOrEmpty(s.RegistrationNumber));
                }

                if (OrderByRegNo) query = query.OrderBy(s => s.RegistrationNumber);
                else if (OrderByRollNo) query = query.OrderBy(s => s.RollNumber);
                else if (OrderByGender) query = query.OrderBy(s => s.Gender);

                var results = await query.ToListAsync();

                System.Windows.Application.Current.Dispatcher.Invoke(() => 
                {
                    _fullStudentList = results;
                    CurrentPage = 1;
                    ApplyPagination();
                });
            }
             catch { }
             finally { IsLoading = false; }
        }

        private void ApplyPagination()
        {
            TotalRecords = _fullStudentList.Count;
            TotalPages = (int)Math.Ceiling((double)TotalRecords / SelectedPageSize);
            if (TotalPages == 0) TotalPages = 1;

            if (CurrentPage > TotalPages) CurrentPage = TotalPages;
            if (CurrentPage < 1) CurrentPage = 1;

            var pagedItems = _fullStudentList
                                .Skip((CurrentPage - 1) * SelectedPageSize)
                                .Take(SelectedPageSize)
                                .ToList();
            
            StudentsList = new ObservableCollection<Student>(pagedItems);
        }

        [RelayCommand]
        private void NextPage()
        {
            if (CanGoNext)
            {
                CurrentPage++;
                ApplyPagination();
            }
        }

        [RelayCommand]
        private void PreviousPage()
        {
            if (CanGoPrev)
            {
                CurrentPage--;
                ApplyPagination();
            }
        }


        [RelayCommand]
        private void ClearFilters()
        {
            SelectedLevel = "Select";
            SelectedProgram = "Select";
            SelectedBatch = "Select";
            SelectedSection = "Select";
            StudentsList.Clear();
            _fullStudentList.Clear();
            TotalRecords = 0;
            CurrentPage = 1;
            TotalPages = 1;
        }
        [RelayCommand]
        public void Preview()
        {
            if (_fullStudentList.Count == 0) { Services.CustomMessageBox.Show("No data to display"); return; }
            if (SelectedBatch == "Select") { Services.CustomMessageBox.Show("Select Batch"); return; }

            var columns = new List<Services.PrintColumnDefinition>
            {
                new() { Header = "SNo", BindingPath = "StudentId", Width = 40 },
                new() { Header = "ANo", BindingPath = "AdmissionNumber", Width = 90 },
                new() { Header = "Roll No", BindingPath = "RollNumber", Width = 110 },
                new() { Header = "Reg. No", BindingPath = "RegistrationNumber", Width = 110 },
                new() { Header = "Student Name", BindingPath = "FullName", Width = 250 },
                new() { Header = "Regulation", BindingPath = "RegulationId", Width = 80 }
            };

            var printService = new Services.PrintService();
            printService.GenerateDynamicReport(
                _fullStudentList, 
                columns,
                SelectedLevel, 
                SelectedProgram, 
                SelectedBatch, 
                SelectedSection
            );
        }

        [RelayCommand]
        public void Print()
        {
            Preview();
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await ViewReport();
        }
    }
}
