using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IUserRepository
    {
        IEnumerable<User> GetUsers();
        User AddUser(User User);
        User UpdateUser(User User);
        User GetUser(int UserId);
        User GetUser(string Username);
        void DeleteUser(int UserId);
    }
}
