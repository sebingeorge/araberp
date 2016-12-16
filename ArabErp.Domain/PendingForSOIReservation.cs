using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class PendingForSOIReservation
    {
        public int SaleOrderItemId { get; set; }
        public int SaleOrderId { get; set; }
        public string SaleOrderRefNo { get; set; }
        public DateTime SaleOrderDate { get; set; }
        public int Quantity { get; set; }
        public string WorkDescriptionRefNo { get; set; }
        public string ItemName { get; set; }
        public int ItemId { get; set; }
        public int ReservedQuantity { get; set; }
        public string WorkDescrShortName { get; set; }
        public string SerialNo { get; set; }
        public string WorkShopRequestRefNo { get; set; }
        public string StoreIssueRefNo { get; set; }
        public string JobCardNo { get; set; }
        public string GRNNo { get; set; }
    }
}
