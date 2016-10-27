using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArabErp.Domain
{
    public class JobCard
    {
        public int JobCardId { get; set; }
        public string JobCardNo { get; set; }
        public DateTime JobCardDate { get; set; }
        public int SaleOrderId { get; set; }
        public int SaleOrderItemId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerOrderRef { get; set; }
        public string VehicleModelName { get; set; }
        public int? InPassId { get; set; }
        public int WorkDescriptionId { get; set; }
        public string WorkDescription { get; set; }
        public string WorkShopRequestRef { get; set; }
        public int GoodsLanded { get; set; }
        public int? FreezerUnitId { get; set; }
        public int? BoxId { get; set; }
        public int? BayId { get; set; }
        public string SpecialRemarks { get; set; }
        public DateTime RequiredDate { get; set; }
        public int EmployeeId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public int JodCardCompleteStatus { get; set; }
        public DateTime JodCardCompletedDate { get; set; }
        public int isProjectBased { get; set; }
        public string FreezerUnitName { get; set; }
        public string VehicleInPassNo { get; set; }
        public string RegistrationNo { get; set; }
        public DateTime VehicleInPassDate { get; set; }
        public string Phone { get; set; }
        public string ContactPerson { get; set; }
        public string Customer { get; set; }
        public string Technician { get; set; }
        public string EmployeeName { get; set; }

        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
       // public string OrganizationRefNo { get; set; }
        public string OrganizationName { get; set; }
        public string Image1 { get; set; }
        public List<JobCardTask> JobCardTasks { get; set; }

        public string BoxName { get; set; }
        public bool IsUsed { get; set; }
        public bool IsTaskUsed { get; set; }
    }
    public class JobCardForDailyActivity
    {
        public int JobCardId { get; set; }
        public string JobCardNo { get; set; }
        public DateTime JobCardDate { get; set; }
        public string EmployeeName { get; set; }
        public string WorkDescr { get; set; }
        public DateTime RequiredDate { get; set; }
    }
}