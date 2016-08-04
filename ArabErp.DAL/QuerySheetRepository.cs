﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class QuerySheetRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public string InsertQuerySheet(QuerySheet objQuerySheet)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
             IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    int internalId = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, txn, typeof(QuerySheet).Name, "0", 1);
                    objQuerySheet.QuerySheetRefNo= "QSH/" + internalId;

                    string sql = @"insert  into QuerySheet(QuerySheetRefNo,QuerySheetDate,ProjectName,ContactPerson,ContactNumber,Email,RoomDetails,ExternalRoomDimension,ColdRoomArea,ColdRoomLocation,TemperatureRequired,PanelThicknessANDSpec,DoorSizeTypeAndNumberOfDoor,FloorDetails,ProductDetails,ProductIncomingTemperature,PipeLength,Refrigerant,EletricalPowerAvailability,Kilowatt,CreatedBy,CreatedDate,OrganizationId)
                                 Values (@QuerySheetRefNo,@QuerySheetDate,@ProjectName,@ContactPerson,@ContactNumber,@Email,@RoomDetails,@ExternalRoomDimension,@ColdRoomArea,@ColdRoomLocation,@TemperatureRequired,@PanelThicknessANDSpec,@DoorSizeTypeAndNumberOfDoor,@FloorDetails,@ProductDetails,@ProductIncomingTemperature,@PipeLength,@Refrigerant,@EletricalPowerAvailability,@Kilowatt,@CreatedBy,@CreatedDate,@OrganizationId);
                             SELECT CAST(SCOPE_IDENTITY() as int)";

                    var id = connection.Query<int>(sql, objQuerySheet, txn).Single();

                    foreach (ProjectCost item in objQuerySheet.Items)
                    {
                        item.QuerySheetId= id;
                        new ProjectCostRepository().InsertProjectCosting(item, connection, txn);
                    }

                    txn.Commit();

                    return id + "|QSH/" + internalId;
                }
                catch (Exception)
                {
                    txn.Rollback();
                    return "0";
                }
            }
        }
                
                   
        public QuerySheet GetQuerySheet(int QuerySheetId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from QuerySheet
                        where QuerySheetId=@QuerySheetId";

                var objQuerySheet = connection.Query<QuerySheet>(sql, new
                {
                    QuerySheetId = QuerySheetId
                }).First<QuerySheet>();

                return objQuerySheet;
            }
        }



        public IEnumerable<QuerySheet> GetQuerySheets(int id, int OrganizationId, DateTime? from, DateTime? to)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = " select * from QuerySheet";
                sql += " where isActive=1 and OrganizationId = @OrganizationId ";
                sql += " and QuerySheetDate BETWEEN ISNULL(@from, DATEADD(MONTH, -1, GETDATE())) AND ISNULL(@to, GETDATE()) ";
                sql += " and QuerySheetId = ISNULL(NULLIF(@id, 0),QuerySheetId)";
                return connection.Query<QuerySheet>(sql, new { OrganizationId = OrganizationId, id = id, to = to, from = from }).ToList();

            }
        }



        public int DeleteQuerySheet(Unit objQuerySheet)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete QuerySheet  OUTPUT DELETED.QuerySheetId WHERE QuerySheetId=@QuerySheetId";


                var id = connection.Execute(sql, objQuerySheet);
                return id;
            }
        }
        public IEnumerable<ProjectCost> GetProjectCostingParameter()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"select CostingId,Description,''Remarks,0.00 Amount from CostingParameters";

                return connection.Query<ProjectCost>(query);
            }
        }


    }
}