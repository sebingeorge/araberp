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

        public IEnumerable<WorkDescription> FillWorkDescriptionList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<WorkDescription>(@"select  wd.WorkDescriptionId WorkDescriptionId,wd.WorkDescriptionRefNo WorkDescriptionRefNo,wd.WorkDescr WorkDescr,v.VehicleModelName VehicleModelName,f.FreezerUnitName FreezerUnitName,b.BoxName BoxName from WorkDescription wd Left join VehicleModel v on wd.VehicleModelId=v.VehicleModelId
								left join  FreezerUnit F on wd.FreezerUnitId=f.FreezerUnitId
								left join Box b on b.BoxId=wd.BoxId
								where wd.isActive=1").ToList();
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
                        item.WorkDescriptionId = id;
                        item.CreatedBy = objWorkDescription.CreatedBy;
                        item.CreatedDate = objWorkDescription.CreatedDate;
                        worksitemrepo.InsertWorkVsItem(item, connection, trn);
                    }


                    var workstaskepo = new WorkVsTaskRepository();

                    foreach (var item in objWorkDescription.WorkVsTasks)
                    {
                        item.WorkDescriptionId = id;
                        item.CreatedBy = objWorkDescription.CreatedBy;
                        item.CreatedDate = objWorkDescription.CreatedDate;
                        workstaskepo.InsertWorkVsTask(item, connection, trn);
                    }
                    objWorkDescription.WorkDescriptionId = id;
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
                        item.WorkDescriptionId = id;
                        item.CreatedBy = objWorkDescription.CreatedBy;
                        item.CreatedDate = objWorkDescription.CreatedDate;
                        worksitemrepo.InsertWorkVsItem(item, connection, trn);
                    }


                    var workstaskepo = new WorkVsTaskRepository();

                    foreach (var item in objWorkDescription.WorkVsTasks)
                    {
                        item.WorkDescriptionId = id;
                        item.CreatedBy = objWorkDescription.CreatedBy;
                        item.CreatedDate = objWorkDescription.CreatedDate;
                        workstaskepo.InsertWorkVsTask(item, connection, trn);
                    }
                    objWorkDescription.WorkDescriptionId = id;
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
                return id;
            }
        }

        public int DeleteWorkDescription(Unit objWorkDescription)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete WorkDescription  OUTPUT DELETED.WorkDescriptionId WHERE WorkDescriptionId=@WorkDescriptionId";


                var id = connection.Execute(sql, objWorkDescription);
                return id;
            }
        }


    }
}