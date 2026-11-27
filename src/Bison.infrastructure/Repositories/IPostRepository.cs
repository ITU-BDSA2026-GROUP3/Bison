using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bison.infrastructure.DTOs;

namespace Bison.infrastructure.Repositories
{
    public interface IPostRepository
    {
        public Task<List<int>> CreateObservations(List<ObservationDto> observations);

        public Task<ObservationDto?> ReadSingleObservation(int observationID);
        public Task<List<ObservationDto>> ReadObservations(string authorID);
        public Task<List<ObservationDto>> ReadAllObservations();
        public Task HandleForgottenUserObservations(string authorID, bool keepObservations);
        public Task<List<int>> CreateComments(List<CommentDto> comments);
        public Task<List<CommentDto>> ReadComments(int obsID);
        public Task<List<int>> CreateProposals(List<ProposalDto> proposals);
        public Task<List<ProposalDto>> ReadProposals(int obsID);
    }
}
