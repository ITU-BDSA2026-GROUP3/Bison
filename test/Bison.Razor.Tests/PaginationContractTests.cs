using Bison.infrastructure.DTOs;
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
            typeof(Task<List<ObservationDto>>),
            method.ReturnType);
    }

    [Fact]
    public void ObservationServiceSupportsUserTimelinePagination()
    {
        MethodInfo? method = typeof(IObservationService).GetMethod(
            "GetObservationsFromAuthor",
            new[] { typeof(string), typeof(int) });

        Assert.NotNull(method);
        Assert.Equal(
            typeof(Task<List<ObservationDto>>),
            method.ReturnType);
    }
}