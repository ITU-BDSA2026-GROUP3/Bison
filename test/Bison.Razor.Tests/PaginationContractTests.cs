using System.Reflection;

namespace Bison.Razor.Tests;

public class PaginationContractTests
{
    [Fact]
    public void ObservationServiceSupportsPublicTimelinePagination()
    {
        MethodInfo? method = typeof(IObservationService).GetMethod("GetObservations",new[] { typeof(int) });
        Assert.NotNull(method);
        Assert.Equal(
            typeof(Task<List<ObservationViewModel>>),
            method.ReturnType);
    }

    [Fact]
    public void ObservationServiceSupportsUserTimelinePagination()
    {
        MethodInfo? method = typeof(IObservationService).GetMethod(
            "GetObservationsFromAuthor",
            new[] { typeof(int), typeof(int) });

        Assert.NotNull(method);
        Assert.Equal(
            typeof(Task<List<ObservationViewModel>>),
            method.ReturnType);
    }

    [Fact]
    public void DatabaseFacadeSupportsPublicTimelinePagination()
    {
        MethodInfo? method = typeof(DBFacade).GetMethod("ReadObservations", new[] { typeof(int), typeof(int) });
        Assert.NotNull(method);
        Assert.Equal(typeof(List<object[]>),method.ReturnType);
    }

    [Fact]
    public void DatabaseFacadeSupportsUserTimelinePagination()
    {
        MethodInfo? method = typeof(DBFacade).GetMethod("ReadObservationsFromAuthor",new[] { typeof(string), typeof(int), typeof(int) });
        Assert.NotNull(method);
        Assert.Equal(typeof(List<object[]>),method.ReturnType);
    }
}