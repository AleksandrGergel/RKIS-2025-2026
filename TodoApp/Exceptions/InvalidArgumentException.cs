using System;

namespace TodoApp.Exceptions
{
    public class InvalidArgumentException : Exception
    {
        public InvalidArgumentException(string message) : base(message) { }
    }
}
