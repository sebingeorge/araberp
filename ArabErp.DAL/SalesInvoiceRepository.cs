using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;
using System.Collections;

namespace ArabErp.DAL
{
    public class SalesInvoiceRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");


        //        public int InsertSalesInvoice(SalesInvoice objSalesInvoice)
        //        {
        //            using (IDbConnection connection = OpenConnection(dataConnection))
        //            {
        //                string sql = @"insert  into SalesInvoice(SalesInvoiceDate,JobCardId,SpecialRemarks,PaymentTerms,CreatedBy,CreatedDate) Values (@SalesInvoiceDate,@JobCardId,@SpecialRemarks,@PaymentTerms,@CreatedBy,@CreatedDate);
        //            SELECT CAST(SCOPE_IDENTITY() as int)";


        //                var id = connection.Query<int>(sql, objSalesInvoice).Single();
        //                return id;
        //            }
        //        }

        public SalesInvoice GetSalesInvoiceHdforPrint(int SalesInvoiceId, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string sql = @"select O.*,SalesInvoiceRefNo,SalesInvoiceDate,CustomerName Customer,Concat(C.DoorNo,',',C.Street,',',C.Phone)CustomerAddress,S.CustomerOrderRef,
                                SI.PaymentTerms,V.RegistrationNo,J.JobCardNo,D.DeliveryChallanRefNo,SI.TotalAmount,ORR.CountryName,CU.CurrencyName,
								U.UserName CreateUser,U.Signature CreateSig,UI.UserName ApproveUser,UI.Signature ApproveSig,DS.DesignationName CreatedDes,
								DSi.DesignationName ApprovedDes
								 from SalesInvoice SI
                                inner join SaleOrder S on S.SaleOrderId=SI.SaleOrderId
                                inner join Customer C ON C.CustomerId=S.CustomerId
                                inner join JobCard J ON J.SaleOrderId=S.SaleOrderId
								inner join Organization O ON si.OrganizationId=o.OrganizationId
								left  JOIN Country ORR ON ORR.CountryId=O.Country
								left JOIN Currency CU ON CU.CurrencyId=O.CurrencyId
                                left join VehicleInPass V ON V.VehicleInPassId=J.InPassId
								left join DeliveryChallan D ON D.JobCardId=J.JobCardId 
								left join [User] U ON U.UserId=SI.CreatedBy
								left join [User] UI ON UI.UserId=SI.IsApprovedBy
								left join Designation DS ON DS.DesignationId=U.DesignationId
								left join Designation DSI ON DSI.DesignationId=UI.DesignationId
							    where SalesInvoiceId=@SalesInvoiceId";

                var objSalesInvoice = connection.Query<SalesInvoice>(sql, new
                {
                    SalesInvoiceId = SalesInvoiceId,
                    OrganizationId = OrganizationId
                }).First<SalesInvoice>();

                return objSalesInvoice;
            }
        }

        public List<SalesInvoice> GetSalesInvoices()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from SalesInvoice
                        where isActive=1";

                var objSalesInvoices = connection.Query<SalesInvoice>(sql).ToList<SalesInvoice>();

                return objSalesInvoices;
            }
        }

        public int UpdateSalesInvoice(SalesInvoice objSalesInvoice)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                string sql = @"UPDATE SalesInvoice SET SalesInvoiceRefNo=@SalesInvoiceRefNo,SalesInvoiceDate = @SalesInvoiceDate ,
                               SalesInvoiceDueDate=@SalesInvoiceDueDate,SaleOrderId=@SaleOrderId,SpecialRemarks=@SpecialRemarks,
                               PaymentTerms = @PaymentTerms,Addition=@Addition,Deduction=@Deduction,AdditionRemarks=@AdditionRemarks,
                               DeductionRemarks=@DeductionRemarks,InvoiceType=@InvoiceType,isProjectBased=@isProjectBased,
                               CreatedBy = @CreatedBy,CreatedDate = @CreatedDate,OrganizationId = @OrganizationId,TotalAmount=@TotalAmount  
                               OUTPUT INSERTED.SalesInvoiceRefNo  WHERE SalesInvoiceId = @SalesInvoiceId";

                try
                {
                    var id = connection.Execute(sql, objSalesInvoice, txn);

                    #region update customer order ref to [SaleOrder] if isService = 1
                    if (objSalesInvoice.isService == 1)
                    {
                        sql = @"UPDATE SaleOrder SET CustomerOrderRef = '" + objSalesInvoice.CustomerOrderRef.Trim() + @"'
                                WHERE SaleOrderId = " + objSalesInvoice.SaleOrderId;
                        if (connection.Execute(sql, transaction: txn) <= 0) throw new Exception();
                    }
                    #endregion

                    var SalesInvoiceItemRepo = new SalesInvoiceItemRepository();
                    foreach (var item in objSalesInvoice.SaleInvoiceItems)
                    {
                        item.SalesInvoiceId = objSalesInvoice.SalesInvoiceId;

                        item.OrganizationId = objSalesInvoice.OrganizationId;
                        SalesInvoiceItemRepo.InsertSalesInvoiceItem(item, connection, txn);
                    }

                    sql = @"UPDATE PrintDescription SET PriceEach = @PriceEach, Amount = @Amount WHERE PrintDescriptionId = @PrintDescriptionId";
                    foreach (var item in objSalesInvoice.PrintDescriptions)
                    {
                        if (item.PrintDescriptionId == 0) continue;
                        item.Amount = (item.Quantity ?? 0) * item.PriceEach;
                        if (connection.Execute(sql, item, txn) <= 0) throw new Exception();
                    }

                    InsertLoginHistory(dataConnection, objSalesInvoice.CreatedBy, "Update", "Sales Invoice", id.ToString(), objSalesInvoice.OrganizationId.ToString());
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


        public int DeleteSalesInvoice(Unit objSalesInvoice)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete SalesInvoice  OUTPUT DELETED.SalesInvoiceId WHERE SalesInvoiceId=@SalesInvoiceId";


                var id = connection.Execute(sql, objSalesInvoice);
                InsertLoginHistory(dataConnection, objSalesInvoice.CreatedBy, "Delete", "Sales Invoice", id.ToString(), "0");
                return id;
            }
        }

        public string DeleteInvoice(int Id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    string query = @"DELETE FROM SalesInvoiceItem WHERE SalesInvoiceId=@Id;
                                     DELETE FROM SalesInvoice OUTPUT deleted.SalesInvoiceRefNo WHERE SalesInvoiceId=@Id;";
                    string output = connection.Query<string>(query, new { Id = Id }, txn).First();
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

        public List<SalesInvoice> GetSalesInvoiceCustomerList(string invType, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = string.Empty;
                if (invType == "Inter")
                {
                    sql = @"SELECT DISTINCT C.CustomerName Customer, SO.SaleOrderId SaleOrderId,CONCAT(SO.SaleOrderRefNo,' - ',Convert(varchar(15),SO.SaleOrderDate,106 )) as SaleOrderRefNoWithDate
                        FROM SaleOrder SO 
                        INNER JOIN SaleOrderItem SOI ON SO.SaleOrderId=SOI.SaleOrderId
						INNER JOIN JobCard JC ON JC.SaleOrderItemId=SOI.SaleOrderItemId
						INNER JOIN Customer C ON C.CustomerId=SO.CustomerId
                        LEFT JOIN SalesInvoiceItem SII ON SOI.SaleOrderItemId=SII.SaleOrderItemId
						WHERE SII.SalesInvoiceId IS NULL  AND SO.isProjectBased = 1 /*AND JC.JodCardCompleteStatus=1*/
						AND SO.isActive=1
						AND JC.isActive=1
						AND C.isActive=1
						AND SOI.isActive=1
                        AND SO.OrganizationId = @OrganizationId ";
                }
                else if (invType == "Final")
                {
                    sql = @"SELECT DISTINCT C.CustomerName Customer, SO.SaleOrderId SaleOrderId,CONCAT(SO.SaleOrderRefNo,' - ',Convert(varchar(15),SO.SaleOrderDate,106 )) as SaleOrderRefNoWithDate
                        FROM SaleOrder SO 
                        INNER JOIN SaleOrderItem SOI ON SO.SaleOrderId=SOI.SaleOrderId
						INNER JOIN JobCard JC ON JC.SaleOrderItemId=SOI.SaleOrderItemId
						INNER JOIN Customer C ON C.CustomerId=SO.CustomerId
                        LEFT JOIN SalesInvoiceItem SII ON SOI.SaleOrderItemId=SII.SaleOrderItemId
						WHERE SII.SalesInvoiceId IS NULL AND SO.isProjectBased = 0 AND JC.JodCardCompleteStatus=1 
						AND SO.isActive=1
						AND JC.isActive=1
						AND C.isActive=1
						AND SOI.isActive=1
                        AND SO.OrganizationId = @OrganizationId";
                }
                else if (invType == "Transportation")
                {
                    sql = @"SELECT DISTINCT C.CustomerName Customer, SO.SaleOrderId SaleOrderId,CONCAT(SO.SaleOrderRefNo,'/',Convert(varchar(15),SO.SaleOrderDate,106 )) as SaleOrderRefNoWithDate
                        FROM SaleOrder SO 
                        INNER JOIN SaleOrderItem SOI ON SO.SaleOrderId=SOI.SaleOrderId
						INNER JOIN JobCard JC ON JC.SaleOrderItemId=SOI.SaleOrderItemId
						INNER JOIN Customer C ON C.CustomerId=SO.CustomerId
                        LEFT JOIN SalesInvoiceItem SII ON SOI.SaleOrderItemId=SII.SaleOrderItemId
						WHERE SII.SalesInvoiceId IS NULL AND JC.JodCardCompleteStatus=1
						AND SO.isActive=1
						AND JC.isActive=1
						AND C.isActive=1
						AND SOI.isActive=1
                        AND SO.OrganizationId = @OrganizationId";
                }

                var objSalesInvoices = connection.Query<SalesInvoice>(sql, new { OrganizationId = OrganizationId }).ToList<SalesInvoice>();

                return objSalesInvoices;
            }
        }
        public List<SalesInvoiceItem> GetPendingSalesInvoiceList(int SaleOrderId,string invType, string DeliveryNo, string CustomerName, string RegNo,string InstallType)
        {
            //  int salesOrderId = Convert.ToInt32(SalesOrderId);
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = string.Empty;
                if (invType == "Inter")
                {
                    sql = @"SELECT * INTO #SaleOrder FROM SaleOrder WHERE SaleOrderId=ISNULL(NULLIF(@SaleOrderId, 0),@SaleOrderId) AND isActive=1;
                            SELECT SO.SaleOrderId SaleOrderId,SOI.WorkDescriptionId WorkDescriptionId,SOI.SaleOrderItemId SaleOrderItemId,SOI.Quantity Quantity,SOI.Rate Rate,SOI.Amount Amount,SOI.VehicleModelId,JC.JobCardNo JobCardNo, JC.JobCardDate INTO #TEMP_ORDER 
                            FROM #SaleOrder SO LEFT JOIN SaleOrderItem SOI ON SO.SaleOrderId=SOI.SaleOrderId
	        				LEFT JOIN JobCard JC ON JC.SaleOrderItemId=SOI.SaleOrderItemId;		        			
                            SELECT * INTO #SalesInvoice FROM SalesInvoice WHERE SaleOrderId=ISNULL(NULLIF(@SaleOrderId, 0),@SaleOrderId) AND isActive=1;
                            SELECT SI.SaleOrderId,SII.SaleOrderItemId INTO #TEMP_INVOICE FROM #SalesInvoice SI LEFT JOIN SalesInvoiceItem SII ON SI.SalesInvoiceId=SII.SalesInvoiceId;
                            SELECT O.SaleOrderId,O.SaleOrderItemId,O.Quantity,O.Rate,O.Amount,O.VehicleModelId,O.WorkDescriptionId WorkDescriptionId,W.WorkDescr WorkDescr,O.JobCardNo JobCardNo, O.JobCardDate INTO #RESULT FROM #TEMP_ORDER O 
                            LEFT JOIN #TEMP_INVOICE I ON O.SaleOrderId=I.SaleOrderId AND O.SaleOrderItemId=I.SaleOrderItemId 
                            LEFT JOIN WorkDescription W ON W.WorkDescriptionId=O.WorkDescriptionId
                            WHERE I.SaleOrderId IS NULL AND I.SaleOrderItemId IS NULL;
                            SELECT R.SaleOrderId SaleOrderId,R.SaleOrderItemId SaleOrderItemId,R.Quantity Quantity,R.Rate Rate,r.Amount Amount,
                            CONCAT(V.VehicleModelName,'',VehicleModelDescription) VehicleModelName,R.WorkDescr WorkDescription,R.JobCardNo JobCardNo, CONVERT(VARCHAR, R.JobCardDate, 106)JobCardDate,VIP.RegistrationNo,VIP.ChassisNo FROM #RESULT R 
                            LEFT JOIN VehicleModel V ON R.VehicleModelId=V.VehicleModelId
                            LEFT JOIN VehicleInPass VIP ON VIP.SaleOrderItemId=R.SaleOrderItemId
                            DROP TABLE #RESULT;
                            DROP TABLE #SaleOrder;
                            DROP TABLE #SalesInvoice;
                            DROP TABLE #TEMP_INVOICE;
                            DROP TABLE #TEMP_ORDER;";
                }
                else if (invType == "Final")
                {
                    sql = @"SELECT * INTO #SaleOrder FROM SaleOrder WHERE SaleOrderId=ISNULL(NULLIF(CAST(0 AS INT), @SaleOrderId), SaleOrderId) AND isActive=1;
                            SELECT SO.SaleOrderId SaleOrderId,SO.CustomerId, SO.SaleOrderRefNo, SO.SaleOrderDate, SOI.WorkDescriptionId WorkDescriptionId,SOI.SaleOrderItemId SaleOrderItemId,SOI.Quantity Quantity,SOI.Rate Rate,SOI.Amount Amount,SOI.VehicleModelId,JC.JobCardNo JobCardNo, JC.JobCardDate, JC.JobCardId,JC.isService INTO #TEMP_ORDER 
                            FROM #SaleOrder SO LEFT JOIN SaleOrderItem SOI ON SO.SaleOrderId=SOI.SaleOrderId
                            LEFT JOIN JobCard JC ON JC.SaleOrderItemId=SOI.SaleOrderItemId
                            WHERE JC.JodCardCompleteStatus=1
                            SELECT * INTO #SalesInvoice FROM SalesInvoice WHERE SaleOrderId=ISNULL(NULLIF(CAST(0 AS INT), @SaleOrderId), SaleOrderId) AND isActive=1;
                            SELECT SI.SaleOrderId,SII.SaleOrderItemId INTO #TEMP_INVOICE FROM #SalesInvoice SI LEFT JOIN SalesInvoiceItem SII ON SI.SalesInvoiceId=SII.SalesInvoiceId;
                           
						    SELECT O.SaleOrderId, O.SaleOrderRefNo,o.CustomerId,O.SaleOrderDate, O.SaleOrderItemId,O.Quantity,O.Rate,O.Amount,O.VehicleModelId,O.WorkDescriptionId WorkDescriptionId,W.WorkDescr WorkDescr,O.JobCardNo JobCardNo, O.JobCardDate, O.JobCardId,O.isService INTO #RESULT FROM #TEMP_ORDER O 
                            LEFT JOIN #TEMP_INVOICE I ON O.SaleOrderId=I.SaleOrderId AND O.SaleOrderItemId=I.SaleOrderItemId 
                            LEFT JOIN WorkDescription W ON W.WorkDescriptionId=O.WorkDescriptionId

                            WHERE I.SaleOrderId IS NULL AND I.SaleOrderItemId IS NULL;

                            SELECT R.SaleOrderId SaleOrderId, R.SaleOrderRefNo, CONVERT(VARCHAR, R.SaleOrderDate, 106)SaleOrderDate, 
							R.SaleOrderItemId SaleOrderItemId,R.Quantity Quantity,R.Rate Rate,r.Amount Amount,C.CustomerName,
                            CONCAT(V.VehicleModelName,'',VehicleModelDescription)
							VehicleModelName,R.WorkDescr WorkDescription,R.JobCardNo JobCardNo, 
							CONVERT(VARCHAR, R.JobCardDate, 106)JobCardDate,VIP.RegistrationNo,VIP.ChassisNo, DC.DeliveryChallanRefNo,
							CONVERT(VARCHAR, DC.DeliveryChallanDate, 106)DeliveryChallanDate FROM #RESULT R 
							LEFT JOIN Customer C ON C.CustomerId=R.CustomerId
                            LEFT JOIN VehicleModel V ON R.VehicleModelId=V.VehicleModelId
                            LEFT JOIN VehicleInPass VIP ON VIP.SaleOrderItemId=R.SaleOrderItemId
                            LEFT JOIN DeliveryChallan DC ON R.JobCardId = DC.JobCardId
							WHERE DC.DeliveryChallanId IS NOT NULL  AND
                            ISNULL(R.isService, 0) = CASE @InstallType WHEN 'service' THEN 1 WHEN 'new' THEN 0 WHEN 'all' THEN ISNULL(R.isService, 0) END
							AND ISNULL(C.CustomerName,'') LIKE '%'+@CustomerName+'%'
                            AND (ISNULL(VIP.RegistrationNo, '') LIKE '%'+@RegNo+'%'
			                OR ISNULL(VIP.ChassisNo, '') LIKE '%'+@RegNo+'%')
				            AND (ISNULL(DC.DeliveryChallanRefNo, '') LIKE '%'+@DeliveryNo+'%'
			                OR ISNULL(DeliveryChallanDate, '') LIKE '%'+@DeliveryNo+'%')

				ORDER BY DeliveryChallanDate desc, DeliveryChallanId desc
                            DROP TABLE #RESULT;
                            DROP TABLE #SaleOrder;
                            DROP TABLE #SalesInvoice;
                            DROP TABLE #TEMP_INVOICE;
                            DROP TABLE #TEMP_ORDER;;";
                }
                else if (invType == "Transportation")
                {
                    sql = @"SELECT * INTO #SaleOrder FROM SaleOrder WHERE SaleOrderId=@SaleOrderId AND isActive=1;
                            SELECT SO.SaleOrderId SaleOrderId,SOI.WorkDescriptionId WorkDescriptionId,SOI.SaleOrderItemId SaleOrderItemId,SOI.Quantity Quantity,SOI.Rate Rate,SOI.Amount Amount,SOI.VehicleModelId,JC.JobCardNo JobCardNo INTO #TEMP_ORDER 
                            FROM #SaleOrder SO LEFT JOIN SaleOrderItem SOI ON SO.SaleOrderId=SOI.SaleOrderId
	        				LEFT JOIN JobCard JC ON JC.SaleOrderItemId=SOI.SaleOrderItemId
		        			WHERE JC.JodCardCompleteStatus=1
                            SELECT * INTO #SalesInvoice FROM SalesInvoice WHERE SaleOrderId=@SaleOrderId AND isActive=1;
                            SELECT SI.SaleOrderId,SII.SaleOrderItemId INTO #TEMP_INVOICE FROM #SalesInvoice SI LEFT JOIN SalesInvoiceItem SII ON SI.SalesInvoiceId=SII.SalesInvoiceId;
                            SELECT O.SaleOrderId,O.SaleOrderItemId,O.Quantity,O.Rate,O.Amount,O.VehicleModelId,O.WorkDescriptionId WorkDescriptionId,W.WorkDescr WorkDescr,O.JobCardNo JobCardNo INTO #RESULT FROM #TEMP_ORDER O 
                            LEFT JOIN #TEMP_INVOICE I ON O.SaleOrderId=I.SaleOrderId AND O.SaleOrderItemId=I.SaleOrderItemId 
                            LEFT JOIN WorkDescription W ON W.WorkDescriptionId=O.WorkDescriptionId
                            WHERE I.SaleOrderId IS NULL AND I.SaleOrderItemId IS NULL;
                            SELECT R.SaleOrderId SaleOrderId,R.SaleOrderItemId SaleOrderItemId,R.Quantity Quantity,R.Rate Rate,r.Amount Amount,
                            CONCAT(V.VehicleModelName,'',VehicleModelDescription) VehicleModelName,R.WorkDescr WorkDescription,R.JobCardNo JobCardNo FROM #RESULT R 
                            LEFT JOIN VehicleModel V ON R.VehicleModelId=V.VehicleModelId
                            DROP TABLE #RESULT;
                            DROP TABLE #SaleOrder;
                            DROP TABLE #SalesInvoice;
                            DROP TABLE #TEMP_INVOICE;
                            DROP TABLE #TEMP_ORDER;";
                }

                return connection.Query<SalesInvoiceItem>(sql, new { SaleOrderId = SaleOrderId, 
                    DeliveryNo = DeliveryNo,
                    CustomerName = CustomerName,
                    RegNo = RegNo,
                    InstallType = InstallType
                }).ToList();
            }
        }

        public List<LabourCostForService> getLabourCost(int id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"SELECT
	                                JCTM.JobCardTaskName,
	                                JCT.ActualHours,
	                                JCTM.MinimumRate,
	                                CAST((ISNULL(JCTM.MinimumRate, 0) * ISNULL(JCT.ActualHours, 0)) AS DECIMAL(18,2)) Amount,
									EMP.EmployeeName
                                FROM JobCard JC
                                INNER JOIN JobCardTask JCT ON JC.JobCardId = JCT.JobCardId
                                INNER JOIN JobCardTaskMaster JCTM ON JCT.JobCardTaskMasterId = JCTM.JobCardTaskMasterId
								INNER JOIN Employee EMP ON JCT.EmployeeId = EMP.EmployeeId
                                WHERE JC.JobCardId = @id";
                return connection.Query<LabourCostForService>(sql, new { id = id }).ToList();
            }
        }

        public List<MaterialCostForService> getMaterialCost(int id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                #region old query 3.1.2016 4.08p
                //                string sql = @"SELECT
                //	                                WR.WorkShopRequestRefNo,
                //	                                SI.StoreIssueRefNo,
                //	                                I.ItemName,
                //	                                I.PartNo,
                //	                                WRI.Quantity,
                //	                                WRI.Remarks,
                //									ISNULL(SR.Rate, 0.00) Rate,
                //                                    CAST(ISNULL(SR.Rate, 0.00) * WRI.Quantity AS DECIMAL(18,2)) Amount
                //                                FROM JobCard JC
                //                                LEFT JOIN WorkShopRequest WR ON JC.JobCardId = WR.JobCardId
                //                                INNER JOIN WorkShopRequestItem WRI ON WR.WorkShopRequestId = WRI.WorkShopRequestId
                //                                INNER JOIN StoreIssue SI ON WR.WorkShopRequestId = SI.WorkShopRequestId
                //                                INNER JOIN StoreIssueItem SII ON WRI.WorkShopRequestItemId = SII.WorkShopRequestItemId
                //                                INNER JOIN Item I ON WRI.ItemId = I.ItemId
                //								LEFT JOIN StandardRate SR ON I.ItemId = SR.ItemId
                //                                WHERE JC.JobCardId = @id"; 
                #endregion

                string sql = @"SELECT DISTINCT
		                            WRI.ItemId
	                            INTO #ITEMS
	                            FROM JobCard JC
	                            LEFT JOIN WorkShopRequest WR ON JC.JobCardId = WR.JobCardId
	                            INNER JOIN WorkShopRequestItem WRI ON WR.WorkShopRequestId = WRI.WorkShopRequestId
	                            INNER JOIN StoreIssue SI ON WR.WorkShopRequestId = SI.WorkShopRequestId
	                            INNER JOIN StoreIssueItem SII ON WRI.WorkShopRequestItemId = SII.WorkShopRequestItemId
	                            WHERE JC.JobCardId = @id

	                            SELECT DISTINCT
		                            I.ItemId,
		                            --I.ItemName,
		                            ISNULL(GI.Rate, ISNULL(SR.Rate, 0)) Rate
	                            INTO #TEMP
	                            FROM GRNItem GI
	                            INNER JOIN (SELECT MAX(GRNItemId)GRNItemId FROM GRNItem 
				                            WHERE ItemId IN (SELECT * FROM #ITEMS) 
				                            GROUP BY ItemId) T1 ON GI.GRNItemId = T1.GRNItemId
	                            RIGHT JOIN (SELECT * FROM #ITEMS) I ON GI.ItemId = I.ItemId
	                            LEFT JOIN StandardRate SR ON I.ItemId = SR.ItemId

	                            DROP TABLE #ITEMS;

	                            SELECT
		                            WR.WorkShopRequestRefNo,
		                            SI.StoreIssueRefNo,
		                            I.ItemId,
		                            I.ItemName,
		                            I.PartNo,
		                            WRI.Quantity,
		                            WRI.Remarks,
		                            ISNULL(#TEMP.Rate, 0.00) Rate,
		                            CAST(ISNULL(#TEMP.Rate, 0.00) * WRI.Quantity AS DECIMAL(18,2)) Amount
	                            FROM JobCard JC
	                            LEFT JOIN WorkShopRequest WR ON JC.JobCardId = WR.JobCardId
	                            INNER JOIN WorkShopRequestItem WRI ON WR.WorkShopRequestId = WRI.WorkShopRequestId
	                            INNER JOIN StoreIssue SI ON WR.WorkShopRequestId = SI.WorkShopRequestId
	                            INNER JOIN StoreIssueItem SII ON WRI.WorkShopRequestItemId = SII.WorkShopRequestItemId
	                            INNER JOIN Item I ON WRI.ItemId = I.ItemId
	                            LEFT JOIN #TEMP ON I.ItemId = #TEMP.ItemId
	                            WHERE JC.JobCardId = @id;

	                            DROP TABLE #TEMP;";

                return connection.Query<MaterialCostForService>(sql, new { id = id }).ToList();
            }
        }

        public SalesInvoice GetSelectedSalesInvoiceHD(int salesinvoiceid, string invType)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = string.Empty;
                if (invType == "Inter")
                {
                    sql = @" SELECT * INTO #SaleOrder FROM SaleOrder WHERE SaleOrderId=@SaleOrderId
                                select Distinct 
                                C.CustomerName Customer,
                                SO.SaleOrderId SaleOrderId,
                                S.SymbolName CurrencySymbol,
                                Convert(varchar(15),Getdate(),106) CurrentDate,
                                isnull((getdate()+CreditPeriod),getdate())SalesInvoiceDueDate,
                                SO.SaleOrderRefNo SaleOrderRefNo ,
                                Concat(ISNULL(C.DoorNo,''),',',ISNULL(C.Street,''),',',ISNULL(C.State,''),',',ISNULL(C.Country,''),',',ISNULL(C.Zip,''))
                                CustomerAddress,
                                SO.CustomerOrderRef CustomerOrderRef,
                                SO.SpecialRemarks SpecialRemarks,
                                SO.PaymentTerms PaymentTerms,
                                SO.isService
                                from #SaleOrder SO 
                                left join SaleOrderItem SOI on SO.SaleOrderId=SOI.SaleOrderId
                                Left join JobCard JC on SO.SaleOrderId=JC.SaleOrderId
                                Left join Customer C on SO.CustomerId=C.CustomerId
                                LEFT JOIN Currency CU ON CU.CurrencyId=C.CurrencyId
                                LEFT JOIN Symbol S ON S.SymbolId=CU.CurrencySymbolId 
                                Left Join WorkDescription WD on SOI.WorkDescriptionId=WD.WorkDescriptionId
                                where SO.isProjectBased = 1 AND SO.isActive=1 AND SOI.isActive=1 AND JC.isActive=1
                                DROP TABLE #SaleOrder";
                }
                else if (invType == "Final")
                {
                    sql = @" SELECT * INTO #SaleOrder FROM SaleOrder WHERE SaleOrderId=@SaleOrderId
                            select Distinct 
                            C.CustomerName Customer,
                            SO.SaleOrderId SaleOrderId,
                            CU.CurrencyName CurrencySymbol,
                            Convert(varchar(15),Getdate(),106) CurrentDate,
                            isnull((getdate()+CreditPeriod),getdate())SalesInvoiceDueDate,
                            SO.SaleOrderRefNo SaleOrderRefNo ,
                            Concat(ISNULL(C.DoorNo,''),',',ISNULL(C.Street,''),',',ISNULL(C.State,''),',',ISNULL(C.Country,''),',',ISNULL(C.Zip,''))
                            CustomerAddress,
                            SO.CustomerOrderRef CustomerOrderRef,
                            SO.SpecialRemarks SpecialRemarks,
                            SO.PaymentTerms PaymentTerms,
                            JC.isService from #SaleOrder SO 
                            left join SaleOrderItem SOI on SO.SaleOrderId=SOI.SaleOrderId
                            Left join JobCard JC on SO.SaleOrderId=JC.SaleOrderId
                            Left join Customer C on SO.CustomerId=C.CustomerId
                            LEFT JOIN Currency CU ON CU.CurrencyId=C.CurrencyId
                            LEFT JOIN Symbol S ON S.SymbolId=CU.CurrencySymbolId 
                            Left Join WorkDescription WD on SOI.WorkDescriptionId=WD.WorkDescriptionId
                            where JC.JodCardCompleteStatus=1 AND SO.isActive=1 AND SOI.isActive=1 AND JC.isActive=1
                            DROP TABLE #SaleOrder";
                }
                else if (invType == "Transportation")
                {
                    sql = @" SELECT * INTO #SaleOrder FROM SaleOrder WHERE SaleOrderId=@SaleOrderId
                            select Distinct 
                            C.CustomerName Customer,
                            SO.SaleOrderId SaleOrderId,
                            S.SymbolName CurrencySymbol,
                            Convert(varchar(15),Getdate(),106) CurrentDate,
                            isnull((getdate()+CreditPeriod),getdate())SalesInvoiceDueDate,
                            SO.SaleOrderRefNo SaleOrderRefNo ,
                            Concat(ISNULL(C.DoorNo,''),',',ISNULL(C.Street,''),',',ISNULL(C.State,''),',',ISNULL(C.Country,''),',',ISNULL(C.Zip,''))
                            CustomerAddress,
                            SO.CustomerOrderRef CustomerOrderRef,
                            SO.SpecialRemarks SpecialRemarks,
                            SO.PaymentTerms PaymentTerms from #SaleOrder SO 
                            left join SaleOrderItem SOI on SO.SaleOrderId=SOI.SaleOrderId
                            Left join JobCard JC on SO.SaleOrderId=JC.SaleOrderId
                            Left join Customer C on SO.CustomerId=C.CustomerId
                            LEFT JOIN Currency CU ON CU.CurrencyId=C.CurrencyId
                            LEFT JOIN Symbol S ON S.SymbolId=CU.CurrencySymbolId 
                            Left Join WorkDescription WD on SOI.WorkDescriptionId=WD.WorkDescriptionId
                            where JC.JodCardCompleteStatus=1 AND SO.isActive=1 AND SOI.isActive=1 AND JC.isActive=1
                            DROP TABLE #SaleOrder";
                }

                var objSalesInvoiceHD = connection.Query<SalesInvoice>(sql, new { SaleOrderId = salesinvoiceid }).First();

                return objSalesInvoiceHD;
            }
        }
        public SalesInvoice InsertSalesInvoice(SalesInvoice model)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var result = new SalesInvoice();
                IDbTransaction trn = connection.BeginTransaction();
                try
                {
                    var internalId = DatabaseCommonRepository.GetNewDocNo(connection, model.OrganizationId, 7, true, trn);
                    model.SalesInvoiceRefNo = internalId.ToString();

                    string sql = @" INSERT INTO SalesInvoice(SalesInvoiceRefNo,SalesInvoiceDate,SalesInvoiceDueDate,SaleOrderId,
                                    SpecialRemarks,PaymentTerms,Addition,Deduction,AdditionRemarks,
                                    DeductionRemarks,InvoiceType,isProjectBased,CreatedBy,CreatedDate,OrganizationId, TotalAmount)
                                    VALUES( @SalesInvoiceRefNo,@SalesInvoiceDate,@SalesInvoiceDueDate,@SaleOrderId,@SpecialRemarks,@PaymentTerms,
                                    @Addition,@Deduction,@AdditionRemarks,@DeductionRemarks,@InvoiceType,@isProjectBased,@CreatedBy,@CreatedDate,@OrganizationId, @TotalAmount);
                                   SELECT CAST(SCOPE_IDENTITY() as int) SalesInvoiceId";


                    result = connection.Query<SalesInvoice>(sql, model, trn).Single<SalesInvoice>();

                    #region update customer order ref to [SaleOrder] 
                    //if (model.isService == 1)
                    {
                        sql = @"UPDATE SaleOrder SET CustomerOrderRef = '" + model.CustomerOrderRef.Trim() + @"'
                                WHERE SaleOrderId = " + model.SaleOrderId;
                        if (connection.Execute(sql, transaction: trn) <= 0) throw new Exception();
                    }
                    #endregion

                    var SalesInvoiceItemRepo = new SalesInvoiceItemRepository();
                    foreach (var item in model.SaleInvoiceItems)
                    {
                        item.SalesInvoiceId = result.SalesInvoiceId;

                        item.OrganizationId = model.OrganizationId;
                        SalesInvoiceItemRepo.InsertSalesInvoiceItem(item, connection, trn);
                    }

                    sql = @"UPDATE PrintDescription SET PriceEach = @PriceEach, Amount = @Amount WHERE PrintDescriptionId = @PrintDescriptionId";
                    foreach (var item in model.PrintDescriptions)
                    {
                        if (item.PrintDescriptionId == null || item.PrintDescriptionId == 0) continue;
                        item.Amount = (item.Quantity ?? 0) * item.PriceEach;
                        if (connection.Execute(sql, item, trn) <= 0) throw new Exception();
                    }

                    InsertLoginHistory(dataConnection, model.CreatedBy, "Create", "Sales Invoice", result.SalesInvoiceId.ToString(), model.OrganizationId.ToString());
                    trn.Commit();
                    //return id + "|INV/" + internalId;
                }
                catch (Exception ex)
                {
                    result.SalesInvoiceId = 0;
                    result.SalesInvoiceRefNo = null;

                    trn.Rollback();

                    throw ex;
                }
                return result;

            }



        }

        public DateTime GetDueDate(DateTime d, int SaleOrderId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                SalesInvoice SalesInvoice = connection.Query<SalesInvoice>(
                    "SELECT ISNULL( DATEADD(day,CreditPeriod,@date),getdate()) SalesInvoiceDueDate FROM SaleOrder S INNER JOIN Customer C ON S.CustomerId=C.CustomerId WHERE SaleOrderId= " + SaleOrderId,
                    new { date = d }).Single<SalesInvoice>();
                DateTime duedate = System.DateTime.Today;
                if (SalesInvoice != null)
                {
                    duedate = SalesInvoice.SalesInvoiceDueDate;
                }
                return duedate;
            }
        }

        /// <summary>
        /// All items in [SalesInvoice] where isProjectBased = 0
        /// </summary>
        /// <returns></returns>
        public IList<SalesInvoice> PreviousList(string type, int OrganizationId, DateTime? from, DateTime? to, int id = 0)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
//                string query = @"SELECT INV.SalesInvoiceId,INV.SalesInvoiceRefNo,INV.SalesInvoiceDate,
//	                             SO.SaleOrderRefNo,SO.SaleOrderDate,D.DeliveryChallanRefNo,
//                                 C.CustomerName Customer,V.RegistrationNo,ChassisNo,
//	                             ISNULL(INV.SpecialRemarks, '-') SpecialRemarks,isnull(INV.TotalAmount,0)TotalAmount
//                                 FROM SalesInvoice INV
//                                 LEFT JOIN SaleOrder SO ON INV.SaleOrderId = SO.SaleOrderId
//                                 LEFT JOIN Customer C ON C.CustomerId=SO.CustomerId
//                                 LEFT JOIN VehicleInPass V ON V.SaleOrderId=SO.SaleOrderId
//                                 LEFT JOIN JobCard J ON J.SaleOrderId=SO.SaleOrderId
//                                 LEFT JOIN DeliveryChallan D ON D.JobCardId=J.JobCardId 
//                                 WHERE 
//								 INV.OrganizationId=1 
//                                 ORDER BY INV.SalesInvoiceDate DESC, INV.CreatedDate DESC";
                string query = @"SELECT DISTINCT INV.SalesInvoiceId,INV.SalesInvoiceRefNo,INV.SalesInvoiceDate,
                                SO.SaleOrderRefNo,SO.SaleOrderDate,C.CustomerName Customer,

                                STUFF((SELECT ', ' +  isnull(V.RegistrationNo,'')
                                FROM  VehicleInPass V
                                WHERE V.SaleOrderItemId=INVI.SaleOrderItemId
                                FOR XML PATH('')), 1, 1, '') RegistrationNo,

                                STUFF((SELECT ', ' + isnull(V.ChassisNo,'')
                                FROM  VehicleInPass V
                                WHERE V.SaleOrderItemId=INVI.SaleOrderItemId
                                FOR XML PATH('')), 1, 1, '')ChasisNo,

                                STUFF((SELECT DISTINCT ', ' + isnull(D.DeliveryChallanRefNo,'')
                                FROM  DeliveryChallan D
                                INNER JOIN JobCard J ON J.SaleOrderId=SO.SaleOrderId 
                                WHERE D.JobCardId=INVI.JobCardId 
                                FOR XML PATH('')), 1, 1, '')DeliveryChallanRefNo,

                                ISNULL(INV.SpecialRemarks, '-') SpecialRemarks,isnull(INV.TotalAmount,0)TotalAmount
                                FROM SalesInvoice INV
                                INNER JOIN SalesInvoiceItem INVI ON INV.SalesInvoiceId=INVI.SalesInvoiceId
                                INNER JOIN  SaleOrder SO ON INV.SaleOrderId = SO.SaleOrderId
                                LEFT JOIN Customer C ON C.CustomerId=SO.CustomerId

                                WHERE INV.OrganizationId=@OrganizationId AND INV.SalesInvoiceDate >= @from AND INV.SalesInvoiceDate <= @to
                                AND INV.SalesInvoiceId=ISNULL(NULLIF(@id, 0),INV.SalesInvoiceId)
                                ORDER BY INV.SalesInvoiceDate DESC,INV.SalesInvoiceId,INV.SalesInvoiceRefNo,
                                SO.SaleOrderRefNo,SO.SaleOrderDate,C.CustomerName";
                return connection.Query<SalesInvoice>(query, new
                {
                    OrganizationId = OrganizationId,
                    id = id,
                    from = from,
                    to = to,
                    type = type
                }).ToList();
            }
        }

        public SalesInvoice GetInvoiceHd(int Id, string type)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string sql = @" select SI.SaleOrderId,SI.SalesInvoiceRefNo,SI.isProjectBased,SI.SalesInvoiceId,
                                SI.SalesInvoiceDate,SI.SalesInvoiceDueDate,C.CustomerName Customer,
                                Concat(C.DoorNo,',',C.Street,',',C.State,',',C.Country,',',C.Zip)CustomerAddress,
                                S.CustomerOrderRef CustomerOrderRef,SI.SpecialRemarks,SI.PaymentTerms,SI.Addition,
                                SI.Deduction,SI.AdditionRemarks,SI.DeductionRemarks,SI.TotalAmount,SI.InvoiceType,
								CUR.CurrencyName CurrencySymbol, S.isService
                                from SalesInvoice SI 
                                inner join SaleOrder S on S.SaleOrderId=SI.SaleOrderId
                                inner join Customer C on S.CustomerId=C.CustomerId
								INNER JOIN Currency CUR ON C.CurrencyId = CUR.CurrencyId
								INNER JOIN Symbol SYM ON CUR.CurrencySymbolId = SYM.SymbolId
                                WHERE SalesInvoiceId=@Id";

                var objSalesInvoice = connection.Query<SalesInvoice>(sql, new
                {
                    Id = Id,
                    type = type
                }).First<SalesInvoice>();

                return objSalesInvoice;
            }
        }
        public List<SalesInvoiceItem> GetInvoiceItems(int Id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string sql = @" select SI.SalesInvoiceId,SI.SaleOrderItemId,SI.JobCardId,W.WorkDescr WorkDescription,SI.Quantity QuantityTxt,
                                SI.Rate,SI.Discount,SI.Amount,/*U.UnitName*/'No(s)' Unit,V.VehicleModelName from SalesInvoiceItem SI 
                                inner join SaleOrderItem S ON S.SaleOrderItemId=SI.SaleOrderItemId
                                inner join WorkDescription W ON W.WorkDescriptionId=S.WorkDescriptionId
                                left join Unit U ON U.UnitId=S.UnitId
                                left join VehicleModel V ON V.VehicleModelId=W.VehicleModelId
                                WHERE SalesInvoiceId= @Id";


                var objInvoiceItem = connection.Query<SalesInvoiceItem>(sql, new { Id = Id }).ToList<SalesInvoiceItem>();

                return objInvoiceItem;
            }
        }

        public IList<SalesInvoice> ApprovalList(int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT
	                                INV.SalesInvoiceId,
	                                INV.SalesInvoiceRefNo,
	                                INV.SalesInvoiceDate,
	                                SO.SaleOrderRefNo,
	                                SO.SaleOrderDate,
	                                ISNULL(INV.SpecialRemarks, '-') SpecialRemarks,isnull(INV.TotalAmount,0)TotalAmount
                                FROM SalesInvoice INV
                                LEFT JOIN SaleOrder SO ON INV.SaleOrderId = SO.SaleOrderId
                                WHERE 
								 INV.OrganizationId=1 and isnull(INV.IsApproved,0)=0
                                ORDER BY INV.SalesInvoiceDate DESC, INV.CreatedDate DESC";

                return connection.Query<SalesInvoice>(query, new
                {
                    OrganizationId = OrganizationId


                }).ToList();
            }
        }
        public int UpdateSIApproval(int id, DateTime date, int user)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Update SalesInvoice set IsApproved=1,IsApprovedDate=@date,IsApprovedBy=@user  WHERE SalesInvoiceId=@id";
                return connection.Execute(sql, new { id = id, date = date, user = user });

            }
        }
        /// <summary>
        /// Get print description from SaleOrderItemIds
        /// </summary>
        /// <param name="SaleOrderItemIds"></param>
        /// <returns></returns>
        public List<PrintDescription> GetPrintDescriptions(List<int> SaleOrderItemIds)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                try
                {
                    string sql = @"SELECT
	                                PD.*
                                FROM PrintDescription PD
                                INNER JOIN DeliveryChallan DC ON PD.DeliveryChallanId = DC.DeliveryChallanId
                                INNER JOIN JobCard JC ON DC.JobCardId = JC.JobCardId
                                WHERE JC.SaleOrderItemId IN @ids";
                    return connection.Query<PrintDescription>(sql, new { ids = SaleOrderItemIds }).ToList();
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public IEnumerable<SalesInvoiceItem> GetDeliveryChallansFromInvoice(int Id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"SELECT
	                                DC.DeliveryChallanRefNo,
	                                JC.JobCardNo,
	                                ISNULL(VI.RegistrationNo, '')RegistrationNo,
	                                ISNULL(VI.ChassisNo, '')ChassisNo
                                FROM SalesInvoiceItem SII
                                INNER JOIN JobCard JC ON SII.JobCardId = JC.JobCardId
                                INNER JOIN DeliveryChallan DC ON JC.JobCardId = DC.JobCardId
                                INNER JOIN VehicleInPass VI ON JC.InPassId = VI.VehicleInPassId
                                WHERE SII.SalesInvoiceId = @Id";
                var objInvoiceItem = connection.Query<SalesInvoiceItem>(sql, new { Id = Id }).ToList<SalesInvoiceItem>();
                return objInvoiceItem;
            }
        }
    }

}