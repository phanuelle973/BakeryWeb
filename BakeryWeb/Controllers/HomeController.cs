using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using BakeryWeb.Models;

namespace BakeryWeb.Controllers
{
    public class HomeController : Controller
    {
        string Exception;
        string ExceptionMessage;
        string UserName;
        string Action;
        string ApplicationMessage;

        Dictionary<string, string> DataObjDict = new Dictionary<string, string>();


        // GET: Home

        public class ResponseData
        {
            public string Action { get; set; }
            public string Exception { get; set; }
            public string ExceptionMessage { get; set; }
            public string ApplicationMessage { get; set; }
        }
        public ActionResult Index()
        {
            ViewBag.Title = "Bakery Web";
            ViewBag.LastUpdate = "[Last Update Friday 05/02/2025 6:07 PM CST]";
            ViewBag.UserName = "pmanuel";
            
            return View();
        }

        public ActionResult AJAXReset(string dataObj)
        {
            AJAXInit(dataObj);
            ExecuteProcedures();
            return Json(new ResponseData
            {
                Action = Action,
                Exception = Exception,
                ExceptionMessage = ExceptionMessage,
                ApplicationMessage = ApplicationMessage
            }, JsonRequestBehavior.DenyGet);
        }
        public void AJAXInit(string AJAXDataObject)
        {
            GetAuthorizations();
            if (String.IsNullOrWhiteSpace(AJAXDataObject))
                return;
            var jss = new JavaScriptSerializer();
            DataObjDict = jss.Deserialize<Dictionary<string, string>>(AJAXDataObject);
            Exception = "N";
            ExceptionMessage = String.Empty;
            ExtractData();
        }

        private void GetAuthorizations()
        {
            UserName = String.IsNullOrEmpty(User.Identity.Name) ? "No Name" : User.Identity.Name.Replace("RRSP\\", "").ToUpper();
            ViewBag.UserName = UserName;
            return;
        }

        private void ExtractData()
        {
            foreach (var item in DataObjDict)
            {
                if (String.IsNullOrWhiteSpace(item.Value))
                    continue;
                string value = item.Value.Trim().ToUpper();
                switch (item.Key)
                {
                    case "Action":
                        Action = value;
                        break;
                }
            }
        }

        private void ExecuteProcedures()
        {
            using (var db = new pmanuelEntities())
            {
                switch (Action)
                {
                    case "RESETDATA":
                        var rdResult = db.bdbResetData();
                        ApplicationMessage = "Data Reset";
                        break;
                    case "RESETSCHEMA":
                        var rsResult2 = db.bdbResetSchema();
                        ApplicationMessage = "Schema Reset. Must Reset Data.";
                        break;
                }
            }
        }




    }
}