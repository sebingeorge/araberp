using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class ProjectCostRepository : BaseRepository
    {
    

          static string dataConnection = GetConnectionString("arab");

          public int InsertProjectCosting(ProjectCost objProjectCostItem, IDbConnection connection, IDbTransaction trn)
          {
              try
              {

                
                  string sql = @"insert  into ProjectCosting(QuerySheetId,CostingId,Remarks,Amount) 
                                                    Values (@QuerySheetId,@CostingId,@Remarks,@Amount);
                       
                SELECT CAST(SCOPE_IDENTITY() as int)";


                  var id = connection.Query<int>(sql, objProjectCostItem, trn).First();
                  return id;
              }
              catch (Exception)
              {
                  throw;
              }

          }

      
    }
}
