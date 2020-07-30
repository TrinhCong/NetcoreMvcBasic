using System;
using System.Collections.Generic;
using System.Text;

namespace NetcoreMvcBasic.Business.Models
{
    public class FilterParam
    {
        public string searchString { get; set; }
        public int cateId { get; set; }
        public int supId { get; set; }
        public int discount { get; set; }
        public int? page { get; set; }
        public int pageSize { get; set; }
        public int unitStock { get; set; }
        
        public int minStock { get; set; }
       
        public int maxStock { get; set; }
        public string condition { get; set; }
        public int? lastPage { get; set; }
    }
}
