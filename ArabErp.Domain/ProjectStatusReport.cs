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
    public class ProjectStatusReport
    {
        public string JobCardNo { get; set; }
        public DateTime JobCardDate { get; set; }
        public string Customer { get; set; }
        public string ProjectName { get; set; }
        public string ContactPerson { get; set; }
        public string ContactNumber { get; set; }
        public string CostingAmount { get; set; }
        public string QuerySheetRefNo { get; set; }
        public string RoomDetails { get; set; }
        public string RoomSize { get; set; }
        public string TempRequired { get; set; }
        public string Door { get; set; }
        public string Floor { get; set; }
        public DateTime JobCardDailyActivityDate { get; set; }
        public string JobCardTaskName { get; set; }
        public string InCharge { get; set; }
        public string EmployeeName { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string EstHours { get; set; }
        public string OverTime { get; set; }
        public string ActualHours { get; set; }
        public string ItemName { get; set; }
        public string IssuedQuantity { get; set; }
    }
}
