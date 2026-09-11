using System.Collections;

namespace SimpleDB
{
    public interface IDatabaseRepository
    {
        public IEnumerable Read(string tableName,int? limit = null);
        public void Store<T>(string tableName,T record);
    }
}