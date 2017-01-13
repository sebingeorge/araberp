using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class ProjectCompletion
    {
      
        public ProjectCompletion()
        {
            ItemBatches = new List<ItemBatch>();
        }

        public int ProjectCompletionId { get; set; }
        public string ProjectCompletionRefNo { get; set; }
        public DateTime ProjectCompletionDate { get; set; }
        public DateTime ProjectWarrantyExpiryDate { get; set; }
        public string ProjectName { get; set; }
        public string Location { get; set; }
        public string CustomerName { get; set; }
        public string SaleOrderRefNo { get; set; }
        public DateTime SaleOrderDate { get; set; }

        public string ChillerTemperature { get; set; }
        public string ChillerDimension { get; set; }
        public string ChillerCondensingUnit { get; set; }
        public string ChillerEvaporator { get; set; }
        public string ChillerRefrigerant { get; set; }
        public string ChillerQuantity { get; set; }

        public string FreezerTemperature { get; set; }
        public string FreezerDimension { get; set; }
        public string FreezerCondensingUnit { get; set; }
        public string FreezerEvaporator { get; set; }
        public string FreezerRefrigerant { get; set; }
        public string FreezerQuantity { get; set; }

        public int SaleOrderId { get; set; }
        public SaleOrder saleorder { get; set; }
        public JobCardCompletion jobcard { get; set; }
        public List<ItemBatch> ItemBatches { get; set; }
        public int OrganizationId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public bool IsUsed { get; set; }

        public string DoorNo { get; set; }

        public string Street { get; set; }

        public string State { get; set; }

        public int Country { get; set; }
        public string CountryName { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string ContactPerson { get; set; }
        public string OrganizationName { get; set; }
        public string Image1 { get; set; }
        public List<ProjectRoomAndUnitDetails> ProjectRoomAndUnitDetails { get; set; }

    }
}
