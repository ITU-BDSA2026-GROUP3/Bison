using System.CommandLine;
using System.IO;
using System.Globalization;
using CsvHelper;
using System.Collections;

namespace SimpleDB
{
    public sealed class CSVDatabase : IDatabaseRepository //Implements the IDatabaseRepository. Sealed means no class can inherit from CDVDatabase
    {
        private static CSVDatabase instance = null;

        public static CSVDatabase Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CSVDatabase();
                }
                return instance;
            }
        }


        private string directoryPath;
        public string DirectoryPath{
            get { return directoryPath; }

            set
            { if (!Path.Exists(value)){ // creates directory if it doesn't exist yet
                    Directory.CreateDirectory(value);
                }
                directoryPath = value;
            }
        }
        private Dictionary<string, Table> TableLookup = new Dictionary<string, Table>();


        private CSVDatabase()
        {
        }

        public void CreateTable<T>(string TableName)
        {
            Table<T> table = new Table<T>(TableName,DirectoryPath);
            TableLookup.Add(TableName, table);
        }


        
        public IEnumerable<T> Read<T>(string TableName, int? limit = null)
        {
            return TableLookup[TableName].Read<T>();

        }
        public void Store<T> (string TableName, T record)
        {
            TableLookup[TableName].Store(record);
        }

        internal abstract class Table
        {
            internal string TableName;

            internal string csvFilePath = "";
            public string CSVFilePath
            {
                get { return csvFilePath; }

                set
                {
                    if (!File.Exists(value))
                    { // creates directory if it doesn't exist yet
                        File.Create(value);
                    }
                    csvFilePath = value;
                }
            }

            internal abstract IEnumerable<T> Read<T>(int? limit = null);
            internal abstract void Store<T>(T record);

        }

        internal class Table<T> : Table
        {
            internal Table(string tName, string dirPath)
            {
                TableName = tName;
                CSVFilePath = Path.Combine(dirPath, $"/{TableName}.csv");
            }

            internal override IEnumerable<T> Read<T>(int? limit = null)
            {
                bool fileExists = File.Exists(CSVFilePath);

                if (!fileExists)
                {
                    return new List<T>();
                }

                using var reader = new StreamReader(CSVFilePath);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                var cheeps = csv.GetRecords<T>().ToList();

                return cheeps;
            }

            internal override void Store<T>(T record)
            {
                bool fileExists = File.Exists(CSVFilePath);

                using var writer = new StreamWriter(CSVFilePath, append: true);
                using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

                if (!fileExists)
                {
                    csv.WriteHeader<T>();
                    csv.NextRecord();
                }

                csv.WriteRecord(record);
                csv.NextRecord();
            }
        }
    }
}

