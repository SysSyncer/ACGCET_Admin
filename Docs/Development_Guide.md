# ACGCET Admin V2 - Development Guide

## 1. Understanding XAML and Code-Behind (.xaml.cs)

When you create a new **View** (like `StudentReportView.xaml`) in WPF, Visual Studio automatically creates a "Code-Behind" file named `StudentReportView.xaml.cs` nested underneath it.

### What is the .xaml.cs file?
This file is partially generated to handle the initialization of your UI components (`InitializeComponent()`).
*   **Legacy WinForms style**: You would write all your button click logic (`Button_Click`) directly in this file.
*   **Modern WPF (MVVM) style**: We leave this file **almost empty**. We only keep the constructor.

### Where does the logic go? (The ViewModel)
Instead of writing code in `.xaml.cs`, we create a **ViewModel** (e.g., `StudentReportViewModel.cs`) in the `ViewModels` folder.
*   **Separation of Concerns**: The View (`.xaml`) handles *how it looks*. The ViewModel handles *how it behaves* and *data*.
*   **Data Binding**: The View binds to properties in the ViewModel (like `Username`, `StudentsList`).
*   **Commands**: Instead of `Click` events, we use `Command="{Binding MyCommand}"` which calls a method in the ViewModel.

### Why use MVVM?
*   **Easier Maintenance**: UI changes don't break logic.
*   **Testability**: You can test ViewModels without running the UI.
*   **Cleaner Code**: Keeps your UI logic organized.

## 2. Project Structure
*   **Views/**: Contains `.xaml` files (The UI).
*   **ViewModels/**: Contains `.cs` files (The Logic/State).
*   **Models/**: Contains Database classes (`Student`, `AdminUser`).

## 3. How to add a new Page?
1.  **Create View**: Right-click `Views` folder -> Add -> New Item -> WPF UserControl -> Name it `MyNewView.xaml`.
2.  **Create ViewModel**: Right-click `ViewModels` folder -> Add -> Class -> Name it `MyNewViewModel.cs`. Inherit from `ObservableObject`.
3.  **Link them**: In `DashboardView.xaml`, add a `DataTemplate` to tell the app: "When you see `MyNewViewModel`, show `MyNewView`."
