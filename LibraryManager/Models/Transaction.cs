using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryManager.Models
{
    public class Transaction
    {
        public string Id { get; set; }
        public string StudentId { get; set; }
        public string BookId { get; set; }
        public string Active { get; set; }
    }
}
