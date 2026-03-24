using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ACGCET_Admin.Models;
using ACGCET_Admin.Services;
using Microsoft.EntityFrameworkCore;

namespace ACGCET_Admin.ViewModels.Dashboard
{
    public partial class MainViewModel : ObservableObject,
        IRecipient<LoginSuccessMessage>
    {
        private readonly AcgcetDbContext _db;
        private readonly LoginViewModel _loginVm;
        private readonly UserPermissionService _permissionService;

        [ObservableProperty] private string _windowTitle = "ACGCET Admin — COE Portal";
        [ObservableProperty] private object _currentView = new object();

        public bool IsSuperAdmin { get; private set; }
        public bool IsCOE { get; private set; }
        public bool IsFaculty { get; private set; }

        public MainViewModel(AcgcetDbContext db, LoginViewModel loginVm, UserPermissionService permissionService)
        {
            _db = db;
            _loginVm = loginVm;
            _permissionService = permissionService;

            CurrentView = _loginVm;

            WeakReferenceMessenger.Default.Register<LoginSuccessMessage>(this);
            WeakReferenceMessenger.Default.Register<LogoutMessage>(this, (_, m) => Receive(m));
        }

        public MainViewModel() { _db = null!; _loginVm = null!; _permissionService = null!; }

        [RelayCommand]
        private static void Exit() => System.Windows.Application.Current.Shutdown();

        // ── Login Success ─────────────────────────────
        public async void Receive(LoginSuccessMessage message)
        {
            try
            {
                var userRoles = await _db.AdminUserRoles
                    .Where(ur => ur.AdminUserId == message.User.AdminUserId)
                    .Select(ur => ur.Role!.RoleName)
                    .ToListAsync();

                IsSuperAdmin = userRoles.Contains("Super Admin")
                            || userRoles.Contains("COE")
                            || userRoles.Contains("CEO")
                            || userRoles.Contains("Administrator");
                IsCOE = userRoles.Contains("COE") || userRoles.Contains("Admin");
                IsFaculty = userRoles.Contains("Faculty");

                if (!IsSuperAdmin && !IsCOE && !IsFaculty)
                {
                    MessageBox.Show(
                        "Your account has no assigned roles. Contact the system administrator.",
                        "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error);
                    ResetLogin();
                    return;
                }

                // Load RBAC permissions for this user
                await _permissionService.LoadPermissionsAsync(_db, message.User.AdminUserId);
                UserPermissionService.Current = _permissionService;

                // Set SESSION_CONTEXT on the main DbContext for audit triggers
                _db.SetSessionContext(message.User.UserName);

                CurrentView = new DashboardViewModel(_db, message.User, IsSuperAdmin, IsCOE);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error loading user roles: {ex.Message}",
                    "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ResetLogin();
            }
        }


        // ── Logout ────────────────────────────────────
        public void Receive(LogoutMessage _) => ResetLogin();

        private void ResetLogin()
        {
            IsSuperAdmin = false;
            IsCOE = false;
            IsFaculty = false;
            _permissionService?.Clear();
            IsFaculty = false;

            _loginVm.Username = string.Empty;
            _loginVm.Password = string.Empty;
            _loginVm.IsErrorVisible = false;
            _loginVm.IsPasswordVisible = false;
            CurrentView = _loginVm;
        }
    }
}
