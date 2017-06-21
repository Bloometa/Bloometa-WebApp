using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bloometa.Models
{
    public class UAccount
    {
        public Guid AccID { get; set; }
        public string Network { get; set; }
        public string Username { get; set; }
        public DateTime Added { get; set; }
    }
}