using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManager.Models
{
    public class TasksList
    {
        public int? Seed { get; set; }
        public IEnumerable<Task> Tasks { get; set; }
    }
}
