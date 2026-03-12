using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using BCrypt.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ACGCET_Admin.Models;
using Microsoft.EntityFrameworkCore;

namespace ACGCET_Admin.ViewModels.AdminControl
{
    /// <summary>Display model for the user management grid.</summary>
    public class AdminUserDisplay
    {
        public int    AdminUserId  { get; set; }
        public string UserName     { get; set; } = string.Empty;
        public string FullName     { get; set; } = string.Empty;
        public string Email        { get; set; } = string.Empty;
        public string Department   { get; set; } = string.Empty;
        public string Designation  { get; set; } = string.Empty;
        public string Roles        { get; set; } = string.Empty;
        public bool   IsActive     { get; set; }
        public bool   IsLocked     { get; set; }
        public DateTime? CreatedDate { get; set; }
    }

    public partial class UserManagementViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _db;
        private readonly AdminUser?      _currentUser;

        [ObservableProperty] private ObservableCollection<AdminUserDisplay> _users = new();
        [ObservableProperty] private ObservableCollection<Role>             _availableRoles = new();
        [ObservableProperty] private AdminUserDisplay?                       _selectedUser;

        // ── Add-User Form State ───────────────────────
        [ObservableProperty] private bool _isAddingUser;
        [ObservableProperty] private string _newFullName    = string.Empty;
        [ObservableProperty] private string _newUsername    = string.Empty;
        [ObservableProperty] private string _newEmail       = string.Empty;
        [ObservableProperty] private string _newPassword    = string.Empty;
        [ObservableProperty] private string _newDepartment  = string.Empty;
        [ObservableProperty] private string _newDesignation = string.Empty;
        [ObservableProperty] private Role?  _newRole;

        // ── UI State ──────────────────────────────────
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotLoading))]
        private bool _isLoading;
        public bool IsNotLoading => !IsLoading;

        [ObservableProperty] private string _statusMessage  = string.Empty;
        [ObservableProperty] private bool   _isStatusError;

        public UserManagementViewModel(AcgcetDbContext db, AdminUser? currentUser)
        {
            _db          = db;
            _currentUser = currentUser;
            _ = LoadAsync();
        }

        public UserManagementViewModel() { _db = null!; }

        private async Task LoadAsync()
        {
            IsLoading = true;
            try
            {
                var roles = await _db.Roles.OrderBy(r => r.RoleName).ToListAsync();
                AvailableRoles = new ObservableCollection<Role>(roles);
                NewRole = roles.FirstOrDefault();

                await RefreshUsersAsync();
            }
            catch (Exception ex) { ShowStatus($"Load error: {ex.Message}", true); }
            finally { IsLoading = false; }
        }

        private async Task RefreshUsersAsync()
        {
            var users = await _db.AdminUsers
                .Include(u => u.AdminUserRoles)
                    .ThenInclude(ur => ur.Role)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var display = users.Select(u => new AdminUserDisplay
            {
                AdminUserId  = u.AdminUserId,
                UserName     = u.UserName,
                FullName     = u.FullName    ?? string.Empty,
                Email        = u.Email       ?? string.Empty,
                Department   = u.Department  ?? string.Empty,
                Designation  = u.Designation ?? string.Empty,
                Roles        = string.Join(", ", u.AdminUserRoles
                                                   .Where(r => r.Role != null)
                                                   .Select(r => r.Role!.RoleName)),
                IsActive     = u.IsActive  ?? true,
                IsLocked     = u.IsLocked  ?? false,
                CreatedDate  = u.CreatedDate
            }).ToList();

            Users = new ObservableCollection<AdminUserDisplay>(display);
        }

        // ── Add User ──────────────────────────────────
        [RelayCommand]
        private void ShowAddForm()
        {
            NewFullName = NewUsername = NewEmail = NewPassword =
            NewDepartment = NewDesignation = string.Empty;
            NewRole      = AvailableRoles.FirstOrDefault();
            IsAddingUser = true;
            StatusMessage = string.Empty;
        }

        [RelayCommand]
        private void CancelAdd()
        {
            IsAddingUser  = false;
            StatusMessage = string.Empty;
        }

        [RelayCommand]
        private async Task SaveNewUserAsync()
        {
            if (string.IsNullOrWhiteSpace(NewUsername) || string.IsNullOrWhiteSpace(NewPassword))
            {
                ShowStatus("Username and password are required.", true);
                return;
            }
            if (string.IsNullOrWhiteSpace(NewFullName))
            {
                ShowStatus("Full name is required.", true);
                return;
            }
            if (NewPassword.Length < 6)
            {
                ShowStatus("Password must be at least 6 characters.", true);
                return;
            }

            IsLoading = true;
            try
            {
                bool exists = await _db.AdminUsers
                    .AnyAsync(u => u.UserName == NewUsername.Trim());
                if (exists)
                {
                    ShowStatus($"Username '{NewUsername}' is already taken.", true);
                    return;
                }

                string hash = await Task.Run(() => BCrypt.Net.BCrypt.HashPassword(NewPassword));

                var newUser = new AdminUser
                {
                    UserName     = NewUsername.Trim(),
                    PasswordHash = hash,
                    FullName     = NewFullName.Trim(),
                    Email        = NewEmail.Trim(),
                    Department   = NewDepartment.Trim(),
                    Designation  = NewDesignation.Trim(),
                    IsActive     = true,
                    IsLocked     = false,
                    FailedLoginAttempts = 0,
                    CreatedDate  = DateTime.Now
                };

                _db.AdminUsers.Add(newUser);
                await _db.SaveChangesAsync();

                if (NewRole != null)
                {
                    _db.AdminUserRoles.Add(new AdminUserRole
                    {
                        AdminUserId  = newUser.AdminUserId,
                        RoleId       = NewRole.RoleId,
                        AssignedBy   = _currentUser?.UserName,
                        AssignedDate = DateTime.Now
                    });
                    await _db.SaveChangesAsync();
                }

                IsAddingUser = false;
                ShowStatus($"User '{newUser.UserName}' created successfully.", false);
                await RefreshUsersAsync();
            }
            catch (Exception ex) { ShowStatus($"Error creating user: {ex.Message}", true); }
            finally { IsLoading = false; }
        }

        // ── Remove User ───────────────────────────────
        [RelayCommand]
        private async Task RemoveUserAsync(AdminUserDisplay? user)
        {
            if (user == null) return;

            if (user.AdminUserId == _currentUser?.AdminUserId)
            {
                ShowStatus("You cannot remove your own account.", true);
                return;
            }

            var result = MessageBox.Show(
                $"Remove user '{user.UserName}' ({user.FullName})?\nThis action cannot be undone.",
                "Confirm Remove", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            IsLoading = true;
            try
            {
                var entity = await _db.AdminUsers
                    .Include(u => u.AdminUserRoles)
                    .FirstOrDefaultAsync(u => u.AdminUserId == user.AdminUserId);

                if (entity == null) { ShowStatus("User not found.", true); return; }

                _db.AdminUserRoles.RemoveRange(entity.AdminUserRoles);
                _db.AdminUsers.Remove(entity);
                await _db.SaveChangesAsync();

                ShowStatus($"User '{user.UserName}' removed.", false);
                await RefreshUsersAsync();
            }
            catch (Exception ex) { ShowStatus($"Error removing user: {ex.Message}", true); }
            finally { IsLoading = false; }
        }

        // ── Toggle Active ─────────────────────────────
        [RelayCommand]
        private async Task ToggleActiveAsync(AdminUserDisplay? user)
        {
            if (user == null) return;
            if (user.AdminUserId == _currentUser?.AdminUserId)
            {
                ShowStatus("You cannot deactivate your own account.", true);
                return;
            }

            IsLoading = true;
            try
            {
                var entity = await _db.AdminUsers.FindAsync(user.AdminUserId);
                if (entity == null) return;

                entity.IsActive = !(entity.IsActive ?? true);
                await _db.SaveChangesAsync();

                ShowStatus($"User '{user.UserName}' is now {(entity.IsActive == true ? "active" : "inactive")}.", false);
                await RefreshUsersAsync();
            }
            catch (Exception ex) { ShowStatus($"Error updating user: {ex.Message}", true); }
            finally { IsLoading = false; }
        }

        // ── Unlock Account ────────────────────────────
        [RelayCommand]
        private async Task UnlockUserAsync(AdminUserDisplay? user)
        {
            if (user == null) return;
            IsLoading = true;
            try
            {
                var entity = await _db.AdminUsers.FindAsync(user.AdminUserId);
                if (entity == null) return;

                entity.IsLocked              = false;
                entity.FailedLoginAttempts   = 0;
                await _db.SaveChangesAsync();

                ShowStatus($"Account '{user.UserName}' unlocked.", false);
                await RefreshUsersAsync();
            }
            catch (Exception ex) { ShowStatus($"Error: {ex.Message}", true); }
            finally { IsLoading = false; }
        }

        private void ShowStatus(string msg, bool isError)
        {
            StatusMessage = msg;
            IsStatusError = isError;
        }
    }
}
