using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class UserRepository : IUserRepository
    {
        private TenantDBContext db;

        public UserRepository(TenantDBContext context)
        {
            db = context;
        }
            
        public IEnumerable<User> GetUsers()
        {
            return db.User;
        }

        public User AddUser(User user)
        {
            db.User.Add(user);
            db.SaveChanges();
            return user;
        }

        public User UpdateUser(User user)
        {
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();
            return user;
        }

        public User GetUser(int userId)
        {
            return db.User.Find(userId);
        }

        public User GetUser(string Username)
        {
            return db.User.Where(item => item.Username == Username).FirstOrDefault();
        }

        public void DeleteUser(int userId)
        {
            User user = db.User.Find(userId);
            db.User.Remove(user);
            db.SaveChanges();
        }
    }
}
