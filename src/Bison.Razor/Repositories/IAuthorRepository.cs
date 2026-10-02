using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bison.Razor.Repositories
{
    public interface IAuthorRepository
    {
        public Task<List<int>> CreateAuthors(List<Author> authors);
        public Task<Author> getAuthor(int authorID);

        public Task<int> nextId();

    }
}