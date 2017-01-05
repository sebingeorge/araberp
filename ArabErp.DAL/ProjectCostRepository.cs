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

      
          public int InsertProjectCosting(QuerySheet model)
          {
              using (IDbConnection connection = OpenConnection(dataConnection))
              {
                  IDbTransaction txn = connection.BeginTransaction();
                  try
                  {

                      //                  
                      var row = 0;
                      foreach (ProjectCost item in model.Items)
                      {
                          item.QuerySheetId = model.QuerySheetId;
                          item.Type = model.Type;
                          string sql = @"insert  into ProjectCosting(QuerySheetId,CostingId,Remarks,Amount) Values (@QuerySheetId,@CostingId,@Remarks,@Amount)
                                                    
                                       UPDATE QuerySheet  SET Type=@Type WHERE QuerySheetId = @QuerySheetId";

                          row = connection.Execute(sql, item, txn);

                      }

                      #region updating costing amount in [QuerySheet] table
                      model.CostingAmount = model.Items.Sum(x => x.Amount);
                      string query = @"UPDATE QuerySheet SET CostingAmount = @CostingAmount WHERE QuerySheetId = @QuerySheetId";
                      connection.Execute(query, new { QuerySheetId = model.QuerySheetId, CostingAmount = model.CostingAmount }, txn);
                      #endregion

                      InsertLoginHistory(dataConnection, model.CreatedBy, "Update", typeof(QuerySheet).Name, model.QuerySheetId.ToString(), model.OrganizationId.ToString());
                      txn.Commit();
                      return row;


                  }
                  catch (Exception ex)
                  {
                      txn.Rollback();
                      throw ex;
                  }
              }
          }
          public int InsertQuerySheetItem(QuerySheetItem objQuerySheetItem, IDbConnection connection, IDbTransaction trn)
          {
              try
              {


                  string sql = @"insert  into QuerySheetItem(QuerySheetId,RoomDetails,ExternalRoomDimension,ColdRoomArea,ColdRoomLocation,TemperatureRequired,PanelThicknessANDSpec,DoorSizeTypeAndNumberOfDoor,FloorDetails,ProductDetails,ProductIncomingTemperature,PipeLength,Refrigerant,EletricalPowerAvailability,Kilowatt, Cost, isActive) 
                    Values (@QuerySheetId,@RoomDetails,@ExternalRoomDimension,@ColdRoomArea,@ColdRoomLocation,@TemperatureRequired,@PanelThicknessANDSpec,@DoorSizeTypeAndNumberOfDoor,@FloorDetails,@ProductDetails,@ProductIncomingTemperature,@PipeLength,@Refrigerant,@EletricalPowerAvailability,@Kilowatt, @Cost,1);
                       
                SELECT CAST(SCOPE_IDENTITY() as int)";


                  var id = connection.Query<int>(sql, objQuerySheetItem, trn).First();

                  return id;
              }
              catch (Exception)
              {
                  throw;
              }

          }

          public List<ProjectCost> GetProjectCost(int QuerySheetId)
          {
              using (IDbConnection connection = OpenConnection(dataConnection))
              {
                  string sql = @" SELECT ProjectCostingId,P.CostingId,QuerySheetId,Description,Amount,Remarks
                                  FROM ProjectCosting P
                                  INNER JOIN  CostingParameters C ON P.CostingId=C.CostingId
                                  WHERE P.QuerySheetId = @QuerySheetId";

                  var objQuerySheetItems = connection.Query<ProjectCost>(sql, new { QuerySheetId = QuerySheetId }).ToList<ProjectCost>();

                  return objQuerySheetItems;
              }
          }

          public List<QuerySheetItem> GetQuerySheetItem(int QuerySheetId)
          {
              using (IDbConnection connection = OpenConnection(dataConnection))
              {
                  string sql = @" SELECT * from QuerySheetItem
                                  WHERE QuerySheetId = @QuerySheetId";
                  return connection.Query<QuerySheetItem>(sql, new { QuerySheetId = QuerySheetId }).ToList();
              }
          }


      
    }
}
