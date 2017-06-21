using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Bloometa.Models.TrackViewModels
{
    public class CreateViewModel
    {
        [Required]
        [DataType(DataType.Text)]
        [RegularExpression(@"^([A-Za-z0-9._-]+)$")]
        public string Username { get; set; }

        [Required]
        public string Network { get; set; }

        public List<string> NetworkOptions { get; set; }
    }
}
