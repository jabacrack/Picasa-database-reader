using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicasaDatabaseReader
{
    class DatabaseReader
    {
        
        private string pathToDatabase;

        public DatabaseReader(string pathToDatabase)
        {
            this.pathToDatabase = pathToDatabase + @"\Google\Picasa2\db3\";
            TableNames = DbUtils.GetTablesNames(this.pathToDatabase).ToArray();
        }

        public string[] TableNames { get; private set; }
        
        #region Logic

        public Task<DataTable> GetTableAsync(string tableName, IProgress<int> progress)
        {
            return Task.Run(() => GetTable(tableName, progress));
        }

        public DataTable GetTable(string tableName, IProgress<int> progress)
        {
            progress.Report(0);

            if (string.IsNullOrEmpty(tableName))
                return new DataTable();
            
            var fields = DbUtils.GetFieldsFiles(pathToDatabase, tableName).Select(FieldFactory.CreateField).ToArray();
            var rowCount = fields.Max(f => f.Count);
            var table = new DataTable(tableName);

            var step = rowCount/100;
            var currentStepValue = 0;
            
            foreach (var field in fields)
            {
                var column = new DataColumn(field.Name, field.Type);
                column.AllowDBNull = true;
                table.Columns.Add(column);
            }

            for (int i = 0; i < rowCount; i++)
            {
                var row = table.NewRow();
                foreach (var field in fields)
                {
                    object value = field.ReadValue() ?? DBNull.Value;
                    row[field.Name] = value;
                }

                table.Rows.Add(row);
                
                 
                if (i >= (currentStepValue + 1) * step )
                    progress.Report(++currentStepValue);
            }

            progress.Report(100);

            return table;
           
        }

        private string[] GetTableFields(string tableName)
        {
            var files = Directory.GetFiles(pathToDatabase, string.Format("{0}_*.", tableName));
            var fields = files.Select(Path.GetFileNameWithoutExtension).Select(str => str.Split('_')[1]).ToArray();
            return fields;
        }

        #endregion
    }
}
