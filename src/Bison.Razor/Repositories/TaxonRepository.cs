


using Bison.Razor.domain_model;
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

            foreach(Taxon taxon in taxons)
            {
                var queryResult = await _bisonContext.Taxons.AddAsync(taxon); // does not write to the database!
            }
            await _bisonContext.SaveChangesAsync(); // persist the changes in the database

            List<int> ids = new List<int>();
            foreach (Taxon taxon in taxons)
            {
                ids.Add(taxon.Id);
            }
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

        public async Task<List<Taxon>> getAllTaxons()
        {
            var query = _bisonContext.Taxons;
            var result = await query.ToListAsync();

            return result;
        }
    }
}