using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

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
    }
}
