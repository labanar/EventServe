using EventServe.Projections;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace EventServe.SampleApp.Infrastructure
{
    public class PartitionedProjectionStateRepository<T> : IPartitionedProjectionStateRepository<T>
        where T: PartitionedProjection
    {
        private readonly SampleContext _context;

        public PartitionedProjectionStateRepository(SampleContext context)
        {
            _context = context;
        }

        public async Task<T> GetProjectionState(Guid partitionId)    
        {
            try
            {
                return await _context.FindAsync<T>(partitionId);
            }
            catch(Exception e)
            {
                return default;
            }
        }

        [Obsolete]
        public async Task ResetState()
        {
            var cmd = $"TRUNCATE TABLE [Sample].[{nameof(_context.Products)}];";
            await _context.Database.ExecuteSqlCommandAsync(cmd);
        }

        public async Task SetProjectionState(Guid partitionId, T state)
        {

            if(await _context.FindAsync<T>(partitionId) != default)
                _context.Update(state);
            else
                await _context.AddAsync(state);

            await _context.SaveChangesAsync();
        }
    }
}
