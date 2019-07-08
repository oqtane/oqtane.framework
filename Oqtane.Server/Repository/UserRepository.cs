using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class UserRepository : IUserRepository
    {
        private TenantContext db;

        public UserRepository(TenantContext context)
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

        public void AddUser(User user)
        {
            try
            {
                db.User.Add(user);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        public void UpdateUser(User user)
        {
            try
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
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
                User user = db.User.Find(userId);
                return user;
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
                User user = db.User.Where(item => item.Username == Username).FirstOrDefault();
                return user;
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
