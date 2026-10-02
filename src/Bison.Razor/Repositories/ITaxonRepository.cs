using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bison.Razor.Repositories
{
    public interface ITaxonRepository
    {
        public Task<List<int>> CreateTaxons(List<Taxon> taxons);
        public Task<Taxon> getTaxon(int taxonId);

        public Task<int> nextId();

    }
}