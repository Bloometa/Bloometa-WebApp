using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bloometa.Models
{
    public class UData
    {
        public DateTime Run { get; set; }

        public int FollowCount { get; set; }
        public int FollowerCount { get; set; }

        public int FollowDifference { get; set; }
        public int FollowerDifference { get; set; }
    }
}