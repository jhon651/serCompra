
using System;
using System.Data.Entity.Infrastructure;

namespace serCompras.Models
{

    public partial class Budget
    {
        public int id { get; set; }
        public int? id_provider { get; set; }
        public int? id_request { get; set; }
        public string purpose { get; set; }
        public DateTime? date_limit { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public byte status { get; set; }
        public string fullname { get; set; }
    }
}
