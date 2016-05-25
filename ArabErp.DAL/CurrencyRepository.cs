using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class CurrencyRepository : BaseRepository 
    {
        static string dataConnection = GetConnectionString("arab");
        public Currency GetCurrency(int CurrencyId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from Currency 
                        where CurrencyId=@CurrencyId";

                var objCurrency = connection.Query<Currency>(sql, new
                {
                    CurrencyId = CurrencyId
                }).First<Currency>();
                return objCurrency;
            }
        }
        public Currency FillSymbol()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var sym = new Currency();
                sym.Symbols = connection.Query<Symbol>("select SymbolId,SymbolName from Symbol").ToList();

                return sym;
            }
        }
        public IEnumerable<Currency> FillCurrencyList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //var sym = new Currency();
                //sym = connection.Query<Currency>("select SymbolId,SymbolName from Symbol").ToList();

                return connection.Query<Currency>("SELECT CurrencyId,CurrencyRefNo,CurrencyName,Elementary,CurrencyExRate,CurrencySymbolId, SymbolId,SymbolName FROM Currency C LEFT jOIN Symbol S ON  C.CurrencySymbolId=S.SymbolId").ToList();
            }
        }

        


        public int InsertCurrency(Currency objCurrency)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"
            INSERT INTO [Currency] (CurrencyRefNo,CurrencyName,Elementary,CurrencyExRate,CurrencySymbolId,OrganizationId) VALUES (@CurrencyRefNo,@CurrencyName,@Elementary,@CurrencyExRate,@CurrencySymbolId,@OrganizationId);
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
        }

     
    }
}
