using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SuperMarketApi.Models
{
    public class Unit
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public bool Updated { get; set; }

    }
}
