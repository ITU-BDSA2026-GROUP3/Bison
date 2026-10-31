using Bison.Razor.domain_model;
using Bison.Razor.DTOs;
using Bison.Razor.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Bison.Razor.Tests;

[Collection("Sequential Tests")]
public class ObservationServiceUnitTests : IDisposable
{
    IObservationService service;
    IPostRepository _postRepository;
    IAuthorRepository _authorRepository;
    ITaxonRepository _taxonRepository;

    public ObservationServiceUnitTests()
    {
        _postRepository = new FakePostRepository();
        _authorRepository = new FakeAuthorRepository();
        _taxonRepository = new FakeTaxonRepository();

        service = new ObservationService(_postRepository, _authorRepository, _taxonRepository);
    }

    [Fact]
    public async Task GetSingleObservationFromRepository()
    {
        //Arrange
        Author author = new Author{Name = "Emil"};
        Taxon taxon = new Taxon{Name = "Real Animal"};
        Observation obs = new Observation{Id = 1, Author = author, Taxon = taxon, Text = "Sej and der var ægte", TimeStamp = DateTime.Now};

        await _postRepository.CreateObservations([obs]);

        //Act
        var records = await service.GetObservations();

        //Assert
        Assert.NotEmpty(records);
        Assert.Single(records);

        var record = records[0];

        CompareObservations(obs, record);
    }

    [Fact]
    public async Task GetMultipleObservationsFromRepository()
    {
        //Arrange
        Author author = new Author{Name = "Emil"};
        Taxon taxon = new Taxon{Name = "Real Animal"};
        Observation obs1 = new Observation{Id = 1, Author = author, Taxon = taxon, Text = "Sej and der var ægte", TimeStamp = DateTime.Now};
        Observation obs2 = new Observation{Id = 2, Author = author, Taxon = taxon, Text = "Jeg så HC Andersen IN THE FLESH", TimeStamp = DateTime.Now};

        await _postRepository.CreateObservations([obs1, obs2]);

        //Act
        var records = await service.GetObservations();

        //Assert
        Assert.NotEmpty(records);
        Assert.Equal(2, records.Count);

        var record1 = Assert.Single(records.Where(o => o.Id == obs1.Id));
        var record2 = Assert.Single(records.Where(o => o.Id == obs2.Id));

        CompareObservations(obs1, record1);
        CompareObservations(obs2, record2);
    }

    [Fact]
    public async Task GetObservationsFromEmptyRepository()
    {
        //Arrange

        //Act
        var records = await service.GetObservations();

        //Assert
        Assert.Empty(records);
    }

    [Fact]
    public async Task GetObservationsFromAuthor()
    {
        //Arrange
        Author author1 = new Author{Name = "Emil", Id = 0};
        Author author2 = new Author{Name = "Kemil (The evil version of Emil that likes Chemestry (VERY DANGEROUS!!!))", Id = 1};
        Taxon taxon = new Taxon{Name = "Real Animal"};
        Observation obs1 = new Observation{Id = 1, Author = author1, Taxon = taxon, Text = "Jeg hader kemi :(", TimeStamp = DateTime.Now};
        Observation obs2 = new Observation{Id = 2, Author = author2, Taxon = taxon, Text = "Jeg ELSKER KEMI", TimeStamp = DateTime.Now};
        Observation obs3 = new Observation{Id = 3, Author = author1, Taxon = taxon, Text = "En sand tragedie for menneskeheden", TimeStamp = DateTime.Now};

        await _postRepository.CreateObservations([obs1, obs2, obs3]);

        //Act
        var records = await service.GetObservationsFromAuthor(0);


        //Assert
        Assert.NotEmpty(records);
        Assert.Equal(2, records.Count);

        var record1 = Assert.Single(records.Where(o => o.Id == obs1.Id));
        var record2 = Assert.Single(records.Where(o => o.Id == obs3.Id));

        CompareObservations(obs1, record1);
        CompareObservations(obs3, record2);
    }

    [Fact]
    public async Task GetObservationsFromNonExistingAuthor()
    {
        //Arrange
        Author author2 = new Author{Name = "Kemil (The evil version of Emil that likes Chemestry (VERY DANGEROUS!!!))", Id = 1};
        Taxon taxon = new Taxon{Name = "Real Animal"};
        Observation obs1 = new Observation{Id = 1, Author = author2, Taxon = taxon, Text = "Jeg ELSKER KEMI", TimeStamp = DateTime.Now};

        await _postRepository.CreateObservations([obs1]);

        //Act
        var records = await service.GetObservationsFromAuthor(0);

        //Assert
        Assert.Empty(records);
    }

    [Fact]
    public async Task GetObservationById()
    {
        //Arrange
        Author author = new Author{Name = "Emil"};
        Taxon taxon = new Taxon{Name = "Real Animal"};
        Observation obs1 = new Observation{Id = 1, Author = author, Taxon = taxon, Text = "jeg er den eneste der har observeret noget nogensinde", TimeStamp = DateTime.Now};

        await _postRepository.CreateObservations([obs1]);

        //Act
        var records = await service.GetObservationFromId(obs1.Id);

        //Assert
        Assert.NotNull(records);
        CompareObservations(obs1, records);
    }

    [Fact]
    public async Task GetObservationByInvalidId()
    {
        //Arrange
        Author author2 = new Author{Name = "Kemil (The evil version of Emil that likes Chemestry (VERY DANGEROUS!!!))", Id = 1};
        Taxon taxon = new Taxon{Name = "Real Animal"};
        Observation obs1 = new Observation{Id = 1, Author = author2, Taxon = taxon, Text = "Jeg ELSKER KEMI", TimeStamp = DateTime.Now};

        await _postRepository.CreateObservations([obs1]);

        //Act
        var records = await service.GetObservationFromId(-3);

        //Assert
        Assert.Null(records);
    }

    [Fact]
    public async Task GetObservationsByTaxon()
    {
        //Arrange
        Author author1 = new Author{Name = "Emil", Id = 1};
        Author author2 = new Author{Name = "Hemmelig mand", Id = 2};
        Taxon taxon1 = new Taxon{Name = "Real Animal", Id = 0};
        Taxon taxon2 = new Taxon{Name = "Fake disgusting Animal", Id = 1};
        Observation obs1 = new Observation{Id = 1, Author = author1, Taxon = taxon1, Text = "Sejt ægte dyr", TimeStamp = DateTime.Now};
        Observation obs2 = new Observation{Id = 2, Author = author2, Taxon = taxon1, Text = "Dette dyr vækker noget i mig som jeg aldrig har følt før", TimeStamp = DateTime.Now};
        Observation obs3 = new Observation{Id = 3, Author = author1, Taxon = taxon2, Text = "Sejt falsk dyr", TimeStamp = DateTime.Now};

        await _postRepository.CreateObservations([obs1, obs2, obs3]);
        await _taxonRepository.CreateTaxons([taxon1, taxon2]);

        //Act
        var records = await service.GetObservationsFromTaxon(taxon1.Id);

        //Assert
        Assert.NotEmpty(records);
        Assert.Equal(2, records.Count);

        var record1 = Assert.Single(records.Where(o => o.Id == obs1.Id));
        var record2 = Assert.Single(records.Where(o => o.Id == obs2.Id));

        CompareObservations(obs1, record1);
        CompareObservations(obs2, record2);
    }

    [Fact]
    public async Task GetObservationsByAncestorTaxon()
    {
        //Arrange
        Author author1 = new Author{Name = "Emil", Id = 1};
        Author author2 = new Author{Name = "Hemmelig mand", Id = 2};
        Taxon taxon1 = new Taxon{Name = "The father of all animals", Id = 0};
        Taxon taxon2 = new Taxon{Name = "the child of the father of all animals", Id = 1, ancestor = taxon1};
        Taxon taxon3 = new Taxon{Name = "rock pretending to be an animal to infiltrate the kingdom and weaken their forces from the inside", Id = 2};
        Observation obs1 = new Observation{Id = 1, Author = author1, Taxon = taxon1, Text = "kæmpe daddy", TimeStamp = DateTime.Now};
        Observation obs2 = new Observation{Id = 2, Author = author2, Taxon = taxon2, Text = "crazy barn", TimeStamp = DateTime.Now};
        Observation obs3 = new Observation{Id = 3, Author = author1, Taxon = taxon3, Text = "Halløj agent Sten. Din mission er at infiltrere dyrenes kongerige og skabe kaos", TimeStamp = DateTime.Now};

        await _postRepository.CreateObservations([obs1, obs2, obs3]);
        await _taxonRepository.CreateTaxons([taxon1, taxon2, taxon3]);

        //Act
        var records = await service.GetObservationsFromTaxon(taxon1.Id);

        //Assert
        Assert.NotEmpty(records);
        Assert.Equal(2, records.Count);

        var record1 = Assert.Single(records.Where(o => o.Id == obs1.Id));
        var record2 = Assert.Single(records.Where(o => o.Id == obs2.Id));

        CompareObservations(obs1, record1);
        CompareObservations(obs2, record2);
    }

    [Fact]
    public async Task GetSingleCommentOnObservation()
    {
        //Arrange
        Author author1 = new Author{Name = "Emil"};
        Author author2 = new Author{Name = "Jonas"};
        Taxon taxon = new Taxon{Name = "Real Animal"};
        Observation obs1 = new Observation{Id = 0, Author = author1, Taxon = taxon, Text = "Prøv at se den her abe jeg lige så", TimeStamp = DateTime.Now};
        Comment com = new Comment{Author = author2, Id = 0, Observation = obs1, Text = "L abe + jeg framemogger dig + aben ligner dig + downvoter alle dine posts + sletter din account"};
        Observation obs2 = new Observation{Id = 1, Author = author2, Taxon = taxon, Text = "Ødelagde lige ham her gutten i hans kommentarfelt", TimeStamp = DateTime.Now};

        await _postRepository.CreateObservations([obs1, obs2]);
        await _postRepository.CreateComments([com]);

        //Act
        var comments = await service.GetComments(obs1.Id);

        //Assert
        Assert.NotEmpty(comments);
        Assert.Single(comments);

        var comment = comments[0];
        CompareComments(com, comment);
    }

    
    //NEEDS LOGIC WHEN INSERTING COMMENTS TO WORK. CAN'T CURRENTLY TEST :(
    /*[Fact]
    public void GetCommentsOnNonExistingObservation()
    {
        //Arrange
        var com = new CommentViewModel(0L, "I COMMENTED HIHIIHIHIHIH");
        DatabaseHandler.AddComment(com);

        //Act
        var comments = service.GetComments(0L);

        //Assert
        Assert.Empty(comments);
    }*/

    [Fact]
    public async Task GetMultipleCommentsOnObservation()
    {
        //Arrange
        Author author1 = new Author{Name = "Emil"};
        Author author2 = new Author{Name = "Jonas"};
        Taxon taxon = new Taxon{Name = "Real Animal"};
        Observation obs1 = new Observation{Id = 0, Author = author1, Taxon = taxon, Text = "Prøv at se den her abe jeg lige så", TimeStamp = DateTime.Now};
        Comment com1 = new Comment{Author = author2, Id = 0, Observation = obs1, Text = "L abe + jeg framemogger dig + aben ligner dig + downvoter alle dine posts + sletter din account"};
        Comment com2 = new Comment{Author = author2, Id = 1, Observation = obs1, Text = "+ goofy"};
        Observation obs2 = new Observation{Id = 1, Author = author2, Taxon = taxon, Text = "Ødelagde lige ham her gutten i hans kommentarfelt", TimeStamp = DateTime.Now};

        await _postRepository.CreateObservations([obs1, obs2]);
        await _postRepository.CreateComments([com1, com2]);

        //Act
        var records = await service.GetComments(obs1.Id);

        //Assert
        Assert.NotEmpty(records);
        Assert.Equal(2, records.Count);

        var record1 = Assert.Single(records.Where(o => o.Text == com1.Text));
        var record2 = Assert.Single(records.Where(o => o.Text == com2.Text));

        CompareComments(com1, record1);
        CompareComments(com2, record2);
    }

    [Fact]
    public async Task GetSingleProposalsOnObservation()
    {
        //Arrange
        Author author = new Author{Name = "Emil"};
        Taxon taxon = new Taxon{Name = "Real Animal"};
        Observation obs1 = new Observation{Id = 0, Author = author, Taxon = taxon, Text = "Sej and der var ægte", TimeStamp = DateTime.Now};
        Observation obs2 = new Observation{Id = 1, Author = author, Taxon = taxon, Text = "sikke en sjov tur på papmuseet", TimeStamp = DateTime.Now};
        Proposal prop = new Proposal{Id = 0, Author = author, Observation = obs1, Taxon = taxon, Text = "den var lavet af pap. er du blind? eller bare dum?", TimeStamp = DateTime.Now};

        await _postRepository.CreateObservations([obs1, obs2]);
        await _postRepository.CreateProposals([prop]);

        //Act
        var proposals = await service.GetProposals(obs1.Id);

        //Assert
        Assert.NotEmpty(proposals);
        Assert.Single(proposals);

        var proposal = proposals[0];

        CompareProposals(prop, proposal);
    }

    //NEEDS LOGIC WHEN INSERTING PROPOSALS TO WORK. CAN'T CURRENTLY TEST :(
    /*[Fact]
    public void GetProposalsOnNonExistingObservation()
    {
        //Arrange


        //Act


        //Assert
        
    }*/

    [Fact]
    public async Task GetMultipleProposalsOnObservation()
    {
        //Arrange
        Author author = new Author{Name = "Emil"};
        Taxon taxon = new Taxon{Name = "Real Animal"};
        Observation obs1 = new Observation{Id = 0, Author = author, Taxon = taxon, Text = "Sej and der var ægte", TimeStamp = DateTime.Now};
        Observation obs2 = new Observation{Id = 1, Author = author, Taxon = taxon, Text = "sikke en sjov tur på papmuseet", TimeStamp = DateTime.Now};
        Proposal prop1 = new Proposal{Id = 0, Author = author, Observation = obs1, Taxon = taxon, Text = "den var lavet af pap. er du blind? eller bare dum?", TimeStamp = DateTime.Now};
        Proposal prop2 = new Proposal{Id = 1, Author = author, Observation = obs1, Taxon = taxon, Text = "den er meget flot, men det er altså ikke en rigtig and", TimeStamp = DateTime.Now};

        await _postRepository.CreateObservations([obs1, obs2]);
        await _postRepository.CreateProposals([prop1, prop2]);

        //Act
        var records = await service.GetProposals(obs1.Id);

        //Assert
        Assert.NotEmpty(records);
        Assert.Equal(2, records.Count);

        var record1 = Assert.Single(records.Where(o => o.Text == prop1.Text));
        var record2 = Assert.Single(records.Where(o => o.Text == prop2.Text));

        CompareProposals(prop1, record1);
        CompareProposals(prop2, record2);
    }


    private void CompareObservations(Observation obs1, ObservationDto obs2)
    {
        Assert.NotNull(obs1);
        Assert.NotNull(obs2);

        Assert.Equal(obs1.Id, obs2.Id);
        Assert.Equal(obs1.Author.Name, obs2.AuthorName);
        Assert.Equal(obs1.Text, obs2.Text);
        Assert.Equal(obs1.TimeStamp.ToString("MM/dd/yy H:mm:ss"), obs2.Timestamp);
        Assert.Equal(obs1.Taxon.Name, obs2.TaxonName);
    }

    private void CompareComments(Comment com1, CommentDto com2)
    {
        Assert.NotNull(com1);
        Assert.NotNull(com2);

        Assert.Equal(com1.Text, com2.Text);
        Assert.Equal(com1.Author.Name, com2.AuthorName);
        Assert.Equal(com1.TimeStamp.ToString("MM/dd/yy H:mm:ss"), com2.Timestamp);
    }

    private void CompareProposals(Proposal prop1, ProposalDto prop2)
    {
        Assert.NotNull(prop1);
        Assert.NotNull(prop2);

        Assert.Equal(prop1.Observation.Id, prop2.Id);
        Assert.Equal(prop1.Taxon.Name, prop2.TaxonName);
        Assert.Equal(prop1.Text, prop2.Text);
        Assert.Equal(prop1.Author.Name, prop2.AuthorName);
        Assert.Equal(prop1.TimeStamp.ToString("MM/dd/yy H:mm:ss"), prop2.Timestamp);
    }

    private void CompareAuthors(Author auth1, Author auth2)
    {
        Assert.NotNull(auth1);
        Assert.NotNull(auth2);

        Assert.Equal(auth1.Name, auth2.Name);
        Assert.Equal(auth1.Email, auth2.Email);
        Assert.Equal(auth1.Id, auth2.Id);
    }

    public void Dispose()
    {
        
    }

    //---------------------------- Fake Repository Classes --------------------------------------------------------------------------------------------------------------------------------

    private class FakePostRepository : IPostRepository
    {
        List<Observation> Observations;
        List<Comment> Comments;
        List<Proposal> Proposals;

        public FakePostRepository()
        {
            Observations = new List<Observation>();
            Comments = new List<Comment>();
            Proposals = new List<Proposal>();
        }

        public Task<List<int>> CreateObservations(List<Observation> observations)
        {
            foreach(Observation observation in observations)
            {
                Observations.Add(observation);
            }

            return Task.FromResult(new List<int>());
        }

        public Task<Observation> ReadSingleObservation(int observationID)
        {
            var observation = Observations.FirstOrDefault(o => o.Id == observationID);

            return Task.FromResult(observation);
        }
        public Task<List<Observation>> ReadObservations(int authorID)
        {
            List<Observation> authorvations = new List<Observation>();
            foreach(Observation observation in Observations)
            {
                if(observation.Author.Id == authorID) authorvations.Add(observation);
            }
            return Task.FromResult(authorvations);
        }
        public Task<List<Observation>> ReadAllObservations()
        {
            return Task.FromResult(Observations);
        }

        public Task<List<int>> CreateComments(List<Comment> comments)
        {
            foreach(Comment comment in comments)
            {
                Comments.Add(comment);
            }

            return Task.FromResult(new List<int>());
        }
        public Task<List<Comment>> ReadComments(int obsID)
        {
            List<Comment> comments = new List<Comment>();
            foreach(Comment comment in Comments)
            {
                if(comment.Observation.Id == obsID) comments.Add(comment);
            }
            return Task.FromResult(comments);
        }
        public Task<List<int>> CreateProposals(List<Proposal> proposals)
        {
            foreach(Proposal proposal in proposals)
            {
                Proposals.Add(proposal);
            }

            return Task.FromResult(new List<int>());
        }
        public Task<List<Proposal>> ReadProposals(int obsID)
        {
            List<Proposal> proposals = new List<Proposal>();
            foreach(Proposal proposal in Proposals)
            {
                if(proposal.Observation.Id == obsID) proposals.Add(proposal);
            }
            return Task.FromResult(proposals);
        }
    }

    private class FakeAuthorRepository : IAuthorRepository
    {
        List<Author> Authors;

        public FakeAuthorRepository()
        {
            Authors = new List<Author>();
        }

        public Task<List<int>> CreateAuthors(List<Author> authors)
        {
            foreach(Author author in authors)
            {
                Authors.Add(author);
            }

            return Task.FromResult(new List<int>());
        }
        public Task<Author> getAuthor(int authorID)
        {
            var author = Authors.FirstOrDefault(o => o.Id == authorID);

            return Task.FromResult(author);
        }
    }

    private class FakeTaxonRepository : ITaxonRepository
    {
        List<Taxon> Taxons;

        public FakeTaxonRepository()
        {
            Taxons = new List<Taxon>();
        }
        
        public Task<List<int>> CreateTaxons(List<Taxon> taxons)
        {
            foreach(Taxon taxon in taxons)
            {
                Taxons.Add(taxon);
            }

            return Task.FromResult(new List<int>());
        }
        public Task<Taxon> getTaxon(int taxonId)
        {
            var taxon = Taxons.FirstOrDefault(o => o.Id == taxonId);

            return Task.FromResult(taxon);
        }
        public Task<List<Taxon>> getAllTaxons()
        {
            return Task.FromResult(Taxons);
        }
    }
}