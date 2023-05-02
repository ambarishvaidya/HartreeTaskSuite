using MessageSink.EntityFramework.Model;
using System.Reflection;

namespace MessageSink.EntityFramework.Data
{
    public class TickRepo : ITickRepo
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly MSSqlDbContext _context;
        public TickRepo(MSSqlDbContext context)
        {
            _context = context;
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }

        public async Task SaveTick(Tick tick)
        {
            await _context.AddAsync(tick);
            await _context.SaveChangesAsync();
        }

        public async Task SaveTicks(Tick[] tick)
        {
            Log.Info($"Saving {tick.Length} Items.");    
            await _context.AddRangeAsync(tick);
            await _context.SaveChangesAsync();
        }
    }
}
