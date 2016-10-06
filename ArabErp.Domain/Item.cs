using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class Item
    {
        public int ItemId { get; set; }
        public string ItemRefNo { get; set; }
        public string PartNo { get; set; }
         [Required]
        public string ItemName { get; set; }
        public string ItemPrintName { get; set; }
         [Required]
        public string  ItemShortName { get; set; }
         [Required]
        public int? ItemGroupId { get; set; }
         [Required]
        public int? ItemSubGroupId { get; set; }
         [Required]
        public int? ItemCategoryId { get; set; }
         [Required]
        public int? ItemUnitId { get; set; }
        public int? ItemQualityId { get; set; }
        public int? CommodityId { get; set; }
        public int? MinLevel { get; set; }
        public int? ReorderLevel { get; set; }
        public int? MaxLevel { get; set; }
        [Required]
        public DateTime ExpiryDate { get; set; }
        public Boolean BatchRequired { get; set; }
        public Boolean StockRequired { get; set; }
        public Boolean CriticalItem { get; set; }
        public Boolean FreezerUnit { get; set; }
        public Boolean Box { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? OrganizationId { get; set; }
        public string CategoryName { get; set; }
        public string ItemGroupName { get; set; }
        public string ItemSubGroupName { get; set; }
        public string UnitName { get; set; }
    }
    public enum  ItemSubGroupId
    {
        SubGroup1,SubGroup2
    }
    public enum ItemQuality
    {
        Quality1, Quality2
    }
    public enum itemUnit
    {
        Unit1, Unit2
    }
   
}
