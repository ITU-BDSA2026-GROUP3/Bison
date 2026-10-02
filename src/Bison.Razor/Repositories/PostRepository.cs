using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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
             .Where(obs => obs.Author.Id == authorID);
            // Execute the query and store the results
            var result = await query.ToListAsync();

            return result;
            // converts result to DTO to be added later
            /*List<TestDTO> DTOs = new List<TestDTO>();
            foreach (var value in result)
            {
                DTOs.Add(new TestDTO(value.Text,value.TimeStamp));
            }
            return DTOs;*/
        }
        public async Task<List<Observation>> ReadAllObservations()
        {
            // Define the query - with our setup, EF Core translates this to an SQLite query in the background
            var query = _bisonContext.Observations;
            // Execute the query and store the results
            var result = await query.ToListAsync();

            return result;
            // converts result to DTO to be added later
            /*List<TestDTO> DTOs = new List<TestDTO>();
            foreach (var value in result)
            {
                DTOs.Add(new TestDTO(value.Text, value.TimeStamp));
            }
            return DTOs;*/
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
             .Where(comment => comment.Observation.Id == obsID);
            // Execute the query and store the results
            var result = await query.ToListAsync();

            return result;
            // converts result to DTO to be added later
            /*List<TestDTO> DTOs = new List<TestDTO>();
            foreach (var value in result)
            {
                DTOs.Add(new TestDTO(value.Text,value.TimeStamp));
            }
            return DTOs;*/
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
             .Where(proposal => proposal.Observation.Id == obsID);
            // Execute the query and store the results
            var result = await query.ToListAsync();

            return result;
            // converts result to DTO to be added later
            /*List<TestDTO> DTOs = new List<TestDTO>();
            foreach (var value in result)
            {
                DTOs.Add(new TestDTO(value.Text,value.TimeStamp));
            }
            return DTOs;*/
        }
        public async Task<int> nextId()
        {
            // needs to be properly implementet
            return 1000000000;
        }
    }
}
