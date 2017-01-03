﻿using ArabErp.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class QuickBookExportController : BaseController
    {
        // GET: QuickBookExport
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult LoadTransactions(int TrnId)
        {
            QuickBookExportRepository _repo = new QuickBookExportRepository();
            if(TrnId == 1)
            {
                var model = _repo.GetPurchaseBillsForPosting();
                return PartialView("_PurchaseGrid",model);
            }
            else if(TrnId == 2)
            {
                return PartialView("_SalesGrid");
            }
            else
            {
                return PartialView("");
            }
        }
        public ActionResult ExportPurchaseBill(int Id)
        {
            QuickBookExportRepository repo = new QuickBookExportRepository();
            var obj = repo.GetPurchaseBillDetails(Id);

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<Table border={0}1{0}>", (Char)34);
            sb.Append("<tr>");
            sb.AppendFormat("<td style={0}font-weight:bold;{0}></td>", (Char)34);
            sb.AppendFormat("<td style={0}font-weight:bold;{0}>Trans #</td>", (Char)34);
            sb.AppendFormat("<td style={0}font-weight:bold;{0}>Type</td>", (Char)34);
            sb.AppendFormat("<td style={0}font-weight:bold;{0}>Date</td>", (Char)34);
            sb.AppendFormat("<td style={0}font-weight:bold;{0}>Num</td>", (Char)34);
            sb.AppendFormat("<td style={0}font-weight:bold;{0}>Name</td>", (Char)34);
            sb.AppendFormat("<td style={0}font-weight:bold;{0}>Memo</td>", (Char)34);
            sb.AppendFormat("<td style={0}font-weight:bold;{0}>Account</td>", (Char)34);
            sb.AppendFormat("<td style={0}font-weight:bold;{0}>Class</td>", (Char)34);
            sb.AppendFormat("<td style={0}font-weight:bold;{0}>Debit</td>", (Char)34);
            sb.AppendFormat("<td style={0}font-weight:bold;{0}>Credit</td>", (Char)34);
            sb.Append("</tr>");

            decimal Debit = 0;
            decimal Credit = 0;
            foreach (var item in obj)
            {
                sb.Append("<tr>");
                sb.AppendFormat("<td></td>", (Char)34);
                sb.AppendFormat("<td>{1}</td>", (Char)34, item.Trans);
                sb.AppendFormat("<td>{1}</td>", (Char)34, item.Type);
                sb.AppendFormat("<td>{1}</td>", (Char)34, item.Date);
                sb.AppendFormat("<td>{1}</td>", (Char)34, item.Num);
                sb.AppendFormat("<td>{1}</td>", (Char)34, item.Name);
                sb.AppendFormat("<td>{1}</td>", (Char)34, item.Memo);
                sb.AppendFormat("<td>{1}</td>", (Char)34, item.Account);
                sb.AppendFormat("<td>{1}</td>", (Char)34, item.Class);
                sb.AppendFormat("<td>{1}</td>", (Char)34, item.Debit);
                sb.AppendFormat("<td>{1}</td>", (Char)34, item.Credit);
                sb.Append("</tr>");

                Debit += Convert.ToDecimal(item.Debit == "" ? "0" : item.Debit);
                Credit += Convert.ToDecimal(item.Credit == "" ? "0" : item.Credit);
            }
            sb.Append("<tr>");
            sb.AppendFormat("<td style={0}font-weight:bold;{0}>Total</td>", (Char)34);
            sb.AppendFormat("<td></td>", (Char)34);
            sb.AppendFormat("<td></td>", (Char)34);
            sb.AppendFormat("<td></td>", (Char)34);
            sb.AppendFormat("<td></td>", (Char)34);
            sb.AppendFormat("<td></td>", (Char)34);
            sb.AppendFormat("<td></td>", (Char)34);
            sb.AppendFormat("<td></td>", (Char)34);
            sb.AppendFormat("<td></td>", (Char)34);
            sb.AppendFormat("<td style={0}font-weight:bold;{0}>{1}</td>", (Char)34, Debit);
            sb.AppendFormat("<td style={0}font-weight:bold;{0}>{1}</td>", (Char)34, Credit);
            sb.Append("</tr>");
            sb.Append("</Table>");

            string ExcelFileName = "Purchase-JV.xls";
            Response.Clear();
            Response.Charset = "";
            Response.ContentType = "application/excel";
            Response.AddHeader("Content-Disposition", "filename=" + ExcelFileName);
            Response.Write(sb);
            Response.End();
            Response.Flush();
            return View();
        }
    }
}