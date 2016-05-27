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
    public class ItemCategory

    {
        public int itmCatId { get; set; }
        public string itmCatRefNo { get; set; }
        public string CategoryName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
    }
}
