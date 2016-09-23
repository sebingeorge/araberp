using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using ArabErp.DAL;
using DapperExtensions;
using System.Data;
using System.Collections;

namespace ArabErp.DAL
{
   public class CustomerVsSalesExecutivesRepository:BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public IEnumerable<Employee> GetEmployeeList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Employee>("select * from Employee");
            }
        }




        public CustomerVsSalesExecutiveList GetCustomerVsSalesExecutives(int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                CustomerVsSalesExecutiveList model = new CustomerVsSalesExecutiveList();
                string sql= @"with merged (CustomerId, EffectiveDate, EmployeeId) AS 
(
    select CV.CustomerId , CV.EffectiveDate, CV.EmployeeId from Customer C  join CustomerVsSalesExecutive CV on C.CustomerId=CV.CustomerId 
	where CV.EffectiveDate=(select Max(EffectiveDate) from   CustomerVsSalesExecutive CVN where  CVN.CustomerId=C.CustomerId)
    ) 

select C.CustomerId , C.CustomerName, CV.EffectiveDate, CV.EmployeeId from Customer C left join merged CV on C.CustomerId=CV.CustomerId 
Where OrganizationId=@OrganizationId";

                model.CustomerVsSalesExecutives = connection.Query<CustomerVsSalesExecutive>(sql, new { OrganizationId = OrganizationId }).ToList<CustomerVsSalesExecutive>();
                return model;

               
            }
        }



//        public int InsertCustomerSalesExe(CustomerVsSalesExecutive objCustomerVsSalesExecutive)
//        {
//            using (IDbConnection connection = OpenConnection(dataConnection))
//            {
//                int id = 0;
//                foreach (var item in objCustomerVsSalesExecutive.)
//                {
//                    if (item.ItemId == null || item.ItemId == 0) continue;
//                    string sql = @"insert  into OpeningStock(CustomerId,EmployeeId,EffectiveDate,CreatedBy,CreatedDate,IsActive) 
//                           Values (@CustomerId,@EmployeeId,@EffectiveDate,@CreatedBy,@CreatedDate,@OrganizationId,1);
//                           SELECT CAST(SCOPE_IDENTITY() as int)";

//                    //id = connection.Query<int>(sql, objOpeningStock).Single();

//                    id = connection.Query<int>(sql, objCustomerVsSalesExecutive).Single();

//                    InsertLoginHistory(dataConnection, objCustomerVsSalesExecutive.CreatedBy, "Create", "Opening Stock", id.ToString(), "0");
//                }


//                return id;

//            }
//        }


        public int InsertCustomerSalesExecutive(IList<CustomerVsSalesExecutive> model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                foreach (CustomerVsSalesExecutive item in model)
                {
                    string checksql = @"select count(*) from CustomerVsSalesExecutive where CustomerId=@CustomerId and EmployeeId=@EmployeeId and EffectiveDate=@EffectiveDate";

                    int CheckCustomerVsSalesExecutive = connection.Query<int>(checksql, item).First();

                    if (CheckCustomerVsSalesExecutive == 0)
                    {
                        string sql = @"insert  into CustomerVsSalesExecutive(CustomerId,EmployeeId,EffectiveDate,CreatedBy,CreatedDate,OrganizationId,IsActive) 
                           Values (@CustomerId,@EmployeeId,@EffectiveDate,@CreatedBy,@CreatedDate,@OrganizationId,1);
                           SELECT CAST(SCOPE_IDENTITY() as int)";

                      //  InsertLoginHistory(dataConnection, item.CreatedBy, "Create", "CustomerVsSalesExecutive", "0", "0");
                    int objCustomerVsSalesExecutive = connection.Query<int>(sql,item).First();

                    }
                    
                  
                }

                return 1;
            }
        }
    }
}
