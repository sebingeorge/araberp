using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace ArabErp.DAL
{
    public class RateSettingsRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");

        public int InsertRateSettings(RateSettings model, string CreatedBy)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    string validationQuery = @"IF EXISTS(SELECT * FROM RateSettings WHERE FromDate BETWEEN CONVERT(DATETIME, @FromDate, 106) AND CONVERT(DATETIME, @ToDate, 106)
			                                                            OR ToDate BETWEEN CONVERT(DATETIME, @FromDate, 106) AND CONVERT(DATETIME, @ToDate, 106))
	                                SELECT 0 --failed
                                ELSE
                                BEGIN
	                                IF EXISTS(SELECT * FROM RateSettings WHERE CONVERT(DATETIME, @FromDate, 106) BETWEEN FromDate AND ToDate
										                                OR CONVERT(DATETIME, @ToDate, 106) BETWEEN FromDate AND ToDate)
		                                SELECT 0--failed
	                                ELSE
		                                SELECT 1
                                END";
                    int fail = connection.Query<int>(validationQuery, model, transaction: txn).First();
                    if (fail == 0) return 0;

                    string query1 = @"IF EXISTS(SELECT TOP 1 RateSettingsId FROM RateSettings WHERE FromDate = @FromDate AND ToDate = @ToDate)
	                                    DELETE FROM RateSettings WHERE FromDate = @FromDate AND ToDate = @ToDate;";
                    connection.Execute(query1, new { FromDate = model.FromDate, ToDate = model.ToDate }, transaction: txn);

                    string query = @"INSERT INTO RateSettings
                                    (
	                                    [WorkDescriptionId],
	                                    [MinRate],
	                                    [MediumRate],
	                                    [MaxRate],
	                                    [FromDate],
	                                    [ToDate]
                                    )
                                    VALUES
                                    (
	                                    @WorkDescriptionId,
                                        @MinRate,
                                        @MediumRate,
                                        @MaxRate,
                                        @FromDate,
                                        @ToDate
                                    );";
                    foreach (var item in model.Items)
                    {
                        if (item.MinRate != 0 && item.MediumRate != 0 && item.MaxRate != 0)
                        {
                            connection.Execute(query, new
                                            {
                                                WorkDescriptionId = item.WorkDescriptionId,
                                                MinRate = item.MinRate,
                                                MediumRate = item.MediumRate,
                                                MaxRate = item.MaxRate,
                                                FromDate = model.FromDate,
                                                ToDate = model.ToDate
                                            }, txn);
                        }
                    }
                    InsertLoginHistory(dataConnection, CreatedBy, "Create", "Rate Settings", "0", "0");
                    txn.Commit();
                    return 1;
                }
                catch (Exception ex)
                {
                    txn.Rollback();
                    throw ex;
                }
            }
        }

        public List<RateSettingsItems> GetWorkDescriptions(int type)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT
	                                WorkDescriptionId,
	                                WorkDescr,
	                                0.00 AS MinRate,
	                                0.00 AS MediumRate,
	                                0.00 AS MaxRate
                                FROM WorkDescription
                                WHERE isProjectBased = @type";

                return connection.Query<RateSettingsItems>(query, new { type = type }).ToList();
            }
        }

        /// <summary>
        /// Return the maximum ToDate for which rate settings are entered
        /// </summary>
        /// <returns></returns>
        public string GetExpiryDate()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                try
                {
                    string query = @"SELECT ISNULL(CONVERT(VARCHAR, MAX(ToDate), 106), 0) FROM RateSettings";
                    return connection.Query<string>(query).First();
                }
                catch (Exception)
                {
                    return "0";
                }
            }
        }

        /// <summary>
        /// Return the period in which the given date already exist
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public string ValidateDate(DateTime date)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                try
                {
                    string query = @"SELECT CONVERT(VARCHAR, FromDate, 106) + ' - ' + CONVERT(VARCHAR, ToDate, 106) 
                                    FROM RateSettings
                                    WHERE @date BETWEEN FromDate AND ToDate";
                    return connection.Query<string>(query, new { date = date }).First();
                }
                catch (InvalidOperationException)
                {
                    //date does not exist - validation success
                    return "1";
                }
            }
        }

        public IEnumerable<RateSettings> GetPreviousList()
        {
            try
            {
                using (IDbConnection connection = OpenConnection(dataConnection))
                {
                    string query = @"SELECT
	                                    DISTINCT CONVERT(DATETIME, FromDate, 106) FromDate,
	                                    CONVERT(DATETIME, ToDate, 106) ToDate
                                    FROM RateSettings;";
                    return connection.Query<RateSettings>(query).ToList();
                }
            }
            catch (InvalidOperationException)
            {
                return new List<RateSettings>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<RateSettingsItems> GetPreviousWorkDescriptions(string from)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT
	                                WD.WorkDescriptionId,
									WD.WorkDescr,
	                                ISNULL(MinRate, 0) MinRate,
	                                ISNULL(MediumRate, 0) MediumRate,
	                                ISNULL(MaxRate, 0) MaxRate
                                FROM RateSettings RS
                                RIGHT JOIN WorkDescription WD ON RS.WorkDescriptionId = WD.WorkDescriptionId
                                WHERE (FromDate = @from OR FromDate IS NULL)
                                AND isProjectBased = 0;";

                return connection.Query<RateSettingsItems>(query, new { from = from }).ToList();
            }
        }

        /// <summary>
        /// Return the rate of a work description for a given date
        /// </summary>
        /// <param name="workDescriptionId"></param>
        /// <param name="RateType">1 = Min, 2 = Medium, 3 = Max</param>
        /// <param name="date"></param>
        /// <returns></returns>
        public decimal GetRate(int workDescriptionId, string date, int RateType)
        {
            try
            {
                using (IDbConnection connection = OpenConnection(dataConnection))
                {
                    string type = "MinRate";
                    if (RateType == 2) type = "MediumRate";
                    else if (RateType == 3) type = "MaxRate";
                    string query = @"SELECT " + type + " FROM RateSettings WHERE WorkDescriptionId = @workDescriptionId AND @date BETWEEN FromDate AND ToDate;";

                    return connection.Query<decimal>(query, new { workDescriptionId = workDescriptionId, date = date }).First();
                }
            }
            catch (InvalidOperationException)
            {
                return 0;
                throw;
            }
        }

        public decimal GetSpecialRate(int workDescriptionId, int customerId)
        {
            try
            {
                using (IDbConnection connection = OpenConnection(dataConnection))
                {
                    //string type = "Special";
                    //if (RateType == 2) type = "MediumRate";
                    //else if (RateType == 3) type = "MaxRate";
                    string query = @"SELECT FixedRate FROM CustomerVsWorkRate WHERE WorkDescriptionId =@workDescriptionId AND CustomerId=@customerId ;";

                    return connection.Query<decimal>(query, new { workDescriptionId = workDescriptionId, customerId = customerId }).First();
                }
            }
            catch (InvalidOperationException)
            {
                return 0;
                throw;
            }
        }
        public List<StandardSellingRateItem> GetStandardRateData()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string query = @"SELECT I.ItemId,I.ItemName,I.PartNo,isnull(S.Rate,0)Rate from StandardRate S right join Item I on I.ItemId=S.ItemId";
                return connection.Query<StandardSellingRateItem>(query).ToList();
            }
        }
        public int InsertStandardRate(StandardSellingRate model)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                    IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    foreach (var item in model.StandardSellingRateItems)
                    {

                        string checksql = @"DELETE from [StandardRate]";

                        connection.Query<int>(checksql, item, txn);

                        string sql = @"INSERT INTO [dbo].[StandardRate]
                                        ([ItemId] ,[Rate] ) VALUES (@ItemId,@Rate)
                                       
                              SELECT CAST(SCOPE_IDENTITY() as int)";

                        int ObjStandardRate = connection.Query<int>(sql, item, txn).First();
                        txn.Commit();
                    }
                }
                catch (Exception)
                {
                    txn.Rollback();
                    throw;
                }

                return 1;
            }
        }

    }
}
