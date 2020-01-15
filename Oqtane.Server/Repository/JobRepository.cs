using Oqtane.Models;

namespace Oqtane.Repository
{
    public class JobRepository : Repository<Job>
    {
        public JobRepository(MasterDBContext context)
            :base(context)
        {

        }
    }
}
