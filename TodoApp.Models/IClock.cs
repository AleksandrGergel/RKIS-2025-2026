using System;

namespace TodoApp.Models
{
    public interface IClock
    {
        DateTime Now { get; }
    }
}
