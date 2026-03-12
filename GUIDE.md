# ACGCET Admin V2 — Developer Guide

> **Who is this guide for?**
> This guide is written for new programmers, junior developers, or anyone who is new to **C#**, **WPF**, **MVVM architecture**, and **Entity Framework Core**. We'll explain everything from the ground up — no prior knowledge assumed.

---

## Table of Contents

1. [What is this application?](#1-what-is-this-application)
2. [Technology Stack & NuGet Packages](#2-technology-stack--nuget-packages)
3. [Project Folder Structure](#3-project-folder-structure)
4. [What is MVVM Architecture?](#4-what-is-mvvm-architecture)
5. [What is Entity Framework Core?](#5-what-is-entity-framework-core)
6. [The Database — Tables We Use](#6-the-database--tables-we-use)
7. [How the App Starts — App.xaml.cs & Dependency Injection](#7-how-the-app-starts--appxamlcs--dependency-injection)
8. [The Database Context — AcgcetDbContext.cs](#8-the-database-context--acgcetdbcontextcs)
9. [CommunityToolkit.Mvvm — Understanding the Attributes](#9-communitytoolkitmvvm--understanding-the-attributes)
10. [Data Flow Diagram](#10-data-flow-diagram)
11. [ViewModel Deep-Dives](#11-viewmodel-deep-dives)
    - [LoginViewModel](#loginviewmodel)
    - [DashboardViewModel & HomeViewModel](#dashboardviewmodel--homeviewmodel)
    - [AdminControlViewModel](#admincontrolviewmodel)
    - [NewUserCreationViewModel](#newusercreationviewmodel)
    - [DataInputLockViewModel](#datainputlockviewmodel)
    - [DeleteEntry Family](#deleteentry-family)
    - [MissingEntry Family](#missingentry-family)
    - [EntryReport Family](#entryreport-family)
    - [Application / ClassWiseStudentsViewModel](#application--classwisestudentsviewmodel)
12. [Services](#12-services)
    - [DataSeeder](#dataseeder)
    - [PrintService](#printservice)
    - [CustomMessageBox](#custommessagebox)
13. [Navigation Pattern Explained](#13-navigation-pattern-explained)
14. [Security — BCrypt Password Hashing](#14-security--bcrypt-password-hashing)
15. [Messaging — WeakReferenceMessenger](#15-messaging--weakreferencemessenger)
16. [Common Patterns and Recipes](#16-common-patterns-and-recipes)

---

## 1. What is this application?

**ACGCET Admin V2** is a **desktop application** built for the administrative office of **ACGCET (Alagappa Chettiar College of Engineering and Technology), Karaikudi**. It is a Windows desktop app written in **C# and WPF (Windows Presentation Foundation)**.

The application is used by college administrators (COE officers, department heads) to:

- **Manage student records** (view class-wise lists, look up by register number, etc.)
- **Manage exam applications** (which students have applied for which exam)
- **Enter and manage marks** (both internal marks per test and external marks from the university exam)
- **Generate exam results** (process pass/fail, grades, grand totals)
- **Produce printed reports** (hall tickets, internal mark sheets, result sheets, timetables)
- **Control data entry locks** (prevent editing once marks are finalised)
- **Delete erroneous entries** (admin can remove wrong submissions)
- **Track missing entries** (find students who applied but whose marks haven't been entered yet)
- **Manage admin users** (create users, assign roles/permissions)

---

## 2. Technology Stack & NuGet Packages

The project uses several powerful libraries. Here is a breakdown:

| Package | What it does |
|---|---|
| **WPF (.NET 8)** | The UI framework — builds the Windows desktop application |
| **Microsoft.EntityFrameworkCore.SqlServer** | Connects C# objects to a SQL Server database (no raw SQL needed in most cases) |
| **CommunityToolkit.Mvvm** | Makes MVVM much easier — provides `[ObservableProperty]`, `[RelayCommand]`, etc. |
| **MaterialDesignThemes** | Adds modern Google Material Design UI components (buttons, cards, icons, drawers) |
| **BCrypt.Net-Next** | Securely hashes passwords before storing them in the database |
| **Microsoft.Extensions.Hosting** | Provides the ASP.NET-style startup/DI host for a desktop application |
| **Microsoft.Extensions.DependencyInjection** | Manages service lifetimes and dependency injection |
| **Microsoft.Extensions.Configuration.Json** | Reads `appsettings.json` for configuration (like database connection strings) |

---

## 3. Project Folder Structure

```
ACGCET_Admin_V2/
├── App.xaml / App.xaml.cs      ← App entry point, DI setup, startup logic
├── MainWindow.xaml              ← The main shell window
├── appsettings.json             ← Database connection string, logging config
├── Models/                      ← C# classes that represent database tables
│   ├── AcgcetDbContext.cs       ← The Entity Framework "bridge" between C# and SQL
│   ├── Student.cs               ← Student table model
│   ├── InternalMark.cs          ← Internal marks table model
│   └── ...                      ← One file per database table
├── ViewModels/                  ← Business logic, data queries, commands
│   ├── Dashboard/               ← Login, Main Dashboard, Home (recent alerts/logs)
│   ├── AdminControl/            ← Admin panel: new user, barcode, data locks
│   ├── Application/             ← Reports: class lists, hall tickets, results, marks
│   ├── DeleteEntry/             ← Delete exam apply, marks, results, student records
│   ├── MissingEntry/            ← Find students with missing marks/applications
│   └── EntryReport/             ← Entry auditing: who entered what data
├── Views/                       ← XAML UI files (the actual screens users see)
├── Services/                    ← Utility services (printing, seeding, message boxes)
│   ├── DataSeeder.cs            ← Pre-populates the database with initial/lookup data
│   ├── PrintService.cs          ← Generates printable documents
│   └── CustomMessageBox.xaml    ← A styled popup dialog
├── Helpers/                     ← Small utility classes (e.g., PasswordHelper)
└── Resources/                   ← Images, icons
```

---

## 4. What is MVVM Architecture?

**MVVM** stands for **Model — View — ViewModel**. It is a design pattern that separates your code into three distinct responsibilities. Think of it like a restaurant:

```
┌─────────────┐       ┌──────────────────┐       ┌───────────────┐
│    Model    │       │   ViewModel      │       │     View      │
│  (Kitchen)  │◄─────►│  (Waiter/Chef)   │◄─────►│  (Menu/Table) │
│             │       │                  │       │               │
│ Database    │       │ Business Logic   │       │ XAML UI       │
│ SQL Tables  │       │ Commands         │       │ Buttons       │
│ C# Classes  │       │ Data Queries     │       │ TextBoxes     │
└─────────────┘       └──────────────────┘       └───────────────┘
```

| Layer | In this project | Responsibility |
|---|---|---|
| **Model** | `Models/*.cs` + `AcgcetDbContext.cs` | Defines the shape of data (tables, columns, relationships) |
| **ViewModel** | `ViewModels/**/*.cs` | Fetches data from the database, holds it in properties, handles button clicks |
| **View** | `Views/**/*.xaml` | Shows the data on screen, sends user input to the ViewModel |

### The key idea: **Binding**

The View and ViewModel talk to each other through **Data Binding**. When a property in the ViewModel changes, the UI updates automatically — you don't manually write code to update text boxes. The ViewModel doesn't know or care about the UI at all.

```csharp
// ViewModel has a property
[ObservableProperty]
private string _studentName = "";

// View (XAML) binds to it
// <TextBlock Text="{Binding StudentName}" />

// When the ViewModel sets StudentName = "Kavitha", the UI updates instantly
```

---

## 5. What is Entity Framework Core?

**Entity Framework Core (EF Core)** is a library that lets you talk to a database **using C# objects and LINQ queries** instead of writing raw SQL.

### Without EF Core (raw SQL):
```sql
SELECT s.FullName, s.RegistrationNumber 
FROM Students s 
WHERE s.BatchId = 5 
ORDER BY s.FullName
```

### With EF Core (C# LINQ):
```csharp
var students = await _dbContext.Students
    .Where(s => s.BatchId == 5)
    .OrderBy(s => s.FullName)
    .ToListAsync();
```

Same result, but written in C# code that is **type-safe** (the compiler will catch typing mistakes), **readable**, and **easier to compose** with conditions.

### Key EF Core concepts:

| Concept | What it means |
|---|---|
| `DbContext` | The main "gateway" class that connects to the database |
| `DbSet<T>` | Represents a table — e.g., `_dbContext.Students` is the `Students` table |
| `.Where(...)` | Filters rows (like SQL `WHERE`) |
| `.OrderBy(...)` | Sorts results (like SQL `ORDER BY`) |
| `.Include(...)` | Loads related data (like SQL `JOIN`) |
| `.ToList()` / `.ToListAsync()` | Executes the query and returns results |
| `.FirstOrDefault()` | Gets the first matching row or `null` |
| `.SaveChanges()` | Writes all pending changes to the database |
| `.Add(entity)` | Inserts a new row |
| `.Remove(entity)` | Marks a row for deletion |
| `.RemoveRange(list)` | Marks multiple rows for deletion |

### Example — loading related data with `Include`:
```csharp
// Get all internal marks for a student, AND include the Paper's name
var marks = await _dbContext.InternalMarks
    .Include(m => m.Paper)       // JOIN with Papers table
    .Include(m => m.TestType)    // JOIN with TestTypes table
    .Where(m => m.StudentId == studentId)
    .ToListAsync();

// Now you can access: marks[0].Paper.PaperName, marks[0].TestType.TestName
```

---

## 6. The Database — Tables We Use

The application connects to a SQL Server database named **`ACGCET_MASTER`**. Here is a complete reference to all 50 tables, grouped by their purpose:

### 🔐 Authentication & Security

| Table | Purpose |
|---|---|
| `AdminUsers` | Stores admin user accounts (username, hashed password, department, designation) |
| `AdminUserRoles` | Links admin users to roles (many-to-many: one user can have many roles) |
| `Roles` | Defines roles like "SuperAdmin", "COE", "Faculty" |
| `Permissions` | Individual permission items (e.g., "CAN_DELETE_MARKS") |
| `RolePermissions` | Links roles to permissions |
| `UserSessionLog` | Tracks login/logout times for each user |
| `VwActiveUserSessions` | A database **View** (not a table) that shows currently logged-in users |

### 🏫 Academic Structure

| Table | Purpose |
|---|---|
| `Degrees` | B.E., M.E., MCA — with graduation level (UG/PG) |
| `Programs` | Department programs — CSE, ECE, EEE, MECH, CIVIL (linked to Degree) |
| `Courses` | A specific course (e.g., "B.E. CSE 2021 Regulation") — links Program + Degree + Regulation |
| `Regulations` | Academic regulations (e.g., Regulation 2017, Regulation 2021) |
| `Batches` | Academic year batches — e.g., "2021-2025" for CSE |
| `Sections` | Sections within a batch — e.g., Section A, Section B |
| `Schemes` | Mark scheme/pattern (e.g., "Regular Scheme 2021") |

### 📚 Subjects / Papers

| Table | Purpose |
|---|---|
| `Papers` | Individual subjects/papers — PaperCode, PaperName, Semester, Credits, PaperType |
| `PaperTypes` | Type of paper — Theory (TH), Practical (PR), Project (PROJ) |
| `PaperMarkDistribution` | Max and min marks for theory/lab, internal/external for each paper |
| `PaperFees` | Fee charged for each paper in an exam (linked to ExamType) |
| `PassingCriteria` | Minimum passing marks/credits required per course per batch |

### 👩‍🎓 Students

| Table | Purpose |
|---|---|
| `Students` | Core student record — Name, Reg. No., Roll No., Admission No., DOB, Gender, Mobile, Course, Batch, Section |
| `StudentAdditionalInfo` | Extra details like parent name, blood group, quota type |
| `Communities` | OC, BC, MBC, SC, ST — student community / caste category |
| `QuotaTypes` | Government Quota, Management Quota |

### 📝 Examinations & Applications

| Table | Purpose |
|---|---|
| `ExamTypes` | Regular or Arrear exam |
| `Examinations` | A specific exam event — ExamCode (e.g., "NOV-DEC-2024"), ExamMonth, ExamYear, Result locked flag |
| `ExamSessions` | Forenoon (FN) or Afternoon (AN) — with start/end times |
| `ExamApplications` | A student's application to write an exam — links Student + Examination, tracks approval status, payment |
| `ExamApplicationPapers` | Individual papers a student is applying for within one exam application |
| `ExamSchedules` | What paper is scheduled on what date in which session |
| `Blocks` | Exam hall buildings (e.g., Main Block) |
| `Rooms` | Individual exam rooms with capacity (rows × columns) |
| `SeatAllocations` | Which student sits in which room, row, column |

### 📊 Marks & Results

| Table | Purpose |
|---|---|
| `InternalMarks` | Internal test marks — per Student, Paper, TestType (IA1, IA2, IA3, Model), and Semester |
| `ExternalMarks` | University exam marks — per Student, Paper, Examination — has TheoryMark, LabMark, TotalMark |
| `ExamResults` | Final processed result — InternalTotal, ExternalTotal, GrandTotal, Grade, ResultStatus per Student + Paper + Examination |
| `ResultStatuses` | Pass (P), Fail (F), Absent (AB), Withheld (WH) |
| `TestTypes` | Types of internal tests — Internal Assessment 1/2/3, Model Exam |
| `RevaluationRequests` | Student requests to re-evaluate their exam paper |
| `RevaluationStatuses` | Status of revaluation requests |

### 🔒 Module Locking & Audit

| Table | Purpose |
|---|---|
| `Modules` | App modules like "InternalMarkEntry", "ExternalMarkEntry", "ExamApply" |
| `ModuleLocks` | Lock status for each module per examination (or globally with ExaminationId = NULL) |
| `LockOverrideRequests` | A department user can request to unlock a module temporarily |
| `DeadlineConfiguration` | Deadline date/time for each module per examination, with optional extension |
| `AuditLog` | Every data change is logged here — who did what, when, on which table/record |
| `MalpracticeDetectionLogs` | Suspicious activity logs — multiple identical marks, etc. |
| `SystemAlerts` | System alerts shown on the dashboard |
| `SystemConfigurations` | Key-value settings for the application |

### 📋 Data Correction

| Table | Purpose |
|---|---|
| `DataCorrectionRequests` | Requests to correct data after locking — needs COE approval |
| `CorrectionRequestTypes` | Types of corrections (e.g., "Mark Correction", "Name Change") |

---

## 7. How the App Starts — App.xaml.cs & Dependency Injection

### What is Dependency Injection?

**Dependency Injection (DI)** is a pattern where instead of creating objects yourself (with `new SomeClass()`), you register them with a central "service container" and ask the container to provide them when needed. This makes code easier to test and modify.

Think of it like a **restaurant kitchen**: instead of every waiter going to the market to buy ingredients, the kitchen has a pantry (the DI container) that supplies everything.

### How ACGCET Admin V2 starts up:

```
App launches
     │
     ▼
App() constructor runs
     │
     ├── Reads appsettings.json (gets the database connection string)
     │
     ├── Registers services into the DI container:
     │   ├── AcgcetDbContext  → connected to SQL Server
     │   ├── MainViewModel    → the root ViewModel
     │   ├── LoginViewModel
     │   ├── DashboardViewModel
     │   ├── DataSeeder (service)
     │   ├── MainWindow (with DataContext = MainViewModel)
     │   └── LoginView, DashboardView
     │
     ▼
OnStartup() runs
     │
     ├── Runs DataSeeder.SeedAsync()
     │   (Seeds lookup tables like Communities, Degrees, etc. if they are empty)
     │
     ├── Gets MainWindow from DI container
     │
     └── Shows MainWindow → User sees the Login screen
```

### The connection string (appsettings.json):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=localhost\\SQLEXPRESS01;Initial Catalog=ACGCET_MASTER;Integrated Security=True;Trust Server Certificate=True"
  }
}
```

This tells EF Core:
- **Server**: `localhost\SQLEXPRESS01` — your local SQL Server Express instance
- **Database**: `ACGCET_MASTER`
- **Auth**: Windows Authentication (no username/password needed — uses your Windows login)

---

## 8. The Database Context — AcgcetDbContext.cs

`AcgcetDbContext` is the **heart of the database layer**. It inherits from EF Core's `DbContext` class.

```csharp
public partial class AcgcetDbContext : DbContext
{
    // Each of these is a "table handle" — use it to query/insert/delete
    public virtual DbSet<Student> Students { get; set; }
    public virtual DbSet<InternalMark> InternalMarks { get; set; }
    public virtual DbSet<ExternalMark> ExternalMarks { get; set; }
    // ... 47 more tables
}
```

The `OnModelCreating()` method configures relationships (foreign keys), unique constraints, default values, and column types — this maps exactly to how the SQL table was created.

### Example relationship definition:

```csharp
// InternalMark belongs to a Student (FK: StudentId)
entity.HasOne(d => d.Student)
    .WithMany(p => p.InternalMarks)        // Student has many InternalMarks
    .HasForeignKey(d => d.StudentId);       // The FK column is StudentId
```

This tells EF Core: "When you load an `InternalMark`, you can navigate to its `Student` using `.Student`, and from a `Student`, you can get all marks using `.InternalMarks`."

---

## 9. CommunityToolkit.Mvvm — Understanding the Attributes

This is one of the most important libraries in the project. It generates boilerplate code for you using **source generators** (code that is generated at compile time).

### `[ObservableProperty]`

**Problem it solves**: In standard MVVM, for the UI to update when a property changes, you need to write this manually:

```csharp
// WITHOUT CommunityToolkit — lots of boilerplate
private string _username;
public string Username
{
    get => _username;
    set
    {
        if (_username != value)
        {
            _username = value;
            OnPropertyChanged(nameof(Username)); // Notify the UI
        }
    }
}
```

**With `[ObservableProperty]`, this entire block becomes one line:**

```csharp
// WITH CommunityToolkit — clean and simple
[ObservableProperty]
private string _username = "";
```

The toolkit **automatically generates** the public `Username` property with the change notification behind the scenes. The naming convention is: `_camelCase` field → `PascalCase` public property.

| Field Name | Generated Property |
|---|---|
| `_username` | `Username` |
| `_studentName` | `StudentName` |
| `_isLoading` | `IsLoading` |
| `_selectedBatch` | `SelectedBatch` |

### `[NotifyPropertyChangedFor(...)]`

Sometimes when one property changes, another property that **depends** on it also needs to notify the UI:

```csharp
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(IsNotLoading))]  // When IsLoading changes, also notify IsNotLoading
private bool _isLoading;

public bool IsNotLoading => !IsLoading;  // Computed property
```

When `IsLoading` is set to `true`, the UI automatically knows that `IsNotLoading` is now `false` and updates buttons that are bound to `IsNotLoading`.

### `[RelayCommand]`

**Problem it solves**: In WPF, buttons are bound to **commands** (objects implementing `ICommand`), not methods directly. Normally you'd have to implement `ICommand` yourself — lots of code.

**With `[RelayCommand]`, just write a method:**

```csharp
[RelayCommand]
private async Task Login()
{
    // This becomes a "LoginCommand" that can be bound to a Button in XAML
}

[RelayCommand]
private void Clear()
{
    // This becomes "ClearCommand"
}
```

The naming convention is: `MethodName()` → `MethodNameCommand`.

In XAML (View), you bind the button like this:
```xml
<Button Command="{Binding LoginCommand}" Content="Login" />
```

### `partial void OnXxxChanged(T value)` — Property Change Hooks

When you use `[ObservableProperty]`, you can also intercept property changes by writing a special partial method:

```csharp
// This runs automatically AFTER SelectedSemester is changed
partial void OnSelectedSemesterChanged(string value)
{
    // Reload papers for the selected semester
    var papers = _dbContext.Papers.Where(p => p.Semester == int.Parse(value)).ToList();
    Papers.Clear();
    foreach (var p in papers) Papers.Add(p);
}
```

This is how **cascading dropdowns** work in this app: selecting a Level → loads Programs → selecting a Program → loads Batches → selecting a Batch → loads Sections.

---

## 10. Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                         USER INTERACTION                        │
│  User clicks a button / selects a dropdown / types in a box    │
└─────────────────────────┬───────────────────────────────────────┘
                          │ WPF Data Binding (Command/Property)
                          ▼
┌─────────────────────────────────────────────────────────────────┐
│                        VIEWMODEL (C#)                           │
│  [RelayCommand] method fires                                    │
│  Properties update via [ObservableProperty]                     │
│  Business logic runs (validation, error checks)                 │
└─────────────────────────┬───────────────────────────────────────┘
                          │ EF Core LINQ Query
                          ▼
┌─────────────────────────────────────────────────────────────────┐
│                   AcgcetDbContext (EF Core)                     │
│  Translates LINQ into SQL                                       │
│  Sends SQL to the database                                      │
└─────────────────────────┬───────────────────────────────────────┘
                          │ SQL
                          ▼
┌─────────────────────────────────────────────────────────────────┐
│              SQL Server — ACGCET_MASTER database                │
│  Returns rows → EF Core converts to C# objects (Model classes) │
└─────────────────────────┬───────────────────────────────────────┘
                          │ Returns List<T> or single object
                          ▼
┌─────────────────────────────────────────────────────────────────┐
│                        VIEWMODEL (C#)                           │
│  Stores result in ObservableCollection or property              │
│  Raises PropertyChanged notification automatically              │
└─────────────────────────┬───────────────────────────────────────┘
                          │ WPF Data Binding (automatic update)
                          ▼
┌─────────────────────────────────────────────────────────────────┐
│                       VIEW (XAML)                               │
│  DataGrid / ListBox / TextBlock refreshes with new data         │
│  User sees the updated result instantly                         │
└─────────────────────────────────────────────────────────────────┘
```

---

## 11. ViewModel Deep-Dives

### LoginViewModel

**File**: `ViewModels/Dashboard/LoginViewModel.cs`
**Tables used**: `AdminUsers`

#### What it does:
Handles the login screen logic — validates username and password, uses BCrypt to securely compare the entered password with the stored hash.

#### Key properties:
| Property | Purpose |
|---|---|
| `Username` | Text bound to the username TextBox |
| `Password` | Text bound to the password field |
| `ErrorMessage` | Error text shown when login fails |
| `IsErrorVisible` | Controls whether the error panel is shown |
| `IsLoading` | Shows a loading spinner while authenticating |

#### Database query logic:
```csharp
// 1. Find user by username (exact match)
var user = await _dbContext.AdminUsers
    .FirstOrDefaultAsync(u => u.UserName == Username);

// 2. If not found → show error
// 3. If locked → show lock message
// 4. Verify password using BCrypt (runs on background thread to not freeze UI)
bool isPasswordValid = await Task.Run(() => BCrypt.Net.BCrypt.Verify(Password, user.PasswordHash));

// 5. If valid → send LoginSuccessMessage via WeakReferenceMessenger
// 6. If invalid → show "Invalid username or password"
```

#### After successful login:
A `LoginSuccessMessage` is broadcast via `WeakReferenceMessenger`. The `MainViewModel` listens for this and navigates to the Dashboard.

---

### DashboardViewModel & HomeViewModel

**File**: `ViewModels/Dashboard/DashboardViewModel.cs`
**Tables used**: `AuditLogs`, `SystemAlerts`

#### DashboardViewModel:
Acts as the **navigation hub** for the entire application. It holds a `CurrentView` property — whatever ViewModel is set here, its corresponding View is shown in the main content area.

```csharp
[RelayCommand]
private void Navigate(string destination)
{
    switch (destination)
    {
        case "Home":        CurrentView = new HomeViewModel(_dbContext); break;
        case "Application": CurrentView = new ApplicationViewModel(_dbContext); break;
        case "AdminControl":CurrentView = new AdminControlViewModel(_dbContext); break;
        case "MissingEntry":CurrentView = new MissingEntryViewModel(_dbContext); break;
        case "DeleteEntry": CurrentView = new DeleteEntryViewModel(_dbContext); break;
        case "EntryReport": CurrentView = new EntryReportViewModel(...); break;
    }
}
```

The sidebar menu buttons in the XAML each call `NavigateCommand` with different parameters.

#### HomeViewModel (Home Dashboard screen):
```csharp
// Fetch last 50 audit log entries
var logs = _dbContext.AuditLogs
    .OrderByDescending(l => l.ActionDate)
    .Take(50)
    .ToList();

// Fetch last 20 system alerts
var alerts = _dbContext.SystemAlerts
    .OrderByDescending(a => a.AlertDateTime)
    .Take(20)
    .ToList();
```

Displays a quick summary of recent activity and any system alerts on the dashboard home screen.

---

### AdminControlViewModel

**File**: `ViewModels/AdminControl/AdminControlViewModel.cs`
**Tables used**: None directly — it is a navigation hub for the Admin section.

The Admin Control section contains sub-screens. `AdminControlViewModel` manages navigation between:
- **NewUserCreation** — Create new admin users
- **ExtMarkEntryBarcode** — Barcode-based external mark entry
- **StudentWiseBarcodeView** — Student-specific barcode lookup
- **DataInputLock** — Toggle module locks

---

### NewUserCreationViewModel

**File**: `ViewModels/AdminControl/NewUserCreationViewModel.cs`
**Tables used**: `Roles`, `AdminUsers`, `AdminUserRoles`

#### What it does:
Allows an admin to create a new admin user account with a bcrypt-hashed password and assign roles.

#### Database query logic:

```csharp
// On load — fetch all roles to show as checkboxes
private void LoadPermissions()
{
    var roles = _dbContext.Roles.ToList();
    // Shown as checkboxes in the UI
}

// On "Create User" click:
// 1. Validate username not empty
// 2. Check passwords match
// 3. Check username not already taken:
if (_dbContext.AdminUsers.Any(u => u.UserName == Username)) { /* error */ }

// 4. Create and save the user:
var newUser = new AdminUser {
    UserName = Username,
    PasswordHash = PasswordHelper.HashPassword(Password), // BCrypt hash
    Department = Department,
    IsActive = true,
    CreatedDate = DateTime.Now
};
_dbContext.AdminUsers.Add(newUser);
_dbContext.SaveChanges(); // INSERT into AdminUsers table

// 5. Save each selected role:
_dbContext.AdminUserRoles.Add(new AdminUserRole {
    AdminUserId = newUser.AdminUserId,
    RoleId = perm.RoleId
});
_dbContext.SaveChanges(); // INSERT into AdminUserRoles table
```

---

### DataInputLockViewModel

**File**: `ViewModels/AdminControl/DataInputLockViewModel.cs`
**Tables used**: `Modules`, `ModuleLocks`

#### What it does:
Allows COE to **lock or unlock** data entry modules globally. When a module is locked, staff cannot enter/edit data in that section.

#### Database query logic:

```csharp
// LEFT JOIN: Get all modules, even if there's no lock record yet
var query = from m in _dbContext.Modules
            join l in _dbContext.ModuleLocks.Where(x => x.ExaminationId == null)
            on m.ModuleId equals l.ModuleId into locks
            from ml in locks.DefaultIfEmpty()   // LEFT JOIN — unlocked modules return null
            select new { Module = m, Lock = ml };
```

> **Why `ExaminationId == null`?** A "global lock" applies to all exams. Exam-specific locks have an `ExaminationId` set. The global lock row has `ExaminationId = NULL`.

#### Saving locks:

```csharp
// For each module toggle in the UI:
var existingLock = _dbContext.ModuleLocks
    .FirstOrDefault(l => l.ModuleId == item.ModuleId && l.ExaminationId == null);

if (existingLock != null)
{
    existingLock.IsLocked = item.IsLocked; // UPDATE existing row
}
else if (item.IsLocked)
{
    _dbContext.ModuleLocks.Add(new ModuleLock { ... }); // INSERT new row only if locking
}
_dbContext.SaveChanges();
```

---

### DeleteEntry Family

**Folder**: `ViewModels/DeleteEntry/`
**Tables used**: `ExamApplications`, `InternalMarks`, `ExternalMarks`, `ExamResults`, `Students`, `ExamSessions`, `Examinations`

The `DeleteEntryViewModel` is a navigation hub for all delete sub-screens. Each sub-screen follows the same pattern:

**Pattern**:
1. User types a **Register Number**
2. User optionally selects an **Exam Session**
3. Clicks **Search** → Fetch records from the database
4. User checks checkboxes on items they want to delete
5. Clicks **Delete** → Confirmation dialog → `RemoveRange()` + `SaveChanges()`

#### DeleteInternalMarkViewModel — Database query:

```csharp
// Search: find student by registration number
var student = await _dbContext.Students
    .FirstOrDefaultAsync(s => s.RegistrationNumber == RegNo);

// Load their marks with Paper and TestType details
var marks = await _dbContext.InternalMarks
    .Include(m => m.Paper)       // JOIN with Papers table
    .Include(m => m.TestType)    // JOIN with TestTypes table
    .Where(m => m.StudentId == student.StudentId)
    .ToListAsync();

// Delete selected:
var ids = selected.Select(m => m.InternalMarkId).ToList();
var toDelete = await _dbContext.InternalMarks
    .Where(m => ids.Contains(m.InternalMarkId)).ToListAsync();
_dbContext.InternalMarks.RemoveRange(toDelete);
await _dbContext.SaveChangesAsync();
```

#### DeleteExternalMarkViewModel — Additionally filters by Exam:

```csharp
// Get the Examination record matching the selected session name
var exam = await _dbContext.Examinations
    .FirstOrDefaultAsync(e => e.ExamMonth == SelectedSession.SessionName);

// Then fetch external marks filtered by student AND examination
var marks = await _dbContext.ExternalMarks
    .Include(m => m.Paper)
    .Where(m => m.StudentId == studentId && m.ExaminationId == exam.ExaminationId)
    .ToListAsync();
```

---

### MissingEntry Family

**Folder**: `ViewModels/MissingEntry/`
**Tables used**: `ExamApplicationPapers`, `ExamApplications`, `Students`, `InternalMarks`, `ExternalMarks`, `ExamResults`, `Papers`, `ExamSessions`

#### What it does:
These screens help the COE find **students who have applied for an exam but whose marks have not been entered yet**. This is critical before result processing.

#### MissingInternalEntryViewModel — Core logic:

```csharp
// 1. Get all exam application papers (students who applied for each paper)
var apps = await _dbContext.ExamApplicationPapers
    .Include(eap => eap.ExamApplication)
        .ThenInclude(ea => ea.Student)   // Deep include: ExamApp → Student
    .Include(eap => eap.Paper)
    .Where(x => x.PaperId == SelectedPaper.PaperId)  // Filter by selected paper
    .ToListAsync();

// 2. Get all internal marks for these papers (to check what's already entered)
var internalMarks = await _dbContext.InternalMarks
    .Where(im => apps.Select(a => a.PaperId).Contains(im.PaperId))
    .ToListAsync();

// 3. Cross-reference: applied but no mark entered = "Missing"
foreach (var app in apps)
{
    bool exists = internalMarks.Any(im =>
        im.StudentId == app.ExamApplication.StudentId &&
        im.PaperId == app.PaperId);

    if (!exists) // Mark is MISSING
    {
        ReportData.Add(new MissingEntryItem {
            RegNo = app.ExamApplication.Student.RegistrationNumber,
            StudentName = app.ExamApplication.Student.FullName,
            PaperCode = app.Paper.PaperCode,
            Status = "Missing Internal"
        });
    }
}
```

---

### EntryReport Family

**Folder**: `ViewModels/EntryReport/`
**Tables used**: `Students`, `Programs`, `Batches`, `Sections`, `Degrees`, `InternalMarks`, `ExternalMarks`, `ExamApplications`, `ExamResults`

#### BaseEntryReportViewModel (Abstract Base Class):

All entry report ViewModels share a common set of filters: **Course Level → Program → Batch → Section**. Instead of writing this code 5 times, a **base class** was created:

```csharp
public abstract partial class BaseEntryReportViewModel : ObservableObject
{
    // Cascading dropdown logic shared by ALL entry reports:
    async partial void OnSelectedCourseLevelChanged(string value)
    {
        // Load programs matching the selected level (UG/PG)
        var programs = await _dbContext.Programs
            .Where(p => p.Degree.GraduationLevel == value).ToListAsync();
    }

    async partial void OnSelectedProgramChanged(Program? value)
    {
        // Load batches for the selected program
        var batches = await _dbContext.Batches
            .Where(b => b.Course.ProgramId == value.ProgramId).ToListAsync();
    }

    async partial void OnSelectedBatchChanged(Batch? value)
    {
        // Load sections for the selected batch
        var sections = await _dbContext.Sections
            .Where(s => s.BatchId == value.BatchId).ToListAsync();
    }

    // Each subclass must implement View() and ClearData()
    public abstract Task View();
    protected abstract void ClearData();
}
```

#### StudentEntryReportViewModel — Database query:

```csharp
// Fetch students filtered by batch and optionally section/program
var query = _dbContext.Students.Include(s => s.Regulation).AsQueryable();
if (SelectedBatch != null)   query = query.Where(s => s.BatchId == SelectedBatch.BatchId);
if (SelectedSection != null) query = query.Where(s => s.SectionId == SelectedSection.SectionId);
var students = await query.ToListAsync();
```

---

### Application / ClassWiseStudentsViewModel

**File**: `ViewModels/Application/ClassWiseStudentsViewModel.cs`
**Tables used**: `Students`, `Batches`, `Programs`, `Sections`, `Regulation`

#### What it does:
Lists students class-by-class with powerful filter options and **pagination**.

#### Key features:
- **Cascading filters**: Level → Program → Batch → Section
- **Sort options**: By Reg. No, Roll No, or Gender
- **Show missing Reg. No**: Find students whose registration number hasn't been entered yet
- **Pagination**: Results are split into pages (10/20/50/100 per page)
- **Print/Preview**: Generate printable reports

#### Database query logic:

```csharp
var query = _dbContext.Students.Include(s => s.Regulation).AsQueryable();

// Apply filters dynamically
if (SelectedProgram != "Select")
    query = query.Where(s => s.Batch.Course.Program.ProgramName == SelectedProgram);
if (SelectedBatch != "Select")
    query = query.Where(s => s.Batch.BatchName == SelectedBatch);
if (SelectedSection != "Select" && SelectedSection != "Overall")
    query = query.Where(s => s.Section.SectionName == SelectedSection);
if (ShowMissingRegNo)
    query = query.Where(s => string.IsNullOrEmpty(s.RegistrationNumber));

// Sort
if (OrderByRegNo)  query = query.OrderBy(s => s.RegistrationNumber);
if (OrderByRollNo) query = query.OrderBy(s => s.RollNumber);
if (OrderByGender) query = query.OrderBy(s => s.Gender);

var results = await query.ToListAsync();
```

#### Pagination:

```csharp
private void ApplyPagination()
{
    TotalRecords = _fullStudentList.Count;
    TotalPages = (int)Math.Ceiling((double)TotalRecords / SelectedPageSize);

    // Get only the "current page" items using Skip and Take
    var pagedItems = _fullStudentList
        .Skip((CurrentPage - 1) * SelectedPageSize)  // Ignore previous pages
        .Take(SelectedPageSize)                        // Take only current page items
        .ToList();

    StudentsList = new ObservableCollection<Student>(pagedItems);
}
```

---

## 12. Services

### DataSeeder

**File**: `Services/DataSeeder.cs`

#### What it does:
On every app startup, `DataSeeder.SeedAsync()` is called. It **populates the database with initial data** if the tables are empty. This is a one-time setup that ensures the app has the required lookup data to function.

#### Seeding order (order matters because of foreign keys):
```
1. Communities (OC, BC, MBC, SC, ST)
2. QuotaTypes (Government, Management)
3. Degrees (B.E., M.E., MCA) — UG / PG
4. Regulations (2017, 2021)
5. ExamTypes (Regular, Arrear)
6. PaperTypes (Theory, Practical, Project)
7. TestTypes (IA1, IA2, IA3, Model)
8. ResultStatuses (Pass, Fail, Absent, Withheld)
9. Programs (CSE, ECE, EEE, MECH, CIVIL) — depends on Degrees
10. Courses (B.E. CSE 2021, etc.) — depends on Programs, Degrees, Regulations
11. Batches (2021-2025, 2022-2026, etc.) — depends on Courses
12. Sections (A, B) — depends on Batches
13. Schemes (Regular Scheme 2021)
14. Papers (HS3151, MA3151, etc.) — depends on Courses, Schemes, PaperTypes
15. ExamSessions (Forenoon, Afternoon)
16. Blocks & Rooms (Main Block, Room 101, 102)
17. Examinations (NOV-DEC-2024) — depends on ExamTypes
```

Each seed method checks `if (!await _context.XxxTable.AnyAsync())` — so it only seeds **if the table is currently empty**. It will never insert duplicates.

---

### PrintService

**File**: `Services/PrintService.cs`

#### What it does:
Generates formatted **printable documents** using WPF's `FlowDocument` and `FixedDocument` APIs. Called from ViewModels like `StudentEntryReportViewModel`, `ClassWiseStudentsViewModel`, etc.

Key methods generate documents for:
- Student lists (class-wise)
- Internal mark sheets
- Hall tickets
- Timetables
- A4 result format

The `PrintColumnDefinition` class defines what columns appear in dynamically-generated reports:
```csharp
var columns = new List<PrintColumnDefinition>
{
    new() { Header = "Reg. No", BindingPath = "RegistrationNumber", Width = 110 },
    new() { Header = "Student Name", BindingPath = "FullName", Width = 250 }
};
```

---

### CustomMessageBox

**File**: `Services/CustomMessageBox.xaml` + `CustomMessageBox.xaml.cs`

A styled dialog box that replaces the default Windows `MessageBox.Show()` with a more modern, Material Design-styled popup that matches the app's visual theme.

Used like this:
```csharp
Services.CustomMessageBox.Show("No data to display");
```

---

## 13. Navigation Pattern Explained

The app uses a **Content Switcher pattern** for navigation — instead of opening new windows, it swaps the content area's `DataContext`:

```
MainWindow
└── [Shell Frame — always visible]
    ├── Sidebar / Drawer         ← Navigation buttons
    └── ContentControl           ← Shows the "current view"
        DataContext = {Binding CurrentView}
```

When `DashboardViewModel.CurrentView` changes, WPF's **DataTemplate** system automatically finds the matching XAML view for that ViewModel type and renders it. This is called ViewModel-first navigation.

```xml
<!-- XAML maps ViewModel types to View templates -->
<DataTemplate DataType="{x:Type vm:HomeViewModel}">
    <views:HomeView />
</DataTemplate>
<DataTemplate DataType="{x:Type vm:ClassWiseStudentsViewModel}">
    <views:ClassWiseStudentsView />
</DataTemplate>
```

When `CurrentView = new ClassWiseStudentsViewModel(...)` is set in C#, WPF automatically shows `ClassWiseStudentsView`.

---

## 14. Security — BCrypt Password Hashing

Passwords are **never stored in plain text**. The `BCrypt.Net-Next` library is used.

### How it works:

```csharp
// When creating a user — hash the password before saving
string hash = BCrypt.Net.BCrypt.HashPassword("plainTextPassword");
// Stores something like: "$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy"

// When logging in — verify the entered password against the stored hash
bool isValid = BCrypt.Net.BCrypt.Verify("plainTextPassword", storedHash);
// BCrypt internally handles the salt — you don't need to manage it
```

BCrypt is intentionally **slow to compute** — this makes brute-force attacks impractical. It is the industry standard for password storage.

The BCrypt verification is run on a **background thread** during login to keep the UI responsive:
```csharp
bool isPasswordValid = await Task.Run(() =>
    BCrypt.Net.BCrypt.Verify(Password, user.PasswordHash));
```

---

## 15. Messaging — WeakReferenceMessenger

`WeakReferenceMessenger` (from CommunityToolkit.Mvvm) allows ViewModels to **communicate with each other without direct references**. This is important because a ViewModel shouldn't directly hold a reference to another unrelated ViewModel.

### How it's used in this project:

#### After successful login:
```csharp
// LoginViewModel sends a message:
WeakReferenceMessenger.Default.Send(new LoginSuccessMessage(user));
```

```csharp
// MainViewModel listens and reacts:
WeakReferenceMessenger.Default.Register<LoginSuccessMessage>(this, (r, msg) =>
{
    // Switch to Dashboard view
    CurrentView = new DashboardViewModel(_dbContext);
});
```

#### On logout:
```csharp
// DashboardViewModel sends:
WeakReferenceMessenger.Default.Send(new LogoutMessage());

// MainViewModel listens and switches back to Login:
WeakReferenceMessenger.Default.Register<LogoutMessage>(this, (r, msg) =>
{
    CurrentView = new LoginViewModel(_dbContext);
});
```

The "Weak" part means these subscriptions don't cause **memory leaks** — the messenger holds only a weak reference to the subscriber, so it can be garbage collected when no longer needed.

---

## 16. Common Patterns and Recipes

### Pattern 1: Cascading Dropdowns

```csharp
// When Level changes → reload Programs
partial void OnSelectedLevelChanged(string value)
{
    Task.Run(() => LoadProgramsAsync(value));
}

private async Task LoadProgramsAsync(string level)
{
    var programs = await _dbContext.Programs
        .Where(p => p.Degree.GraduationLevel == level)
        .ToListAsync();

    // Must update UI collections on the UI thread
    System.Windows.Application.Current.Dispatcher.Invoke(() =>
    {
        Programs = new ObservableCollection<string>(programs.Select(p => p.ProgramName));
    });
}
```

> **Why `Dispatcher.Invoke`?** Database queries run on a background thread for performance. But UI collections can only be modified on the UI thread. `Dispatcher.Invoke` marshals the code back to the UI thread.

### Pattern 2: Loading State (Spinner)

```csharp
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(IsNotLoading))]
private bool _isLoading;

public bool IsNotLoading => !IsLoading;

[RelayCommand]
private async Task Search()
{
    if (IsLoading) return;   // Prevent double-click
    IsLoading = true;        // Show spinner, disable button
    try
    {
        // ... do database work ...
    }
    finally
    {
        IsLoading = false;   // Always hide spinner, even on error
    }
}
```

### Pattern 3: Select + Delete with Confirmation

```csharp
[RelayCommand]
private async Task Delete()
{
    // 1. Get checked items
    var selected = MarkList.Where(m => m.IsSelected).ToList();

    // 2. Guard: nothing selected
    if (!selected.Any()) { MessageBox.Show("Select marks to delete"); return; }

    // 3. Confirmation dialog
    if (MessageBox.Show($"Delete {selected.Count} marks?", "Confirm",
        MessageBoxButton.YesNo) == MessageBoxResult.Yes)
    {
        // 4. Collect IDs, fetch from DB, delete
        var ids = selected.Select(m => m.InternalMarkId).ToList();
        var toDelete = await _dbContext.InternalMarks
            .Where(m => ids.Contains(m.InternalMarkId)).ToListAsync();
        _dbContext.InternalMarks.RemoveRange(toDelete);
        await _dbContext.SaveChangesAsync();

        // 5. Reload the list
        await Search();
        MessageBox.Show("Deleted successfully.");
    }
}
```

### Pattern 4: Adding a New ViewModel & View (Step by Step)

1. Create `ViewModels/YourSection/YourNewViewModel.cs` extending `ObservableObject`
2. Add constructor accepting `AcgcetDbContext dbContext`
3. Add properties with `[ObservableProperty]`
4. Add commands with `[RelayCommand]`
5. Create `Views/YourSection/YourNewView.xaml` with `DataContext` bound to properties
6. Register a `DataTemplate` in App.xaml or ResourceDictionary mapping ViewModel → View
7. In the parent ViewModel's `Navigate()` switch statement, add a new `case`:
   ```csharp
   case "YourNew":
       CurrentView = new YourNewViewModel(_dbContext);
       break;
   ```
8. Add a sidebar button in the parent View calling `NavigateCommand` with `"YourNew"`

---

## Quick Reference Card

| Concept | What to look at |
|---|---|
| App startup & DI | `App.xaml.cs` |
| Database connection | `appsettings.json` |
| All database tables | `Models/AcgcetDbContext.cs` — the `DbSet<>` declarations |
| Table definitions | `Models/*.cs` — one file per table |
| Login logic | `ViewModels/Dashboard/LoginViewModel.cs` |
| Main navigation | `ViewModels/Dashboard/DashboardViewModel.cs` |
| Data entry lock | `ViewModels/AdminControl/DataInputLockViewModel.cs` |
| Add/remove marks | `ViewModels/DeleteEntry/` folder |
| Missing marks report | `ViewModels/MissingEntry/` folder |
| Student list + print | `ViewModels/Application/ClassWiseStudentsViewModel.cs` |
| Print documents | `Services/PrintService.cs` |
| Initial data setup | `Services/DataSeeder.cs` |
| Password hashing | `Helpers/PasswordHelper.cs` + BCrypt |

---

*This guide was written to help new developers understand the ACGCET_Admin_V2 project. If you have questions about a specific screen or feature not covered here, trace it from the View (.xaml) → find the ViewModel via `DataContext` → follow the `[RelayCommand]` methods and `[ObservableProperty]` data to understand the flow.*
