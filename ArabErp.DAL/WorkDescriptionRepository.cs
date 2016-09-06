using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class WorkDescriptionRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");

        public IEnumerable<WorkDescription> FillWorkDescriptionList(string vehiclemodel, string freezerunit, string box)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<WorkDescription>(@" select wd.WorkDescriptionId WorkDescriptionId,wd.WorkDescriptionRefNo WorkDescriptionRefNo,wd.WorkDescr WorkDescr,
                                                            v.VehicleModelName VehicleModelName,f.FreezerUnitName FreezerUnitName,b.BoxName BoxName 
                                                            from WorkDescription wd 
                                                            Left join VehicleModel v on wd.VehicleModelId=v.VehicleModelId
								                            left join  FreezerUnit F on wd.FreezerUnitId=f.FreezerUnitId
								                            left join Box b on b.BoxId=wd.BoxId
								                            where wd.isActive=1 and wd.isProjectBased=0  AND v.VehicleModelName LIKE '%'+@vehiclemodel+'%'
                                                            AND f.FreezerUnitName LIKE '%'+@freezerunit+'%' AND b.BoxName LIKE '%'+@box+'%'", new { vehiclemodel = vehiclemodel, freezerunit = freezerunit, box = box }).ToList();
            }
        }
        public WorkDescription InsertWorkDescription(WorkDescription objWorkDescription)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //var result = new WorkDescription();
                IDbTransaction trn = connection.BeginTransaction();
                string sql = @"insert  into WorkDescription(WorkDescriptionRefNo,VehicleModelId,FreezerUnitId,BoxId,
                               WorkDescr,WorkDescrShortName,isNewInstallation,isRepair,isSubAssembly,isProjectBased,CreatedBy,CreatedDate,OrganizationId) 
                               Values (@WorkDescriptionRefNo,@VehicleModelId,@FreezerUnitId,@BoxId,@WorkDescr,@WorkDescrShortName,@isNewInstallation,
                               @isRepair,@isSubAssembly,0,@CreatedBy,@CreatedDate,@OrganizationId);
                               SELECT CAST(SCOPE_IDENTITY() as int)";

                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(WorkDescription).Name, "0", 1);
                    objWorkDescription.WorkDescriptionRefNo = "WD/" + internalid;
                    var id = connection.Query<int>(sql, objWorkDescription, trn).Single();

                    var worksitemrepo = new WorkVsItemRepository();
                    foreach (var item in objWorkDescription.WorkVsItems)
                    {
                        if (item.ItemId == 0 || item.Quantity == 0) continue;
                        item.WorkDescriptionId = id;
                        item.CreatedBy = objWorkDescription.CreatedBy;
                        item.CreatedDate = objWorkDescription.CreatedDate;
                        worksitemrepo.InsertWorkVsItem(item, connection, trn);
                    }


                    var workstaskepo = new WorkVsTaskRepository();
                    foreach (var item in objWorkDescription.WorkVsTasks)
                    {
                        if (item.JobCardTaskMasterId == 0) continue;
                        item.WorkDescriptionId = id;
                        item.CreatedBy = objWorkDescription.CreatedBy;
                        item.CreatedDate = objWorkDescription.CreatedDate;
                        workstaskepo.InsertWorkVsTask(item, connection, trn);
                    }

                    objWorkDescription.WorkDescriptionId = id;
                    InsertLoginHistory(dataConnection, objWorkDescription.CreatedBy, "Create", "Work Description", id.ToString(), "0");
                    trn.Commit();
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                    objWorkDescription.WorkDescriptionId = 0;
                    objWorkDescription.WorkDescriptionRefNo = null;
                }
                return objWorkDescription;
            }
        }
        public WorkDescription InsertProjectWorkDescription(WorkDescription objWorkDescription)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //var result = new WorkDescription();
                IDbTransaction trn = connection.BeginTransaction();
                string sql = @"insert  into WorkDescription(WorkDescriptionRefNo,VehicleModelId,FreezerUnitId,BoxId,
                               WorkDescr,WorkDescrShortName,isNewInstallation,isRepair,isSubAssembly,isProjectBased,CreatedBy,CreatedDate,OrganizationId) 
                               Values (@WorkDescriptionRefNo,@VehicleModelId,@FreezerUnitId,@BoxId,@WorkDescr,@WorkDescrShortName,@isNewInstallation,
                               @isRepair,@isSubAssembly,1,@CreatedBy,@CreatedDate,@OrganizationId);
                               SELECT CAST(SCOPE_IDENTITY() as int)";

                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(WorkDescription).Name, "0", 1);
                    objWorkDescription.WorkDescriptionRefNo = "WD/" + internalid;
                    var id = connection.Query<int>(sql, objWorkDescription, trn).Single();

                    var worksitemrepo = new WorkVsItemRepository();
                    foreach (var item in objWorkDescription.WorkVsItems)
                    {
                        if (item.ItemId == 0 || item.Quantity == 0) continue;
                        item.WorkDescriptionId = id;
                        item.CreatedBy = objWorkDescription.CreatedBy;
                        item.CreatedDate = objWorkDescription.CreatedDate;
                        worksitemrepo.InsertWorkVsItem(item, connection, trn);
                    }


                    var workstaskepo = new WorkVsTaskRepository();

                    foreach (var item in objWorkDescription.WorkVsTasks)
                    {
                        if (item.JobCardTaskMasterId == 0) continue;
                        item.WorkDescriptionId = id;
                        item.CreatedBy = objWorkDescription.CreatedBy;
                        item.CreatedDate = objWorkDescription.CreatedDate;
                        workstaskepo.InsertWorkVsTask(item, connection, trn);
                    }
                    objWorkDescription.WorkDescriptionId = id;
                    InsertLoginHistory(dataConnection, objWorkDescription.CreatedBy, "Create", "Work Description", id.ToString(), "0");
                    trn.Commit();
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                    objWorkDescription.WorkDescriptionId = 0;
                    objWorkDescription.WorkDescriptionRefNo = null;
                }
                return objWorkDescription;
            }
        }

        public WorkDescription GetWorkDescription(int WorkDescriptionId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from WorkDescription
                        where WorkDescriptionId=@WorkDescriptionId";

                var objWorkDescription = connection.Query<WorkDescription>(sql, new
                {
                    WorkDescriptionId = WorkDescriptionId
                }).First<WorkDescription>();
                 var worksitemrepo = new WorkVsItemRepository();
                 var workstaskrepo = new WorkVsTaskRepository();
                 objWorkDescription.WorkVsItems = worksitemrepo.GetWorkDescriptionWorkVsItems(WorkDescriptionId);
                 objWorkDescription.WorkVsTasks = workstaskrepo.GetWorkDescriptionWorkVsTasks(WorkDescriptionId);
                return objWorkDescription;
            }
        }

        public List<WorkDescription> GetWorkDescriptions()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from WorkDescription
                        where isActive=1";

                var objWorkDescriptions = connection.Query<WorkDescription>(sql).ToList<WorkDescription>();

                return objWorkDescriptions;
            }
        }

        public int UpdateWorkDescription(WorkDescription objWorkDescription)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE WorkDescription SET VehicleModelId = @VehicleModelId ,FreezerUnitId = @FreezerUnitId ,BoxId = @BoxId ,WorkDescr = @WorkDescr,WorkDescrShortName= @WorkDescrShortName,CreatedBy = @CreatedBy,CreatedDate = @CreatedDate  OUTPUT INSERTED.WorkDescriptionId  WHERE WorkDescriptionId = @WorkDescriptionId";
                
                var id = connection.Execute(sql, objWorkDescription);
                InsertLoginHistory(dataConnection, objWorkDescription.CreatedBy, "Update", "Work Description", id.ToString(), "0");
                return id;
            }
        }

        public int CHECK(int WorkDescriptionId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"SELECT count(W.WorkDescriptionId)count FROM WorkDescription W 
                               INNER JOIN CustomerVsWorkRate C ON C.WorkDescriptionId=W.WorkDescriptionId 
                               LEFT JOIN SalesQuotationItem S ON S.WorkDescriptionId=W.WorkDescriptionId 
                               WHERE W.WorkDescriptionId=@WorkDescriptionId";

                var id = connection.Query<int>(sql, new { WorkDescriptionId = WorkDescriptionId }).FirstOrDefault();

                return id;

            }

        }

        /// <summary>
        /// Delete HD Details
        /// </summary>
        /// <returns></returns>
        public int DeleteWorkDescriptionHD(int Id, string CreatedBy)
        {
            int result4 = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" DELETE FROM WorkDescription WHERE WorkDescriptionId=@Id";

                {

                    var id = connection.Execute(sql, new { Id = Id });
                    InsertLoginHistory(dataConnection, CreatedBy, "Delete", "Work Description", id.ToString(), "0");
                    return id;

                }

            }
        }
        /// <summary>
        /// Delete  Item DT Details
        /// </summary>
        /// <returns></returns>
        public int DeleteWorkDescriptionItem(int Id)
        {
            int result2 = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" DELETE FROM WorkVsItem WHERE WorkDescriptionId=@Id";

                {

                    var id = connection.Execute(sql, new { Id = Id });
                    return id;

                }

            }
        }

        /// <summary>
        /// Delete  Task DT Details
        /// </summary>
        /// <returns></returns>
        public int DeleteWorkDescriptionTask(int Id)
        {
            int result3 = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" DELETE FROM WorkVsTask WHERE WorkDescriptionId=@Id";

                {

                    var id = connection.Execute(sql, new { Id = Id });
                    return id;

                }

            }
        }

    }
}