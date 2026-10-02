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
    public interface IAuthorRepository
    {
        public async Task<int> CreateAuthors(List<Author> authors);
        public async Task<Author> getAuthors(int authorID);

        public async Task<Author> getAuthors(string authorEmail);
        public async Task<List<Author>> getAuthors(string authorName);


        public Task<int> nextId();

    }
}