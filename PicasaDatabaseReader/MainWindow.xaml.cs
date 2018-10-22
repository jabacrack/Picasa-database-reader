using System.Windows;

namespace PicasaDatabaseReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }

//        public MainWindowViewModel PresentationModel {get { return DataContext as MainWindowViewModel; }}
//
//        private void SelectDatabaseDirectoryClick(object sender, RoutedEventArgs e)
//        {

//        }
    }
}
