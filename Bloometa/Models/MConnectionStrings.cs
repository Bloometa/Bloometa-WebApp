using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bloometa.Models
{
    public class MConnectionStrings
    {
        public string DB { get; set; }
        public string Storage { get; set; }

        public string SendGridUser { get; set; }
        public string SendGridKey { get; set; }
    }
}
