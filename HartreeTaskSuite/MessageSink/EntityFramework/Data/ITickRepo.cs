using MessageSink.EntityFramework.Model;

namespace MessageSink.EntityFramework.Data
{
    public interface ITickRepo
    {
        Task SaveChanges();
        Task SaveTick(Tick tick);
        Task SaveTicks(Tick[] tick);
    }
}
