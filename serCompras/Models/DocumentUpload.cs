using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace serCompras.Models
{
    public class DocumentUpload
    {
        public DocumentUpload()
        {
            Files = new List<HttpPostedFileBase>();
        }
        [Key]
        public List<HttpPostedFileBase> Files { get; set; }
    }
}