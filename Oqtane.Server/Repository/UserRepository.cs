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
            try
            {
                return db.User.ToList();
            }
            catch
            {
                throw;
            }
        }

        public User AddUser(User user)
        {
            try
            {
                db.User.Add(user);
                db.SaveChanges();
                return user;
            }
            catch
            {
                throw;
            }
        }

        public User UpdateUser(User user)
        {
            try
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return user;
            }
            catch
            {
                throw;
            }
        }

        public User GetUser(int userId)
        {
            try
            {
                return db.User.Find(userId);
            }
            catch
            {
                throw;
            }
        }

        public User GetUser(string Username)
        {
            try
            {
                return db.User.Where(item => item.Username == Username).FirstOrDefault();
            }
            catch
            {
                throw;
            }
        }

        public void DeleteUser(int userId)
        {
            try
            {
                User user = db.User.Find(userId);
                db.User.Remove(user);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
    }
}
