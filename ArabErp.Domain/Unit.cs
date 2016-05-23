﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace ArabErp.Domain
{
   public class Unit
    {
       public int UnitId { get; set; }
       [Required(ErrorMessage = "Please Enter Code")]
       public string UnitRefNo { get; set; }
       [Required(ErrorMessage = "Please Enter Name")]
       public string UnitName { get; set; }
       public string CreatedBy { get; set; }
       public DateTime? CreatedDate { get; set; }
       public int OrganizationId { get; set; }
    }
}
