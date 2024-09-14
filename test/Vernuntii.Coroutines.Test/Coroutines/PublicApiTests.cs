using PublicApiGenerator;

namespace Vernuntii.Coroutines;

public class PublicApiTests
{
    [Fact]
    public Task CoroutinesAssembly_UsesShippedPublicApi()
    {
        var assembly = typeof(Coroutine).Assembly;
        var publicApi = assembly.GeneratePublicApi();
        return Verify(publicApi);
    }
}
