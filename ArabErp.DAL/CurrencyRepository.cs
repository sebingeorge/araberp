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
        public string ConnectionString()
        {
            return dataConnection;
        }
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

                return connection.Query<Currency>("SELECT CurrencyId,CurrencyRefNo,CurrencyName,Elementary,CurrencyExRate,CurrencySymbolId, SymbolId,SymbolName FROM Currency C LEFT jOIN Symbol S ON  C.CurrencySymbolId=S.SymbolId where C.isActive=1").ToList();
            }
        }

        


        public Currency InsertCurrency(Currency model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction trn = connection.BeginTransaction();
                string sql = @"
            INSERT INTO [Currency] (CurrencyRefNo,CurrencyName,Elementary,CurrencyExRate,CurrencySymbolId,OrganizationId) VALUES (@CurrencyRefNo,@CurrencyName,@Elementary,@CurrencyExRate,@CurrencySymbolId,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";

                int id = 0;

                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(Currency).Name, "0", 1);
                    model.CurrencyRefNo = "CUR/" + internalid;
                    id = connection.Query<int>(sql, model, trn).Single();
                    model.CurrencyId = id;

                    trn.Commit();
                }
                catch (Exception e)
                {
                    trn.Rollback();
                    model.CurrencyId = 0;
                    model.CurrencyRefNo = null;
                }
                return model;
            }
        }
        public Currency UpdateCurrency(Currency model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE Currency SET CurrencyName = @CurrencyName,Elementary = @Elementary,CurrencyExRate = @CurrencyExRate,
                                CurrencySymbolId = @CurrencySymbolId,CreatedBy = @CreatedBy,CreatedDate= GETDATE(),OrganizationId = @OrganizationId 
                                OUTPUT INSERTED.CurrencyId  WHERE CurrencyId = @CurrencyId";

                try
                {
                    var id = connection.Execute(sql, model);
                    model.CurrencyId = id;
                }
                catch (Exception ex)
                {

                    model.CurrencyId = 0;

                }
                return model;
            }
        }
        public Currency DeleteCurrency(Currency model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE Currency SET isActive = 0 OUTPUT INSERTED.CurrencyId  WHERE CurrencyId = @CurrencyId";

                try
                {
                    var id = connection.Execute(sql, model);
                    model.CurrencyId = id;
                }
                catch (Exception ex)
                {

                    model.CurrencyId = 0;

                }
                return model;
            }
        }
        public Currency GetCurrencyFrmOrganization(int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select S.SymbolName,C.CurrencyName from Organization O 
                             inner join Currency C on C.CurrencyId = O.CurrencyId
                             inner join Symbol S on C.CurrencySymbolId = S.SymbolId
                             where O.OrganizationId=@OrganizationId";
                var objCurrency = connection.Query<Currency>(sql, new
                {
                    OrganizationId = OrganizationId
                }).First<Currency>();
                return objCurrency;
            }
        }
     
    }
}
