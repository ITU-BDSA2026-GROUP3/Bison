using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bison.Core.domain_model;

namespace Bison.infrastructure.Repositories
{
    public interface IAuthorRepository
    {
        public Task<List<string>> CreateAuthors(List<Author> authors);
        public Task<Author?> getAuthor(string authorID);
        public Task AnonymizeAuthor(string authorID);

    }
}