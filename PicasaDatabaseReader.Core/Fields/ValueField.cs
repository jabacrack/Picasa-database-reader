namespace PicasaDatabaseReader.Core.Fields
{
    public class ValueField<TRecord>: FieldBase
        where TRecord: struct
    {
        public ValueField(string name, string path, uint count) 
            : base(typeof(TRecord), name, path, count)
        {
        }
    }
}
