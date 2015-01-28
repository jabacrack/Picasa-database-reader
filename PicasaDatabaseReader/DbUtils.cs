using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicasaDatabaseReader
{
    public class DbUtils
    {
        public static bool IsTableFile(string filepath)
        {
            using (var reader = new BinaryReader(File.OpenRead(filepath)))
            {
                try
                {
                    return reader.ReadUInt32() == 0x3fcccccd;
                }
                catch (Exception e)
                {
                    return false;
                }
            }
        }

        public static IEnumerable<string> GetTablesNames(string pathToDatabase)
        {
            return Directory.GetFiles(pathToDatabase, "*_0")
                .Where(IsTableFile)
                .Select(Path.GetFileNameWithoutExtension)
                .Select(str => str.Substring(0, str.IndexOf("_0")))
                .ToArray();
        }

        public static IEnumerable<string> GetFieldsFiles(string databasePath, string tableName)
        {
            var files = Directory.GetFiles(databasePath, string.Format("{0}_*.pmp", tableName));
            return files;
        }

        public static HeaderData ReadHeader(BinaryReader reader)
        {
            ConstatnsChecker<UInt32>(reader.ReadUInt32(), 0x3fcccccd); // constant 0x3fcccccd
            var fieldType = reader.ReadUInt16(); //field type
            ConstatnsChecker(reader.ReadUInt16(), 0x1332); // constant 0x1332
            ConstatnsChecker<UInt32>(reader.ReadUInt32(), 0x00000002); // constant 0x00000002 
            ConstatnsChecker(reader.ReadUInt16(), fieldType); // copy of field type
            ConstatnsChecker(reader.ReadUInt16(), 0x1332); // constant 0x1332
            var count = reader.ReadUInt32(); //number of entries
            return new HeaderData(fieldType, count);
        }

        private static void ConstatnsChecker<T>(T x, T y)
            where T: struct
        {
            if (!EqualityComparer<T>.Default.Equals(x,y))
                throw new Exception("Bad file header.");

        }

    }
}
