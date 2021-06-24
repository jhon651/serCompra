
using System;
using System.Data.Entity.Infrastructure;

namespace serCompras.Models
{

    public partial class Login
    {
        public int id { get; set; }
        public int? id_provider { get; set; }
        public string legal_name { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public int id_role { get; set; }
        public string role { get; set; }

        public static implicit operator Login(DbRawSqlQuery<Login> v)
        {
            throw new NotImplementedException();
        }
    }
}
