

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
        public PostRepository(BisonContext bisonContext)
        {
            _bisonContext = bisonContext;
        }

        public async Task<int> CreateAuthors(List<Author> authors)
        {
            List<int> ids = new List<int>();
            foreach(Author author in authors)
            {
                var queryResult = await _bisonContext.authors.AddAsync(author); // does not write to the database!
                ids.Add(queryResult.Entity.Id);
            }
            await _bisonContext.SaveChangesAsync(); // persist the changes in the database
            return ids;
        }

        public async Task<List<Author>> getAuthors(int authorID)
        {
            // Define the query - with our setup, EF Core translates this to an SQLite query in the background
            var query = _bisonContext.Authors
             .Where(obs => obs.Author.Id == authorID);
            // Execute the query and store the results
            var result = await query.ToListAsync();

            return result;
        }

        public async Task<List<Author>> getAuthors(string authorEmail)
        {
            // Define the query - with our setup, EF Core translates this to an SQLite query in the background
            var query = _bisonContext.Authors
             .Where(obs => obs.Author.Email == authorEmail);
            // Execute the query and store the results
            var result = await query.ToListAsync();

            return result;
        }

        public async Task<List<Author>> getAuthors(string authorName)
        {
            // Define the query - with our setup, EF Core translates this to an SQLite query in the background
            var query = _bisonContext.Authors
             .Where(obs => obs.Author.Name == authorName);
            // Execute the query and store the results
            var result = await query.ToListAsync();

            return result;
        }
        
        public async Task<int> nextId()
        {
            // needs to be properly implementet
            return 1000000000;
        }
    }
}
