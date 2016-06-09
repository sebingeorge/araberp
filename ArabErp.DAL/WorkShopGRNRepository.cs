using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using ArabErp.DAL;
using DapperExtensions;
using System.Data;
using System.Collections;

namespace ArabErp.DAL
{
    public class WorkShopGRNRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public int InsertWorkShopGRN(WorkShopGRN model)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                   IDbTransaction trn = connection.BeginTransaction();
                try
                {
                    int id = 0;

                    string sql = @"insert  into WorkShopGRN(GRNNo,GRNDate,SupplierId,SONoAndDate,CurrencyId,SupplierDCNoAndDate,SpecialRemarks,
                                                Addition,AdditionRemarks,Deduction,DeductionRemarks,GRNTotalAmount,
                                                CreatedBy,CreatedDate,OrganizationId) 

                                        Values (@GRNNo,@GRNDate,@SupplierId,@SONoAndDate,@CurrencyId,@SupplierDCNoAndDate,@SpecialRemarks,
                                                @Addition,@AdditionRemarks,@Deduction,@DeductionRemarks,@GRNTotalAmount,
                                                @CreatedBy,@CreatedDate,@OrganizationId);
                                            SELECT CAST(SCOPE_IDENTITY() as int)";



                id = connection.Query<int>(sql, model, trn).Single();
                var saleorderitemrepo = new WorkShopGRNItemRepository();
                foreach (var item in model.WorkShopGRNItems)
                {
                    item.WorkShopGRNId = id;
                    new WorkShopGRNItemRepository().InsertWGRNItem(item, connection, trn);

                }

                trn.Commit();
                return id;
                }
                catch (Exception)
                {
                    trn.Rollback();
                    throw;
                    return 0;

                }


            }
        }


        public WorkShopGRN GetWorkShopGRN(int WorkShopGRNId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from WorkShopGRN
                        where WorkShopGRNId=@WorkShopGRNId";

                var objWorkShopGRN = connection.Query<WorkShopGRN>(sql, new
                {
                    WorkShopGRNId = WorkShopGRNId
                }).First<WorkShopGRN>();

                return objWorkShopGRN;
            }
        }

        public List<WorkShopGRN> GetWorkShopGRNs()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from WorkShopGRN
                        where isActive=1";

                var objWorkShopGRNs = connection.Query<WorkShopGRN>(sql).ToList<WorkShopGRN>();

                return objWorkShopGRNs;
            }
        }



        public int DeleteWorkShopGRN(Unit objWorkShopGRN)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete WorkShopGRN  OUTPUT DELETED.WorkShopGRNId WHERE WorkShopGRNId=@WorkShopGRNId";


                var id = connection.Execute(sql, objWorkShopGRN);
                return id;
            }
        }

        public IEnumerable<Supplier> GetSupplierList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Supplier>("select * from Supplier");
            }
        }

        public List<Dropdown> FillCurrency()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                return connection.Query<Dropdown>("select CurrencyId Id,CurrencyName Name from Currency").ToList();
            }
        }

        public List<Dropdown> FillItem()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                return connection.Query<Dropdown>("select ItemId Id,ItemName Name from Item").ToList();
            }
        }

        public string GetPartNo(int ItemId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string query = "SELECT PartNo+'|'+UnitName FROM Item INNER JOIN Unit ON UnitId=ItemUnitId WHERE ItemId = @ItemId";
                return connection.Query<string>(query, new { ItemId = ItemId }).First<string>();
            }
        }

    }
}