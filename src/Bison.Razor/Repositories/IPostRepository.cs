using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bison.Razor.DTOs;

namespace Bison.Razor.Repositories
{
    public class TestDTO // for testing!
    {
        public TestDTO(string text, DateTime t) // DateTime is a no no "Usually, it is advisable that DTOs consist of fields of predefined types only, i.e., int, strings, etc. DateTime is not a predefined type"
        {
        }
    }
    public interface IPostRepository
    {
        public Task<List<int>> CreateObservations(List<Observation> observations);
        public Task<List<ObservationDto>> ReadObservations(int authorID);
        public Task<List<ObservationDto>> ReadAllObservations();

        public Task<List<int>> CreateComments(List<Comment> comments);
        public Task<List<Comment>> ReadComments(int obsID);
        public Task<List<int>> CreateProposals(List<Proposal> proposals);
        public Task<List<Proposal>> ReadProposals(int obsID);
    }
}
