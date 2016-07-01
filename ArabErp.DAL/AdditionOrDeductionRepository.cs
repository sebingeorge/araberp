using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class AdditionOrDeductionRepository:BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");

        public IEnumerable<AdditionOrDeduction> FillAdditionDeductionList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                return connection.Query<AdditionOrDeduction>("SELECT AddDedId,AddDedRefNo,AddDedName,Case when AddDedType=1 then 'Addition' Else 'Deduction' End As AddDedType FROM AdditionDeduction WHERE isActive=1").ToList();
            }
        }

        public AdditionOrDeduction Insert(AdditionOrDeduction objAdditionOrDeduction)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var result = new AdditionOrDeduction();

                IDbTransaction trn = connection.BeginTransaction();

                string sql = @"INSERT INTO AdditionDeduction (AddDedRefNo,AddDedName,AddDedType,AddDedRemarks,CreatedBy,CreatedDate,OrganizationId,isActive) 
                               VALUES(@AddDedRefNo,@AddDedName, @AddDedType, @AddDedRemarks,@CreatedBy,@CreatedDate,@OrganizationId,1);
                               SELECT CAST(SCOPE_IDENTITY() as int)";


                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(AdditionOrDeduction).Name, "0", 1);
                    objAdditionOrDeduction.AddDedRefNo = "AD/" + internalid;

                    int id = connection.Query<int>(sql, objAdditionOrDeduction, trn).Single();
                    objAdditionOrDeduction.AddDedId= id;
                    //connection.Dispose();
                    trn.Commit();
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                    objAdditionOrDeduction.AddDedId = 0;
                    objAdditionOrDeduction.AddDedRefNo = null;

                }
                return objAdditionOrDeduction;
            }
        }

        public AdditionOrDeduction Update(AdditionOrDeduction objAdditionOrDeduction)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" Update AdditionDeduction Set AddDedRefNo=@AddDedRefNo,AddDedName=@AddDedName,AddDedType=@AddDedType,AddDedRemarks=@AddDedRemarks OUTPUT INSERTED.AddDedId WHERE AddDedId=@AddDedId";
                
                var id = connection.Execute(sql, objAdditionOrDeduction);
                return objAdditionOrDeduction;
            }
        }

        public int Delete(AdditionOrDeduction objAdditionOrDeduction)
        {
            int result = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" Update AdditionDeduction Set isActive=0 WHERE AddDedId=@AddDedId";
                try
                {

                    var id = connection.Execute(sql, objAdditionOrDeduction);
                    objAdditionOrDeduction.AddDedId = id;
                    result = 0;

                }
                catch (SqlException ex)
                {
                    int err = ex.Errors.Count;
                    if (ex.Errors.Count > 0) // Assume the interesting stuff is in the first error
                    {
                        switch (ex.Errors[0].Number)
                        {
                            case 547: // Foreign Key violation
                                result = 1;
                                break;

                            default:
                                result = 2;
                                break;
                        }
                    }

                }

                return result;
            }
        }
   
        public string GetRefNo(AdditionOrDeduction objAdditionOrDeduction)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string RefNo = "";
                var result = new AdditionOrDeduction();

                IDbTransaction trn = connection.BeginTransaction();

                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(AdditionOrDeduction).Name, "0", 0);
                    RefNo = "AD/" + internalid;
                    trn.Commit();
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                }
                return RefNo;
            }
        }

        public AdditionOrDeduction GetAdditionOrDeduction(int AddDedId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from AdditionDeduction
                        where AddDedId=@AddDedId";

                var objAdditionOrDeduction = connection.Query<AdditionOrDeduction>(sql, new
                {
                    AddDedId = AddDedId
                }).First<AdditionOrDeduction>();

                return objAdditionOrDeduction;
            }
        }
    }
}
