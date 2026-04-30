using System;

namespace TodoApp.Exceptions
{
    public class ProfileNotFoundException : Exception
    {
        public ProfileNotFoundException(string message) : base(message) { }
    }
}
