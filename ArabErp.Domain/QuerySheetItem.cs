using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
  public  class QuerySheetItem
  {
        public int QuerySheetId { get; set; }
        public int QuerySheetItemId { get; set; }
        public string RoomDetails { get; set; }
        public string ExternalRoomDimension { get; set; }
        public string ColdRoomArea { get; set; }
        public string ColdRoomLocation { get; set; }
        public string TemperatureRequired { get; set; }
        public string PanelThicknessANDSpec { get; set; }
        public string DoorSizeTypeAndNumberOfDoor { get; set; }
        public string FloorDetails { get; set; }
        public string ProductDetails { get; set; }
        public string Kilowatt { get; set; }
        public decimal Amount { get; set; }
        public string Remarks { get; set; }
        public string ProductIncomingTemperature { get; set; }
        public string PipeLength { get; set; }
        public string Refrigerant { get; set; }
        public string EletricalPowerAvailability { get; set; }
        public decimal Cost { get; set; }
        public string Type { get; set; }
   

        public List<QuerySheetUnit> ProjectRoomUnits { get; set; }
        public List<QuerySheetDoor> ProjectRoomDoors { get; set; }
    }
  public class QuerySheetUnit
  {
      public int QuerySheetItemId { get; set; }
      public string CondenserUnitId { get; set; }
      public string EvaporatorUnitId { get; set; }
      public decimal Quantity { get; set; }
      public int sno { get; set; }
  }
  public class QuerySheetDoor
  {
      public int QuerySheetItemId { get; set; }
      public decimal Quantity { get; set; }
      public string DoorId { get; set; }
  }

}
