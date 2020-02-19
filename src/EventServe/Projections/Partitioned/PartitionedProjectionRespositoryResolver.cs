using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventServe.Projections.Partitioned
{
      public class PartitionedProjectionRespositoryResolver : IPartitionedProjectionRepositoryResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public PartitionedProjectionRespositoryResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task<IPartitionedProjectionStateRepository> Resolve()
        {
            try
            {
                var repo = (IPartitionedProjectionStateRepository)_serviceProvider.GetService(typeof(IPartitionedProjectionStateRepository));
                return Task.FromResult(repo);
            }
            catch(Exception e)
            {
                return null;
            }

        }
    }
}
