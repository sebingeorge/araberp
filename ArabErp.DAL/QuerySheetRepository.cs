using System;
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
    int internalId = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, txn, typeof(SaleOrder).Name, "0", 1);

                    objQuerySheet.QuerySheetRefNo= "QSH/" + internalId;




                string sql = @"insert  into QuerySheet(QuerySheetRefNo,ProjectName,ContactPerson,ContactNumber,Email,RoomDetails,ExternalRoomDimension,ColdRoomArea,ColdRoomLocation,TemperatureRequired,PanelThicknessANDSpec,DoorSizeTypeAndNumberOfDoor,FloorDetails,ProductDetails,ProductIncomingTemperature,PipeLength,Refrigerant,EletricalPowerAvailability,CreatedBy,CreatedDate,OrganizationId) Values (@QuerySheetRefNo,@ProjectName,@ContactPerson,@ContactNumber,@Email,@RoomDetails,@ExternalRoomDimension,@ColdRoomArea,@ColdRoomLocation,@TemperatureRequired,@PanelThicknessANDSpec,@DoorSizeTypeAndNumberOfDoor,@FloorDetails,@ProductDetails,@ProductIncomingTemperature,@PipeLength,@Refrigerant,@EletricalPowerAvailability,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objQuerySheet,txn).Single();


                    txn.Commit();

                    return id + "|QSH/" + internalId;
                }
                catch (Exception ex )
                {
                    
                    txn.Rollback();
                    throw ex;
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

        public List<QuerySheet> GetQuerySheets()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from QuerySheet
                        where isActive=1";

                var objQuerySheets = connection.Query<QuerySheet>(sql).ToList<QuerySheet>();

                return objQuerySheets;
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


    }
}