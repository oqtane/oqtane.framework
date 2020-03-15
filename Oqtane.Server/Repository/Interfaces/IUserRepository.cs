using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IUserRepository
    {
        IEnumerable<User> GetUsers();
        User AddUser(User user);
        User UpdateUser(User user);
        User GetUser(int userId);
        User GetUser(string username);
        void DeleteUser(int userId);
    }
}
