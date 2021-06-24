
using System;
using System.Data.Entity.Infrastructure;

namespace serCompras.Models
{

    public partial class Counters
    {
        public int providersCount { get; set; }
        public int requestCount { get; set; }
        public int budgetsCount { get; set; }
        public int newprovidersCount { get; set; }

    }
}
