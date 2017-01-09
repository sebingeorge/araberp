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
                string sql = @"select Q.SalesQuotationId, Q.QuotationRefNo, Q.QuotationDate, C.CustomerName, E.EmployeeName, Q.GrandTotal,RR.Description
                               from SalesQuotation Q 
                               inner join Customer C on C.CustomerId = Q.CustomerId
                               inner join Employee E on E.EmployeeId = Q.SalesExecutiveId
                               inner join SalesQuotationStatus RR on RR.SalesQuotationStatusId=q.SalesQuotationStatusId
							   LEFT JOIN SaleOrder SO ON Q.SalesQuotationId = SO.SalesQuotationId
							   LEFT JOIN VehicleInPass VIP ON SO.SaleOrderId = VIP.SaleOrderId
                               where Q.isActive = 1 AND VIP.VehicleInPassId IS NULL and Q.isProjectBased =" + isProjectBased + @" AND SO.SaleOrderId IS NULL AND ISNULL(Q.IsQuotationApproved, 0) = 1";

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
                    model.TotalWorkAmount = model.SalesQuotationItems.Sum(m => ((m.Rate * m.Quantity) - m.Discount));

                    //if (model.isAfterSales)
                    if (model.Materials != null && model.Materials.Count > 0)
                    {

                        model.TotalMaterialAmount = model.Materials.Sum(m => (m.Rate ?? 0 * m.Quantity ?? 0));

                    }
                    model.GrandTotal = (model.TotalWorkAmount + model.TotalMaterialAmount);
                    if (model.isProjectBased && !model.isAfterSales)
                    {
                        model.QuotationRefNo = DatabaseCommonRepository.GetNewDocNo(connection, model.OrganizationId, 2, true, trn);
                    }
                    else if (!model.isProjectBased && !model.isAfterSales)
                    {
                        model.QuotationRefNo = DatabaseCommonRepository.GetNewDocNo(connection, model.OrganizationId, 1, true, trn);
                    }


                    else if (model.isAfterSales)
                    {
                        model.QuotationRefNo = DatabaseCommonRepository.GetNewDocNo(connection, model.OrganizationId, 28, true, trn);
                    }
                    #region automatically approve if no custom rates are set
                    if (!model.isProjectBased)
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
                                        QuotationValidToDate,ExpectedDeliveryDate,IsQuotationApproved,ApprovedBy,TotalWorkAmount,TotalMaterialAmount,GrandTotal,CurrencyId,QuotationStatus,Remarks,SalesQuotationStatusId,
                                        QuotationStage,Competitors,PaymentTerms,DiscountRemarks,CreatedBy,CreatedDate,OrganizationId,isProjectBased,isAfterSales,QuerySheetId,isWarranty, ProjectCompletionId, DeliveryChallanId, Discount, DeliveryTerms)
                                        Values (@QuotationRefNo,@QuotationDate,@CustomerId,@ContactPerson,@SalesExecutiveId,@PredictedClosingDate,@QuotationValidToDate,
                                        @ExpectedDeliveryDate,@IsQuotationApproved,@ApprovedBy,@TotalWorkAmount,@TotalMaterialAmount,@GrandTotal,@CurrencyId,@QuotationStatus,@Remarks,@SalesQuotationStatusId,
                                        @QuotationStage,@Competitors,@PaymentTerms,@DiscountRemarks,@CreatedBy,@CreatedDate,@OrganizationId,@isProjectBased,@isAfterSales,NULLIF(@QuerySheetId, 0),@isWarranty, NULLIF(@ProjectCompletionId, 0), NULLIF(@DeliveryChallanId, 0), @Discount, @DeliveryTerms);
                                        SELECT CAST(SCOPE_IDENTITY() as int) SalesQuotationId";

                    model.SalesQuotationId = connection.Query<int>(sql, model, trn).First<int>();

                    var saleorderitemrepo = new SalesQuotationItemRepository();
                    foreach (var item in model.SalesQuotationItems)
                    {
                        item.SalesQuotationId = model.SalesQuotationId;
                        item.OrganizationId = model.OrganizationId;
                        item.VehicleModelId = model.VehicleModelId;
                        saleorderitemrepo.InsertSalesQuotationItem(item, connection, trn);
                    }
                    if (model.Materials != null && model.Materials.Count > 0)
                    {
                        foreach (var item in model.Materials)
                        {
                            if (item.ItemId == null) continue;
                            item.SalesQuotationId = model.SalesQuotationId;
                            saleorderitemrepo.InsertSalesQuotationMaterial(item, connection, trn);
                        }
                    }

                    //if (model.isAfterSales)
                    //{
                    //    foreach (var item in model.Materials)
                    //    {
                    //        item.SalesQuotationId = model.SalesQuotationId;
                    //        saleorderitemrepo.InsertSalesQuotationMaterial(item, connection, trn);
                    //    }
                    //}
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
                model.TotalWorkAmount = model.SalesQuotationItems.Sum(m => ((m.Rate * m.Quantity) - m.Discount));

                //if (model.isAfterSales)
                if (model.Materials != null && model.Materials.Count > 0)
                {
                    model.TotalMaterialAmount = model.Materials.Sum(m => (m.Rate ?? 0 * m.Quantity ?? 0));
                }
                model.GrandTotal = (model.TotalWorkAmount + model.TotalMaterialAmount);
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

                    string refno = connection.Query<string>("select QuotationRefNo from SalesQuotation where SalesQuotationId = " + model.GrantParentId.ToString(), null, trn).Single();
                    //int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(SalesQuotation).Name, "0", 1);
                    model.QuotationRefNo = refno + "/REV" + RevisionId.ToString();
                    sql = @"
                                    insert  into SalesQuotation(QuotationRefNo, QuotationDate, CustomerId, ContactPerson, SalesExecutiveId, PredictedClosingDate, QuotationValidToDate, ExpectedDeliveryDate, IsQuotationApproved, ApprovedBy,GrandTotal, TotalWorkAmount, TotalMaterialAmount, QuotationStatus, Remarks, SalesQuotationStatusId, QuotationStage, Competitors, PaymentTerms, DiscountRemarks, CreatedBy, CreatedDate, OrganizationId, ParentId, GrantParentId, RevisionNo, isProjectBased, isWarranty, RevisionReason, CurrencyId)
                                    Values (@QuotationRefNo, @QuotationDate, @CustomerId, @ContactPerson, @SalesExecutiveId, @PredictedClosingDate, @QuotationValidToDate, @ExpectedDeliveryDate, @IsQuotationApproved, @ApprovedBy, @GrandTotal, @TotalWorkAmount, @TotalMaterialAmount, @QuotationStatus, @Remarks, @SalesQuotationStatusId, @QuotationStage, @Competitors, @PaymentTerms, @DiscountRemarks, @CreatedBy, @CreatedDate, @OrganizationId, @ParentId, @GrantParentId, @RevisionNo, @isProjectBased, @isWarranty, @RevisionReason, @CurrencyId);
                                    SELECT CAST(SCOPE_IDENTITY() as int) SalesQuotationId";

                    model.SalesQuotationId = connection.Query<int>(sql, model, trn).First<int>();

                    var saleorderitemrepo = new SalesQuotationItemRepository();
                    foreach (var item in model.SalesQuotationItems)
                    {
                        item.SalesQuotationId = model.SalesQuotationId;
                        item.VehicleModelId = model.VehicleModelId;
                        saleorderitemrepo.InsertSalesQuotationItem(item, connection, trn);
                    }
                    if (model.Materials != null && model.Materials.Count > 0)
                    {
                        foreach (var item in model.Materials)
                        {
                            if (item.ItemId == null) continue;
                            item.SalesQuotationId = model.SalesQuotationId;
                            saleorderitemrepo.InsertSalesQuotationMaterial(item, connection, trn);
                        }
                    }
                    //if (model.isAfterSales)
                    //{
                    //    foreach (var item in model.Materials)
                    //    {
                    //        item.SalesQuotationId = model.SalesQuotationId;
                    //        saleorderitemrepo.InsertSalesQuotationMaterial(item, connection, trn);
                    //    }
                    //}
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

                var objSalesQuotation = connection.Query<SalesQuotation>(sql, new { SalesQuotationId = SalesQuotationId }).First<SalesQuotation>();

                sql = @"SELECT ISNULL(IsQuotationApproved, 0) FROM SalesQuotation WHERE SalesQuotationId=@SalesQuotationId";
                objSalesQuotation.IsUsed = Convert.ToBoolean(connection.Query<int>(sql, new { SalesQuotationId = SalesQuotationId }).First());
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
                string sql = @"Update SalesQuotation  SET SalesQuotationStatusId=@SalesQuotationStatusId   OUTPUT INSERTED.SalesQuotationId WHERE SalesQuotationId=@SalesQuotationId";

                try
                {
                    objSalesQuotation.SalesQuotationId = connection.Query<int>(sql, new { SalesQuotationStatusId = objSalesQuotation.SalesQuotationStatusId, SalesQuotationId = objSalesQuotation.SalesQuotationId }).First<int>();

                }
                catch
                {
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
                string sql = @"SELECT 
	                                [SalesQuotationItemId],
	                                [SalesQuotationId],
	                                [SlNo],
	                                S.[WorkDescriptionId],
	                                [Remarks],
	                                [PartNo],
	                                [Quantity],
	                                [UnitId],
	                                [Rate],
	                                [Discount],
	                                (S.Rate * S.Quantity) Amount,
	                                ((S.Rate * S.Quantity)-(S.Discount)) TotalAmount,
	                                [RateType],
	                                'No(s)' UnitName,
									WD.FreezerUnitId,
									WD.BoxId,
                                    WD.VehicleModelId,
									WD.WorkDescr
                                FROM SalesQuotationItem S
									INNER JOIN WorkDescription WD ON S.WorkDescriptionId = WD.WorkDescriptionId
                                WHERE SalesQuotationId = @SalesQuotationId";

                var SalesQuotationItems = connection.Query<SalesQuotationItem>(sql, new
                {
                    SalesQuotationId = SalesQuotationId
                }).ToList<SalesQuotationItem>();

                return SalesQuotationItems;
            }
        }
        public List<SalesQuotationMaterial> GetSalesQuotationMaterials(int SalesQuotationId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from SalesQuotationMaterial S inner join Item I ON I.ItemId=S.ItemId
                               left join Unit U on  U.UnitId=I.ItemUnitId
                               where SalesQuotationId=@SalesQuotationId";

                var SalesQuotationMaterials = connection.Query<SalesQuotationMaterial>(sql, new
                {
                    SalesQuotationId = SalesQuotationId
                }).ToList<SalesQuotationMaterial>();

                return SalesQuotationMaterials;
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
        public List<SalesQuotation> GetSalesQuotationApproveList(int IsProjectBased, int IsAfterSales, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select E.EmployeeName SalesExecutiveName ,C.CustomerName,SQ.*,C.DoorNo +','+ C.Street+','+C.State CustomerAddress,RR.Description from SalesQuotation SQ 
                            inner join Customer C on SQ.CustomerId=C.CustomerId
							inner join Employee E on  E.EmployeeId =SQ.SalesExecutiveId
							inner join SalesQuotationStatus RR on  RR.SalesQuotationStatusId =SQ.SalesQuotationStatusId
                            where SQ.ApprovedBy is null and  SQ.isActive=1 and isnull(SQ.IsQuotationApproved,0)=0
                            and SQ.OrganizationId = " + OrganizationId.ToString() + " and SQ.IsProjectBased = " + IsProjectBased.ToString() + " and SQ.isAfterSales= " + IsAfterSales.ToString() + "ORDER BY SQ.QuotationDate DESC, SQ.QuotationRefNo";

                var objSalesQuotations = connection.Query<SalesQuotation>(sql).ToList<SalesQuotation>();

                return objSalesQuotations;
            }
        }


        public int Cancel(int Id, int CancelStatus)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                string sql = string.Empty;

                if (CancelStatus == 1)
                {
                    sql = @"Update SalesQuotation  set  isActive=0 WHERE SalesQuotationId=@Id";
                    var id = connection.Execute(sql, new { Id = Id });
                    return id;
                }

                return 0;

            }
        }
        public string DeleteSalesQuotation(int SalesQuotationId, string isAfterSales)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                string query = string.Empty;
                try
                {

                    if (isAfterSales == "true")
                    {
                        query = "DELETE FROM SalesQuotationMaterial  WHERE SalesQuotationId = @SalesQuotationId;";
                    }
                    query += @"DELETE FROM SalesQuotationItem WHERE SalesQuotationId = @SalesQuotationId;
                               DELETE FROM SalesQuotation OUTPUT deleted.QuotationRefNo WHERE SalesQuotationId = @SalesQuotationId;";
                    string output = connection.Query<string>(query, new { SalesQuotationId = SalesQuotationId }, txn).First();
                    txn.Commit();
                    return output;
                }
                catch (Exception ex)
                {
                    txn.Rollback();
                    throw ex;
                }
            }
        }

        public List<Dropdown> FillSalesQuotationStatus()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                return connection.Query<Dropdown>("select SalesQuotationStatusId Id,Description Name from SalesQuotationStatus").ToList();
            }
        }
        /// <summary>
        /// Pending Sales Quotation for Sale Order
        /// </summary>
        /// <param name="IsProjectBased"></param>
        /// <returns></returns>
        public List<SalesQuotation> GetSalesQuotationForSO(int IsProjectBased, int OrganizationId)
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
                             and ((@IsProjectBased=0 and SQ.IsProjectBased in (0,2)) or ( @IsProjectBased=1  and SQ.IsProjectBased in (1))) and SQ.OrganizationId=@OrganizationId
                            
                             ORDER BY SQ.ExpectedDeliveryDate DESC, SQ.QuotationDate DESC");

                var objSalesQuotations = connection.Query<SalesQuotation>(sql, new { IsProjectBased = IsProjectBased, OrganizationId = OrganizationId }).ToList<SalesQuotation>();

                return objSalesQuotations;
            }
        }

        public int UpdateSalesQuotation(SalesQuotation objSalesQtn)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = string.Empty;
                IDbTransaction txn = connection.BeginTransaction();

                objSalesQtn.TotalWorkAmount = objSalesQtn.SalesQuotationItems.Sum(m => ((m.Rate * m.Quantity) - m.Discount));
                if (objSalesQtn.Materials != null && objSalesQtn.Materials.Count > 0)
                {
                    objSalesQtn.TotalMaterialAmount = objSalesQtn.Materials.Sum(m => (m.Rate ?? 0 * m.Quantity ?? 0));
                }

                #region query to delete from [SalesQuotationMaterial] table if after sales is true
                //if (objSalesQtn.isAfterSales)
                //{
                //    objSalesQtn.TotalMaterialAmount = objSalesQtn.Materials.Sum(m => (m.Amount ?? 0));
                //    sql = "DELETE FROM SalesQuotationMaterial WHERE SalesQuotationId = @SalesQuotationId;";
                //}
                #endregion

                objSalesQtn.GrandTotal = (objSalesQtn.TotalWorkAmount + objSalesQtn.TotalMaterialAmount);

                #region automatically approve if no custom rates are set
                try
                {
                    if (!objSalesQtn.isProjectBased)
                    {
                        List<int> rateType = (from SalesQuotationItem s in objSalesQtn.SalesQuotationItems
                                              where s.RateType == 0
                                              select s.RateType).ToList();
                        if (rateType.Count > 0)
                            objSalesQtn.IsQuotationApproved = false;
                        else objSalesQtn.IsQuotationApproved = true;
                    }
                }
                catch { }
                #endregion

                #region query to update [SalesQuotation] table and delete from [SalesQuotationItem] table
                sql += @"UPDATE   SalesQuotation SET   QuotationDate = @QuotationDate,CustomerId=@CustomerId,ContactPerson=@ContactPerson,SalesExecutiveId=@SalesExecutiveId,PredictedClosingDate=@PredictedClosingDate,
                                        QuotationValidToDate = @QuotationValidToDate,ExpectedDeliveryDate = @ExpectedDeliveryDate,IsQuotationApproved=@IsQuotationApproved,ApprovedBy=@ApprovedBy,TotalWorkAmount=@TotalWorkAmount,TotalMaterialAmount=@TotalMaterialAmount,GrandTotal=@GrandTotal,CurrencyId=@CurrencyId,QuotationStatus=@QuotationStatus,Remarks=@Remarks,SalesQuotationStatusId=@SalesQuotationStatusId,
                                        QuotationStage = @QuotationStage,Competitors = @Competitors,PaymentTerms = @PaymentTerms,DiscountRemarks=@DiscountRemarks,CreatedBy=@CreatedBy,CreatedDate=@CreatedDate,OrganizationId=@OrganizationId,isProjectBased=@isProjectBased,isAfterSales=@isAfterSales,QuerySheetId=NULLIF(@QuerySheetId,0),isWarranty=@isWarranty, Discount = @Discount, DeliveryTerms = @DeliveryTerms
	                                    where SalesQuotationId = @SalesQuotationId;
	                               
                         DELETE FROM SalesQuotationItem WHERE SalesQuotationId = @SalesQuotationId;";
                #endregion

                try
                {
                    var id = connection.Execute(sql, objSalesQtn, txn);

                    #region insert into [SalesQuotationItem] table
                    var saleorderitemrepo = new SalesQuotationItemRepository();
                    foreach (var item in objSalesQtn.SalesQuotationItems)
                    {
                        item.SalesQuotationId = objSalesQtn.SalesQuotationId;
                        item.VehicleModelId = objSalesQtn.VehicleModelId;
                        saleorderitemrepo.InsertSalesQuotationItem(item, connection, txn);
                    }
                    #endregion

                    #region delete and insert into [SalesQuotationMaterial] table
                    sql = "DELETE FROM SalesQuotationMaterial WHERE SalesQuotationId = " + objSalesQtn.SalesQuotationId;
                    connection.Execute(sql, transaction: txn);
                    if (objSalesQtn.Materials != null && objSalesQtn.Materials.Count > 0)
                    {
                        foreach (var item in objSalesQtn.Materials)
                        {
                            if (item.ItemId == null) continue;
                            item.SalesQuotationId = objSalesQtn.SalesQuotationId;
                            saleorderitemrepo.InsertSalesQuotationMaterial(item, connection, txn);
                        }
                    }
                    #endregion

                    #region insert into [SalesQuotationMaterial] table if aftersales=true
                    if (objSalesQtn.isAfterSales)
                    {
                        foreach (var item in objSalesQtn.Materials)
                        {
                            item.SalesQuotationId = objSalesQtn.SalesQuotationId;
                            saleorderitemrepo.InsertSalesQuotationMaterial(item, connection, txn);
                        }
                    }
                    #endregion

                    sql = @" insert  into SalesQuotationHistory(SalesQuotationId,QuotationRefNo,QuotationDate,SalesExecutiveId,GrandTotal,OrganizationId,CreatedBy,CreatedDate)
                                        Values (@SalesQuotationId,@QuotationRefNo,@QuotationDate,@SalesExecutiveId,@GrandTotal,@OrganizationId,@CreatedBy,@CreatedDate)";

                    connection.Execute(sql, objSalesQtn, txn);

                    InsertLoginHistory(dataConnection, objSalesQtn.CreatedBy, "Update", "Sales Quotation", id.ToString(), objSalesQtn.OrganizationId.ToString());
                    txn.Commit();
                    return id;
                }
                catch (Exception ex)
                {
                    txn.Rollback();
                    throw ex;
                }
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

        public IEnumerable<SalesQuotationList> GetPreviousList(int isProjectBased, int AfterSales, int id, int cusid, int OrganizationId, DateTime? from, DateTime? to)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select Q.SalesQuotationId, Q.QuotationRefNo, Q.QuotationDate, C.CustomerName, E.EmployeeName, Q.GrandTotal,RR.Description
                               from SalesQuotation Q 
                               inner join Customer C on C.CustomerId = Q.CustomerId
                               inner join Employee E on E.EmployeeId = Q.SalesExecutiveId
                               inner join SalesQuotationStatus RR on RR.SalesQuotationStatusId=q.SalesQuotationStatusId
                               where Q.SalesQuotationId= ISNULL(NULLIF(@id, 0), Q.SalesQuotationId) AND C.CustomerId = ISNULL(NULLIF(@cusid, 0), C.CustomerId) 
                               and Q.QuotationDate BETWEEN ISNULL(@from, DATEADD(MONTH, -1, GETDATE())) AND ISNULL(@to, GETDATE())
                               and Q.OrganizationId = @OrganizationId  and isProjectBased =" + isProjectBased + " and isAfterSales=" + AfterSales + @" 
                                AND ISNULL(Q.isActive, 1) = 1 ORDER BY Q.QuotationDate DESC, Q.SalesQuotationId DESC";

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
        /// Check if the service is under warranty
        /// </summary>
        /// <param name="id">DeliveryChallanId or ProjectCompletionId</param>
        /// <param name="isProject">1 = Project, 0 = Transportation</param>
        /// <returns></returns>
        public bool isUnderWarranty(int id, string type)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string table = type.StartsWith("Project") ? "ProjectCompletion" : "DeliveryChallan";
                string sql = @"IF(
	                                (SELECT CONVERT(DATE, " + type + @"WarrantyExpiryDate, 106) 
	                                FROM " + table + @" WHERE " + table + @"Id = @id) 
	                                < CONVERT(DATE, GETDATE(), 106)
                                  )
                                  SELECT 'false'
                                  ELSE SELECT 'true'";
                var isWarranty = connection.Query<bool>(sql, new { id = id }).First();
                return isWarranty;
            }
        }

        public SalesQuotation GetSalesQuotationHD(int SalesQuotationId, int organizationId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select O.*,S.*,
                             
                               CU.CustomerName,(CU.DoorNo + CU.Street+ CU.State)CustomerAddress,sq.Description SalesQuotationStatusName,U.[Signature] ApprovedUsersig,U.UserName EmpNmae,
                               D.DesignationName,E.EmployeeName SalesExecutiveName,CO.CountryName,C.CurrencyName from SalesQuotation S
                               inner join Customer Cu ON CU.CustomerId=S.CustomerId
                               inner join Employee E ON  E.EmployeeId=S.SalesExecutiveId
                               inner join Organization O ON O.OrganizationId=S.OrganizationId
                               left join Country CO  on CO.CountryId=O.Country
                               inner join SalesQuotationStatus SQ ON SQ.SalesQuotationStatusId=S.SalesQuotationStatusId
                               Left JOIN Currency C ON C.CurrencyId=O.CurrencyId
							   left join [user] U ON U.UserId=S.CreatedBy
							   left join Designation D ON D.DesignationId=U.DesignationId
                               where SalesQuotationId=@SalesQuotationId";

                var objSalesQuotation = connection.Query<SalesQuotation>(sql, new { SalesQuotationId = SalesQuotationId, organizationId = organizationId }).First<SalesQuotation>();

                return objSalesQuotation;
            }
        }


        public List<SalesQuotationItem> GetSalesQuotationItemsPrint(int SalesQuotationId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" SELECT STUFF((SELECT ', ' + w.WorkDescr
                                    FROM SalesQuotationItem S
                                    inner join WorkDescription W ON w.WorkDescriptionId=s.WorkDescriptionId
                                    WHERE s.SalesQuotationId=@SalesQuotationId
                                    ORDER BY WorkDescr
                                    FOR XML PATH('')), 1, 1, '') as WorkDescription,
	                                [SalesQuotationItemId],
	                                [SalesQuotationId],
	                                [SlNo],
	                                S.[WorkDescriptionId],
	                                [Remarks],
	                                [PartNo],
	                                [Quantity],
	                                [UnitId],
	                                [Rate],
	                                [Discount],
	                               (S.Rate * S.Quantity) Amount,
	                                ((S.Rate * S.Quantity)-(S.Discount)) TotalAmount,
	                                [RateType],
	                                'Nos' UnitName,
	                                W.*,
	                                V.* 
                                from SalesQuotationItem S inner join WorkDescription W ON S.WorkDescriptionId=W.WorkDescriptionId
                                    LEFT JOIN VehicleModel V ON  V.VehicleModelId=W.VehicleModelId
                                where SalesQuotationId=@SalesQuotationId";

                var SalesQuotationItems = connection.Query<SalesQuotationItem>(sql, new
                {
                    SalesQuotationId = SalesQuotationId
                }).ToList<SalesQuotationItem>();

                return SalesQuotationItems;
            }
        }
        public IEnumerable<SalesQuotationList> GetSalesQuotaationListPrint(int month, int year)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select Q.SalesQuotationId, Q.QuotationRefNo, Q.QuotationDate, C.CustomerName, E.EmployeeName, Q.GrandTotal,RR.[Description] [Status],
                               U.UserName,isnull(Q.RevisionNo,0)RevisionNo, Q.RevisionReason, I.ItemName [Description]
                               from SalesQuotation Q 
                               inner join Customer C on C.CustomerId = Q.CustomerId
                               inner join Employee E on E.EmployeeId = Q.SalesExecutiveId
                               inner join SalesQuotationStatus RR on RR.SalesQuotationStatusId=q.SalesQuotationStatusId
                               INNER JOIN SalesQuotationItem SQI ON Q.SalesQuotationId = SQI.SalesQuotationId
							   LEFT JOIN WorkDescription WD ON SQI.WorkDescriptionId = WD.WorkDescriptionId
							   LEFT JOIN Item I ON WD.FreezerUnitId = I.ItemId
							   left join [User] U  on U.UserId = Q.CreatedBy
							   where ISNULL(MONTH(Q.QuotationDate), @Month) = @Month
							   AND ISNULL(YEAR(Q.QuotationDate), @Year) =  @Year
                               and Q.isActive = 1  ";

                return connection.Query<SalesQuotationList>(sql, new { Month = month, Year = year }).ToList();
            }
        }


        public SalesQuotation GetRoomDetailsFromQuerySheet(int querySheetId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                SalesQuotation model = new SalesQuotation();
                string query = @"SELECT
	                                *
                                FROM QuerySheetItem
                                WHERE QuerySheetId = @id

                                SELECT QuerySheetItemId INTO #Rooms FROM QuerySheetItem WHERE QuerySheetId = @id

                                SELECT
	                                *
                                FROM QuerySheetItemUnit
                                WHERE QuerySheetItemId IN (SELECT QuerySheetItemId FROM #Rooms)

                                SELECT
	                                *
                                FROM QuerySheetItemDoor
                                WHERE QuerySheetItemId IN (SELECT QuerySheetItemId FROM #Rooms)

                                DROP TABLE #Rooms;";

                using (var dataset = connection.QueryMultiple(query, new { id = querySheetId }))
                {
                    model.ProjectRooms = dataset.Read<QuerySheetItem>().AsList();
                    List<QuerySheetUnit> units = dataset.Read<QuerySheetUnit>().AsList();
                    List<QuerySheetDoor> doors = dataset.Read<QuerySheetDoor>().AsList();
                    foreach (var item in model.ProjectRooms)
                    {
                        item.ProjectRoomUnits = units.Where(x => x.QuerySheetItemId == item.QuerySheetItemId).Select(x => x).ToList();
                        item.ProjectRoomDoors = doors.Where(x => x.QuerySheetItemId == item.QuerySheetItemId).Select(x => x).ToList();
                    }
                }
                return model;
            }
        }

        public SalesQuotation InsertProjectQuotation(SalesQuotation model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    #region saving quotation head [SalesQuotation]
                    string sql = @"insert  into SalesQuotation(QuotationRefNo,QuotationDate,CustomerId,ContactPerson,SalesExecutiveId,PredictedClosingDate,
                                    QuotationValidToDate,ExpectedDeliveryDate,IsQuotationApproved,ApprovedBy,TotalWorkAmount,TotalMaterialAmount,GrandTotal,CurrencyId,QuotationStatus,Remarks,SalesQuotationStatusId,
                                    QuotationStage,Competitors,PaymentTerms,DiscountRemarks,CreatedBy,CreatedDate,OrganizationId,isProjectBased,isAfterSales,QuerySheetId,isWarranty, ProjectCompletionId, DeliveryChallanId, Discount, DeliveryTerms)
                                    Values (@QuotationRefNo,@QuotationDate,@CustomerId,@ContactPerson,@SalesExecutiveId,@PredictedClosingDate,@QuotationValidToDate,
                                    @ExpectedDeliveryDate,@IsQuotationApproved,@ApprovedBy,@TotalWorkAmount,@TotalMaterialAmount,@GrandTotal,@CurrencyId,@QuotationStatus,@Remarks,@SalesQuotationStatusId,
                                    @QuotationStage,@Competitors,@PaymentTerms,@DiscountRemarks,@CreatedBy,@CreatedDate,@OrganizationId,@isProjectBased,@isAfterSales,NULLIF(@QuerySheetId, 0),@isWarranty, NULLIF(@ProjectCompletionId, 0), NULLIF(@DeliveryChallanId, 0), @Discount, @DeliveryTerms);
                                    SELECT CAST(SCOPE_IDENTITY() AS INT) SalesQuotationId";
                    model.SalesQuotationId = connection.Query<int>(sql, model, txn).First();
                    #endregion
                    #region saving quotation rooms [SalesQuotationItem]
                    sql = @"INSERT INTO SalesQuotationItem 
                            (
	                            [SalesQuotationId],
	                            [Quantity],
	                            [OrganizationId],
	                            [isActive]
                            )
                            VALUES
                            (
	                            @SalesQuotationId,
                                @Quantity,
                                @OrganizationId,
                                @isActive
                            )
                            SELECT CAST(SCOPE_IDENTITY() AS INT) SalesQuotationItemId";
                    foreach (var item in model.SalesQuotationItems)
                    {
                        item.SalesQuotationId = model.SalesQuotationId;
                        item.Quantity = 1;
                        item.OrganizationId = model.OrganizationId;
                        item.isActive = true;
                        item.SalesQuotationItemId = connection.Query<int>(sql, item, txn).First();
                    }
                    #endregion
                    #region saving project quotation units
                    foreach (QuerySheetItem items in model.ProjectRooms)
                    {
                        foreach (QuerySheetUnit item in items.ProjectRoomUnits)
                        {
                            item.QuerySheetItemId = items.QuerySheetItemId;
                            sql = @"insert  into ProjectQuotationItemUnit(SalesQuotationItemId,EvaporatorUnitId,CondenserUnitId,Quantity) 
                                    Values (@QuerySheetItemId,@EvaporatorUnitId,@CondenserUnitId,@Quantity)";
                            connection.Execute(sql, item, txn);
                        }
                        foreach (QuerySheetDoor item in items.ProjectRoomDoors)
                        {
                            item.QuerySheetItemId = items.QuerySheetItemId;
                            sql = @"insert  into ProjectQuotationItemDoor(SalesQuotationItemId,DoorId,Quantity) 
                                    Values (@QuerySheetItemId,@DoorId,@Quantity)";
                            connection.Execute(sql, item, txn);
                        }
                    }
                    #endregion
                    txn.Commit();
                    return model;
                }
                catch (Exception)
                {
                    txn.Rollback();
                    throw;
                }
            }
        }
    }
}