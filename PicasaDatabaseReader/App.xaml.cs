using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace PicasaDatabaseReader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IServiceCollection serviceCollection;


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            serviceCollection = new ServiceCollection()
                .AddLogging(builder => builder.AddSerilog());
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }
}
