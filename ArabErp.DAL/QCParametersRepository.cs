using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;


namespace ArabErp.DAL
{
     public class QCParametersRepository:BaseRepository
     {
         static string dataConnection = GetConnectionString("arab");
         public string ConnectionString()
         {
             return dataConnection;
         }
         public QCParameters InsertQCPara(QCParameters objQCParameters)
         {
             using (IDbConnection connection = OpenConnection(dataConnection))
             {
                 var result = new QCParameters();

                 IDbTransaction trn = connection.BeginTransaction();

                 string sql = @"INSERT INTO QCParam (QCParamName,QCParaId,QCRefNo,CreatedBy,CreatedDate,OrganizationId,isActive) 
                              VALUES(@QCParamName,@QCParaId,@QCRefNo,@CreatedBy,@CreatedDate,@OrganizationId,1);
                              SELECT CAST(SCOPE_IDENTITY() as int)";

                 try
                 {
                     int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(QCParameters).Name, "0", 1);
                     objQCParameters.QCRefNo = "QC/" + internalid;

                     int id = connection.Query<int>(sql, objQCParameters, trn).Single();
                     objQCParameters.QCParaId = id;
                     InsertLoginHistory(dataConnection, objQCParameters.CreatedBy, "Create", "Box", internalid.ToString(), "0");
                     //connection.Dispose();
                     trn.Commit();
                 }
                 catch (Exception ex)
                 {
                     trn.Rollback();
                     objQCParameters.QCParaId = 0;
                     objQCParameters.QCRefNo = null;

                 }
                 return objQCParameters;
             }
         }

         
         public IEnumerable<Dropdown> FillParaType()
         {

             using (IDbConnection connection = OpenConnection(dataConnection))
             {

                 return connection.Query<Dropdown>("SELECT QCParaId Id,QCParaName Name FROM QCParaType").ToList();
             }
         }
         public IEnumerable<QCParameters> FillQCParameterList()
         {

             using (IDbConnection connection = OpenConnection(dataConnection))
             {
                 return connection.Query<QCParameters>("SELECT O.QCParamId,QCRefNo,QCParamName,QCParaName  From QCParam O INNER JOIN QCParaType C ON C.QCParaId=O.QCParaId  WHERE O.isActive=1").ToList();
             }
         }
         public QCParameters GetQCParameters(int QCParaId)
         {

             using (IDbConnection connection = OpenConnection(dataConnection))
             {
                 string sql = @"SELECT * FROM QCParam
                        WHERE QCParamId=@QCParamId";

                 var objQC = connection.Query<QCParameters>(sql, new
                 {
                     QCParamId = QCParaId
                 }).First<QCParameters>();

                 return objQC;
             }
         }
         public int DeleteQCPara(QCParameters objQCParameters)
         {
             int result = 0;
             using (IDbConnection connection = OpenConnection(dataConnection))
             {
                 string sql = @" Update QCParam Set isActive=0 WHERE QCParamId=@QCParamId";
                 try
                 {

                     var id = connection.Execute(sql, objQCParameters);
                     objQCParameters.QCParamId = id;
                     result = 0;
                     InsertLoginHistory(dataConnection, objQCParameters.CreatedBy, "Delete", "QCParam", id.ToString(), "0");
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

         public QCParameters UpdateQCParameter(QCParameters objQCParameters)
         {
             using (IDbConnection connection = OpenConnection(dataConnection))
             {
                 string sql = @"Update QCParam Set QCParamName=@QCParamName,QCParaId=@QCParaId,QCRefNo=@QCRefNo,CreatedBy=@CreatedBy,CreatedDate=@CreatedDate,OrganizationId=@OrganizationId OUTPUT INSERTED.OrganizationId WHERE QCParamId=@QCParamId";


                 var id = connection.Execute(sql, objQCParameters);
                 InsertLoginHistory(dataConnection, objQCParameters.CreatedBy, "Update", "QCParam", id.ToString(), "0");
                 return objQCParameters;
             }
         }
    }
}
