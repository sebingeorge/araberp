using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class SalesQuotationRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public int InsertSalesQuotation(SalesQuotation objSalesQuotation)
        {

   
                using (IDbConnection connection = OpenConnection(dataConnection))
                {
                    IDbTransaction trn = connection.BeginTransaction();
                    try
                    {
                        string sql = @"insert  into SalesQuotation(QuotationRefNo,QuotationDate,CustomerId,ContactPerson,SalesExecutiveId,PredictedClosingDate,QuotationValidToDate,ExpectedDeliveryDate,IsQuotationApproved,ApprovedBy,Amount,QuotationStatus,Remarks,SalesQuotationRejectReasonId,QuotationRejectReason,Competitors,PaymentTerms,DiscountRemarks,CreatedBy,CreatedDate,OrganizationId) Values (@QuotationRefNo,@QuotationDate,@CustomerId,@ContactPerson,@SalesExecutiveId,@PredictedClosingDate,@QuotationValidToDate,@ExpectedDeliveryDate,@IsQuotationApproved,@ApprovedBy,@Amount,@QuotationStatus,@Remarks,@SalesQuotationRejectReasonId,@QuotationRejectReason,@Competitors,@PaymentTerms,@DiscountRemarks,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                        var id = connection.Query<int>(sql, objSalesQuotation, trn).Single();

                        var saleorderitemrepo = new SalesQuotationItemRepository();
                        foreach (var item in objSalesQuotation.SalesQuotationItems)
                        {
                            item.SalesQuotationId = id;
                            saleorderitemrepo.InsertSalesQuotationItem(item, connection, trn);
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


        public SalesQuotation GetSalesQuotation(int SalesQuotationId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from SalesQuotation
                        where SalesQuotationId=@SalesQuotationId";

                var objSalesQuotation = connection.Query<SalesQuotation>(sql, new
                {
                    SalesQuotationId = SalesQuotationId
                }).First<SalesQuotation>();

                return objSalesQuotation;
            }
        }

        public List<SalesQuotation> GetSalesQuotations()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from SalesQuotation
                        where isActive=1";

                var objSalesQuotations = connection.Query<SalesQuotation>(sql).ToList<SalesQuotation>();

                return objSalesQuotations;
            }
        }



        public int DeleteSalesQuotation(Unit objSalesQuotation)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete SalesQuotation  OUTPUT DELETED.SalesQuotationId WHERE SalesQuotationId=@SalesQuotationId";


                var id = connection.Execute(sql, objSalesQuotation);
                return id;
            }
        }

    
        public List<Dropdown> FillSalesQuotationRejectReason()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                return connection.Query<Dropdown>("select SalesQuotationRejectReasonId Id,ReasonDescription Name from SalesQuotationRejectReason").ToList();
            }
        }
    }
}