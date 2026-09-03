namespace SimpleDB
{
    public sealed class CSVDatabase<T> : IDatabaseRepository<T> //Implements the IDatabaseRepository. Sealed means no class can inherit from CDVDatabase
    {
        public IEnumerable<T> Read(int? limit = null)
        {
            
        }
        public void Store(T record)
        {
            
        }
    }
}

