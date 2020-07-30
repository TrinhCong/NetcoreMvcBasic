using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace NetcoreMvcBasic.Business.Entities
{
    [Table("suppliers")]
    public class Supplier
    {
        public Supplier()
        {
            this.Products = new HashSet<Product>();
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DisplayName("ID")]
        public int supplier_id { get; set; }
        public string company_name { get; set; }
        public string contact_name { get; set; }
        public string contact_title { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string region { get; set; }
        public string postal_code { get; set; }
        public string country { get; set; }
        public string phone { get; set; }
        public string fax { get; set; }
        public string homepage { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}
