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
        public static int GetInternalIDFromDatabase(IDbConnection connection, IDbTransaction trn,string DOCUMENTTYPEID, string UNIQUEID,int DOUPDATE)
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
    }
}
