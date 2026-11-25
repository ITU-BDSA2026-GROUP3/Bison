

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Bison.domain_model;

namespace Bison.infrastructure.Repositories
{


    public class AuthorRepository : IAuthorRepository
    {
        private readonly BisonContext _bisonContext;
        public AuthorRepository(BisonContext bisonContext)
        {
            _bisonContext = bisonContext;
        }

        public async Task<List<string>> CreateAuthors(List<Author> authors)
        {
            foreach(Author author in authors)
            {
                var queryResult = await _bisonContext.Users.AddAsync(author); // does not write to the database!
            }
            await _bisonContext.SaveChangesAsync(); // persist the changes in the database

            // count the ideas after the db has been updated
            List<string> ids = new List<string>();
            foreach (Author author in authors)
            {
                ids.Add(author.Id);
            }
            return ids;
        }

        public async Task<Author?> getAuthor(string authorID)
        {
            // Define the query - with our setup, EF Core translates this to an SQLite query in the background
            var query = _bisonContext.Users
             .Where(author => author.Id == authorID);
            // Execute the query and store the results
            var result = await query.SingleOrDefaultAsync(); //Temporary change to fix bug when calling the function with an ID that doesn't exist. Can now return null

            return result;
        }

        public async Task AnonymizeAuthor(string authorID)
        {
            var author = await _bisonContext.Users
                .SingleOrDefaultAsync(author => author.Id == authorID);

            if (author == null)
            {
                throw new InvalidOperationException("Author not found.");
            }

            var comments = await _bisonContext.Comments
                .Where(comment => comment.Author != null &&
                                  comment.Author.Id == authorID)
                .ToListAsync();

            var proposals = await _bisonContext.Proposals
                .Where(proposal => proposal.Author != null &&
                                   proposal.Author.Id == authorID)
                .ToListAsync();

            author.IsDeleted = true;
            author.UserName = $"deleted-{author.Id}";
            author.NormalizedUserName = $"DELETED-{author.Id}";
            author.Email = null;
            author.NormalizedEmail = null;
            author.PhoneNumber = null;

            foreach (var comment in comments)
            {
                comment.IsDeleted = true;
            }

            foreach (var proposal in proposals)
            {
                proposal.IsDeleted = true;
            }

            await _bisonContext.SaveChangesAsync();
        }
    }
}
