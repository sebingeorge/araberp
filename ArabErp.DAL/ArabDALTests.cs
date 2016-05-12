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

            var rep = new JobCardRepository();
            //Item objItem = rep.GetItem(1);


            //var l = rep.GetGroup1Items();
            var l = rep.TestMyQueryResultValue();

            System.Console.Out.WriteLine(l);

        }
    }
}
