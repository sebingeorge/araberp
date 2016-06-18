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
        public SalesQuotation InsertSalesQuotation(SalesQuotation objSalesQuotation)
        {

   
                using (IDbConnection connection = OpenConnection(dataConnection))
                {
                    var result = new SalesQuotation();
                    IDbTransaction trn = connection.BeginTransaction();
                    try
                    {
                        string sql = @"DECLARE	@return_value int,
                                        @INTERNALID bigint,
                                        @ERRORCODE nvarchar(100)
                                        EXEC	@return_value = [dbo].[GET_NEXT_SYSTEM_INTERNALID]
                                        @UNIQUEID = N'0',
                                        @DOCUMENTTYPEID = N'SALES QUOTATION',
                                        @DOUPDATE = 1,
                                        @INTERNALID = @INTERNALID OUTPUT,
                                        @ERRORCODE = @ERRORCODE OUTPUT;
                                        insert  into SalesQuotation(QuotationRefNo,QuotationDate,CustomerId,ContactPerson,SalesExecutiveId,PredictedClosingDate,QuotationValidToDate,ExpectedDeliveryDate,IsQuotationApproved,ApprovedBy,Amount,QuotationStatus,Remarks,SalesQuotationRejectReasonId,QuotationRejectReason,Competitors,PaymentTerms,DiscountRemarks,CreatedBy,CreatedDate,OrganizationId)
                                        Values (CONCAT('SQ/',@INTERNALID),@QuotationDate,@CustomerId,@ContactPerson,@SalesExecutiveId,@PredictedClosingDate,@QuotationValidToDate,@ExpectedDeliveryDate,@IsQuotationApproved,@ApprovedBy,@Amount,@QuotationStatus,@Remarks,@SalesQuotationRejectReasonId,@QuotationRejectReason,@Competitors,@PaymentTerms,@DiscountRemarks,@CreatedBy,@CreatedDate,@OrganizationId);
                                        SELECT CAST(SCOPE_IDENTITY() as int) SalesQuotationId,CONCAT('SQ/',@INTERNALID) QuotationRefNo";

                         result = connection.Query<SalesQuotation>(sql, objSalesQuotation, trn).First<SalesQuotation>();

                        var saleorderitemrepo = new SalesQuotationItemRepository();
                        foreach (var item in objSalesQuotation.SalesQuotationItems)
                        {
                            item.SalesQuotationId = result.SalesQuotationId;
                            saleorderitemrepo.InsertSalesQuotationItem(item, connection, trn);
                        }

                        trn.Commit();

                      
                    }
                    catch (Exception)
                    {
                        trn.Rollback();
                        result.SalesQuotationId = 0;
                        result.QuotationRefNo = null;
                        throw;
                      

                    }
                    return result;
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

        public void ApproveSalesQuotation(SalesQuotation objSalesQuotation)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Update SalesQuotation  SET ApprovedBy=@ApprovedBy WHERE SalesQuotationId=@SalesQuotationId";


                var id = connection.Execute(sql, new { ApprovedBy = objSalesQuotation.ApprovedBy , SalesQuotationId = objSalesQuotation.SalesQuotationId });
                
            }
        }

        public List<SalesQuotationItem> GetSalesQuotationItems(int SalesQuotationId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from SalesQuotationItem
                        where SalesQuotationId=@SalesQuotationId";

                var SalesQuotationItems = connection.Query<SalesQuotationItem>(sql, new
                {
                    SalesQuotationId = SalesQuotationId
                }).ToList<SalesQuotationItem>();

                return SalesQuotationItems;
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
        public List<SalesQuotation> GetSalesQuotationApproveList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select E.EmployeeName SalesExecutiveName ,C.CustomerName,SQ.*,C.DoorNo +','+ C.Street+','+C.State CustomerAddress from SalesQuotation SQ 
                            inner join Customer C on SQ.CustomerId=C.CustomerId inner join Employee E
                            on  E.EmployeeId =SQ.SalesExecutiveId
                        where SQ.ApprovedBy is null and  SQ.isActive=1";

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