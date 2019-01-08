using System;
using System.Collections.ObjectModel;
using System.Data;
using System.IO.Abstractions;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Binding;
using PicasaDatabaseReader.Core.Interfaces;
using ReactiveUI;

namespace PicasaDatabaseReader.UI.ViewModels
{
    public class MainWindowViewModel: ReactiveObject, IMainWindowViewModel
    {
        public MainWindowViewModel(IFileSystem fileSystem, IDatabaseReaderProvider databaseReaderProvider)
        {
            BrowseToDatabase = ReactiveCommand.Create(() => { });

            _databaseReader = this.WhenAnyValue(model => model.PathToDatabase)
                .Select(s => fileSystem.Directory.Exists(s)? databaseReaderProvider.GetDatabaseReader(s) : null)
                .ToProperty(this, model => model.DatabaseReader);

            var tableNames = new ObservableCollectionExtended<string>();
            TableNames = tableNames;

            var tablesNamesSource = new SourceList<string>();
            tablesNamesSource.Connect().Bind(tableNames).Subscribe();

            this.WhenAnyValue(model => model.DatabaseReader)
                .Select(reader => reader != null ? reader.GetTableNames() : Observable.Empty<string>())
                .Subscribe(async obs =>
                {
                    tablesNamesSource.Clear();
                    SelectedTable = null;

                    var items = await obs.ToArray().FirstAsync();
                    tablesNamesSource.AddRange(items);
                });

            _dataTable = this.WhenAnyValue(model => model.DatabaseReader, model => model.SelectedTable)
                .SelectMany(tuple => tuple.Item1 != null && tuple.Item2 != null
                    ? tuple.Item1.GetDataTable(tuple.Item2)
                    : Observable.Return(new DataTable()))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(table => table.DefaultView)
                .ToProperty(this, model => model.DataTable);
        }

        public ObservableCollection<string> TableNames { get; set; }

        public ReactiveCommand<Unit, Unit> BrowseToDatabase { get; set; }

        private string _pathToDatabase;

        public string PathToDatabase
        {
            get => _pathToDatabase;
            set => this.RaiseAndSetIfChanged(ref _pathToDatabase, value);
        }

        private string _selectedTable;

        public string SelectedTable
        {
            get => _selectedTable;
            set => this.RaiseAndSetIfChanged(ref _selectedTable, value);
        }

        private readonly ObservableAsPropertyHelper<IDatabaseReader> _databaseReader;
        public IDatabaseReader DatabaseReader => _databaseReader.Value;

        private readonly ObservableAsPropertyHelper<DataView> _dataTable;
        public DataView DataTable => _dataTable.Value;
    }
}
