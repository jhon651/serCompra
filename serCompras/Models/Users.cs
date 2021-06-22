using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace serCompras.Models
{
    [Table("user")]
    public partial class User
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Id_provider")]
        public int Id_provider { get; set; }

        [Display(Name = "Firstname")]
        public string Firstname { get; set; }

        [Display(Name = "Lastname")]
        public string Lastname { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Id_role")]
        public int Id_role { get; set; }

        [Display(Name = "Created_at")]
        public string Created_at { get; set; }

        [Display(Name = "Status")]
        public int Status { get; set; }
    }
}
