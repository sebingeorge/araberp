using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

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

        public SalesInvoice GetSalesInvoice(int SalesInvoiceId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string sql = @"select * from SalesInvoice
                        where SalesInvoiceId=@SalesInvoiceId";

                var objSalesInvoice = connection.Query<SalesInvoice>(sql, new
                {
                    SalesInvoiceId = SalesInvoiceId
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
                string sql = @"UPDATE SalesInvoice SET SalesInvoiceDate = @SalesInvoiceDate ,JobCardId = @JobCardId ,SpecialRemarks = @SpecialRemarks ,PaymentTerms = @PaymentTerms,CreatedBy = @CreatedBy,CreatedDate = @CreatedDate,OrganizationId = @OrganizationId  OUTPUT INSERTED.SalesInvoiceId  WHERE SalesInvoiceId = @SalesInvoiceId";


                var id = connection.Execute(sql, objSalesInvoice);
                return id;
            }
        }

        public int DeleteSalesInvoice(Unit objSalesInvoice)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete SalesInvoice  OUTPUT DELETED.SalesInvoiceId WHERE SalesInvoiceId=@SalesInvoiceId";


                var id = connection.Execute(sql, objSalesInvoice);
                return id;
            }
        }
        public List<SalesInvoice> GetSalesInvoiceCustomerList(string invType)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = string.Empty;
                if(invType == "Inter")
                {
                    sql = @"SELECT DISTINCT C.CustomerName Customer, SO.SaleOrderId SaleOrderId,CONCAT(SO.SaleOrderRefNo,'/',Convert(varchar(15),SO.SaleOrderDate,106 )) as SaleOrderRefNoWithDate
                        FROM SaleOrder SO LEFT JOIN SaleOrderItem SOI ON SO.SaleOrderId=SOI.SaleOrderId
						LEFT JOIN SalesInvoiceItem SII ON SOI.SaleOrderItemId=SII.SaleOrderItemId
						LEFT JOIN JobCard JC ON JC.SaleOrderItemId=SOI.SaleOrderItemId
						LEFT JOIN Customer C ON C.CustomerId=SO.CustomerId
						WHERE SII.SalesInvoiceId IS NULL  AND SO.isProjectBased = 1 AND SO.isActive=1
						AND SO.isActive=1
						AND JC.isActive=1
						AND C.isActive=1
						AND SOI.isActive=1";
                }
                else if(invType == "Final")
                {
                    sql = @"SELECT DISTINCT C.CustomerName Customer, SO.SaleOrderId SaleOrderId,CONCAT(SO.SaleOrderRefNo,'/',Convert(varchar(15),SO.SaleOrderDate,106 )) as SaleOrderRefNoWithDate
                        FROM SaleOrder SO LEFT JOIN SaleOrderItem SOI ON SO.SaleOrderId=SOI.SaleOrderId
						LEFT JOIN SalesInvoiceItem SII ON SOI.SaleOrderItemId=SII.SaleOrderItemId
						LEFT JOIN JobCard JC ON JC.SaleOrderItemId=SOI.SaleOrderItemId
						LEFT JOIN Customer C ON C.CustomerId=SO.CustomerId
						WHERE SII.SalesInvoiceId IS NULL AND JC.JodCardCompleteStatus=1 AND SO.isActive=1
						AND SO.isActive=1
						AND JC.isActive=1
						AND C.isActive=1
						AND SOI.isActive=1";
                }
                else if (invType == "Transportation")
                {
                    sql = @"SELECT DISTINCT C.CustomerName Customer, SO.SaleOrderId SaleOrderId,CONCAT(SO.SaleOrderRefNo,'/',Convert(varchar(15),SO.SaleOrderDate,106 )) as SaleOrderRefNoWithDate
                        FROM SaleOrder SO LEFT JOIN SaleOrderItem SOI ON SO.SaleOrderId=SOI.SaleOrderId
						LEFT JOIN SalesInvoiceItem SII ON SOI.SaleOrderItemId=SII.SaleOrderItemId
						LEFT JOIN JobCard JC ON JC.SaleOrderItemId=SOI.SaleOrderItemId
						LEFT JOIN Customer C ON C.CustomerId=SO.CustomerId
						WHERE SII.SalesInvoiceId IS NULL AND JC.JodCardCompleteStatus=1 AND SO.isActive=1
						AND SO.isActive=1
						AND JC.isActive=1
						AND C.isActive=1
						AND SOI.isActive=1";
                }               

                var objSalesInvoices = connection.Query<SalesInvoice>(sql).ToList<SalesInvoice>();

                return objSalesInvoices;
            }
        }
        public List<SalesInvoiceItem> GetPendingSalesInvoiceList(int SaleOrderId, string invType)
        {
            //  int salesOrderId = Convert.ToInt32(SalesOrderId);
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = string.Empty;
                if (invType == "Inter")
                {
                    sql = @"SELECT * INTO #SaleOrder FROM SaleOrder WHERE SaleOrderId=@SaleOrderId AND isActive=1;
                            SELECT SO.SaleOrderId SaleOrderId,SOI.WorkDescriptionId WorkDescriptionId,SOI.SaleOrderItemId SaleOrderItemId,SOI.Quantity Quantity,SOI.Rate Rate,SOI.Amount Amount,SOI.VehicleModelId,JC.JobCardNo JobCardNo INTO #TEMP_ORDER 
                            FROM #SaleOrder SO LEFT JOIN SaleOrderItem SOI ON SO.SaleOrderId=SOI.SaleOrderId
	        				LEFT JOIN JobCard JC ON JC.SaleOrderItemId=SOI.SaleOrderItemId;		        			
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
                else if (invType == "Final")
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
                
                return connection.Query<SalesInvoiceItem>(sql, new { SaleOrderId = SaleOrderId }).ToList();
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
                                SO.SaleOrderRefNo SaleOrderRefNo ,
                                Concat(C.DoorNo,',',C.Street,',',C.State,',',C.Country,',',C.Zip)
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
                                where SO.isProjectBased = 1 AND SO.isActive=1 AND SOI.isActive=1 AND JC.isActive=1
                                DROP TABLE #SaleOrder";
               }
               else if (invType == "Final")
               {
                   sql = @" SELECT * INTO #SaleOrder FROM SaleOrder WHERE SaleOrderId=@SaleOrderId
                            select Distinct 
                            C.CustomerName Customer,
                            SO.SaleOrderId SaleOrderId,
                            S.SymbolName CurrencySymbol,
                            Convert(varchar(15),Getdate(),106) CurrentDate,
                            SO.SaleOrderRefNo SaleOrderRefNo ,
                            Concat(C.DoorNo,',',C.Street,',',C.State,',',C.Country,',',C.Zip)
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
               else if (invType == "Transportation")
               {
                   sql = @" SELECT * INTO #SaleOrder FROM SaleOrder WHERE SaleOrderId=@SaleOrderId
                            select Distinct 
                            C.CustomerName Customer,
                            SO.SaleOrderId SaleOrderId,
                            S.SymbolName CurrencySymbol,
                            Convert(varchar(15),Getdate(),106) CurrentDate,
                            SO.SaleOrderRefNo SaleOrderRefNo ,
                            Concat(C.DoorNo,',',C.Street,',',C.State,',',C.Country,',',C.Zip)
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
                    int internalId = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(SalesInvoice).Name, "0", 1);
                    model.SalesInvoiceRefNo = "INV/" + internalId.ToString();
                    string sql = @" INSERT INTO SalesInvoice(SalesInvoiceRefNo,SalesInvoiceDate,SaleOrderId,Addition,Deduction,AdditionRemarks,DeductionRemarks,InvoiceType,isProjectBased)
                                   VALUES(      @SalesInvoiceRefNo,GETDATE(),@SaleOrderId,@Addition,@Deduction,@AdditionRemarks,@DeductionRemarks,@InvoiceType,@isProjectBased);
                                   SELECT CAST(SCOPE_IDENTITY() as int) SalesInvoiceId";


                    result = connection.Query<SalesInvoice>(sql, model, trn).Single<SalesInvoice>();

                    var SalesInvoiceItemRepo = new SalesInvoiceItemRepository();
                    foreach (var item in model.SaleInvoiceItems)
                    {
                        item.SalesInvoiceId = result.SalesInvoiceId;
                        item.OrganizationId = model.OrganizationId;
                        SalesInvoiceItemRepo.InsertSalesInvoiceItem(item, connection, trn);
                    }
                    
                    trn.Commit();
                }
                catch (Exception ex)
                {
                    result.SalesInvoiceId = 0;
                    result.SalesInvoiceRefNo = null;
                    
                    trn.Rollback();

                    throw;
                }
                return result;

            }



        }


    }
}