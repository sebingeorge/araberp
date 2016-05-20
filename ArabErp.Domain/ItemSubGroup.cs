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
  public  class ItemSubGroup
    {
        public int ItemSubGroupId { get; set; }
        [Required(ErrorMessage = "Please Enter Code")]
        public string ItemSubGroupRefNo { get; set; }
        [Required(ErrorMessage = "Please Enter Ref.No")]
        public string ItemSubGroupName { get; set; }
        [Required(ErrorMessage = "Please Enter Name")]
        public int ItemGroupId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public string ItemGroupName { get; set; }
        public List<Group> ItemGroup { get; set; }
    }
  public class Group
  {
      public int ItemGroupId { get; set; }
      public string ItemGroupName { get; set; }
  }
}
