﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace ArabErp.Domain
{
   public class PurchaseBill
    {
        //public PurchaseBill()
        //{
        //    PurchaseBillItems = new List<PurchaseBillItem>();
        //}

        public int PurchaseBillId { get; set; }
        public string PurchaseBillRefNo { get; set; }
        public int SupplierId { get; set; }
        public string Supplier { get; set; }
        public DateTime PurchaseBillDate { get; set; }
       [Required]
        public string BillNoDate { get; set; }
        public string Remarks { get; set; }
        public decimal PurchaseBillAmount { get; set; }
        public string AdditionRemarks { get; set; }
        public string DeductionRemarks { get; set; }
        public decimal? Deduction { get; set; }
        public decimal? Addition { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public bool isActive { get; set; }
        public List<PurchaseBillItem> Items { get; set; }
    }
}