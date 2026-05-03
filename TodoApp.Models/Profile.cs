using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoApp.Models
{
    public class Profile
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Login { get; set; }

        [Required]
        [MaxLength(100)]
        public string Password { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Range(1900, 2100)]
        public int BirthYear { get; set; }

        public List<TodoItem> Todos { get; set; }

        public Profile()
        {
            Id = Guid.NewGuid();
            Login = string.Empty;
            Password = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            BirthYear = 0;
            Todos = new List<TodoItem>();
        }

        public Profile(string login, string password, string firstName, string lastName, int birthYear)
        {
            Id = Guid.NewGuid();
            Login = login;
            Password = password;
            FirstName = firstName;
            LastName = lastName;
            BirthYear = birthYear;
            Todos = new List<TodoItem>();
        }

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        public string GetInfo()
        {
            int age = DateTime.Now.Year - BirthYear;
            return $"{FirstName} {LastName}, {age} лет";
        }
    }
}
