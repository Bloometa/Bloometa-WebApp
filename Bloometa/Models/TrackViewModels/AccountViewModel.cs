using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bloometa.Models.TrackViewModels
{
    public class AccountViewModel
    {
        public UAccount AccDetails { get; set; }
        public List<UData> Reporting { get; set; }

        public List<int> ReportingFollowers { get; set; }
        public List<int> ReportingFollowing { get; set; }
        public List<DateTime> ReportingDays { get; set; }

        public int MonthTotalFollowers { get; set; }
        public int MonthTotalFollowing { get; set; }
    }
}
