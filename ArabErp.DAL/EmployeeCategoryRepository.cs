﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;


namespace ArabErp.DAL
{
    public class EmployeeCategoryRepository:BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public string ConnectionString()
        {
            return dataConnection;
        }
        public EmployeeCategory InsertEmployeeCategory(EmployeeCategory model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction trn = connection.BeginTransaction();
                string sql = @"INSERT INTO EmployeeCategory(EmpCategoryRefNo,EmpCategoryName,CreatedBy,CreatedDate,OrganizationId) VALUES(@EmpCategoryRefNo,@EmpCategoryName,@CreatedBy,getDate(),@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";
                int id = 0;

                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(EmployeeCategory).Name, "0", 1);
                    model.EmpCategoryRefNo = "EMPC/" + internalid;
                    id = connection.Query<int>(sql, model, trn).Single();
                    model.EmpCategoryId = id;
                    InsertLoginHistory(dataConnection, model.CreatedBy, "Create", "Employee Category", id.ToString(), "0");
                    trn.Commit();
                }
                catch (Exception e)
                {
                    trn.Rollback();
                    model.EmpCategoryId = 0;
                    model.EmpCategoryRefNo = null;
                }
                return model;
            }
        }
        public EmployeeCategory UpdateEmployeeCategory(EmployeeCategory model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE EmployeeCategory SET EmpCategoryName = @EmpCategoryName, CreatedBy = @CreatedBy,CreatedDate= GETDATE(),OrganizationId = @OrganizationId OUTPUT INSERTED.EmpCategoryId  WHERE EmpCategoryId = @EmpCategoryId";

                try
                {
                    var id = connection.Execute(sql, model);
                    model.EmpCategoryId = id;
                    InsertLoginHistory(dataConnection, model.CreatedBy, "Update", "Employee Category", id.ToString(), "0");
                }
                catch (Exception ex)
                {

                    model.EmpCategoryId = 0;

                }
                return model;
            }
        }
        public EmployeeCategory DeleteEmployeeCategory(EmployeeCategory model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE EmployeeCategory SET isActive = 0 OUTPUT INSERTED.EmpCategoryId  WHERE EmpCategoryId = @EmpCategoryId";

                try
                {
                    var id = connection.Execute(sql, model);
                    model.EmpCategoryId = id;
                    InsertLoginHistory(dataConnection, model.CreatedBy, "Delete", "Employee Category", id.ToString(), "0");
                }
                catch (Exception ex)
                {

                    model.EmpCategoryId = 0;

                }
                return model;
            }
        }

        public IEnumerable<EmployeeCategory> FillEmployeeCategoryList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                return connection.Query<EmployeeCategory>("SELECT EmpCategoryId,EmpCategoryRefNo,EmpCategoryName  FROM EmployeeCategory where isActive = 1").ToList();
            }
        }

        public EmployeeCategory GetEmployeeCategory(int EmpCategoryId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from EmployeeCategory
                        where EmpCategoryId=@EmpCategoryId";

                var objEmployeeCategory = connection.Query<EmployeeCategory>(sql, new
                {
                    EmpCategoryId = EmpCategoryId
                }).First<EmployeeCategory>();

                return objEmployeeCategory;
            }
        }

        public List<EmployeeCategory> GetEmployeeCategory()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from EmployeeCategory
                        where OrganizationId>0";

                var objEmployeeCategory = connection.Query<EmployeeCategory>(sql).ToList<EmployeeCategory>();

                return objEmployeeCategory;
            }
        }
  
}
}
