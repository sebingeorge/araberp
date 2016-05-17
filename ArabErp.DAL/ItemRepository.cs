using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class ItemRepository :BaseRepository
    {
        public List<Dropdown> FillItemSubGroup()
        {
            var list = connection.Query<Dropdown>("SELECT ItemSubGroupId Id,ItemSubGroupName Name FROM [dbo].[ItemSubGroup]").ToList();

           return list;
            
        }
    }
}
