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
            DataContext = new MainWindowPM();
        }

//        public MainWindowPM PresentationModel {get { return DataContext as MainWindowPM; }}
//
//        private void SelectDatabaseDirectoryClick(object sender, RoutedEventArgs e)
//        {
//            var dialog = new FolderBrowserDialog();
//            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
//            {
//                PresentationModel.PathToDatabase = dialog.SelectedPath;
//            }
//        }
    }
}
