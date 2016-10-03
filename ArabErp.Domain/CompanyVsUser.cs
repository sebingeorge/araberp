using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
  public  class CompanyVsUser
    {
        public int CompanyVsUserId { get; set; }
        public int cmpCode { get; set; }
        public string cmpName { get; set; }
        public int UserId { get; set; }
        public int isPermission { get; set; }
    }
}
