using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class WorkShopRequest
    {
        public int WorkShopRequestId { get; set; }
        public string WorkShopRequestRefNo { get; set; }
        public string WorkDescription { get; set; }
        public DateTime WorkShopRequestDate { get; set; }
        public int SaleOrderId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerOrderRef { get; set; }
        public string SpecialRemarks { get; set; }
        public DateTime RequiredDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }

        //for view model
        public string SaleOrderRefNo { get; set; }
        public string CustomerName { get; set; }


        public bool isAdditionalRequest { get; set; }
        public int JobCardId { get; set; }
        public string SoNoWithDate { get; set; }
      
        public List<WorkShopRequestItem> Items { get; set; }
    }
}
