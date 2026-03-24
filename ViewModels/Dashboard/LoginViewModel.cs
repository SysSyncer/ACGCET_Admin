using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ACGCET_Admin.Models;
using Microsoft.EntityFrameworkCore;

namespace ACGCET_Admin.ViewModels.Dashboard
{
    public record LoginSuccessMessage(AdminUser User);

    public partial class LoginViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _dbContext;

        [ObservableProperty] private string _username = string.Empty;
        [ObservableProperty] private string _password = string.Empty;
        [ObservableProperty] private string _errorMessage = string.Empty;
        [ObservableProperty] private bool _isErrorVisible;
        [ObservableProperty] private bool _isPasswordVisible;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotLoading))]
        private bool _isLoading;

        public bool IsNotLoading => !IsLoading;

        public LoginViewModel(AcgcetDbContext dbContext) => _dbContext = dbContext;
        public LoginViewModel() => _dbContext = null!;

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (IsLoading) return;

            IsLoading = true;
            IsErrorVisible = false;
            ErrorMessage = string.Empty;

            try
            {
                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Please enter username and password.";
                    IsErrorVisible = true;
                    return;
                }

                var user = await _dbContext.AdminUsers
                    .FirstOrDefaultAsync(u => u.UserName == Username.Trim());

                if (user == null)
                {
                    ErrorMessage = "Invalid username or password.";
                    IsErrorVisible = true;
                    return;
                }

                if (user.IsLocked == true)
                {
                    ErrorMessage = "Account is locked. Contact the system administrator.";
                    IsErrorVisible = true;
                    return;
                }

                if (user.IsActive == false)
                {
                    ErrorMessage = "Account is inactive. Contact the system administrator.";
                    IsErrorVisible = true;
                    return;
                }

                bool valid = await Task.Run(() => BCrypt.Net.BCrypt.Verify(Password, user.PasswordHash));

                if (!valid)
                {
                    user.FailedLoginAttempts = (user.FailedLoginAttempts ?? 0) + 1;
                    if (user.FailedLoginAttempts >= 5)
                        user.IsLocked = true;
                    await _dbContext.SaveChangesAsync();
                    ErrorMessage = user.IsLocked == true
                        ? "Account locked due to too many failed attempts. Contact the system administrator."
                        : "Invalid username or password.";
                    IsErrorVisible = true;
                    return;
                }

                // Admin portal is restricted to Administrator and COE roles only
                var hasAccess = await _dbContext.AdminUserRoles
                    .Include(r => r.Role)
                    .AnyAsync(r => r.AdminUserId == user.AdminUserId &&
                                   (r.Role!.RoleName == "Administrator" || r.Role.RoleName == "COE"));

                if (!hasAccess)
                {
                    ErrorMessage = "Access denied. This portal is for COE/Admin users only. Please use the Faculty portal.";
                    IsErrorVisible = true;
                    return;
                }

                // Set SESSION_CONTEXT for audit triggers
                _dbContext.SetSessionContext(user.UserName);

                // Track login
                user.LastLoginDate = System.DateTime.Now;
                user.FailedLoginAttempts = 0;
                await _dbContext.SaveChangesAsync();

                WeakReferenceMessenger.Default.Send(new LoginSuccessMessage(user));
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"Database error: {ex.Message}";
                IsErrorVisible = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void TogglePasswordVisibility() => IsPasswordVisible = !IsPasswordVisible;
    }
}
