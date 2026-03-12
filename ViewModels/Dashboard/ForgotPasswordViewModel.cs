using System;
using System.Threading.Tasks;
using System.Windows;
using BCrypt.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ACGCET_Admin.Models;
using ACGCET_Admin.Services;
using Microsoft.EntityFrameworkCore;

namespace ACGCET_Admin.ViewModels.Dashboard
{
    public record ResetPasswordCompleteMessage(bool Success);

    public enum ForgotStep { EnterUsername, VerifyOtp, NewPassword }

    public partial class ForgotPasswordViewModel : ObservableObject
    {
        private readonly AcgcetDbContext _db;
        private readonly EmailService    _email;

        private AdminUser? _foundUser;
        private string     _otpCode   = string.Empty;
        private DateTime   _otpExpiry = DateTime.MinValue;

        // ── Step ──────────────────────────────────────
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsStep1))]
        [NotifyPropertyChangedFor(nameof(IsStep2))]
        [NotifyPropertyChangedFor(nameof(IsStep3))]
        private ForgotStep _currentStep = ForgotStep.EnterUsername;

        public bool IsStep1 => CurrentStep == ForgotStep.EnterUsername;
        public bool IsStep2 => CurrentStep == ForgotStep.VerifyOtp;
        public bool IsStep3 => CurrentStep == ForgotStep.NewPassword;

        // ── Step 1 ────────────────────────────────────
        [ObservableProperty] private string _usernameInput = string.Empty;

        // ── Step 2 ────────────────────────────────────
        [ObservableProperty] private string _otpInput      = string.Empty;

        // ── Step 3 ────────────────────────────────────
        [ObservableProperty] private string _newPassword   = string.Empty;
        [ObservableProperty] private string _confirmPassword = string.Empty;

        // ── UI State ──────────────────────────────────
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotLoading))]
        private bool _isLoading;

        public bool IsNotLoading => !IsLoading;

        [ObservableProperty] private string _errorMessage   = string.Empty;
        [ObservableProperty] private bool   _isErrorVisible;
        [ObservableProperty] private string _successMessage = string.Empty;
        [ObservableProperty] private bool   _isSuccessVisible;

        // ── Email hint ────────────────────────────────
        [ObservableProperty] private string _emailHint = string.Empty;

        public ForgotPasswordViewModel(AcgcetDbContext db, EmailService email)
        {
            _db    = db;
            _email = email;
        }

        public ForgotPasswordViewModel() { _db = null!; _email = null!; }

        // ── Step 1: Send OTP ──────────────────────────
        [RelayCommand]
        private async Task SendOtpAsync()
        {
            ClearMessages();
            if (string.IsNullOrWhiteSpace(UsernameInput))
            {
                ShowError("Please enter your username.");
                return;
            }

            IsLoading = true;
            try
            {
                _foundUser = await _db.AdminUsers
                    .FirstOrDefaultAsync(u => u.UserName == UsernameInput.Trim());

                if (_foundUser == null)
                {
                    await Task.Delay(500); // prevent enumeration
                    ShowError("No account found with that username.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(_foundUser.Email))
                {
                    ShowError("This account has no email address registered. Contact the system administrator.");
                    return;
                }

                _otpCode   = new Random().Next(100000, 999999).ToString();
                _otpExpiry = DateTime.Now.AddMinutes(10);

                await _email.SendOtpEmailAsync(_foundUser.Email, _foundUser.FullName ?? _foundUser.UserName, _otpCode);

                // Mask the email for display: ha*****@gmail.com
                var em   = _foundUser.Email;
                var idx  = em.IndexOf('@');
                EmailHint = idx > 1
                    ? em[..2] + new string('*', Math.Max(1, idx - 2)) + em[idx..]
                    : em;

                CurrentStep = ForgotStep.VerifyOtp;
                ShowSuccess($"OTP sent to {EmailHint}. Valid for 10 minutes.");
            }
            catch (Exception ex)
            {
                ShowError($"Failed to send OTP: {ex.Message}");
            }
            finally { IsLoading = false; }
        }

        // ── Step 2: Verify OTP ────────────────────────
        [RelayCommand]
        private void VerifyOtp()
        {
            ClearMessages();
            if (string.IsNullOrWhiteSpace(OtpInput))
            {
                ShowError("Please enter the OTP.");
                return;
            }

            if (DateTime.Now > _otpExpiry)
            {
                ShowError("OTP has expired. Please go back and request a new one.");
                return;
            }

            if (OtpInput.Trim() != _otpCode)
            {
                ShowError("Invalid OTP. Please try again.");
                return;
            }

            CurrentStep = ForgotStep.NewPassword;
        }

        // ── Step 3: Reset Password ────────────────────
        [RelayCommand]
        private async Task ResetPasswordAsync()
        {
            ClearMessages();
            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                ShowError("Please enter a new password.");
                return;
            }
            if (NewPassword.Length < 6)
            {
                ShowError("Password must be at least 6 characters.");
                return;
            }
            if (NewPassword != ConfirmPassword)
            {
                ShowError("Passwords do not match.");
                return;
            }

            IsLoading = true;
            try
            {
                var user = await _db.AdminUsers.FindAsync(_foundUser!.AdminUserId);
                if (user == null) { ShowError("User not found."); return; }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(NewPassword);
                user.IsLocked     = false;
                user.FailedLoginAttempts = 0;
                await _db.SaveChangesAsync();

                MessageBox.Show(
                    "Password reset successfully! You can now log in with your new password.",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                WeakReferenceMessenger.Default.Send(new ResetPasswordCompleteMessage(true));
            }
            catch (Exception ex)
            {
                ShowError($"Error resetting password: {ex.Message}");
            }
            finally { IsLoading = false; }
        }

        // ── Navigation ────────────────────────────────
        [RelayCommand]
        private void BackToLogin() =>
            WeakReferenceMessenger.Default.Send(new ResetPasswordCompleteMessage(false));

        [RelayCommand]
        private void BackToStep1()
        {
            OtpInput    = string.Empty;
            CurrentStep = ForgotStep.EnterUsername;
            ClearMessages();
        }

        [RelayCommand]
        private void ResendOtp()
        {
            OtpInput    = string.Empty;
            CurrentStep = ForgotStep.EnterUsername;
            ClearMessages();
        }

        // ── Helpers ───────────────────────────────────
        private void ShowError(string msg)   { ErrorMessage = msg;   IsErrorVisible   = true;  IsSuccessVisible = false; }
        private void ShowSuccess(string msg) { SuccessMessage = msg; IsSuccessVisible = true;  IsErrorVisible   = false; }
        private void ClearMessages()         { IsErrorVisible = false; IsSuccessVisible = false; }
    }
}
