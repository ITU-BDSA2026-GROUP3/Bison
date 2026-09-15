using System.Collections;
using static SimpleDB.CSVDatabase;

namespace SimpleDB
{
    public interface IDatabaseRepository
    {
        public IEnumerable<T> Read<T>(string tableName,int? limit = null);
        public void Store<T>(string tableName,T record);

        public void CreateTable<T>(string TableName);

    }
}