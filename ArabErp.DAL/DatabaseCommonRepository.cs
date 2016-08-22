using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.DAL
{
    public static class DatabaseCommonRepository 
    {
        static string dataConnection = BaseRepository.GetConnectionString("arab");
        public static int GetInternalIDFromDatabase(IDbConnection connection, IDbTransaction trn, string DOCUMENTTYPEID, string UNIQUEID, int DOUPDATE)
        {
            string SQLNo = @"BEGIN  
                             SET NOCOUNT ON  
                             declare @reccount int ;
                             set @reccount=(SELECT	 Count(*)
                             FROM		MST_SYSTEM_DOCUMENT_SERIALNO  
                             WHERE	MST_DOCUMENTID = @DOCUMENTTYPEID AND  
                             MST_UNIQUEID = @UNIQUEID);
                             if @reccount=0
                             INSERT INTO [dbo].[MST_SYSTEM_DOCUMENT_SERIALNO]
                              ([MST_UNIQUEID],[MST_DOCUMENTID],[MST_LASTSERIALNO])
                             VALUES(@UNIQUEID,@DOCUMENTTYPEID,0);
                             IF( @DOUPDATE=1)
                             BEGIN
                             
                             		UPDATE	MST_SYSTEM_DOCUMENT_SERIALNO
                             		SET		MST_LASTSERIALNO = MST_LASTSERIALNO +1
                             		WHERE	MST_DOCUMENTID = @DOCUMENTTYPEID AND
                             				MST_UNIQUEID = @UNIQUEID;
                             		SELECT	 MST_LASTSERIALNO
                             		FROM		MST_SYSTEM_DOCUMENT_SERIALNO  
                             		WHERE	MST_DOCUMENTID = @DOCUMENTTYPEID AND  
                             			  	MST_UNIQUEID = @UNIQUEID;  
                             END
                             ELSE
                             BEGIN
                             				SELECT	 MST_LASTSERIALNO+1 MST_LASTSERIALNO
                             		FROM		MST_SYSTEM_DOCUMENT_SERIALNO  
                             		WHERE	MST_DOCUMENTID = @DOCUMENTTYPEID AND  
                             			  	MST_UNIQUEID = @UNIQUEID;  
                             END
                             END
                             ";

            int internalid = connection.Query<int>(SQLNo, new { DOCUMENTTYPEID = DOCUMENTTYPEID, UNIQUEID = UNIQUEID, DOUPDATE = DOUPDATE }, trn).First<int>();
            return internalid;
        }

        public static string GetNextReferenceNo(string DocumentType)
        {
            try
            {
                using (IDbConnection connection = BaseRepository.OpenConnection(dataConnection))
                {
                    string query = @"SELECT CAST(MST_UNIQUEID AS VARCHAR)+ '/' + CAST(MST_LASTSERIALNO + 1 AS VARCHAR) FROM MST_SYSTEM_DOCUMENT_SERIALNO WHERE MST_DOCUMENTID = @DocumentType";
                    return connection.Query<string>(query, new { DocumentType = DocumentType }, null).First();
                }
            }
            catch (InvalidOperationException)
            {
                return "0/1";
            }
            catch(Exception)
            {
                throw;
            }
        }
        public static string GetNextRefNoWithNoUpdate(string DocumentType)
        {
            try
            {
                using (IDbConnection connection = BaseRepository.OpenConnection(dataConnection))
                {
                    string query = @"SELECT CAST(MST_LASTSERIALNO + 1 AS VARCHAR) FROM MST_SYSTEM_DOCUMENT_SERIALNO WHERE MST_DOCUMENTID = @DocumentType";
                    return connection.Query<string>(query, new { DocumentType = DocumentType }, null).First();
                }
            }
            catch (InvalidOperationException)
            {
                return "1";
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string GetNewDocNo(IDbConnection connection, int OrganizationId, int PrefixId, bool NeedToUpdate)
        {
            string sql = @"declare @prefix as varchar(50);
                declare @LastDocNo as varchar(10);
                declare @HasOrgSettings varchar(5);
                declare @PrefixId int;

                select @prefix = ISNULL(PO.Prefix, P.Prefix), @LastDocNo = ISNULL(PO.LastDocumentNo,ISNULL(P.LastDocumentNo,0)) +1,
                @HasOrgSettings = case when Id is null then 'No' else 'Yes' end,
                @PrefixId = P.PrefixId
                from PrefixSettings P left join PrefixSettingsVsOrganization PO on P.PrefixId = PO.PrefixId
                and PO.OrganizationId = " + OrganizationId.ToString() + @" where P.PrefixId = " + PrefixId.ToString() + @";
                select @prefix + '/' + @LastDocNo;";

            if (NeedToUpdate)
            {
                sql += @"if(@HasOrgSettings = 'Yes')
                    begin
                        update PrefixSettingsVsOrganization set LastDocumentNo = @LastDocNo where OrganizationId = " + OrganizationId.ToString() + @" and PrefixId = " + PrefixId.ToString() + @";
                    end
                    if(@HasOrgSettings = 'No')
                    begin
                        update PrefixSettings set LastDocumentNo = @LastDocNo where PrefixId = " + PrefixId.ToString() + @";
                    end";
            }

            return connection.Query<string>(sql).Single();
        }
        public static string GetNextDocNo(int PrefixId, int OrganizationId)
        {
            try
            {
                using (IDbConnection connection = BaseRepository.OpenConnection(dataConnection))
                {
                    return GetNewDocNo(connection, OrganizationId, PrefixId, false);
                }
            }
            catch (InvalidOperationException)
            {
                return "0/1";
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
