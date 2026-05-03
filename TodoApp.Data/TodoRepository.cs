using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TodoApp.Models;

namespace TodoApp.Data
{
    public class TodoRepository
    {
        public List<TodoItem> GetAll()
        {
            using var context = new AppDbContext();
            return context.Todos.AsNoTracking().OrderBy(t => t.Id).ToList();
        }

        public List<TodoItem> GetAll(Guid profileId)
        {
            using var context = new AppDbContext();
            return context.Todos
                .AsNoTracking()
                .Where(t => t.ProfileId == profileId)
                .OrderBy(t => t.Id)
                .ToList();
        }

        public void Add(TodoItem item)
        {
            using var context = new AppDbContext();
            context.Todos.Add(item);
            context.SaveChanges();
        }

        public void Update(TodoItem item)
        {
            using var context = new AppDbContext();
            context.Todos.Update(item);
            context.SaveChanges();
        }

        public void Delete(int id)
        {
            using var context = new AppDbContext();
            var item = context.Todos.FirstOrDefault(t => t.Id == id);
            if (item == null)
                return;

            context.Todos.Remove(item);
            context.SaveChanges();
        }

        public void SetStatus(int id, TodoStatus status)
        {
            using var context = new AppDbContext();
            var item = context.Todos.FirstOrDefault(t => t.Id == id);
            if (item == null)
                return;

            item.SetStatus(status);
            context.SaveChanges();
        }

        public void ReplaceForProfile(Guid profileId, IEnumerable<TodoItem> todos)
        {
            using var context = new AppDbContext();
            var existingTodos = context.Todos.Where(t => t.ProfileId == profileId);
            context.Todos.RemoveRange(existingTodos);

            foreach (var todo in todos)
            {
                todo.ProfileId = profileId;
                todo.Profile = null;
            }

            context.Todos.AddRange(todos);
            context.SaveChanges();
        }
    }
}
