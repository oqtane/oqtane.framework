using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IUserRepository
    {
        IEnumerable<User> GetUsers();
        void AddUser(User User);
        void UpdateUser(User User);
        User GetUser(int UserId);
        User GetUser(string Username);
        void DeleteUser(int UserId);
    }
}
