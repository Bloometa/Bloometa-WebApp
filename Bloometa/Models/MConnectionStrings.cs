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

        public string TwitterKey { get; set; }
        public string TwitterSecret { get; set; }

        public string FacebookKey { get; set; }
        public string FacebookSecret { get; set; }

        public string MicrosoftKey { get; set; }
        public string MicrosoftSecret { get; set; }

        public string GoogleKey { get; set; }
        public string GoogleSecret { get; set; }
    }
}
