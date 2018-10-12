using System.IO.Abstractions;

namespace PicasaDatabaseReader.Core.Fields
{
    public class String2Field : StringField
    {
        public String2Field(string name, string path, uint count, IFileSystem fileSystem) 
            : base(name, path, count, fileSystem)
        {
        }
    }
}
