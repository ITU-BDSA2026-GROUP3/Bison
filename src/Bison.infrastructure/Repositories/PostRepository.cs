using Bison.Core.domain_model;
using Bison.infrastructure.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Net.Mime.MediaTypeNames;

namespace Bison.infrastructure.Repositories
{


    public class PostRepository : IPostRepository
    {
        private readonly BisonContext _bisonContext;
        public PostRepository(BisonContext bisonContext)
        {
            _bisonContext = bisonContext;
        }

        public async Task<List<ObservationDto>> ReadObservations(string authorID)
        {
            // Define the query - with our setup, EF Core translates this to an SQLite query in the background
            var query = _bisonContext.Observations
             .Include(obs => obs.Author)
             .Include(obs => obs.Taxon) // eager loading, connects the foreign keys in the database to the corresponding entity, otherwise the returning field would be null.
             .Where(obs => !obs.IsDeleted && obs.Author != null && obs.Author.Id == authorID); // filter out deleted observations and only include observations from the specified author

            // Execute the query and store the results

            var QueryResult = await query.ToListAsync();
            List<ObservationDto> observations = new List<ObservationDto>();
            foreach (var obs in QueryResult)
            {
                ObservationDto dto = new ObservationDto
                {
                    Id = obs.Id,
                    AuthorName = obs.Author?.UserName ?? "deleted user",
                    Text = obs.Text,
                    Timestamp = obs.TimeStamp.ToString("MM/dd/yy H:mm:ss"),
                    ImgBytes = obs.ImgBytes, //imgbytes can be null in that case handle it the same as if there is no bytes(very unlikely to ever happen) 
                    Licence = obs.Licence,
                    TaxonName = obs.Taxon.Name
                };
                observations.Add(dto);
            }

            return observations;
        }
        public async Task<List<ObservationDto>> ReadAllObservations()
        {
            // Define the query - with our setup, EF Core translates this to an SQLite query in the background
            var query = _bisonContext.Observations
             .Include(obs => obs.Author)
             .Include(obs => obs.Taxon) // eager loading, connects the foreign keys in the database to the corresponding entity, otherwise the returning field would be null.
             .Where(obs => !obs.IsDeleted); // filter out deleted observations

            // Execute the query and store the results
            var QueryResult = await query.ToListAsync();
            List<ObservationDto> observations = new List<ObservationDto>();
            foreach (var obs in QueryResult)
            {
                ObservationDto dto = new ObservationDto
                {
                    Id = obs.Id,
                    AuthorName = obs.Author?.UserName ?? "deleted user",
                    Text = obs.Text,
                    Timestamp = obs.TimeStamp.ToString("MM/dd/yy H:mm:ss"),
                    ImgBytes = obs.ImgBytes, //imgbytes can be null in that case handle it the same as if there is no bytes(very unlikely to ever happen) 
                    Licence = obs.Licence,
                    TaxonName = obs.Taxon.Name
                };
                observations.Add(dto);
            }

            return observations;
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
        // Please be aware that if the DTO AuthorName or TaxonName does not exist, they will be saved as null
        public async Task<List<int>> CreateObservations(List<ObservationDto> observations)
        {
            List<int> ids = new List<int>();
            foreach (ObservationDto dto in observations)
            {
                // We assume names are unique, and that is exists, yes this implementation feels stupid
                Author? author = await _bisonContext.Users
                .Where(author => author.UserName == dto.AuthorName).FirstOrDefaultAsync();

                Taxon taxon = await _bisonContext.Taxons
                .Where(taxon => taxon.Name == dto.TaxonName).FirstOrDefaultAsync();

                Observation obs = new Observation
                {
                    Text = dto.Text,
                    TimeStamp = DateTime.UtcNow,
                    Author = author,
                    Taxon = taxon,
                    ImgBytes = dto.ImgBytes,
                    Licence = dto.Licence,
                };

                var queryResult = await _bisonContext.Observations.AddAsync(obs); // does not write to the database!
                ids.Add(queryResult.Entity.Id);
            }
            await _bisonContext.SaveChangesAsync(); // persist the changes in the database
            return ids;
        }
        public async Task<ObservationDto?> ReadSingleObservation(int observationID)
        {
            var query = _bisonContext.Observations
             .Include(obs => obs.Author)
             .Include(obs => obs.Taxon)
             .Where(obs => obs.Id == observationID && !obs.IsDeleted);
            // Execute the query and store the results
            var result = await query.SingleOrDefaultAsync();
            ObservationDto dto = new ObservationDto
            {
                Id = result.Id,
                AuthorName = result.Author?.UserName ?? "deleted user",
                Text = result.Text,
                Timestamp = result.TimeStamp.ToString("MM/dd/yy H:mm:ss"),
                ImgBytes = result.ImgBytes, //imgbytes can be null in that case handle it the same as if there is no bytes(very unlikely to ever happen) 
                Licence = result.Licence,
                TaxonName = result.Taxon.Name
            };
            return dto;
        }

        public async Task<List<int>> CreateComments(List<CommentDto> comments)
        {
            List<int> ids = new List<int>();
            foreach (CommentDto dto in comments)
            {
                Author? author = await _bisonContext.Users
                .Where(author => author.Id == dto.AuthorId).FirstOrDefaultAsync();

                Observation? obs = await _bisonContext.Observations
                .Where(obs => obs.Id == dto.obsId).FirstOrDefaultAsync();

                Comment comment = new Comment
                {
                    Observation = obs,
                    Text = dto.Text,
                    TimeStamp = DateTime.UtcNow,
                    Author = author,
                };
                var queryResult = await _bisonContext.Comments.AddAsync(comment); // does not write to the database!
                ids.Add(queryResult.Entity.Id);
            }
            await _bisonContext.SaveChangesAsync(); // persist the changes in the database
            return ids;
        }

        public async Task<List<CommentDto>> ReadComments(int obsID)
        {
            // Define the query - with our setup, EF Core translates this to an SQLite query in the background
            var query = _bisonContext.Comments
                .Include(comment => comment.Author)
                .Include(comment => comment.Observation)
                .Where(comment => comment.Observation.Id == obsID && !comment.IsDeleted);
            // Execute the query and store the results
            var QueryResult = await query.ToListAsync();

            List<CommentDto> comments = new List<CommentDto>();
            foreach (var comment in QueryResult)
            {
                CommentDto dto = new CommentDto
                {
                    obsId = comment.Observation.Id,
                    AuthorId = comment.Author.Id,
                    AuthorName = comment.Author.UserName,
                    Text = comment.Text,
                    Timestamp = comment.TimeStamp.ToString("MM/dd/yy H:mm:ss"),
                };

                comments.Add(dto);
            }

            return comments;
        }
        public async Task<List<int>> CreateProposals(List<ProposalDto> proposals)
        {
            List<int> ids = new List<int>();
            foreach (ProposalDto dto in proposals)
            {
                Author? author = await _bisonContext.Users
                .Where(author => author.Id == dto.AuthorId).FirstOrDefaultAsync();

                Observation? obs = await _bisonContext.Observations
                .Where(obs => obs.Id == dto.obsId).FirstOrDefaultAsync();

                Taxon taxon = await _bisonContext.Taxons
                .Where(taxon => taxon.Name == dto.TaxonName).FirstOrDefaultAsync();

                Proposal proposal = new Proposal
                {
                    Observation = obs,
                    Author = author,
                    Text = dto.Text,
                    TimeStamp = DateTime.UtcNow,
                    Taxon = taxon

                };
                var queryResult = await _bisonContext.Proposals.AddAsync(proposal); // does not write to the database!
                ids.Add(queryResult.Entity.Id);
            }
            await _bisonContext.SaveChangesAsync(); // persist the changes in the database
            return ids;
        }

        public async Task<List<ProposalDto>> ReadProposals(int obsID)
        {
            // Define the query - with our setup, EF Core translates this to an SQLite query in the background
            var query = _bisonContext.Proposals
                .Include(proposal => proposal.Author)
                .Include(proposal => proposal.Observation)
                .Include(proposal => proposal.Taxon)
                .Where(proposal => proposal.Observation.Id == obsID && !proposal.IsDeleted);
            // Execute the query and store the results
            var QueryResult = await query.ToListAsync();
            List<ProposalDto> proposals = new List<ProposalDto>();
            foreach (var proposal in QueryResult)
            {
                ProposalDto dto = new ProposalDto
                {
                    AuthorName = proposal.Author.UserName,
                    Text = proposal.Text,
                    Timestamp = proposal.TimeStamp.ToString("MM/dd/yy H:mm:ss"),
                    TaxonName = proposal.Taxon.Name,
                    Id = proposal.Observation.Id
                };

                proposals.Add(dto);
            }
            return proposals;
        }

        public async Task<List<ObservationDto>> ReadObservationsFromTaxon(int taxonID)
        {
            var query = _bisonContext.Observations
             .Include(obs => obs.Author)
             .Include(obs => obs.Taxon) // eager loading, connects the foreign keys in the database to the corresponding entity, otherwise the returning field would be null.
             .Where(obs => !obs.IsDeleted); // filter out deleted observations
            var QueryResult = await query.ToListAsync();

            var taxon = await _bisonContext.Taxons.Where(taxon => taxon.Id == taxonID).FirstOrDefaultAsync();
            List<ObservationDto> filteredObservations = new List<ObservationDto>();
            if (QueryResult.Count != 0 && taxon != null)
            {
                Observation[] observationsArray = QueryResult.ToArray();
                var filt = filterChecked.__default.FilterByChecked(taxon, Dafny.Sequence<Observation>.FromArray(observationsArray));

                foreach (var obs in filt)
                {
                    ObservationDto dto = new ObservationDto
                    {
                        Id = obs.Id,
                        AuthorName = obs.Author?.UserName ?? "deleted user",
                        Text = obs.Text,
                        Timestamp = obs.TimeStamp.ToString("MM/dd/yy H:mm:ss"),
                        ImgBytes = obs.ImgBytes, //imgbytes can be null in that case handle it the same as if there is no bytes(very unlikely to ever happen) 
                        TaxonName = obs.Taxon.Name,
                        Licence = obs.Licence
                    };

                    filteredObservations.Add(dto);
                }
            }
            return filteredObservations;
        }
    }
}
