using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class RateSettings
    {
        public int RateSettingsId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        /// <summary>
        /// 1 = project,   0 = transport
        /// </summary>
        public int? Type { get; set; }
        public List<RateSettingsItems> Items { get; set; }
    }
}
