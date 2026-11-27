using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bison.infrastructure.DTOs;

namespace Bison.infrastructure.Repositories
{
    public interface IAuthorRepository
    {
        public Task<List<string>> CreateAuthors(List<AuthorDto> authors);
        public Task<AuthorDto> getAuthor(string authorID);
        public Task AnonymizeAuthor(string authorID);

    }
}