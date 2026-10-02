using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bison.Razor.Repositories
{
    public class TestDTO // for testing!
    {
        public TestDTO(string text, DateTime t)
        {
        }
    }
    public interface ITaxonRepository
    {
        public async Task<int> CreateTaxons(List<Taxon> taxons);
        public async Task<Author> getTaxons(int taxonId);
        public async Task<List<Author>> getTaxons(string taxonName);


        public Task<int> nextId();

    }
}