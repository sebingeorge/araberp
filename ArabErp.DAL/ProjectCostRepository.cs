﻿using System;
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
          public int InsertQuerySheetItem(QuerySheetItem objQuerySheetItem, IDbConnection connection, IDbTransaction trn)
          {
              try
              {


                  string sql = @"insert  into QuerySheetItem(QuerySheetId,RoomDetails,ExternalRoomDimension,ColdRoomArea,ColdRoomLocation,TemperatureRequired,PanelThicknessANDSpec,DoorSizeTypeAndNumberOfDoor,FloorDetails,ProductDetails,ProductIncomingTemperature,PipeLength,Refrigerant,EletricalPowerAvailability,Kilowatt,isActive) 
                                                    Values (@QuerySheetId,@RoomDetails,@ExternalRoomDimension,@ColdRoomArea,@ColdRoomLocation,@TemperatureRequired,@PanelThicknessANDSpec,@DoorSizeTypeAndNumberOfDoor,@FloorDetails,@ProductDetails,@ProductIncomingTemperature,@PipeLength,@Refrigerant,@EletricalPowerAvailability,@Kilowatt,1);
                       
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
