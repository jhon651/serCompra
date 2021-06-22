using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace serCompras.Models
{
    public class SerComprasEntities : DbContext
    {
        public SerComprasEntities() : base("serComprasDB") {}
        public virtual DbSet<User> Users { get; set; }
    }
}