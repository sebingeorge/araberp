using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class PrefixSettingsVsOrganization
    {
        public int? PrefixId { get; set; }
        public string Prefix { get; set; }
        public string TransactionName { get; set; }
        public int? OrganizationId { get; set; }
    }
}
