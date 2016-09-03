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
        public IEnumerable<SalesQuotationList> GetSalesQuotaationList(int isProjectBased)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select Q.SalesQuotationId, Q.QuotationRefNo, Q.QuotationDate, C.CustomerName, E.EmployeeName, Q.Amount,RR.ReasonDescription
                               from SalesQuotation Q 
                               inner join Customer C on C.CustomerId = Q.CustomerId
                               inner join Employee E on E.EmployeeId = Q.SalesExecutiveId
                               inner join SalesQuotationRejectReason RR on RR.SalesQuotationRejectReasonId=q.SalesQuotationRejectReasonId
                               where Q.isActive = 1 and isProjectBased ="+ isProjectBased;

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
                        //int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(SalesQuotation).Name, "0",1);
                        //model.QuotationRefNo = "SQ/" + internalid;
                        if (model.isProjectBased == 0)
                        {
                            model.QuotationRefNo = DatabaseCommonRepository.GetNewDocNo(connection, model.OrganizationId, 1, true,trn);
                        }
                        else if (model.isProjectBased == 1)
                        {
                            model.QuotationRefNo = DatabaseCommonRepository.GetNewDocNo(connection, model.OrganizationId, 2, true,trn);
                        }
                        else if (model.isProjectBased == 2)
                        {
                            model.QuotationRefNo = DatabaseCommonRepository.GetNewDocNo(connection, model.OrganizationId, 28, true, trn);
                        }
                        #region automatically approve if no custom rates are set
                        if (model.isProjectBased == 0)
                        {
                            List<int> rateType = (from SalesQuotationItem s in model.SalesQuotationItems
                                                  where s.RateType == 0
                                                  select s.RateType).ToList();
                            if (rateType.Count > 0)
                                model.IsQuotationApproved = false;
                            else model.IsQuotationApproved = true;
                        }
                        #endregion

                        string sql = @" insert  into SalesQuotation(QuotationRefNo,QuotationDate,CustomerId,ContactPerson,SalesExecutiveId,PredictedClosingDate,
                                        QuotationValidToDate,ExpectedDeliveryDate,IsQuotationApproved,ApprovedBy,Amount,CurrencyId,QuotationStatus,Remarks,SalesQuotationRejectReasonId,
                                        QuotationRejectReason,Competitors,PaymentTerms,DiscountRemarks,CreatedBy,CreatedDate,OrganizationId,isProjectBased,QuerySheetId,isWarranty)
                                        Values (@QuotationRefNo,@QuotationDate,@CustomerId,@ContactPerson,@SalesExecutiveId,@PredictedClosingDate,@QuotationValidToDate,
                                        @ExpectedDeliveryDate,@IsQuotationApproved,@ApprovedBy,@Amount,@CurrencyId,@QuotationStatus,@Remarks,@SalesQuotationRejectReasonId,
                                        @QuotationRejectReason,@Competitors,@PaymentTerms,@DiscountRemarks,@CreatedBy,@CreatedDate,@OrganizationId,@isProjectBased,@QuerySheetId,@isWarranty);
                                        SELECT CAST(SCOPE_IDENTITY() as int) SalesQuotationId";

                        model.SalesQuotationId = connection.Query<int>(sql, model, trn).First<int>();

                        var saleorderitemrepo = new SalesQuotationItemRepository();
                        foreach (var item in model.SalesQuotationItems)
                        {
                            item.SalesQuotationId = model.SalesQuotationId;
                            saleorderitemrepo.InsertSalesQuotationItem(item, connection, trn);
                        }
                        InsertLoginHistory(dataConnection, model.CreatedBy, "Create", "Sales Quotation", model.SalesQuotationId.ToString(), "0");
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
                            insert  into SalesQuotation(QuotationRefNo,QuotationDate,CustomerId,ContactPerson,SalesExecutiveId,PredictedClosingDate,QuotationValidToDate,ExpectedDeliveryDate,IsQuotationApproved,ApprovedBy,Amount,QuotationStatus,Remarks,SalesQuotationRejectReasonId,QuotationRejectReason,Competitors,PaymentTerms,DiscountRemarks,CreatedBy,CreatedDate,OrganizationId,ParentId,GrantParentId,RevisionNo,isProjectBased,isWarranty)
                            Values (@QuotationRefNo,@QuotationDate,@CustomerId,@ContactPerson,@SalesExecutiveId,@PredictedClosingDate,@QuotationValidToDate,@ExpectedDeliveryDate,@IsQuotationApproved,@ApprovedBy,@Amount,@QuotationStatus,@Remarks,@SalesQuotationRejectReasonId,@QuotationRejectReason,@Competitors,@PaymentTerms,@DiscountRemarks,@CreatedBy,@CreatedDate,@OrganizationId,@ParentId,@GrantParentId,@RevisionNo,@isProjectBased,@isWarranty);
                            SELECT CAST(SCOPE_IDENTITY() as int) SalesQuotationId";

                    model.SalesQuotationId = connection.Query<int>(sql, model, trn).First<int>();

                    var saleorderitemrepo = new SalesQuotationItemRepository();
                    foreach (var item in model.SalesQuotationItems)
                    {
                        item.SalesQuotationId = model.SalesQuotationId;
                        saleorderitemrepo.InsertSalesQuotationItem(item, connection, trn);
                    }
                    InsertLoginHistory(dataConnection, model.CreatedBy, "Revision", "Sales Quotation", model.SalesQuotationId.ToString(), "0");
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
                string sql = @"Update SalesQuotation  SET IsQuotationApproved=1, ApprovedBy=@CreatedBy  OUTPUT INSERTED.SalesQuotationId WHERE SalesQuotationId=@SalesQuotationId";


                var id = connection.Query(sql, new { CreatedBy = objSalesQuotation.@CreatedBy, SalesQuotationId = objSalesQuotation.SalesQuotationId });
                
            }
        }
        public SalesQuotation StatusUpdate(SalesQuotation objSalesQuotation)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Update SalesQuotation  SET SalesQuotationRejectReasonId=@SalesQuotationRejectReasonId   OUTPUT INSERTED.SalesQuotationId WHERE SalesQuotationId=@SalesQuotationId";

                try
                {
                    objSalesQuotation.SalesQuotationId = connection.Query<int>(sql, new { SalesQuotationRejectReasonId = objSalesQuotation.SalesQuotationRejectReasonId, SalesQuotationId = objSalesQuotation.SalesQuotationId }).First<int>();

                }
                catch {
                    objSalesQuotation.SalesQuotationId = 0;
                    objSalesQuotation.QuotationRefNo = null;
                
                }
                InsertLoginHistory(dataConnection, objSalesQuotation.CreatedBy, "Status Change", "Sales Quotation", objSalesQuotation.SalesQuotationId.ToString(), "0");
                return objSalesQuotation;

            }
        }

        public List<SalesQuotationItem> GetSalesQuotationItems(int SalesQuotationId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select *,VehicleModelName from SalesQuotationItem S inner join WorkDescription W ON S.WorkDescriptionId=W.WorkDescriptionId
                               LEFT JOIN VehicleModel V ON  V.VehicleModelId=W.VehicleModelId
                            
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
        public List<SalesQuotation> GetSalesQuotationApproveList(int IsProjectBased)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select E.EmployeeName SalesExecutiveName ,C.CustomerName,SQ.*,C.DoorNo +','+ C.Street+','+C.State CustomerAddress,RR.ReasonDescription from SalesQuotation SQ 
                            inner join Customer C on SQ.CustomerId=C.CustomerId
							inner join Employee E on  E.EmployeeId =SQ.SalesExecutiveId
							inner join SalesQuotationRejectReason RR on  RR.SalesQuotationRejectReasonId =SQ.SalesQuotationRejectReasonId
                            where SQ.ApprovedBy is null and  SQ.isActive=1 and isnull(SQ.IsQuotationApproved,0)=0
                            and SQ.IsProjectBased = " + IsProjectBased.ToString();

                var objSalesQuotations = connection.Query<SalesQuotation>(sql).ToList<SalesQuotation>();

                return objSalesQuotations;
            }
        }


        public int DeleteSalesQuotation(int Id,int CancelStatus)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = string.Empty;
            
                if(CancelStatus ==1)
                {
                    sql = @"Update SalesQuotation  set  isActive=0 WHERE SalesQuotationId=@Id";
                    var id = connection.Execute(sql, new { Id = Id });
                    return id;
                 }
                
                return 0;
                
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
        /// <summary>
        /// Pending Sales Quotation for Sale Order
        /// </summary>
        /// <param name="IsProjectBased"></param>
        /// <returns></returns>
        public List<SalesQuotation> GetSalesQuotationForSO(int IsProjectBased)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = String.Format(@"select distinct E.EmployeeName SalesExecutiveName ,C.CustomerName,SQ.*,STUFF((SELECT ', ' + CAST(W.WorkDescr AS VARCHAR(MAX)) [text()]
                             FROM SalesQuotationItem S inner join WorkDescription W on W.WorkDescriptionId=S.WorkDescriptionId
                             WHERE S.SalesQuotationId = SI.SalesQuotationId
                             FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'),1,2,' ') WorkDescription,
							 DATEDIFF(DAY, SQ.QuotationDate, GETDATE()) Ageing,
							 DATEDIFF(DAY, GETDATE(), SQ.ExpectedDeliveryDate) DaysLeft,
                             SQ.ExpectedDeliveryDate
							 from SalesQuotation SQ 
                             inner join SalesQuotationItem SI on SI.SalesQuotationId=SQ.SalesQuotationId
                             inner join Customer C on SQ.CustomerId=C.CustomerId
							 inner join Employee E on  E.EmployeeId =SQ.SalesExecutiveId
							 left join SaleOrder SO on SO.SalesQuotationId=SQ.SalesQuotationId
                             where   SQ.isActive=1 and isnull(SQ.IsQuotationApproved,0)=1 AND SO.SalesQuotationId IS NULL
                            and ((@IsProjectBased=0 and SQ.IsProjectBased in (0,2)) or ( @IsProjectBased=1  and SQ.IsProjectBased in (1)))
                            
                             ORDER BY SQ.ExpectedDeliveryDate DESC, SQ.QuotationDate DESC");

                var objSalesQuotations = connection.Query<SalesQuotation>(sql, new { IsProjectBased = IsProjectBased}).ToList<SalesQuotation>();
                //AND SQ.IsProjectBased in(case when isnumeric(@IsProjectBased)=0 then (0,2) else @IsProjectBased end)
                return objSalesQuotations;
            }
        }
        public int GetUserApprovalCancelStatus(int UserId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
               
                string sql = @"select Cancel CancelStatus from QuotationApprovalSettings where UserId = " + UserId.ToString() + "";
                var CancelStatus = connection.Query<int>(sql).Single();
                return CancelStatus;
            }
        }

        public IEnumerable<SalesQuotationList> GetPreviousList(int isProjectBased, int id, int cusid, int OrganizationId, DateTime? from, DateTime? to)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select Q.SalesQuotationId, Q.QuotationRefNo, Q.QuotationDate, C.CustomerName, E.EmployeeName, Q.Amount,RR.ReasonDescription
                               from SalesQuotation Q 
                               inner join Customer C on C.CustomerId = Q.CustomerId
                               inner join Employee E on E.EmployeeId = Q.SalesExecutiveId
                               inner join SalesQuotationRejectReason RR on RR.SalesQuotationRejectReasonId=q.SalesQuotationRejectReasonId
                               where Q.SalesQuotationId= ISNULL(NULLIF(@id, 0), Q.SalesQuotationId) AND C.CustomerId = ISNULL(NULLIF(@cusid, 0), C.CustomerId) 
                               and Q.QuotationDate BETWEEN ISNULL(@from, DATEADD(MONTH, -1, GETDATE())) AND ISNULL(@to, GETDATE())
                               and Q.OrganizationId = @OrganizationId  and isProjectBased =" + isProjectBased;

                return connection.Query<SalesQuotationList>(sql, new { OrganizationId = OrganizationId, id = id, cusid = cusid, to = to, from = from }).ToList();
            }
        }

        public int CHECK(int SalesQuotationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" SELECT Count(SalesQuotationId)Count FROM SalesQuotation     
                                WHERE IsQuotationApproved =1 AND SalesQuotationId=@SalesQuotationId";

                var id = connection.Query<int>(sql, new { SalesQuotationId = SalesQuotationId }).FirstOrDefault();

                return id;

            }

        }


        /// <summary>
        /// Delete SQ HD Details
        /// </summary>
        /// <returns></returns>
        public int DeleteSQHD(int Id)
        {
            int result = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" DELETE FROM SalesQuotation WHERE SalesQuotationId=@Id";

                {

                    var id = connection.Execute(sql, new { Id = Id });
                    return id;

                }

            }
        }
        /// <summary>
        /// Delete SQ DT Details
        /// </summary>
        /// <returns></returns>
        public int DeleteSQDT(int Id)
        {
            int result3 = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" DELETE FROM SalesQuotationItem WHERE SalesQuotationId=@Id";

                {

                    var id = connection.Execute(sql, new { Id = Id });
                    return id;

                }

            }
        }



    }
}