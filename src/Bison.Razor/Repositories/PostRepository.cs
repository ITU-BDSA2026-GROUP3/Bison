using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Bison.Razor.DTOs;

namespace Bison.Razor.Repositories
{


    public class PostRepository : IPostRepository
    {
        private readonly BisonContext _bisonContext;
        public PostRepository(BisonContext bisonContext)
        {
            _bisonContext = bisonContext;
        }

        public async Task<List<Observation>> ReadObservations(int authorID)
        {
            // Define the query - with our setup, EF Core translates this to an SQLite query in the background
            var query = _bisonContext.Observations
             .Include(obs => obs.Author)
             .Include(obs => obs.Taxon) // eager loading, connects the foreign keys in the database to the corresponding entity, otherwise the returning field would be null.
             .Where(obs => obs.Author.Id == authorID);

            // Execute the query and store the results

            var result = await query.ToListAsync();
            return result;
        }
        public async Task<List<Observation>> ReadAllObservations()
        {
            // Define the query - with our setup, EF Core translates this to an SQLite query in the background
            var query = _bisonContext.Observations
             .Include(obs => obs.Author)
             .Include(obs => obs.Taxon); // eager loading, connects the foreign keys in the database to the corresponding entity, otherwise the returning field would be null.


            // Execute the query and store the results
            var result = await query.ToListAsync();

            return result;
        }

        public async Task<List<int>> CreateObservations(List<Observation> observations)
        {
            List<int> ids = new List<int>();
            foreach (Observation obs in observations)
            {
                var queryResult = await _bisonContext.Observations.AddAsync(obs); // does not write to the database!
                ids.Add(queryResult.Entity.Id);
            }
            await _bisonContext.SaveChangesAsync(); // persist the changes in the database
            return ids;
        }
        public async Task<Observation> ReadSingleObservation(int observationID)
        {
            var query = _bisonContext.Observations
             .Include(obs => obs.Author)
             .Include(obs => obs.Taxon)
             .Where(obs => obs.Id == observationID);
            // Execute the query and store the results
            var result = await query.SingleAsync();

            return result;
        }

        public async Task<List<int>> CreateComments(List<Comment> comments)
        {
            List<int> ids = new List<int>();
            foreach (Comment comment in comments)
            {
                var queryResult = await _bisonContext.Comments.AddAsync(comment); // does not write to the database!
                ids.Add(queryResult.Entity.Id);
            }
            await _bisonContext.SaveChangesAsync(); // persist the changes in the database
            return ids;
        }

        public async Task<List<Comment>> ReadComments(int obsID)
        {
            // Define the query - with our setup, EF Core translates this to an SQLite query in the background
            var query = _bisonContext.Comments
                .Include(comment => comment.Author)
                .Include(comment => comment.Observation)
                .Where(comment => comment.Observation.Id == obsID);
            // Execute the query and store the results
            var result = await query.ToListAsync();

            return result;
        }
        public async Task<List<int>> CreateProposals(List<Proposal> proposals)
        {
            List<int> ids = new List<int>();
            foreach (Proposal proposal in proposals)
            {
                var queryResult = await _bisonContext.Proposals.AddAsync(proposal); // does not write to the database!
                ids.Add(queryResult.Entity.Id);
            }
            await _bisonContext.SaveChangesAsync(); // persist the changes in the database
            return ids;
        }

        public async Task<List<Proposal>> ReadProposals(int obsID)
        {
            // Define the query - with our setup, EF Core translates this to an SQLite query in the background
            var query = _bisonContext.Proposals
                .Include(proposal => proposal.Author)
                .Include(proposal => proposal.Observation)
                .Include(proposal => proposal.Taxon)
                .Where(proposal => proposal.Observation.Id == obsID);
            // Execute the query and store the results
            var result = await query.ToListAsync();

            return result;
        }
    }
}
