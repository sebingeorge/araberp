using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace ArabErp.DAL
{
    public class DropdownRepository: BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        /// <summary>
        /// Shows all job cards waiting for completion
        /// </summary>
        /// <returns></returns>
        public List<Dropdown> JobCardDropdown()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("SELECT JobCardId Id, JobCardNo Name FROM JobCard").ToList();
            }
        }
    }
}
