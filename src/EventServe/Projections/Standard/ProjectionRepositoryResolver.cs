using EventServe.Projections.Standard;
using System;
using System.Threading.Tasks;

namespace EventServe.Projections
{
    public class ProjectionRepositoryResolver : IProjectionRepositoryResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public ProjectionRepositoryResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task<IProjectionStateRepository> Resolve()
        {
            return Task.FromResult((IProjectionStateRepository)_serviceProvider.GetService(typeof(IProjectionStateRepository)));
        }
    }
}
