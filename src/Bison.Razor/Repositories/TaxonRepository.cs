



namespace Bison.Razor.Repositories
{

    public class TaxonRepository : ITaxonRepository
    {
        private readonly BisonContext _bisonContext;
        public PostRepository(BisonContext bisonContext)
        {
            _bisonContext = bisonContext;
        }
    public async Task<int> CreateTaxons(List<Author> taxons)
        {
            List<int> ids = new List<int>();
            foreach(Taxon taxon in taxons)
            {
                var queryResult = await _bisonContext.Taxons.AddAsync(taxon); // does not write to the database!
                ids.Add(queryResult.Entity.Id);
            }
            await _bisonContext.SaveChangesAsync(); // persist the changes in the database
            return ids;
        }

        public async Task<Author> getTaxons(int taxonId)
        {
            // Define the query - with our setup, EF Core translates this to an SQLite query in the background
            var query = _bisonContext.Authors
             .Where(obs => obs.Taxon.Id == taxonId);
            // Execute the query and store the results
            var result = await query.ToListAsync();

            return result;
        }

        public async Task<List<Author>> getTaxons(string taxonName)
        {
            // Define the query - with our setup, EF Core translates this to an SQLite query in the background
            var query = _bisonContext.Authors
             .Where(obs => obs.Taxon.Name == taxonName);
            // Execute the query and store the results
            var result = await query.ToListAsync();

            return result;
        }
        
        public async Task<int> nextId()
        {
            // needs to be properly implementet
            return 1000000000;
        }
    }
}