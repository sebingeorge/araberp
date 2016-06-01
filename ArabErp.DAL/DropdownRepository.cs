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
        /// Return all job cards waiting for completion
        /// </summary>
        /// <returns></returns>
        public List<Dropdown> JobCardDropdown()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("SELECT JobCardId Id, JobCardNo Name FROM JobCard WHERE ISNULL(JodCardCompleteStatus, 0) = 0 AND ISNULL(isActive, 1) = 1").ToList();
            }
        }
        /// <summary>
        /// Return all items that are active
        /// </summary>
        /// <returns></returns>
        public List<Dropdown> ItemDropdown()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("SELECT ItemId Id, ItemName Name FROM Item WHERE ISNULL(isActive, 1) = 1").ToList();
            }
        }
    }
}
