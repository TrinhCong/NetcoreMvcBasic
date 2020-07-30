using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace NetcoreMvcBasic.Business.Entities
{
    [Table("categories")]
    public class Category
    {
        public Category()
        {
            this.Products = new HashSet<Product>();
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DisplayName("ID")]
        public int category_id { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [StringLength(100,ErrorMessage = "Danh mục không được quá 100 ký tự")]
        [DisplayName("Tên danh mục")]
        public string category_name { get; set; }
        public IEnumerable<Category> Categories { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
