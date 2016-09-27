using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;
using System.Collections;

namespace ArabErp.DAL
{
    public class StockJournalRepository:BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public string GetItemDetails(int? itemId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"select * into #item  from  item where ItemId=@ItemId and ISNULL(isActive, 1) = 1;
                                 select ItemId,sum(ISNULL(Quantity,0)) StockQuantity  into #StockUpdate from StockUpdate where ItemId=@ItemId  group by ItemId;
                                select Concat(I.ItemId,'|',PartNo,'|',StockQuantity,'|',UnitName) from #item I left join  #StockUpdate SU on I.ItemId=SU.ItemId
                                Left Join Unit U on U.UnitId=I.ItemUnitId
                                    where ISNULL(U.isActive, 1) = 1;
                                drop table #item;
                                drop table #StockUpdate;";
                return connection.Query<string>(query,
                    new { ItemId = itemId }).First<string>();
            }
        }
        public int InsertStockJournal(StockJournal model)  
        { 
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                IDbTransaction trn  = connection.BeginTransaction();
                try
                {
                    int id = 0;

                    string sql = @"INSERT INTO StockJournal (StockJournalRefno,StockPointId,Remarks,IssuedBy,StockJournalDate,CreatedBy,CreatedDate,OrganizationId) VALUES (@StockJournalRefno,@StockPointId,@Remarks,@IssuedBy,GETDATE(),@CreatedBy,GETDATE(),@OrganizationId);
                                 SELECT CAST(SCOPE_IDENTITY() as int)";


                    id = connection.Query<int>(sql, model, trn).Single();
                    var StockJournalItemsRepo = new StockJournalItemsRepository();
                    int? StockPointId = model.StockPointId;
                    string RefNo = model.StockJournalRefno;
                    string Remarks= model.Remarks;
                    string CreatedBy = model.CreatedBy;
                    int? OrgId = model.OrganizationId;
                    foreach (var item in model.StockJournelItems)
                    {
                        item.StockJournalId = id;
                        item.StockPointId = StockPointId;
                        item.Remarks = Remarks;
                        item.CreatedBy = CreatedBy;
                        item.OrganizationId = OrgId;
                        item.StockJournalRefno = RefNo;
                        new StockJournalItemsRepository().InsertStockJournalItem(item, connection, trn);

                    }
                    InsertLoginHistory(dataConnection, model.CreatedBy, "Create", "Stock Journal", id.ToString(), "0");
                    trn.Commit();
                    return id;
                }
                catch (Exception)
                {
                    trn.Rollback();
                    return 0;
                }


            }
        }

        public IEnumerable PreviousList(int OrganizationId, DateTime? from, DateTime? to, int id, int stockpoint)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT
	                                SJ.StockJournalId,
	                                SJ.StockJournalRefno,
	                                CONVERT(VARCHAR, SJ.StockJournalDate, 106) StockJournalDate,
	                                SP.StockPointName,
	                                ISNULL(SJ.Remarks, '-') Remarks,
	                                EMP.EmployeeName,

	                                STUFF((SELECT ', ' + CAST(T2.ItemName + ' ('+CAST(T1.Quantity AS VARCHAR(MAX))+')' AS VARCHAR(MAX)) [text()]
	                                FROM StockJournalItem T1 INNER JOIN Item T2 ON T1.ItemId = T2.ItemId
	                                WHERE SJ.StockJournalId = T1.StockJournalId
	                                FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'),1,2,'') ItemName

                                FROM StockJournal SJ
	                                INNER JOIN Stockpoint SP ON SJ.StockPointId = SP.StockPointId
	                                INNER JOIN Employee EMP ON SJ.IssuedBy = EMP.EmployeeId
                                WHERE SJ.OrganizationId = 1
	                                AND SJ.isActive = 1
                                AND CONVERT(DATE, SJ.StockJournalDate, 106) BETWEEN ISNULL(@from, DATEADD(MONTH, -1, GETDATE())) AND ISNULL(@to, GETDATE())
                                AND SJ.StockJournalId = ISNULL(NULLIF(CAST(@id AS INT), 0), SJ.StockJournalId)
                                AND SJ.StockPointId = ISNULL(NULLIF(CAST(@stockpoint AS INT), 0), SJ.StockPointId)
                                ORDER BY SJ.StockJournalDate DESC, SJ.CreatedDate DESC";
                return connection.Query<StockJournal>(query,
                new
                {
                    OrganizationId = OrganizationId,
                    id = id,
                    from = from,
                    to = to,
                    stockpoint = stockpoint
                }).ToList();
            }
        }
        public StockJournal GetStockJournalHD(int StockJournalId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string sql = @" SELECT StockJournalId,StockJournalRefno,StockPointId,StockJournalDate,Remarks,E.EmployeeId IssuedBy
                                FROM StockJournal S
                                INNER JOIN Employee E ON E.EmployeeId=S.IssuedBy
                                WHERE StockJournalId=@StockJournalId";

                var objStockJournal = connection.Query<StockJournal>(sql, new
                {
                    StockJournalId = StockJournalId
                }).First<StockJournal>();

                return objStockJournal;
            }
        }

    }
}
