using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PicasaDatabaseReader.Core;
using PicasaDatabaseReader.Core.Interfaces;
using PicasaDatabaseReader.UI.ViewModels;
using PicasaDatabaseReader.UI.Views;
using Serilog;

namespace PicasaDatabaseReader.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var serviceCollection = new ServiceCollection()
                .AddLogging(builder => builder.AddSerilog())
                .AddScoped<IFileSystem, FileSystem>()
                .AddScoped<IDatabaseReaderProvider, DatabaseReaderProvider>()
                .AddScoped<IMainWindowViewModel, MainWindowViewModel>()
                .AddScoped<MainWindow>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var mainWindow = serviceProvider.GetService<MainWindow>();
            var mainWindowViewModel = serviceProvider.GetService<IMainWindowViewModel>();
            var fileSystem = serviceProvider.GetService<IFileSystem>();

            var databasePath = Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%\\Google\\Picasa2\\db3");
            if (fileSystem.Directory.Exists(databasePath))
            {
                mainWindowViewModel.PathToDatabase = databasePath;
            }

            mainWindow.ViewModel = mainWindowViewModel;
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }
}
