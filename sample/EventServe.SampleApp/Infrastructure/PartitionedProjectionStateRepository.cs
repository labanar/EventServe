using EventServe.Projections;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace EventServe.SampleApp.Infrastructure
{
    public class PartitionedProjectionStateRepository : IPartitionedProjectionStateRepository
    {
        private readonly SampleContext _context;

        public PartitionedProjectionStateRepository(SampleContext context)
        {
            _context = context;
        }

        public async Task<T> GetProjectionState<T>(Guid partitionId) where T : PartitionedProjection
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

        public async Task<T> SetProjectionState<T>(Guid partitionId, T state) where T : PartitionedProjection
        {

            if(await _context.FindAsync<T>(partitionId) != default)
                _context.Update(state);
            else
                await _context.AddAsync(state);

            await _context.SaveChangesAsync();
            return state;
        }
    }
}
