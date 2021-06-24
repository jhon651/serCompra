
using System;
using System.Data.Entity.Infrastructure;

namespace serCompras.Models
{

    public partial class User
    {
        public int id { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string role { get; set; }
        public Byte status { get; set; }

        public static implicit operator User(DbRawSqlQuery<User> v)
        {
            throw new NotImplementedException();
        }
    }
}
