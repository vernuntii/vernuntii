using Vernuntii.Git;

namespace Vernuntii.Extensions.Configurers
{
    internal class GitConfigurer
    {
        public IServiceProvider ServiceProvider { get; }
        public IRepository Repository { get; }

        public GitConfigurer(IServiceProvider serviceProvider, IRepository repository)
        {
            Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }
    }
}
