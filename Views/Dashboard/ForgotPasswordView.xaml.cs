using System.Windows.Controls;
using ACGCET_Admin.ViewModels.Dashboard;

namespace ACGCET_Admin.Views.Dashboard
{
    public partial class ForgotPasswordView : UserControl
    {
        public ForgotPasswordView()
        {
            InitializeComponent();
        }

        private void NewPassword_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is ForgotPasswordViewModel vm)
                vm.NewPassword = ((PasswordBox)sender).Password;
        }

        private void ConfirmPassword_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is ForgotPasswordViewModel vm)
                vm.ConfirmPassword = ((PasswordBox)sender).Password;
        }
    }
}
