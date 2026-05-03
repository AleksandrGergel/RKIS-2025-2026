using System;

namespace TodoApp.Models
{
    public class SystemClock : IClock
    {
        public DateTime Now => DateTime.Now;
    }
}
