using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TodoApp.Models;

namespace TodoApp.Data
{
    public class ProfileRepository
    {
        public List<Profile> GetAll()
        {
            using var context = new AppDbContext();
            return context.Profiles.AsNoTracking().OrderBy(p => p.Login).ToList();
        }

        public Profile? GetByCredentials(string login, string password)
        {
            using var context = new AppDbContext();
            return context.Profiles.AsNoTracking().FirstOrDefault(p => p.Login == login && p.Password == password);
        }

        public Profile? GetById(Guid id)
        {
            using var context = new AppDbContext();
            return context.Profiles.AsNoTracking().FirstOrDefault(p => p.Id == id);
        }

        public bool LoginExists(string login)
        {
            using var context = new AppDbContext();
            return context.Profiles.Any(p => p.Login == login);
        }

        public void Add(Profile profile)
        {
            using var context = new AppDbContext();
            context.Profiles.Add(profile);
            context.SaveChanges();
        }

        public void Update(Profile profile)
        {
            using var context = new AppDbContext();
            context.Profiles.Update(profile);
            context.SaveChanges();
        }

        public void ReplaceAll(IEnumerable<Profile> profiles)
        {
            using var context = new AppDbContext();
            context.Profiles.RemoveRange(context.Profiles);
            context.Profiles.AddRange(profiles);
            context.SaveChanges();
        }
    }
}
