using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ArabErp.Domain
{
  public  class Company
    {
      public int cmpCode { get; set; }
      public string cmpUsercode { get; set; }
      public string cmpName { get; set; }
      public string cmpShrtName { get; set; }
      public string cmpRemarks { get; set; }
      public string cmpDoorNo { get; set; }
      public string cmpStreet { get; set; }
      public string cmpArea { get; set; }
      public string cmpState { get; set; }
      public string cmpPhNo { get; set; }
      public string cmpFax { get; set; }
      public string cmpEmail { get; set; }
      public string cmpWeb { get; set; }
      public string cmpPAN { get; set; }
      public string cmpSSN { get; set; }
      public string cmpVATNo { get; set; }
      public string cmpCSTNo { get; set; }
      public string cmpIECode { get; set; }
      public string cmpWorkSheetNo { get; set; }
      public string cmpFormPrefix { get; set; }
      public string cmpFinancialYr { get; set; }
      public string CreatedBy { get; set; }
      public DateTime? CreatedDate { get; set; }
      public bool isActive { get; set; }

    }
}
