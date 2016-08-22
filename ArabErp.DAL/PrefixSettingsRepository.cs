using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class PrefixSettingsRepository:BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public IEnumerable<PrefixSettingsVsOrganization> GetPrefixSettings(int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select P.PrefixId, ISNULL(PO.Prefix, P.Prefix) Prefix, P.TransactionName, " + OrganizationId.ToString() + @" as OrganizationId, 
                ISNULL(LastDocumentNo,0)LastDocumentNo
                from PrefixSettings P left join PrefixSettingsVsOrganization PO on P.PrefixId = PO.PrefixId
                and PO.OrganizationId = " + OrganizationId.ToString();
                return connection.Query<PrefixSettingsVsOrganization>(sql);
            }
        }
    }
}
