using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
  public  class TransactionHistory
    {
        public int TransactionHistoryId { get; set; }
        public int UserId { get; set; }
        public DateTime TransTime { get; set; }
        public string Mode { get; set; }
        public string Form { get; set; }
        public int? FormTransCode { get; set; }
        public string IPAddress { get; set; }
        public int OrganizationId { get; set; }
        public string UserName { get; set; }
    }
}
