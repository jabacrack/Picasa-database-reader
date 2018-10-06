using System;
using System.IO;

namespace PicasaDatabaseReader.Core.Fields
{
    public abstract class FieldBase: IDisposable, IField
    {
        private string filepath;
        protected BinaryReader reader;

        public FieldBase(string filepath, HeaderData header)
        {
            if (string.IsNullOrEmpty(filepath))
                throw new Exception("Cannot create field for empty path.");

            reader = new BinaryReader(File.OpenRead(filepath));
            Count = header.RecordsCount;
            var fileName = Path.GetFileNameWithoutExtension(filepath);
            Name = fileName.Remove(0, fileName.IndexOf('_') + 1);
        }

        public abstract object ReadValue();
        

        public uint Count { get; private set; }
        public string Name { get; private set; }
        public Type Type { get; protected set; }

        public void Close()
        {
            if (reader != null)
            {
                reader.Close();
                reader = null;
            }
        }

        public void Dispose()
        {
            Close();
        }
    }
}