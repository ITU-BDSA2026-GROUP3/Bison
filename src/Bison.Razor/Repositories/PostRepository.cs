using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Bison.Razor.DTOs;
using Bison.Razor.domain_model;

namespace Bison.Razor.Repositories
{


    public class PostRepository : IPostRepository
    {
        private readonly BisonContext _bisonContext;
        public PostRepository(BisonContext bisonContext)
        {
            _bisonContext = bisonContext;
        }

        public async Task<List<Observation>> ReadObservations(string authorID)
        {
            // Define the query - with our setup, EF Core translates this to an SQLite query in the background
            var query = _bisonContext.Observations
             .Include(obs => obs.Author)
             .Include(obs => obs.Taxon) // eager loading, connects the foreign keys in the database to the corresponding entity, otherwise the returning field would be null.
             .Where(obs => !obs.IsDeleted && obs.Author != null && obs.Author.Id == authorID); // filter out deleted observations and only include observations from the specified author

            // Execute the query and store the results

            var result = await query.ToListAsync();
            return result;
        }
        public async Task<List<Observation>> ReadAllObservations()
        {
            // Define the query - with our setup, EF Core translates this to an SQLite query in the background
            var query = _bisonContext.Observations
             .Include(obs => obs.Author)
             .Include(obs => obs.Taxon) // eager loading, connects the foreign keys in the database to the corresponding entity, otherwise the returning field would be null.
             .Where(obs => !obs.IsDeleted); // filter out deleted observations

            // Execute the query and store the results
            var result = await query.ToListAsync();
            return result;
        }

        public async Task HandleForgottenUserObservations(string authorID, bool keepObservations)
        {
            var observations = await _bisonContext.Observations
                .Where(observation =>
                    observation.Author != null &&
                    observation.Author.Id == authorID)
                .ToListAsync();

            foreach (var observation in observations)
            {
                if (keepObservations)
                {
                    observation.Author = null;
                }
                else
                {
                    observation.IsDeleted = true;
                }
            }

            await _bisonContext.SaveChangesAsync();
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
        public async Task<Observation?> ReadSingleObservation(int observationID)
        {
            var query = _bisonContext.Observations
             .Include(obs => obs.Author)
             .Include(obs => obs.Taxon)
             .Where(obs => obs.Id == observationID && !obs.IsDeleted);
            // Execute the query and store the results
            var result = await query.SingleOrDefaultAsync();
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
                .Where(comment => comment.Observation.Id == obsID && !comment.IsDeleted);
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
                .Where(proposal => proposal.Observation.Id == obsID && !proposal.IsDeleted);
            // Execute the query and store the results
            var result = await query.ToListAsync();

            return result;
        }
    }
}
