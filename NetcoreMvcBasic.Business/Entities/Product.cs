using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace NetcoreMvcBasic.Business.Entities
{
    [Table("products")]
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DisplayName("ID")]
        public int product_id { get; set; }
        [DisplayName("Sản phẩm")]
        public string product_name { get; set; }
        
        //Đặt khóa ngoại ở đây
        [ForeignKey(nameof(Category))]
        [DisplayName("Danh mục")]
        public int category_id { get; set; }

        //Đặt khóa ngoại ở đây
        [ForeignKey(nameof(Supplier))]
        [DisplayName("Nhà cung cấp")]
        public int supplier_id { get; set; }
        [DisplayName("Quy cách")]
        public string quantity_per_unit { get; set; }
        [DisplayName("Đơn giá")]
        public float unit_price { get; set; }
        [DisplayName("Số lượng tồn")]
        public int units_in_stock { get; set; }
        [DisplayName("Đ/v xuất bán")]
        public int units_on_order { get; set; }
        [DisplayName("Mức độ đặt hàng lại")]
        public int reorder_level { get; set; }
        [DisplayName("Giảm giá")]
        public int discontinued { get; set; }
        [NotMapped]
        public virtual Category Category { get; set; }
        [NotMapped]
        public virtual Supplier Supplier { get; set; }
    }
}
