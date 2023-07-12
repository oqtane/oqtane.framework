using System.Threading.Tasks;
using Oqtane.Models;

namespace Oqtane.Infrastructure
{
    public interface IUserManager
    {
        Task<User> AddUser(User user);
    }
}
