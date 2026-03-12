using System.IO;
using System.Windows;
using ACGCET_Admin.Models;
using ACGCET_Admin.Services;
using ACGCET_Admin.ViewModels.Dashboard;
using ACGCET_Admin.Views.Dashboard;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ACGCET_Admin
{
    public partial class App : Application
    {
        private readonly IHost? _host;

        public App()
        {
            try
            {
                _host = Host.CreateDefaultBuilder()
                    .ConfigureAppConfiguration((_, config) =>
                    {
                        config.SetBasePath(Directory.GetCurrentDirectory());
                        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    })
                    .ConfigureServices((context, services) =>
                    {
                        string cs = context.Configuration.GetConnectionString("DefaultConnection")
                            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

                        services.AddDbContext<AcgcetDbContext>(
                            options => options.UseSqlServer(cs,
                                sql => sql.EnableRetryOnFailure(
                                    maxRetryCount: 3,
                                    maxRetryDelay: TimeSpan.FromSeconds(5),
                                    errorNumbersToAdd: null)),
                            contextLifetime: ServiceLifetime.Transient,
                            optionsLifetime: ServiceLifetime.Singleton);

                        // Services
                        services.AddSingleton<EmailService>();

                        // ViewModels
                        services.AddSingleton<MainViewModel>();
                        services.AddTransient<LoginViewModel>();

                        // Main window
                        services.AddSingleton<MainWindow>(provider =>
                        {
                            var vm  = provider.GetRequiredService<MainViewModel>();
                            var win = new MainWindow(vm);
                            return win;
                        });
                    })
                    .Build();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Startup Error: {ex.Message}\n{ex.StackTrace}",
                    "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);
                if (_host != null)
                {
                    await _host.StartAsync();
                    var mainWindow = _host.Services.GetRequiredService<MainWindow>();
                    MainWindow = mainWindow;
                    mainWindow.Show();

                    // Pre-warm EF Core so first login is fast
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await using var db = _host.Services.GetRequiredService<AcgcetDbContext>();
                            await db.Database.ExecuteSqlRawAsync("SELECT 1");
                        }
                        catch { /* ignore warmup errors — DB may not be ready yet */ }
                    });
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Startup Error: {ex.Message}\n{ex.StackTrace}",
                    "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (_host != null)
            {
                using (_host) { await _host.StopAsync(); }
            }
            base.OnExit(e);
        }
    }
}
