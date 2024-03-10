using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using CustomerOnboarding.Entities;
using CustomerOnboarding.Interfaces;

namespace CustomerOnboarding.Repositories
{
    public class StateRepository : GenericRepository<State>, IStateRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public StateRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<List<State>> GetAllStateDetails()
        {
            var states = await _dbContext.States
                .Include(q => q.Lgas)
                .ToListAsync();
            return states;
        }

        public async Task<State> GetStateDetails(int id)
        {
            var state = await _dbContext.States
               .Include(q => q.Lgas)
               .FirstOrDefaultAsync(q => q.Id == id);

            return state;
        }
    }
}
