using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryManager.Models
{
    public class Profile
    {
        public string StudentId { get; set; }
        public string FirstName { get; set; }
        public List<Book> CurrentBooks { get; set; }
        public List<Book> PreviousBooks { get; set; }
    }
}
