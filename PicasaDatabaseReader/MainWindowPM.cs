using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using PicasaDatabaseReader.Annotations;

namespace PicasaDatabaseReader
{
    public class MainWindowPM : INotifyPropertyChanged, IProgress<int>
    {
        private DataTable table;
        private string pathToDatabase;
        private string[] tables;
        private string selectedTable;
        private DatabaseReader databaseReader;
        private int progress;
        private bool isBusy;

        public MainWindowPM()
        {
            PathToDatabase = @"E:\Документы\Picasa database";
        }

        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                if (value.Equals(isBusy)) return;
                isBusy = value;
                OnPropertyChanged();
            }
        }

        public int Progress
        {
            get { return progress; }
            set
            {
                if (value.Equals(progress)) return;
                progress = value;
                OnPropertyChanged();
            }
        }

        public string PathToDatabase
        {
            get { return pathToDatabase; }
            set
            {
                if (value == pathToDatabase) return;
                pathToDatabase = value;
                databaseReader = new DatabaseReader(value);
                Tables = databaseReader.TableNames;
                SelectedTable = databaseReader.TableNames.FirstOrDefault();
                OnPropertyChanged();
            }
        }

        public string SelectedTable
        {
            get { return selectedTable; }
            set
            {
                if (value == selectedTable) return;
                selectedTable = value;
                OnPropertyChanged();
                UpdateTable();
            }
        }

        private async void UpdateTable()
        {
            IsBusy = true;
            Table = await databaseReader.GetTableAsync(SelectedTable, this);
            IsBusy = false;
        }

        public string[] Tables
        {
            get { return tables; }
            set
            {
                if (Equals(value, tables)) return;
                tables = value;
                OnPropertyChanged();
            }
        }

        public DataTable Table
        {
            get { return table; }
            set
            {
                if (Equals(value, table)) return;
                table = value;
                OnPropertyChanged();
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public void Report(int value)
        {
            Progress = value;
        }
    }
}
