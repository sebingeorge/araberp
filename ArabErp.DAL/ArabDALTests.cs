using ArabErp.Domain;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.DAL
{
    [TestFixture]
    public class ArabDALTests
    {
        [Test]
        public void TestGetItem()
        {



            //var stockpoint = new Stockpoint();
            //stockpoint.StockPointName = "test1";
            //stockpoint.StockPointShrtName = "t";
            //stockpoint.CreatedBy = "sebin";
            //stockpoint.CreatedDate = System.DateTime.Now;
            //stockpoint.OrganizationId = 1;

            var rep = new StockpointRepository();
            //int i = rep.InsertStockpoint(stockpoint);

            //var readstockpoint = rep.GetStockpoint(i);

            //System.Console.Out.WriteLine(readstockpoint.StockPointName);

            // i = rep.UpdateStockpoint(readstockpoint);

            //var sp = rep.GetStockpoints();

       




        }
    }
}
