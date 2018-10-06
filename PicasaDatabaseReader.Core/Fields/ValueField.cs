using System;
using System.Runtime.InteropServices;

namespace PicasaDatabaseReader.Core.Fields
{
    public class ValueField<TRecord>: FieldBase
        where TRecord: struct
    {
        public ValueField(string filepath, HeaderData header) : base(filepath, header)
        {
            Type = typeof (TRecord);
        }

        public override object ReadValue()
        {
            var type = typeof (TRecord);
            var size = Marshal.SizeOf(type);
            var bytes = reader.ReadBytes(size);

            if (bytes.Length < size)
                return null;

            //part of unsafe code from http://stackoverflow.com/questions/7255951/how-to-get-byte-size-of-type-in-generic-list
            //or I'm should create 5 readers for 5 value types
            //I need to think about this
            unsafe
            {
                fixed (byte* p = bytes)
                {
                    return Marshal.PtrToStructure((IntPtr) p, type);
                }
            }
        }
    }
}
