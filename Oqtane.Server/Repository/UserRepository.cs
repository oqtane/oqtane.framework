using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class UserRepository : IUserRepository
    {
        private TenantDBContext _db;

        public UserRepository(TenantDBContext context)
        {
            _db = context;
        }
            
        public IEnumerable<User> GetUsers()
        {
            return _db.User;
        }

        public User AddUser(User user)
        {
            _db.User.Add(user);
            _db.SaveChanges();
            return user;
        }

        public User UpdateUser(User user)
        {
            _db.Entry(user).State = EntityState.Modified;
            _db.SaveChanges();
            return user;
        }

        public User GetUser(int userId)
        {
            return _db.User.Find(userId);
        }

        public User GetUser(string username)
        {
            return _db.User.Where(item => item.Username == username).FirstOrDefault();
        }

        public void DeleteUser(int userId)
        {
            User user = _db.User.Find(userId);
            _db.User.Remove(user);
            _db.SaveChanges();
        }
    }
}
