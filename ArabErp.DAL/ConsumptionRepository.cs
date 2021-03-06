﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;
using System.Collections;

namespace ArabErp.DAL
{
    public class ConsumptionRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public string InsertConsumption(Consumption objConsumption)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    var internalId = DatabaseCommonRepository.GetNewDocNo(connection, objConsumption.OrganizationId, 23, true,txn);

                    objConsumption.ConsumptionNo = internalId;

                    objConsumption.TotalAmount = objConsumption.ConsumptionItems.Sum(m => m.Amount);

                    string sql = @"insert into Consumption(ConsumptionNo,ConsumptionDate,JobCardId,TotalAmount,SpecialRemarks,CreatedBy,CreatedDate,OrganizationId) Values (@ConsumptionNo,@ConsumptionDate,@JobCardId,@TotalAmount,@SpecialRemarks,@CreatedBy,@CreatedDate,@OrganizationId);
                        SELECT CAST(SCOPE_IDENTITY() as int)";

                    var id = connection.Query<int>(sql, objConsumption, txn).Single();

                    foreach (ConsumptionItem item in objConsumption.ConsumptionItems)
                    {
                        item.ConsumptionId = id;
                        new ConsumptionItemRepository().InsertConsumptionItem(item, connection, txn);
                    }
                    InsertLoginHistory(dataConnection, objConsumption.CreatedBy, "Create", "Consumption", id.ToString(), "0");
                    txn.Commit();

                    return id + "|CON/" + internalId;
                }
                catch (Exception)
                {
                    txn.Rollback();
                    return "0";
                }
            }
        }

        public Consumption GetConsumptionHD(int ConsumptionId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string sql = @" SELECT ConsumptionNo,ConsumptionDate,C.JobCardId,CONVERT(Varchar(15),J.JobCardDate,106)JobCardDate,
                                CONCAT(S.SaleOrderRefNo,' - ',CONVERT(Varchar(15),S.SaleOrderDate,106))SONODATE,
                                F.FreezerUnitName,B.BoxName,C.TotalAmount,C.SpecialRemarks 
                                FROM Consumption C
                                INNER JOIN JobCard J ON J.JobCardId=C.JobCardId
                                INNER JOIN SaleOrder S ON S.SaleOrderId=J.SaleOrderId
                                LEFT JOIN FreezerUnit F ON F.FreezerUnitId=J.FreezerUnitId
                                LEFT JOIN Box B ON B.BoxId=J.BoxId
                                WHERE ConsumptionId=@ConsumptionId";

                var objConsumption = connection.Query<Consumption>(sql, new
                {
                    ConsumptionId = ConsumptionId
                }).First<Consumption>();

                return objConsumption;
            }
        }

         public List<Consumption> GetConsumptions()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from Consumption
                        where isActive=1";

                var objConsumptions = connection.Query<Consumption>(sql).ToList<Consumption>();

                return objConsumptions;
            }
        }



        public int DeleteConsumption(Unit objConsumption)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete Consumption  OUTPUT DELETED.ConsumptionId WHERE ConsumptionId=@ConsumptionId";
                var id = connection.Execute(sql, objConsumption);
                InsertLoginHistory(dataConnection, objConsumption.CreatedBy, "Delete", "Consumption", id.ToString(), "0");
                return id;
            }
        }



        public IEnumerable PreviousList(int OrganizationId, DateTime? from, DateTime? to, int id, int jobcard)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"SELECT
	                                C.ConsumptionId,
	                                C.ConsumptionNo,
	                                CONVERT(VARCHAR, C.ConsumptionDate, 106) ConsumptionDate,
	                                JC.JobCardNo,
	                                CONVERT(VARCHAR, JC.JobCardDate, 106) JobCardDate,
	                                ISNULL(C.SpecialRemarks, '-') SpecialRemarks,
                                    C.TotalAmount,
                                    
	                                STUFF((SELECT ', ' + CAST(T2.ItemName + ' ('+CAST(T1.Amount AS VARCHAR)+')' AS VARCHAR) [text()]
	                                FROM ConsumptionItem T1 INNER JOIN Item T2 ON T1.ItemId = T2.ItemId
	                                WHERE C.ConsumptionId = T1.ConsumptionId
	                                FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'),1,2,'') ItemName
                                    
                                FROM Consumption C
	                                INNER JOIN JobCard JC ON C.JobCardId = JC.JobCardId
                                WHERE C.OrganizationId = @OrganizationId
	                                AND C.isActive = 1  
                                    AND CONVERT(DATE, C.ConsumptionDate, 106) BETWEEN ISNULL(@from, DATEADD(MONTH, -1, GETDATE())) AND ISNULL(@to, GETDATE())
                                    AND C.ConsumptionId = ISNULL(NULLIF(CAST(@id AS INT), 0), C.ConsumptionId)
                                    AND C.JobCardId = ISNULL(NULLIF(CAST(@jobcard AS INT), 0), C.JobCardId)
                                ORDER BY C.ConsumptionDate DESC, C.CreatedDate DESC";

                return connection.Query<Consumption>(sql, new
                {
                    OrganizationId = OrganizationId,
                    id = id,
                    from = from,
                    to = to,
                    jobcard = jobcard
                }).ToList();
            }
        }
    }
}
