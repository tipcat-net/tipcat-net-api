using Microsoft.EntityFrameworkCore;
using Moq;
using TipCatDotNet.Api.Data;

namespace TipCatDotNet.ApiTests.Utils;

public static class MockContextFactory
{
    public static Mock<AetherDbContext> Create()
    {
        return new Mock<AetherDbContext>(new DbContextOptions<AetherDbContext>());
    }
}