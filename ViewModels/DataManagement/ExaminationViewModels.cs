using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ACGCET_Admin.Models;
using Microsoft.EntityFrameworkCore;

namespace ACGCET_Admin.ViewModels.DataManagement
{
    // ════════════════════════════════════════════════════
    //  EXAMINATIONS
    // ════════════════════════════════════════════════════
    public partial class ManageExaminationsViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _db;

        [ObservableProperty] private string _examCode = "";
        [ObservableProperty] private string _examMonth = "";
        [ObservableProperty] private int? _examYear;
        [ObservableProperty] private DateTime? _startDate;
        [ObservableProperty] private DateTime? _endDate;
        [ObservableProperty] private ExamType? _selectedExamType;
        [ObservableProperty] private ObservableCollection<ExamType> _examTypeList = new();
        [ObservableProperty] private ObservableCollection<Examination> _examinations = new();
        [ObservableProperty] private Examination? _selectedExamination;

        public ObservableCollection<string> Months { get; } = new()
        {
            "January","February","March","April","May","June",
            "July","August","September","October","November","December"
        };

        public ManageExaminationsViewModel(AcgcetDbContext db) { _db = db; _ = LoadAsync(); }
        public ManageExaminationsViewModel() { _db = null!; }

        [RelayCommand]
        private async Task LoadAsync()
        {
            ExamTypeList = new ObservableCollection<ExamType>(await _db.ExamTypes.OrderBy(e => e.TypeName).ToListAsync());
            Examinations = new ObservableCollection<Examination>(
                await _db.Examinations.Include(e => e.ExamType)
                    .OrderByDescending(e => e.ExamYear).ThenBy(e => e.ExamMonth).ToListAsync());
        }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(ExamCode))
            { MessageBox.Show("Exam Code required.", "Validation"); return; }

            if (await _db.Examinations.AnyAsync(e => e.ExamCode == ExamCode))
            { MessageBox.Show("Exam Code already exists.", "Duplicate"); return; }

            _db.Examinations.Add(new Examination
            {
                ExamCode = ExamCode.Trim(),
                ExamMonth = string.IsNullOrWhiteSpace(ExamMonth) ? null : ExamMonth,
                ExamYear = ExamYear,
                ExamTypeId = SelectedExamType?.ExamTypeId,
                StartDate = StartDate.HasValue ? DateOnly.FromDateTime(StartDate.Value) : null,
                EndDate = EndDate.HasValue ? DateOnly.FromDateTime(EndDate.Value) : null,
                IsResultLocked = false
            });

            try
            {
                await _db.SaveChangesAsync();
                MessageBox.Show("Examination added!", "Success");
                Clear();
                await LoadAsync();
            }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Save failed: {ex.InnerException?.Message ?? ex.Message}", "Error"); }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedExamination == null) return;
            if (MessageBox.Show($"Delete exam '{SelectedExamination.ExamCode}'?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            _db.Examinations.Remove(SelectedExamination);
            try { await _db.SaveChangesAsync(); await LoadAsync(); }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Cannot delete — referenced.\n{ex.InnerException?.Message}", "Error"); }
        }

        [RelayCommand]
        private void Clear()
        {
            ExamCode = ""; ExamMonth = ""; ExamYear = null; StartDate = null; EndDate = null;
            SelectedExamType = null; SelectedExamination = null;
        }
    }

    // ════════════════════════════════════════════════════
    //  EXAM APPLICATIONS
    // ════════════════════════════════════════════════════
    public partial class ManageExamApplicationsViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _db;

        [ObservableProperty] private string _searchRegNo = "";
        [ObservableProperty] private Student? _selectedStudent;
        [ObservableProperty] private Examination? _selectedExamination;
        [ObservableProperty] private decimal? _totalFees;
        [ObservableProperty] private bool _isPaid;

        [ObservableProperty] private ObservableCollection<Examination> _examinationList = new();
        [ObservableProperty] private ObservableCollection<ExamApplication> _applications = new();
        [ObservableProperty] private ExamApplication? _selectedApplication;

        // Papers for selected application
        [ObservableProperty] private ObservableCollection<Paper> _availablePapers = new();
        [ObservableProperty] private ObservableCollection<PaperSelectionItem> _paperSelections = new();

        public ManageExamApplicationsViewModel(AcgcetDbContext db) { _db = db; _ = LoadAsync(); }
        public ManageExamApplicationsViewModel() { _db = null!; }

        [RelayCommand]
        private async Task LoadAsync()
        {
            ExaminationList = new ObservableCollection<Examination>(
                await _db.Examinations.OrderByDescending(e => e.ExamYear).ToListAsync());
            Applications = new ObservableCollection<ExamApplication>(
                await _db.ExamApplications
                    .Include(a => a.Student).Include(a => a.Examination)
                    .OrderByDescending(a => a.ApplicationDate)
                    .Take(200).ToListAsync());
        }

        [RelayCommand]
        private async Task SearchStudent()
        {
            if (string.IsNullOrWhiteSpace(SearchRegNo)) return;
            SelectedStudent = await _db.Students
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.RegistrationNumber == SearchRegNo.Trim());

            if (SelectedStudent == null)
            { MessageBox.Show("Student not found.", "Search"); return; }

            // Load available papers for student's course
            if (SelectedStudent.CourseId != null)
            {
                var papers = await _db.Papers.Where(p => p.CourseId == SelectedStudent.CourseId)
                    .OrderBy(p => p.Semester).ThenBy(p => p.PaperCode).ToListAsync();
                PaperSelections = new ObservableCollection<PaperSelectionItem>(
                    papers.Select(p => new PaperSelectionItem
                    {
                        PaperId = p.PaperId,
                        PaperCode = p.PaperCode,
                        PaperName = p.PaperName,
                        Semester = p.Semester
                    }));
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            if (SelectedStudent == null) { MessageBox.Show("Search and select a student first.", "Validation"); return; }
            if (SelectedExamination == null) { MessageBox.Show("Select an Examination.", "Validation"); return; }

            if (await _db.ExamApplications.AnyAsync(a => a.StudentId == SelectedStudent.StudentId
                && a.ExaminationId == SelectedExamination.ExaminationId))
            { MessageBox.Show("Application already exists for this student and exam.", "Duplicate"); return; }

            var selectedPapers = PaperSelections.Where(p => p.IsSelected).ToList();

            var app = new ExamApplication
            {
                StudentId = SelectedStudent.StudentId,
                ExaminationId = SelectedExamination.ExaminationId,
                ApplicationDate = DateTime.Now,
                TotalFees = TotalFees,
                IsPaid = IsPaid,
                PaymentDate = IsPaid ? DateTime.Now : null,
                ApprovalStatus = "Pending",
                CreatedBy = "Admin"
            };

            _db.ExamApplications.Add(app);
            await _db.SaveChangesAsync();

            // Add selected papers
            foreach (var ps in selectedPapers)
            {
                _db.ExamApplicationPapers.Add(new ExamApplicationPaper
                {
                    ExamApplicationId = app.ExamApplicationId,
                    PaperId = ps.PaperId,
                    Semester = ps.Semester
                });
            }

            try
            {
                await _db.SaveChangesAsync();
                MessageBox.Show("Exam Application created!", "Success");
                Clear();
                await LoadAsync();
            }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Save failed: {ex.InnerException?.Message ?? ex.Message}", "Error"); }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedApplication == null) return;
            if (MessageBox.Show("Delete this application and its papers?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            // Delete child papers first
            var papers = await _db.ExamApplicationPapers
                .Where(p => p.ExamApplicationId == SelectedApplication.ExamApplicationId).ToListAsync();
            _db.ExamApplicationPapers.RemoveRange(papers);
            _db.ExamApplications.Remove(SelectedApplication);

            try { await _db.SaveChangesAsync(); await LoadAsync(); }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Cannot delete.\n{ex.InnerException?.Message}", "Error"); }
        }

        [RelayCommand]
        private void Clear()
        {
            SearchRegNo = ""; SelectedStudent = null; SelectedExamination = null;
            TotalFees = null; IsPaid = false; SelectedApplication = null;
            PaperSelections.Clear();
        }
    }

    public partial class PaperSelectionItem : ObservableObject
    {
        public int PaperId { get; set; }
        public string PaperCode { get; set; } = "";
        public string PaperName { get; set; } = "";
        public int Semester { get; set; }
        [ObservableProperty] private bool _isSelected;
        [ObservableProperty] private decimal? _fees;
    }

    // ════════════════════════════════════════════════════
    //  EXAM SCHEDULE
    // ════════════════════════════════════════════════════
    public partial class ManageExamScheduleViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _db;

        [ObservableProperty] private Examination? _selectedExamination;
        [ObservableProperty] private Paper? _selectedPaper;
        [ObservableProperty] private DateTime? _examDate;
        [ObservableProperty] private ExamSession? _selectedExamSession;

        [ObservableProperty] private ObservableCollection<Examination> _examinationList = new();
        [ObservableProperty] private ObservableCollection<Paper> _paperList = new();
        [ObservableProperty] private ObservableCollection<ExamSession> _examSessionList = new();
        [ObservableProperty] private ObservableCollection<ExamSchedule> _schedules = new();
        [ObservableProperty] private ExamSchedule? _selectedSchedule;

        public ManageExamScheduleViewModel(AcgcetDbContext db) { _db = db; _ = LoadAsync(); }
        public ManageExamScheduleViewModel() { _db = null!; }

        [RelayCommand]
        private async Task LoadAsync()
        {
            ExaminationList = new ObservableCollection<Examination>(
                await _db.Examinations.OrderByDescending(e => e.ExamYear).ToListAsync());
            PaperList = new ObservableCollection<Paper>(
                await _db.Papers.OrderBy(p => p.PaperCode).ToListAsync());
            ExamSessionList = new ObservableCollection<ExamSession>(
                await _db.ExamSessions.OrderBy(e => e.SessionName).ToListAsync());
            Schedules = new ObservableCollection<ExamSchedule>(
                await _db.ExamSchedules
                    .Include(s => s.Examination).Include(s => s.Paper).Include(s => s.ExamSession)
                    .OrderByDescending(s => s.ExamDate).ToListAsync());
        }

        [RelayCommand]
        private async Task Save()
        {
            if (SelectedExamination == null || SelectedPaper == null || ExamDate == null)
            { MessageBox.Show("Examination, Paper, and Date required.", "Validation"); return; }

            if (await _db.ExamSchedules.AnyAsync(s => s.ExaminationId == SelectedExamination.ExaminationId
                && s.PaperId == SelectedPaper.PaperId))
            { MessageBox.Show("Schedule already exists for this exam and paper.", "Duplicate"); return; }

            _db.ExamSchedules.Add(new ExamSchedule
            {
                ExaminationId = SelectedExamination.ExaminationId,
                PaperId = SelectedPaper.PaperId,
                ExamDate = DateOnly.FromDateTime(ExamDate.Value),
                ExamSessionId = SelectedExamSession?.ExamSessionId
            });

            try
            {
                await _db.SaveChangesAsync();
                MessageBox.Show("Schedule added!", "Success");
                Clear();
                await LoadAsync();
            }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Save failed: {ex.InnerException?.Message ?? ex.Message}", "Error"); }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedSchedule == null) return;
            if (MessageBox.Show("Delete this schedule entry?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            _db.ExamSchedules.Remove(SelectedSchedule);
            try { await _db.SaveChangesAsync(); await LoadAsync(); }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Cannot delete — seat allocations may exist.\n{ex.InnerException?.Message}", "Error"); }
        }

        [RelayCommand]
        private void Clear()
        {
            SelectedExamination = null; SelectedPaper = null; ExamDate = null;
            SelectedExamSession = null; SelectedSchedule = null;
        }
    }

    // ════════════════════════════════════════════════════
    //  ROOMS
    // ════════════════════════════════════════════════════
    public partial class ManageRoomsViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _db;

        [ObservableProperty] private string _roomNumber = "";
        [ObservableProperty] private Block? _selectedBlock;
        [ObservableProperty] private int? _floorNumber;
        [ObservableProperty] private int? _rowCount;
        [ObservableProperty] private int? _columnCount;
        [ObservableProperty] private int? _totalCapacity;

        [ObservableProperty] private ObservableCollection<Block> _blockList = new();
        [ObservableProperty] private ObservableCollection<Room> _rooms = new();
        [ObservableProperty] private Room? _selectedRoom;

        public ManageRoomsViewModel(AcgcetDbContext db) { _db = db; _ = LoadAsync(); }
        public ManageRoomsViewModel() { _db = null!; }

        [RelayCommand]
        private async Task LoadAsync()
        {
            BlockList = new ObservableCollection<Block>(await _db.Blocks.OrderBy(b => b.BlockName).ToListAsync());
            Rooms = new ObservableCollection<Room>(
                await _db.Rooms.Include(r => r.Block).OrderBy(r => r.RoomNumber).ToListAsync());
        }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(RoomNumber))
            { MessageBox.Show("Room Number required.", "Validation"); return; }

            if (SelectedBlock != null && await _db.Rooms.AnyAsync(r => r.RoomNumber == RoomNumber && r.BlockId == SelectedBlock.BlockId))
            { MessageBox.Show("Room already exists in that block.", "Duplicate"); return; }

            _db.Rooms.Add(new Room
            {
                RoomNumber = RoomNumber.Trim(),
                BlockId = SelectedBlock?.BlockId,
                FloorNumber = FloorNumber,
                RowCount = RowCount,
                ColumnCount = ColumnCount,
                TotalCapacity = TotalCapacity,
                IsActive = true
            });

            try
            {
                await _db.SaveChangesAsync();
                MessageBox.Show("Room added!", "Success");
                Clear();
                await LoadAsync();
            }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Save failed: {ex.InnerException?.Message ?? ex.Message}", "Error"); }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedRoom == null) return;
            if (MessageBox.Show($"Delete room '{SelectedRoom.RoomNumber}'?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            _db.Rooms.Remove(SelectedRoom);
            try { await _db.SaveChangesAsync(); await LoadAsync(); }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Cannot delete — seat allocations exist.\n{ex.InnerException?.Message}", "Error"); }
        }

        [RelayCommand]
        private void Clear()
        {
            RoomNumber = ""; SelectedBlock = null; FloorNumber = null;
            RowCount = null; ColumnCount = null; TotalCapacity = null; SelectedRoom = null;
        }
    }

    // ════════════════════════════════════════════════════
    //  SEAT ALLOCATIONS
    // ════════════════════════════════════════════════════
    public partial class ManageSeatAllocationsViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _db;

        [ObservableProperty] private ExamSchedule? _selectedSchedule;
        [ObservableProperty] private string _searchRegNo = "";
        [ObservableProperty] private Student? _selectedStudent;
        [ObservableProperty] private Room? _selectedRoom;
        [ObservableProperty] private int _seatNumber;
        [ObservableProperty] private int? _rowNumber;
        [ObservableProperty] private int? _columnNumber;

        [ObservableProperty] private ObservableCollection<ExamSchedule> _scheduleList = new();
        [ObservableProperty] private ObservableCollection<Room> _roomList = new();
        [ObservableProperty] private ObservableCollection<SeatAllocation> _allocations = new();
        [ObservableProperty] private SeatAllocation? _selectedAllocation;

        public ManageSeatAllocationsViewModel(AcgcetDbContext db) { _db = db; _ = LoadAsync(); }
        public ManageSeatAllocationsViewModel() { _db = null!; }

        [RelayCommand]
        private async Task LoadAsync()
        {
            ScheduleList = new ObservableCollection<ExamSchedule>(
                await _db.ExamSchedules.Include(s => s.Examination).Include(s => s.Paper)
                    .OrderByDescending(s => s.ExamDate).ToListAsync());
            RoomList = new ObservableCollection<Room>(
                await _db.Rooms.Where(r => r.IsActive == true).Include(r => r.Block).OrderBy(r => r.RoomNumber).ToListAsync());
            Allocations = new ObservableCollection<SeatAllocation>(
                await _db.SeatAllocations
                    .Include(a => a.ExamSchedule).ThenInclude(s => s!.Paper)
                    .Include(a => a.Student).Include(a => a.Room)
                    .OrderByDescending(a => a.SeatAllocationId)
                    .Take(200).ToListAsync());
        }

        [RelayCommand]
        private async Task SearchStudent()
        {
            if (string.IsNullOrWhiteSpace(SearchRegNo)) return;
            SelectedStudent = await _db.Students.FirstOrDefaultAsync(s => s.RegistrationNumber == SearchRegNo.Trim());
            if (SelectedStudent == null) MessageBox.Show("Student not found.", "Search");
        }

        [RelayCommand]
        private async Task Save()
        {
            if (SelectedSchedule == null || SelectedStudent == null || SelectedRoom == null || SeatNumber < 1)
            { MessageBox.Show("Schedule, Student, Room and Seat Number required.", "Validation"); return; }

            if (await _db.SeatAllocations.AnyAsync(a => a.ExamScheduleId == SelectedSchedule.ExamScheduleId
                && a.RoomId == SelectedRoom.RoomId && a.SeatNumber == SeatNumber))
            { MessageBox.Show("Seat already allocated.", "Duplicate"); return; }

            _db.SeatAllocations.Add(new SeatAllocation
            {
                ExamScheduleId = SelectedSchedule.ExamScheduleId,
                StudentId = SelectedStudent.StudentId,
                RoomId = SelectedRoom.RoomId,
                SeatNumber = SeatNumber,
                RowNumber = RowNumber,
                ColumnNumber = ColumnNumber
            });

            try
            {
                await _db.SaveChangesAsync();
                MessageBox.Show("Seat allocated!", "Success");
                Clear();
                await LoadAsync();
            }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Save failed: {ex.InnerException?.Message ?? ex.Message}", "Error"); }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedAllocation == null) return;
            if (MessageBox.Show("Remove this seat allocation?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            _db.SeatAllocations.Remove(SelectedAllocation);
            try { await _db.SaveChangesAsync(); await LoadAsync(); }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Cannot delete.\n{ex.InnerException?.Message}", "Error"); }
        }

        [RelayCommand]
        private void Clear()
        {
            SelectedSchedule = null; SearchRegNo = ""; SelectedStudent = null;
            SelectedRoom = null; SeatNumber = 0; RowNumber = null; ColumnNumber = null;
            SelectedAllocation = null;
        }
    }
}
