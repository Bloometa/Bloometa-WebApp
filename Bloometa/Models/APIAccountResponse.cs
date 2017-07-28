using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bloometa.Models
{
    public class APIAccountResponse
    {
        public List<UError> ResponseErrors { get; set; }
        public Guid ResponseID { get {
            return Guid.NewGuid();
        } }
        public DateTime ResponseTime { get {
            return DateTime.UtcNow;
        } }

        public List<UAccount> Accounts { get; set; }
    }
}