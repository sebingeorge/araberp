using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class FGTracking
    {
        public int SalesInvoiceId { get; set; }
        public string SalesInvoiceRefNo { get; set; }
        public DateTime SalesInvoiceDate { get; set; }
        public decimal SalesInvoiceAmount { get; set; }
        public string SaleInvoiceRemarks { get; set; }
        public string ItemName { get; set; }

        public string JobCardNo { get; set; }
        public DateTime JobCardDate { get; set; }
        public int ActualHours { get; set; }
        public string JobCardTaskName { get; set; }
        public string TaskEmployeeName { get; set; }

        public int ItemBatchId { get; set; }
        public string SerialNo { get; set; }
        public string DeliveryChallanRefNo { get; set; }
        public DateTime DeliveryChallanDate { get; set; }
        public DateTime WarrantyExpireDate { get; set; }
        public int WarrantyLeft { get; set; }
        public string GRNNo { get; set; }
        public DateTime GRNDate { get; set; }
        public string SupplierName { get; set; }
        public decimal GRNAmount { get; set; }
        public int GRNQuantity { get; set; }
        public decimal GRNRate { get; set; }
        public string StockPointName { get; set; }
        public string PurchaseBillRefNo { get; set; }
        public DateTime PurchaseBillDate { get; set; }
        public decimal PurchaseBillRate { get; set; }
        public decimal PurchaseBillAmount { get; set; }
        public string SaleOrderRefNo { get; set; }
        public DateTime SaleOrderDate { get; set; }
        public string EmployeeName { get; set; }
        public string CustomerName { get; set; }

        public List<FGTracking> JobCardTasks { get; set; }
    }
}
