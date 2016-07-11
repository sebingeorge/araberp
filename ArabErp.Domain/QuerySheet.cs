﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class QuerySheet
    {
        public int QuerySheetId { get; set; }
        public string ProjectName { get; set; }
        public string ContactPerson { get; set; }
        public string ContactNumber { get; set; }
        public string Email { get; set; }
        public string RoomDetails { get; set; }
        public string ExternalRoomDimension { get; set; }
        public string ColdRoomArea { get; set; }
        public string ColdRoomLocation { get; set; }
        public string TemperatureRequired { get; set; }
        public string PanelThicknessANDSpec { get; set; }
        public string DoorSizeTypeAndNumberOfDoor { get; set; }
        public string FloorDetails { get; set; }
        public string ProductDetails { get; set; }
        public string ProductIncomingTemperature { get; set; }
        public string PipeLength { get; set; }
        public string Refrigerant { get; set; }
        public string EletricalPowerAvailability { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public bool isActive { get; set; }
    }
}
