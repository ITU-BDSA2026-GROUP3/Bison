 using Bison.Core.domain_model;
using Bison.infrastructure.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Bison.infrastructure.Repositories
{

    public class TaxonRepository : ITaxonRepository
    {
        private readonly BisonContext _bisonContext;
        public TaxonRepository(BisonContext bisonContext)
        {
            _bisonContext = bisonContext;
        }
        public async Task<List<int>> CreateTaxons(List<TaxonDto> taxons)
        {

            foreach(TaxonDto dto in taxons)
            {
                Taxon? ancestor = await _bisonContext.Taxons
                .Where(taxon => taxon.Id == dto.ancestorId).FirstOrDefaultAsync();

                Taxon taxon = new Taxon
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    Description = dto.Description,
                    ancestor = ancestor
                };
                var queryResult = await _bisonContext.Taxons.AddAsync(taxon); // does not write to the database!
            }
            await _bisonContext.SaveChangesAsync(); // persist the changes in the database

            List<int> ids = new List<int>();
            foreach (TaxonDto dto in taxons)
            {
                if (dto.Id != null)
                    ids.Add(dto.Id);
            }
            return ids;
        }

        public async Task<TaxonDto?> getTaxon(int taxonId)
        {
            // Define the query - with our setup, EF Core translates this to an SQLite query in the background
            var query = _bisonContext.Taxons
             .Where(taxon => taxon.Id == taxonId);
            // Execute the query and store the results
            var result = await query.SingleOrDefaultAsync();
            TaxonDto dto = new TaxonDto
            {
                Id = result.Id,
                Name = result.Name,
                Description = result.Description,
                ancestorId = result.ancestor.Id
            };
            return dto;
        }

        public async Task<List<TaxonDto>> getAllTaxons()
        {
            var query = _bisonContext.Taxons
            .Include(tax =>tax.ancestor);
            var QueryResult = await query.ToListAsync();


            List<TaxonDto> taxons = new List<TaxonDto>();
            foreach (Taxon taxon in QueryResult)
            {
                TaxonDto dto = new TaxonDto
                {
                    Id = taxon.Id,
                    Name = taxon.Name,
                    Description = taxon.Description,
                    ancestorId = taxon.ancestor.Id
                };
                taxons.Add(dto);
            }

            return taxons;
        }
    }
}