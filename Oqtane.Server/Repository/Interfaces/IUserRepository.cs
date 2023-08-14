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
        User GetUser(int userId, bool tracking);
        User GetUser(string username);
        User GetUser(string username, string email);
        void DeleteUser(int userId);
    }
}
