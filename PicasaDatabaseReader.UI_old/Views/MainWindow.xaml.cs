using System;
using System.Collections.Specialized;
using System.IO.Abstractions;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using DynamicData.Binding;
using PicasaDatabaseReader.UI.ViewModels;
using ReactiveUI;
using EventsMixin = System.Windows.EventsMixin;

namespace PicasaDatabaseReader.UI.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IViewFor<IMainWindowViewModel>
    {
        private readonly IFileSystem _fileSystem;

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel", typeof(IMainWindowViewModel), typeof(MainWindow), new PropertyMetadata(null));

        public MainWindow(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            InitializeComponent();

            this.WhenActivated(disposable =>
                {
                    this.Bind(ViewModel,
                            model => model.PathToDatabase,
                            window => window.PathToDatabaseText.Text,
                            EventsMixin.Events(PathToDatabaseText).KeyUp)
                        .DisposeWith(disposable);

                    this.OneWayBind(ViewModel,
                            model => model.TableNames,
                            window => window.TableNamesComboBox.ItemsSource)
                        .DisposeWith(disposable);

                    this.OneWayBind(ViewModel,
                            model => model.DataTable,
                            window => window.DataTableDataGrid.ItemsSource)
                        .DisposeWith(disposable);

                    this.OneWayBind(ViewModel,
                            model => model.DatabaseReader,
                            window => window.TableNamesComboBox.IsEnabled,
                            selector: reader => reader != null)
                        .DisposeWith(disposable);

                    this.Bind(ViewModel,
                            model => model.SelectedTable,
                            window => window.TableNamesComboBox.SelectedItem)
                        .DisposeWith(disposable);

                    this.BindCommand(ViewModel,
                            model => model.BrowseToDatabase,
                            window => window.BrowseToDatabaseButton)
                        .DisposeWith(disposable);
                }
            );
        }

        private void DoBrowseToDatabase(string pathToDatabase)
        {
            var dialog = new FolderBrowserDialog();

            if (_fileSystem.Directory.Exists(pathToDatabase))
            {
                dialog.SelectedPath = pathToDatabase;
            }
            else
            {
                var fromDirectoryName = _fileSystem.DirectoryInfo.FromDirectoryName(pathToDatabase);
                while (fromDirectoryName.Parent != null)
                {
                    fromDirectoryName = fromDirectoryName.Parent;
                    if (fromDirectoryName.Exists)
                    {
                        dialog.SelectedPath = fromDirectoryName.FullName;
                        break;
                    }
                }
            }

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ViewModel.PathToDatabase = dialog.SelectedPath;
            }
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (IMainWindowViewModel)value;
        }

        public IMainWindowViewModel ViewModel
        {
            get => (IMainWindowViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        private void DataTableDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex()).ToString();
        }
    }
}
