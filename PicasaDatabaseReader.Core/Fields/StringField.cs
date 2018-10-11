namespace PicasaDatabaseReader.Core.Fields
{
    public class StringField : FieldBase
    {
        public StringField(string name, string path, uint count)
            : base(typeof(string), name, path, count)
        {
        }
    }
}