using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace ArabErp.Domain
{
   public class ItemGroup
    {
        public int ItemGroupId { get; set; }
        [Required(ErrorMessage = "Please Enter Code")]
        public string ItemGroupRefNo { get; set; }
        [Required(ErrorMessage = "Please Enter Name")]
        public string ItemGroupName { get; set; }
        public int ItemCategoryId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public string CategoryName { get; set; }
        public List<Categories> ItemCategory { get; set; }
    }
   public class Categories
   {
       public int itmCatId { get; set; }
       public string CategoryName { get; set; }
   }
}
