using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ACGCET_Admin.Models;
using ACGCET_Admin.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;

namespace ACGCET_Admin.ViewModels.AdminControl
{
    public partial class NewUserCreationViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _dbContext = null!;

        [ObservableProperty]
        private string _username = "";

        // PasswordBox binding is tricky in MVVM, will use specific logic or just bind to string for now (not secure but legacy style)
        // Better: Bind PasswordBox in View to this property via helper or simple binding
        [ObservableProperty]
        private string _password = "";

        [ObservableProperty]
        private string _confirmPassword = "";

        [ObservableProperty]
        private string _department = "";

        [ObservableProperty]
        private string _designation = "";

        [ObservableProperty]
        private string _fullName = "";

        [ObservableProperty]
        private ObservableCollection<PermissionItem> _permissions = new();

        public ObservableCollection<string> Departments { get; } = new ObservableCollection<string> 
        { 
            "Civil Engineering", "Mechanical Engineering", "EEE", "ECE", "CSE", "COE Office", "Principal Office" 
        };

        public NewUserCreationViewModel(AcgcetDbContext dbContext)
        {
            _dbContext = dbContext;
            if (_dbContext != null)
            {
                LoadPermissions();
            }
        }

        public NewUserCreationViewModel() { _dbContext = null!; }

        private void LoadPermissions()
        {
            // Load Roles as permissions
            var roles = _dbContext.Roles.ToList();
            Permissions.Clear();
            foreach (var role in roles)
            {
                Permissions.Add(new PermissionItem { RoleId = role.RoleId, RoleName = role.RoleName, IsSelected = false });
            }
        }

        [RelayCommand]
        private void CreateUser()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Username and Password are required.");
                return;
            }

            if (Password != ConfirmPassword)
            {
                MessageBox.Show("Passwords do not match.");
                return;
            }

            // Check if user exists
            if (_dbContext.AdminUsers.Any(u => u.UserName == Username))
            {
                MessageBox.Show("Username already exists.");
                return;
            }

            var newUser = new AdminUser
            {
                UserName = Username,
                PasswordHash = PasswordHelper.HashPassword(Password),
                Department = Department,
                Designation = Designation,
                FullName = FullName,
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            _dbContext.AdminUsers.Add(newUser);
            _dbContext.SaveChanges();

            // Add Roles
            foreach (var perm in Permissions)
            {
                if (perm.IsSelected)
                {
                    _dbContext.AdminUserRoles.Add(new AdminUserRole
                    {
                        AdminUserId = newUser.AdminUserId,
                        RoleId = perm.RoleId,
                        AssignedBy = "Admin", // TODO: Get current user
                        AssignedDate = DateTime.Now
                    });
                }
            }
            _dbContext.SaveChanges();

            MessageBox.Show("User Created Successfully!");
            Clear();
        }

        [RelayCommand]
        private void Clear()
        {
            Username = "";
            Password = "";
            ConfirmPassword = "";
            Department = "";
            Designation = "";
            FullName = "";
            foreach (var p in Permissions) p.IsSelected = false;
        }

        [RelayCommand]
        private void Refresh()
        {
            if (_dbContext != null)
                LoadPermissions();
        }
    }

    public partial class PermissionItem : ObservableObject
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = "";
        [ObservableProperty]
        private bool _isSelected;
    }
}
