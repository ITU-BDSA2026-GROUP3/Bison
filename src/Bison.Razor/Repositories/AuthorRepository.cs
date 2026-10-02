

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Bison.Razor.Repositories
{


    public class AuthorRepository : IAuthorRepository
    {
        private readonly BisonContext _bisonContext;
        public AuthorRepository(BisonContext bisonContext)
        {
            _bisonContext = bisonContext;
        }

        public async Task<List<int>> CreateAuthors(List<Author> authors)
        {
            foreach(Author author in authors)
            {
                var queryResult = await _bisonContext.Authors.AddAsync(author); // does not write to the database!
            }
            await _bisonContext.SaveChangesAsync(); // persist the changes in the database

            // count the ideas after the db has been updated
            List<int> ids = new List<int>();
            foreach (Author author in authors)
            {
                ids.Add(author.Id);
            }
            return ids;
        }

        public async Task<Author> getAuthor(int authorID)
        {
            // Define the query - with our setup, EF Core translates this to an SQLite query in the background
            var query = _bisonContext.Authors
             .Where(author => author.Id == authorID);
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
