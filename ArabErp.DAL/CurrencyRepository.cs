using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class CurrencyRepository : IDisposable
    {
        private SqlConnection connection;

        public CurrencyRepository()
        {
            if (connection == null)
            {
                connection = ConnectionManager.connection;
            }
        }


        public Currency NewCurrency()
        {
            var cur = new Currency();
            return cur;
        }

        public Currency GetCurrency(int CurrencyId)
        {

            string sql = @"select * from Currency 
                        where CurrencyId=@CurrencyId";

            var objCurrency = connection.Query<Currency>(sql, new
            {
                CurrencyId = CurrencyId
            }).First<Currency>();
            return objCurrency;
        }

        public int InsertCurrency(Currency objCurrency)
        {
            string sql = @"
            INSERT INTO [Currency] (CurrencyRefNo,CurrencyName,Elementary,CurrencyExRate,CurrencySymbolId,OrganizationId) VALUES (@CurrencyRefNo,@CurrencyName,@Elementary,@CurrencySymbolId,@CurrencyExRate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";



            var id = connection.Query<int>(sql, new
            {
                CurrencyRefNo = objCurrency.CurrencyRefNo,
                CurrencyName = objCurrency.CurrencyName,
                Elementary = objCurrency.Elementary,
                CurrencyExRate = objCurrency.CurrencyExRate,
                CurrencySymbolId = objCurrency.CurrencySymbolId,
                OrganizationId = objCurrency.OrganizationId

            }).Single();
            return id;
        }

        public void Dispose()
        {
            connection.Dispose();
        }
    }
}
