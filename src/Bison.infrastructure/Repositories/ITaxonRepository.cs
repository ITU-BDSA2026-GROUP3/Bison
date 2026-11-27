using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bison.infrastructure.DTOs;

namespace Bison.infrastructure.Repositories
{
    public interface ITaxonRepository
    {
        public Task<List<int>> CreateTaxons(List<TaxonDto> taxons);
        public Task<TaxonDto?> getTaxon(int taxonId);
        public Task<List<TaxonDto>> getAllTaxons();
    }
}