using System;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using PicasaDatabaseReader.Annotations;
using PicasaDatabaseReader.Core;
using ReactiveUI;

namespace PicasaDatabaseReader
{
    public class MainWindowViewModel : ReactiveObject
    {
//        public MainWindowViewModel()
//        {
//            PathToDatabase = @"C:\Users\Spade\AppData\Local";
//        }
//
//        public bool IsBusy
//        {
//            get { return isBusy; }
//            set
//            {
//                if (value.Equals(isBusy)) return;
//                isBusy = value;
//                OnPropertyChanged();
//            }
//        }
//
//        public int Progress
//        {
//            get { return progress; }
//            set
//            {
//                if (value.Equals(progress)) return;
//                progress = value;
//                OnPropertyChanged();
//            }
//        }
//
//        public string PathToDatabase
//        {
//            get { return pathToDatabase; }
//            set
//            {
//                throw new NotImplementedException();
//
////                if (value == pathToDatabase) return;
////                pathToDatabase = value;
////                databaseReader = new DatabaseReader(value);
////                Tables = databaseReader.TableNames;
////                SelectedTable = databaseReader.TableNames.FirstOrDefault();
////                OnPropertyChanged();
//            }
//        }
//
//        public string SelectedTable
//        {
//            get { return selectedTable; }
//            set
//            {
//                if (value == selectedTable) return;
//                selectedTable = value;
//                OnPropertyChanged();
//                UpdateTable();
//            }
//        }
//
//        private async void UpdateTable()
//        {
//            throw new NotImplementedException();
//
//
//            //            IsBusy = true;
//            //            Table = await databaseReader.GetTableAsync(SelectedTable, this);
//            //            IsBusy = false;
//        }
//
//        public string[] Tables
//        {
//            get { return tables; }
//            set
//            {
//                if (Equals(value, tables)) return;
//                tables = value;
//                OnPropertyChanged();
//            }
//        }
//
//        public DataTable Table
//        {
//            get { return table; }
//            set
//            {
//                if (Equals(value, table)) return;
//                table = value;
//                OnPropertyChanged();
//            }
//        }
    }
}
