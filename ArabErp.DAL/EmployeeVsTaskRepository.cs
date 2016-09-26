using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class EmployeeVsTaskRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
    }

}
