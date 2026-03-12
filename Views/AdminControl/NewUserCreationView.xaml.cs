using System.Windows;
using System.Windows.Controls;
using ACGCET_Admin.ViewModels.AdminControl;

namespace ACGCET_Admin.Views.AdminControl
{
    public partial class NewUserCreationView : UserControl
    {
        public NewUserCreationView()
        {
            InitializeComponent();
        }

        private void PbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is NewUserCreationViewModel vm)
            {
                vm.Password = ((PasswordBox)sender).Password;
            }
        }

        private void PbConfirm_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is NewUserCreationViewModel vm)
            {
                vm.ConfirmPassword = ((PasswordBox)sender).Password;
            }
        }

        private void Button_Clear_Click(object sender, RoutedEventArgs e)
        {
            pbPassword.Password = "";
            pbConfirm.Password = "";
        }
    }
}
