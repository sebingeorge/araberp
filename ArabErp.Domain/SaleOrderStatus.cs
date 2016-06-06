﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class SaleOrderStatus
    {
        public int SaleOrderId { get; set; }
        public string CustomerName { get; set; }
        public string SaleOrderRefNo { get; set; }
        public DateTime SaleOrderDate { get; set; }
        public DateTime EDateDelivery { get; set; }
        public string JobCardApproval { get; set; }
        public string JobCard { get; set; }
        public string JobCardComplete { get; set; }
        public string WorkShopRequest { get; set; }
        public string PurchaseRequest { get; set; }
        public string SuppyOrder { get; set; }
        public string GRN { get; set; }
    }
}
