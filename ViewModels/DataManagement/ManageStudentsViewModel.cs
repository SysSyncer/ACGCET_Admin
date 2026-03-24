using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ACGCET_Admin.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

namespace ACGCET_Admin.ViewModels.DataManagement
{
    public partial class ManageStudentsViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _db;

        // ── Single-student form fields ──
        [ObservableProperty] private string _admissionNumber = "";
        [ObservableProperty] private string _rollNumber = "";
        [ObservableProperty] private string _registrationNumber = "";
        [ObservableProperty] private string _fullName = "";
        [ObservableProperty] private DateTime? _dateOfBirth;
        [ObservableProperty] private string _gender = "";
        [ObservableProperty] private string _mobileNumber = "";
        [ObservableProperty] private string _emailAddress = "";
        [ObservableProperty] private int? _joinYear;

        [ObservableProperty] private Course? _selectedCourse;
        [ObservableProperty] private Batch? _selectedBatch;
        [ObservableProperty] private Section? _selectedSection;
        [ObservableProperty] private Regulation? _selectedRegulation;
        [ObservableProperty] private Community? _selectedCommunity;

        // ── Dropdown sources ──
        [ObservableProperty] private ObservableCollection<Course> _courseList = new();
        [ObservableProperty] private ObservableCollection<Batch> _batchList = new();
        [ObservableProperty] private ObservableCollection<Section> _sectionList = new();
        [ObservableProperty] private ObservableCollection<Regulation> _regulationList = new();
        [ObservableProperty] private ObservableCollection<Community> _communityList = new();

        // ── Grid ──
        [ObservableProperty] private ObservableCollection<Student> _students = new();
        [ObservableProperty] private Student? _selectedStudent;

        // ── CSV ──
        [ObservableProperty] private string _csvFilePath = "";
        [ObservableProperty] private string _csvStatus = "";
        [ObservableProperty] private bool _isImporting;

        public ObservableCollection<string> Genders { get; } = new() { "Male", "Female", "Other" };

        public ManageStudentsViewModel(AcgcetDbContext db) { _db = db; _ = LoadAsync(); }
        public ManageStudentsViewModel() { _db = null!; }

        partial void OnSelectedCourseChanged(Course? value)
        {
            if (value != null)
                BatchList = new ObservableCollection<Batch>(
                    _db.Batches.Where(b => b.CourseId == value.CourseId).OrderByDescending(b => b.BatchYear).ToList());
            else
                BatchList.Clear();
            SelectedBatch = null;
        }

        partial void OnSelectedBatchChanged(Batch? value)
        {
            if (value != null)
                SectionList = new ObservableCollection<Section>(
                    _db.Sections.Where(s => s.BatchId == value.BatchId).OrderBy(s => s.SectionName).ToList());
            else
                SectionList.Clear();
            SelectedSection = null;
        }

        [RelayCommand]
        private async Task LoadAsync()
        {
            CourseList = new ObservableCollection<Course>(await _db.Courses.OrderBy(c => c.CourseName).ToListAsync());
            RegulationList = new ObservableCollection<Regulation>(await _db.Regulations.OrderByDescending(r => r.RegulationYear).ToListAsync());
            CommunityList = new ObservableCollection<Community>(await _db.Communities.OrderBy(c => c.CommunityName).ToListAsync());

            Students = new ObservableCollection<Student>(
                await _db.Students
                    .Include(s => s.Course).Include(s => s.Batch).Include(s => s.Section)
                    .OrderByDescending(s => s.CreatedDate)
                    .Take(200)
                    .ToListAsync());
        }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(FullName))
            { MessageBox.Show("Student Name is required.", "Validation"); return; }

            if (!string.IsNullOrWhiteSpace(RegistrationNumber) &&
                await _db.Students.AnyAsync(s => s.RegistrationNumber == RegistrationNumber))
            { MessageBox.Show("Registration Number already exists.", "Duplicate"); return; }

            _db.Students.Add(new Student
            {
                AdmissionNumber = NullIfEmpty(AdmissionNumber),
                RollNumber = NullIfEmpty(RollNumber),
                RegistrationNumber = NullIfEmpty(RegistrationNumber),
                FullName = FullName.Trim(),
                DateOfBirth = DateOfBirth.HasValue ? DateOnly.FromDateTime(DateOfBirth.Value) : null,
                Gender = NullIfEmpty(Gender),
                MobileNumber = NullIfEmpty(MobileNumber),
                EmailAddress = NullIfEmpty(EmailAddress),
                CommunityId = SelectedCommunity?.CommunityId,
                CourseId = SelectedCourse?.CourseId,
                BatchId = SelectedBatch?.BatchId,
                SectionId = SelectedSection?.SectionId,
                RegulationId = SelectedRegulation?.RegulationId,
                JoinYear = JoinYear,
                JoinDate = DateOnly.FromDateTime(DateTime.Today),
                CreatedBy = "Admin",
                CreatedDate = DateTime.Now
            });

            try
            {
                await _db.SaveChangesAsync();
                MessageBox.Show("Student added!", "Success");
                ClearForm();
                await LoadAsync();
            }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Save failed: {ex.InnerException?.Message ?? ex.Message}", "Error"); }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedStudent == null) return;
            if (MessageBox.Show($"Delete student '{SelectedStudent.FullName}'?\nThis will fail if marks or applications exist.",
                "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            _db.Students.Remove(SelectedStudent);
            try { await _db.SaveChangesAsync(); await LoadAsync(); }
            catch (DbUpdateException ex)
            { MessageBox.Show($"Cannot delete — student has related records.\n{ex.InnerException?.Message}", "Error"); }
        }

        // ── CSV Bulk Upload ──────────────────────────────────

        [RelayCommand]
        private void BrowseCsv()
        {
            var dlg = new OpenFileDialog
            {
                Title = "Select Student CSV File",
                Filter = "CSV Files (*.csv)|*.csv",
                Multiselect = false
            };
            if (dlg.ShowDialog() == true)
                CsvFilePath = dlg.FileName;
        }

        [RelayCommand]
        private async Task ImportCsv()
        {
            if (string.IsNullOrWhiteSpace(CsvFilePath) || !File.Exists(CsvFilePath))
            { MessageBox.Show("Select a valid CSV file first.", "Validation"); return; }

            IsImporting = true;
            CsvStatus = "Reading CSV...";

            try
            {
                var lines = await File.ReadAllLinesAsync(CsvFilePath);
                if (lines.Length < 2)
                { MessageBox.Show("CSV file is empty or has no data rows.", "Validation"); IsImporting = false; return; }

                // Parse header to map column indices
                var headers = lines[0].Split(',').Select(h => h.Trim().ToLower()).ToArray();

                int idxAdm = Array.IndexOf(headers, "admissionnumber");
                int idxRoll = Array.IndexOf(headers, "rollnumber");
                int idxReg = Array.IndexOf(headers, "registrationnumber");
                int idxName = Array.IndexOf(headers, "fullname");
                int idxDob = Array.IndexOf(headers, "dateofbirth");
                int idxGen = Array.IndexOf(headers, "gender");
                int idxMob = Array.IndexOf(headers, "mobilenumber");
                int idxEmail = Array.IndexOf(headers, "emailaddress");
                int idxJYr = Array.IndexOf(headers, "joinyear");

                if (idxName < 0)
                { MessageBox.Show("CSV must have a 'FullName' column.", "Validation"); IsImporting = false; return; }

                // Pre-load lookup data for matching
                var courses = await _db.Courses.ToDictionaryAsync(c => c.CourseCode.ToUpper(), c => c.CourseId);
                var batches = await _db.Batches.ToListAsync();
                var sections = await _db.Sections.ToListAsync();
                var regulations = await _db.Regulations.ToDictionaryAsync(r => r.RegulationYear.ToString(), r => r.RegulationId);
                var communities = await _db.Communities.ToDictionaryAsync(c => c.CommunityName.ToUpper(), c => c.CommunityId);

                int idxCourseCode = Array.IndexOf(headers, "coursecode");
                int idxBatchYear = Array.IndexOf(headers, "batchyear");
                int idxSectionCode = Array.IndexOf(headers, "sectioncode");
                int idxRegulation = Array.IndexOf(headers, "regulationyear");
                int idxCommunity = Array.IndexOf(headers, "community");

                int added = 0, skipped = 0;
                var errors = new List<string>();

                for (int i = 1; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue;

                    var cols = ParseCsvLine(lines[i]);
                    string name = idxName < cols.Length ? cols[idxName].Trim() : "";
                    if (string.IsNullOrWhiteSpace(name)) { skipped++; continue; }

                    string? regNo = idxReg >= 0 && idxReg < cols.Length ? NullIfEmpty(cols[idxReg]) : null;
                    if (regNo != null && await _db.Students.AnyAsync(s => s.RegistrationNumber == regNo))
                    { skipped++; errors.Add($"Row {i + 1}: RegNo '{regNo}' already exists."); continue; }

                    // Resolve FKs
                    int? courseId = null, batchId = null, sectionId = null, regulationId = null, communityId = null;

                    if (idxCourseCode >= 0 && idxCourseCode < cols.Length)
                    {
                        string cc = cols[idxCourseCode].Trim().ToUpper();
                        if (courses.TryGetValue(cc, out int cid)) courseId = cid;
                    }

                    if (idxBatchYear >= 0 && idxBatchYear < cols.Length && int.TryParse(cols[idxBatchYear].Trim(), out int by))
                    {
                        var batch = batches.FirstOrDefault(b => b.BatchYear == by && b.CourseId == courseId);
                        batchId = batch?.BatchId;
                    }

                    if (idxSectionCode >= 0 && idxSectionCode < cols.Length)
                    {
                        string sc = cols[idxSectionCode].Trim().ToUpper();
                        var section = sections.FirstOrDefault(s => s.SectionCode.ToUpper() == sc && s.BatchId == batchId);
                        sectionId = section?.SectionId;
                    }

                    if (idxRegulation >= 0 && idxRegulation < cols.Length)
                    {
                        string ry = cols[idxRegulation].Trim();
                        if (regulations.TryGetValue(ry, out int rid)) regulationId = rid;
                    }

                    if (idxCommunity >= 0 && idxCommunity < cols.Length)
                    {
                        string cn = cols[idxCommunity].Trim().ToUpper();
                        if (communities.TryGetValue(cn, out int cid)) communityId = cid;
                    }

                    DateOnly? dob = null;
                    if (idxDob >= 0 && idxDob < cols.Length && DateTime.TryParse(cols[idxDob].Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                        dob = DateOnly.FromDateTime(d);

                    int? jy = null;
                    if (idxJYr >= 0 && idxJYr < cols.Length && int.TryParse(cols[idxJYr].Trim(), out int jyr))
                        jy = jyr;

                    _db.Students.Add(new Student
                    {
                        AdmissionNumber = idxAdm >= 0 && idxAdm < cols.Length ? NullIfEmpty(cols[idxAdm]) : null,
                        RollNumber = idxRoll >= 0 && idxRoll < cols.Length ? NullIfEmpty(cols[idxRoll]) : null,
                        RegistrationNumber = regNo,
                        FullName = name,
                        DateOfBirth = dob,
                        Gender = idxGen >= 0 && idxGen < cols.Length ? NullIfEmpty(cols[idxGen]) : null,
                        MobileNumber = idxMob >= 0 && idxMob < cols.Length ? NullIfEmpty(cols[idxMob]) : null,
                        EmailAddress = idxEmail >= 0 && idxEmail < cols.Length ? NullIfEmpty(cols[idxEmail]) : null,
                        CommunityId = communityId,
                        CourseId = courseId,
                        BatchId = batchId,
                        SectionId = sectionId,
                        RegulationId = regulationId,
                        JoinYear = jy,
                        JoinDate = DateOnly.FromDateTime(DateTime.Today),
                        CreatedBy = "CSV Import",
                        CreatedDate = DateTime.Now
                    });
                    added++;

                    CsvStatus = $"Processing row {i} of {lines.Length - 1}...";
                }

                if (added > 0)
                {
                    await _db.SaveChangesAsync();
                }

                string msg = $"Import complete!\nAdded: {added}\nSkipped: {skipped}";
                if (errors.Count > 0)
                    msg += $"\n\nFirst issues:\n{string.Join("\n", errors.Take(10))}";

                CsvStatus = $"Done — {added} added, {skipped} skipped.";
                MessageBox.Show(msg, "CSV Import Result");
                await LoadAsync();
            }
            catch (DbUpdateException ex)
            {
                CsvStatus = "Import failed.";
                MessageBox.Show($"Database error during import:\n{ex.InnerException?.Message ?? ex.Message}", "Error");
            }
            catch (Exception ex)
            {
                CsvStatus = "Import failed.";
                MessageBox.Show($"Error reading CSV:\n{ex.Message}", "Error");
            }
            finally
            {
                IsImporting = false;
            }
        }

        [RelayCommand]
        private void ClearForm()
        {
            AdmissionNumber = ""; RollNumber = ""; RegistrationNumber = ""; FullName = "";
            DateOfBirth = null; Gender = ""; MobileNumber = ""; EmailAddress = ""; JoinYear = null;
            SelectedCourse = null; SelectedBatch = null; SelectedSection = null;
            SelectedRegulation = null; SelectedCommunity = null; SelectedStudent = null;
        }

        private static string? NullIfEmpty(string? s) =>
            string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        private static string[] ParseCsvLine(string line)
        {
            var fields = new List<string>();
            bool inQuotes = false;
            var current = new System.Text.StringBuilder();

            foreach (char c in line)
            {
                if (c == '"')
                    inQuotes = !inQuotes;
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(current.ToString());
                    current.Clear();
                }
                else
                    current.Append(c);
            }
            fields.Add(current.ToString());
            return fields.ToArray();
        }
    }
}
