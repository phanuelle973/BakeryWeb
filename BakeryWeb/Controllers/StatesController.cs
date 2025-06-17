using BakeryWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace BakeryWeb.Controllers
{
    public class StatesController : Controller
    {
        // GET: States

        string Exception;
        string ExceptionMessage;
        string UserName;
        string Action;
        string ApplicationMessage;
        string SortOption = String.Empty;
        string SortColumn = String.Empty;
        string SortIcon = String.Empty;
        Dictionary<string, string> DataObjDict = new Dictionary<string, string>();
        Dictionary<int, string> StatesDict = new Dictionary<int, string>();
        List<State> StatesList = new List<State>();
        List<DisplayView> DisplayViewList = new List<DisplayView>();

        public class DisplayView
        {
            public string StateID { get; set; }
            public string StateCode { get; set; }
            public string StateAbbr { get; set; }
            public string StateName { get; set; }
            public string Active { get; set; }
            public string Deleted { get; set; }
            public string CreateUser { get; set; }
            public string CreateDate { get; set; }
        }



        public class ResponseData
        {
            public string Action { get; set; }
            public string Exception { get; set; }
            public string ExceptionMessage { get; set; }
            public string ApplicationMessage { get; set; }
            public List<DisplayView> DisplayViewList { get; set; }
            public string RecordCount { get; set; }
            public string DisplayTime { get; set; }
            public string SortColumn { get; set; }
            public string SortIcon { get; set; }




        }
        public ActionResult Index()
        {
            ViewBag.Title = "Bakery Web";
            ViewBag.SubTitle = "States";
            ViewBag.LastUpdate = "[Last Update Friday 05/02/2025 6:07 PM CST]";
            ViewBag.UserName = "pmanuel";
            return View();
        }

        public ActionResult AJAXGetData(string dataObj)
        {
            AJAXInit(dataObj);
            using (var db = new pmanuelEntities())
            {
                LoadDictionaries(db);
                LoadData(db);
                FormatData();
                SortData();
            }
            ExecuteProcedures();
            return Json(new ResponseData
            {
                Action = Action,
                DisplayTime = DateTime.Now.ToString(),
                DisplayViewList = DisplayViewList,
                RecordCount = DisplayViewList.Count.ToString(),
                SortColumn = SortColumn,
                SortIcon = SortIcon,
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
                    case "SortOption":
                        SortOption = value;
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

        private void LoadData(pmanuelEntities db)
        {
            StatesList = (from s in db.States
                          where s.Active == true
                          && s.Deleted == false
                          select s).ToList();
            if (!StatesList.Any())
            {
                Exception = "Y";
                ExceptionMessage = "There were no State records.";
                return;
            }
        }

        private void FormatData()
        {
            foreach (var item in StatesList)
            {
                var entry = new DisplayView()
                {
                    StateID = item.StateID.ToString(),
                    StateCode = item.StateCode.ToString(),
                    StateAbbr = item.StateAbbr,
                    StateName = item.StateName,
                    Active = item.Active ? "Y" : "N",
                    Deleted = item.Deleted ? "Y" : "N",
                    CreateUser = item.CreateUser,
                    CreateDate = item.CreateDate.ToString()
                };
                DisplayViewList.Add(entry);
                entry.StateAbbr = StatesDict.TryGetValue(item.StateCode, out string StateAbbr) ? StateAbbr : "N/A";

            }
        }

        private void SortData()
        {
            SortIcon = String.Empty;
            switch (SortOption)
            {
                case "1":
                    DisplayViewList = DisplayViewList.OrderBy(a =>  Convert.ToInt32(a.StateCode)).ToList();
                    SortIcon = "bi-caret-up-fill";
                    SortColumn = "StateCode";
                    break;
                case "2":
                    DisplayViewList = DisplayViewList.OrderByDescending(a =>  Convert.ToInt32(a.StateCode)).ToList();
                    SortIcon = "bi-caret-down-fill";
                    SortColumn = "StateCode";
                    break;
                case "3":
                    DisplayViewList = DisplayViewList.OrderBy(a => a.StateAbbr).ToList();
                    SortIcon = "bi-caret-up-fill";
                    SortColumn = "StateAbbr";
                    break;
                case "4":
                    DisplayViewList = DisplayViewList.OrderByDescending(a => a.StateAbbr).ToList();
                    SortIcon = "bi-caret-down-fill";
                    SortColumn = "StateAbbr";
                    break;
                case "5":
                    DisplayViewList = DisplayViewList.OrderBy(a => a.StateName).ToList();
                    SortIcon = "bi-caret-up-fill";
                    SortColumn = "StateName";
                    break;
                case "6":
                    DisplayViewList = DisplayViewList.OrderByDescending(a => a.StateName).ToList();
                    SortIcon = "bi-caret-down-fill";
                    SortColumn = "StateName";
                    break;
                case "7":
                    DisplayViewList = DisplayViewList.OrderBy(a => a.Active).ToList();
                    SortIcon = "bi-caret-up-fill";
                    SortColumn = "Active";
                    break;
                case "8":
                    DisplayViewList = DisplayViewList.OrderByDescending(a => a.Active).ToList();
                    SortIcon = "bi-caret-down-fill";
                    SortColumn = "Active";
                    break;
                case "9":
                    DisplayViewList = DisplayViewList.OrderBy(a => a.Deleted).ToList();
                    SortIcon = "bi-caret-up-fill";
                    SortColumn = "Deleted";
                    break;
                case "10":
                    DisplayViewList = DisplayViewList.OrderByDescending(a => a.Deleted).ToList();
                    SortIcon = "bi-caret-down-fill";
                    SortColumn = "Deleted";
                    break;
                case "11":
                    DisplayViewList = DisplayViewList.OrderBy(a => a.CreateUser).ToList();
                    SortIcon = "bi-caret-up-fill";
                    SortColumn = "CreateUser";
                    break;
                case "12":
                    DisplayViewList = DisplayViewList.OrderByDescending(a => a.CreateUser).ToList();
                    SortIcon = "bi-caret-down-fill";
                    SortColumn = "CreateUser";
                    break;
                case "13":
                    DisplayViewList = DisplayViewList.OrderBy(a =>  Convert.ToDateTime(a.CreateDate)).ToList();
                    SortIcon = "bi-caret-up-fill";
                    SortColumn = "CreateDate";
                    break;
                case "14":
                    DisplayViewList = DisplayViewList.OrderByDescending(a =>  Convert.ToDateTime(a.CreateDate)).ToList();
                    SortIcon = "bi-caret-down-fill";
                    SortColumn = "CreateDate";
                    break;
                default:
                    DisplayViewList = DisplayViewList.OrderBy(a =>  Convert.ToInt32(a.StateCode)).ToList();
                    SortIcon = "bi-caret-up-fill";
                    SortColumn = "StateCode";
                    break;
            }
        }
        private void LoadDictionaries(pmanuelEntities db)
        {
            StatesDict = (from s in db.States select s).ToDictionary(a => a.StateCode, a => a.StateAbbr);
        }

    }
}