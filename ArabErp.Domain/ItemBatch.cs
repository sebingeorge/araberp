using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class ItemBatch
    {
        public int ItemBatchId { get; set; }
        public int GRNItemId { get; set; }
        public int GRNId { get; set; }
        public int? SaleOrderItemId { get; set; }
        public int? SaleOrderId { get; set; }
        public int? DeliveryChallanId { get; set; }
        public int? WarrantyPeriodInMonths { get; set; }
        public DateTime WarrantyStartDate { get; set; }
        public DateTime WarrantyExpireDate { get; set; }
        public string SerialNo { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public bool isActive { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public string Remarks { get; set; }
        public string isDirect { get; set; }
        public string GRNNo { get; set; }
        public DateTime GRNDate { get; set; }
        public string SupplierName { get; set; }
        public int Ageing { get; set; }
        public string StockPointName { get; set; }
        public string SaleOrderRefNo { get; set; }
        public string SaleOrderDate { get; set; }
        public string WorkDescrShortName { get; set; }
        public string WorkDescrRefNo { get; set; }
        public bool isSelected { get; set; }
        public string CustomerName { get; set; }
        public string DeliveryChallanRefNo { get; set; }
        /// <summary>
        /// Warranty left in months
        /// </summary>
        public int WarrantyLeft { get; set; }
        public int OpeningStockId { get; set; }
        public int isOpeningStock { get; set; }
    }
}
