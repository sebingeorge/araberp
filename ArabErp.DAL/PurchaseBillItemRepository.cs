﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class PurchaseBillItemRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
    

        public int InsertPurchaseBillItem(PurchaseBillItem model, IDbConnection connection, IDbTransaction trn)
        {
            try
            {

                string sql = @"insert  into PurchaseBillItem(PurchaseBillId,GRNItemId,Rate,TaxPercentage,TaxAmount,Discount,Amount) Values (@PurchaseBillId,@GRNItemId,@Rate,@TaxPercentage,@TaxAmount,@Discount,@Amount);
                SELECT CAST(SCOPE_IDENTITY() as int)";

                var id = connection.Query<int>(sql, model, trn).Single();
                return id;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public PurchaseBillItem GetPurchaseBillItem(int PurchaseBillItemId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from PurchaseBillItem
                        where PurchaseBillItemId=@PurchaseBillItemId";

                var objPurchaseBillItem = connection.Query<PurchaseBillItem>(sql, new
                {
                    PurchaseBillItemId = PurchaseBillItemId
                }).First<PurchaseBillItem>();

                return objPurchaseBillItem;
            }
        }

        public List<PurchaseBillItem> GetPurchaseBillItems()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from PurchaseBillItem
                        where isActive=1";

                var objPurchaseBillItems = connection.Query<PurchaseBillItem>(sql).ToList<PurchaseBillItem>();

                return objPurchaseBillItems;
            }
        }



        public int DeletePurchaseBillItem(Unit objPurchaseBillItem)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete PurchaseBillItem  OUTPUT DELETED.PurchaseBillItemId WHERE PurchaseBillItemId=@PurchaseBillItemId";


                var id = connection.Execute(sql, objPurchaseBillItem);
                return id;
            }
        }


    }
}