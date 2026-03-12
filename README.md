# 🎓 ACGCET Admin Software V2 (WPF Edition)

Welcome to the modern **ACGCET Admin Software**, rebuilt using **C# .NET 8** and **WPF** (Windows Presentation Foundation). This application replaces the legacy VB.NET WinForms system with a robust, scalable, and beautiful interface.

## 🚀 Introduction
This project uses the **MVVM (Model-View-ViewModel)** architectural pattern to ensure clean separation between the User Interface (UI) and the Business Logic.

- **Framework**: .NET 8.0 (Long Term Support)
- **UI Toolkit**: [Material Design In XAML](http://materialdesigninxaml.net/)
- **Database**: SQL Server 2019+
- **ORM**: Entity Framework Core 8.0 (Database-First)
- **Security**: BCrypt Password Hashing

---

## 🛠️ Prerequisites
To develop or run this application, you need:

1.  **Visual Studio 2022** (Community, Professional, or Enterprise).
    *   *Workload required*: ".NET Desktop Development".
2.  **.NET 8.0 SDK** (usually included with VS 2022).
3.  **SQL Server** (Developer or Express edition).
4.  **SQL Server Management Studio (SSMS)** or **Azure Data Studio**.

---

## ⚙️ Getting Started

### 1. Database Setup
Before running the code, ensure the database is ready.
Navigate to the `SQL_Scripts` folder (or the root `Test` folder) and execute these files in order:
1.  `COMPLETE_SCHEMA_OPTIMIZED.sql` (Creates DB & Tables)
2.  `SEED_DATA_OPTIMIZED.sql` (Inserts initial data)
3.  `AUDIT_TRIGGERS_OPTIMIZED.sql` (Sets up audit logging)
4.  `COE_PROCEDURES_OPTIMIZED.sql` (Stored Procedures)

### 2. Configure Connection
Open `appsettings.json` in the project root and update the connection string if needed:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=ACGCET_Admin_V2;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### 3. Open in Visual Studio 2022
1.  Launch **Visual Studio 2022**.
2.  Select **"Open a project or solution"**.
3.  Navigate to `d:\Final Project\Test\ACGCET_Admin_V2`.
4.  Select `ACGCET_Admin_V2.csproj`.
5.  Press **F5** to build and run!

---

## 🏗️ Architecture Explained (MVVM)

Unlike the old "Code-Behind" style (where logic was inside `Button_Click` events), we use **MVVM**:

### 📂 Folder Structure
-   **`Models/`**: The Database Tables. (e.g., `AdminUser.cs`, `Student.cs`). These are generated automatically by Entity Framework.
-   **`ViewModels/`**: The "Brain" of the application.
    *   `LoginViewModel.cs`: Handles login logic, password checking.
    *   `MainViewModel.cs`: Manages navigation between screens.
    *   **Rules**: No UI code here (No `MessageBox`, no `Button`). Only data and commands.
-   **`Views/`**: The UI (XAML).
    *   `LoginView.xaml`: The design of the login screen.
    *   **Rules**: No logic here. Only design.
-   **`App.xaml.cs`**: The entry point. It sets up **Dependency Injection (DI)**.

### 🔄 How it Works
1.  **View** (`LoginView.xaml`) binds its text boxes to...
2.  **ViewModel** (`LoginViewModel.cs`) properties (`Username`, `Password`).
3.  When user clicks "Login", a **Command** in ViewModel runs.
4.  ViewModel asks **DbContext** (EF Core) for data.
5.  ViewModel sends a message to switch the View if successful.

---

## 👩‍💻 Developer Guide

### How to Add a New Page (e.g., "Faculty List")

1.  **Create the View**:
    *   Right-click `Views` folder -> Add -> New Item -> **User Control (WPF)** -> `FacultyView.xaml`.
    *   Design it using Material Design controls.

2.  **Create the ViewModel**:
    *   Right-click `ViewModels` folder -> Add -> Class -> `FacultyViewModel.cs`.
    *   Inherit from `ObservableObject`.

3.  **Register them in `App.xaml.cs`**:
    *   Inside `ConfigureServices`, add:
        ```csharp
        services.AddTransient<FacultyViewModel>();
        services.AddTransient<FacultyView>();
        ```

4.  **Add Navigation Template in `MainWindow.xaml`**:
    *   Inside `<Window.Resources>`, add:
        ```xml
        <DataTemplate DataType="{x:Type viewmodels:FacultyViewModel}">
            <views:FacultyView/>
        </DataTemplate>
        ```

5.  **Navigate to it**:
    *   In `MainViewModel` or via Messenger:
        ```csharp
        CurrentView = new FacultyViewModel();
        ```

### How to Update Database Models
If you change the SQL Schema (e.g., add a column), run this command in terminal to update C# models:
```powershell
dotnet ef dbcontext scaffold "Name=ConnectionStrings:DefaultConnection" Microsoft.EntityFrameworkCore.SqlServer -o Models --context AcgcetDbContext --force
```

---

## 📚 Key Libraries Used
-   **CommunityToolkit.Mvvm**: detailed MVVM helper (RelayCommand, ObservableProperty).
-   **MaterialDesignThemes**: Google's Material Design styling for WPF.
-   **BCrypt.Net-Next**: Secure password hashing.
-   **Microsoft.EntityFrameworkCore**: Database access.

Happy Coding! 🚀
