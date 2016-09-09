using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class CustomerVsSalesExecutiveList
    {
        public CustomerVsSalesExecutiveList()
        {
            CustomerVsSalesExecutives = new List<CustomerVsSalesExecutive>();
        }
        public int dummy { get; set; }
        public List<CustomerVsSalesExecutive> CustomerVsSalesExecutives { get; set; }
        
        
    }
}
