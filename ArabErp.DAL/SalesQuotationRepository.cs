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
        public IEnumerable<SalesQuotationList> GetSalesQuotaationList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = string.Empty;
                sql += " select Q.SalesQuotationId, Q.QuotationRefNo, Q.QuotationDate, C.CustomerName, E.EmployeeName,";
                sql += " Q.Amount";
                sql += " from SalesQuotation Q ";
                sql += " inner join Customer C on C.CustomerId = Q.CustomerId";
                sql += " inner join Employee E on E.EmployeeId = Q.SalesExecutiveId";
                sql += " where Q.isActive = 1";

                return connection.Query<SalesQuotationList>(sql);
            }
        }
        public SalesQuotation InsertSalesQuotation(SalesQuotation model)
        {

   
                using (IDbConnection connection = OpenConnection(dataConnection))
                {
                   
                    IDbTransaction trn = connection.BeginTransaction();
                    try
                    {
                        int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(SalesQuotation).Name, "0",1);
                        model.QuotationRefNo = "SQ/" + internalid;
                        string sql = @"
                                        insert  into SalesQuotation(QuotationRefNo,QuotationDate,CustomerId,ContactPerson,SalesExecutiveId,PredictedClosingDate,QuotationValidToDate,ExpectedDeliveryDate,IsQuotationApproved,ApprovedBy,Amount,QuotationStatus,Remarks,SalesQuotationRejectReasonId,QuotationRejectReason,Competitors,PaymentTerms,DiscountRemarks,CreatedBy,CreatedDate,OrganizationId)
                                        Values (@QuotationRefNo,@QuotationDate,@CustomerId,@ContactPerson,@SalesExecutiveId,@PredictedClosingDate,@QuotationValidToDate,@ExpectedDeliveryDate,@IsQuotationApproved,@ApprovedBy,@Amount,@QuotationStatus,@Remarks,@SalesQuotationRejectReasonId,@QuotationRejectReason,@Competitors,@PaymentTerms,@DiscountRemarks,@CreatedBy,@CreatedDate,@OrganizationId);
                                        SELECT CAST(SCOPE_IDENTITY() as int) SalesQuotationId";

                        model.SalesQuotationId = connection.Query<int>(sql, model, trn).First<int>();

                        var saleorderitemrepo = new SalesQuotationItemRepository();
                        foreach (var item in model.SalesQuotationItems)
                        {
                            item.SalesQuotationId = model.SalesQuotationId;
                            saleorderitemrepo.InsertSalesQuotationItem(item, connection, trn);
                        }

                        trn.Commit();

                      
                    }
                    catch (Exception)
                    {
                        trn.Rollback();
                        model.SalesQuotationId = 0;
                        model.QuotationRefNo = null;
                        throw;
                      

                    }
                    return model;
                }
            
        
        }
        public SalesQuotation ReviseSalesQuotation(SalesQuotation model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = string.Empty;
                IDbTransaction trn = connection.BeginTransaction();
                try
                {
                    sql = "update SalesQuotation set isActive = 0 where SalesQuotationId = " + model.ParentId.ToString() + ";";
                    sql += "update SalesQuotation set isActive = 0 where SalesQuotationId = " + model.GrantParentId.ToString() + ";";
                    sql += "update SalesQuotation set isActive = 0 where SalesQuotationId = " + model.SalesQuotationId.ToString() + ";";

                    connection.Query(sql, null, trn);

                    sql = string.Empty;
                    sql = "select count(*)+1 from SalesQuotation where GrantParentId = " + model.GrantParentId.ToString() + ";";

                    int RevisionId = connection.Query<int>(sql, null, trn).Single();

                    model.RevisionNo = RevisionId;

                    string refno = connection.Query<string>("select QuotationRefNo from SalesQuotation where SalesQuotationId = " + model.GrantParentId.ToString(), null,trn).Single();
                    //int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(SalesQuotation).Name, "0", 1);
                    model.QuotationRefNo = refno + "/REV"+RevisionId.ToString();
                    sql = @"
                            insert  into SalesQuotation(QuotationRefNo,QuotationDate,CustomerId,ContactPerson,SalesExecutiveId,PredictedClosingDate,QuotationValidToDate,ExpectedDeliveryDate,IsQuotationApproved,ApprovedBy,Amount,QuotationStatus,Remarks,SalesQuotationRejectReasonId,QuotationRejectReason,Competitors,PaymentTerms,DiscountRemarks,CreatedBy,CreatedDate,OrganizationId,ParentId,GrantParentId,RevisionNo)
                            Values (@QuotationRefNo,@QuotationDate,@CustomerId,@ContactPerson,@SalesExecutiveId,@PredictedClosingDate,@QuotationValidToDate,@ExpectedDeliveryDate,@IsQuotationApproved,@ApprovedBy,@Amount,@QuotationStatus,@Remarks,@SalesQuotationRejectReasonId,@QuotationRejectReason,@Competitors,@PaymentTerms,@DiscountRemarks,@CreatedBy,@CreatedDate,@OrganizationId,@ParentId,@GrantParentId,@RevisionNo);
                            SELECT CAST(SCOPE_IDENTITY() as int) SalesQuotationId";

                    model.SalesQuotationId = connection.Query<int>(sql, model, trn).First<int>();

                    var saleorderitemrepo = new SalesQuotationItemRepository();
                    foreach (var item in model.SalesQuotationItems)
                    {
                        item.SalesQuotationId = model.SalesQuotationId;
                        saleorderitemrepo.InsertSalesQuotationItem(item, connection, trn);
                    }

                    trn.Commit();


                }
                catch (Exception)
                {
                    trn.Rollback();
                    model.SalesQuotationId = 0;
                    model.QuotationRefNo = null;
                    throw;


                }
                return model;
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
                string sql = @"Update SalesQuotation  SET ApprovedBy=@ApprovedBy  OUTPUT INSERTED.SalesQuotationId WHERE SalesQuotationId=@SalesQuotationId";


                var id = connection.Query(sql, new { ApprovedBy = objSalesQuotation.ApprovedBy , SalesQuotationId = objSalesQuotation.SalesQuotationId });
                
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