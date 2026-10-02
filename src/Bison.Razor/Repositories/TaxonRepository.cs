



using Microsoft.EntityFrameworkCore;

namespace Bison.Razor.Repositories
{

    public class TaxonRepository : ITaxonRepository
    {
        private readonly BisonContext _bisonContext;
        public TaxonRepository(BisonContext bisonContext)
        {
            _bisonContext = bisonContext;
        }
    public async Task<List<int>> CreateTaxons(List<Taxon> taxons)
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

        public async Task<Taxon> getTaxon(int taxonId)
        {
            // Define the query - with our setup, EF Core translates this to an SQLite query in the background
            var query = _bisonContext.Taxons
             .Where(taxon => taxon.Id == taxonId);
            // Execute the query and store the results
            var result = await query.SingleAsync();

            return result;
        }
        
        public async Task<int> nextId()
        {
            // needs to be properly implementet
            return 1000000000;
        }
    }
}