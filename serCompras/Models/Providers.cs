
using System;
using System.Data.Entity.Infrastructure;

namespace serCompras.Models
{

    public partial class Providers
    {
        public int id { get; set; }
        public string legal_name { get; set; }
        public string rut { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public string fullname { get; set; }
        public byte status { get; set; }

    }


}
