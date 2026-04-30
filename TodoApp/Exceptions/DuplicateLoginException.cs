using System;

namespace TodoApp.Exceptions
{
    public class DuplicateLoginException : Exception
    {
        public DuplicateLoginException(string message) : base(message) { }
    }
}
