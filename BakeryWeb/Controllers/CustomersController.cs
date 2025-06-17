using BakeryWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace BakeryWeb.Controllers
{
    public class CustomersController : Controller
    {
        // GET: Customers

        string Exception;
        string ExceptionMessage;
        string UserName;
        string Action;
        string ApplicationMessage;
        string SortOption = String.Empty;
        string SortColumn = String.Empty;
        string SortIcon = String.Empty;
        Dictionary<string, string> DataObjDict = new Dictionary<string, string>();
        Dictionary<int, string> CustomersDict = new Dictionary<int, string>();
        Dictionary<int, string> StatesDict = new Dictionary<int, string>();
        List<Customer> CustomersList = new List<Customer>();
        List<DisplayView> DisplayViewList = new List<DisplayView>();

        public class DisplayView
        {
            public string CustomerID { get; set; }
            public string LastName { get; set; }
            public string FirstName { get; set; }
            public string MI { get; set; }
            public string Address { get; set; }
            public string Address2 { get; set; }
            public string City { get; set; }
            public string StateCode { get; set; }
            public string ZipCode { get; set; }
            public string PrimaryPhone { get; set; }
            public string Active { get; set; }
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
            ViewBag.SubTitle = "Customers";
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
            CustomersList = (from s in db.Customers
                             where s.Active == true
                             select s).ToList();
            if (!CustomersList.Any())
            {
                Exception = "Y";
                ExceptionMessage = "There were no Customer records.";
                return;
            }
        }

        private void FormatData()
        {
            foreach (var item in CustomersList)
            {
                var entry = new DisplayView()
                {
                    CustomerID = item.CustomerID.ToString(),
                    LastName = item.LastName,
                    FirstName = item.FirstName,
                    MI = item.MI,
                    Address = item.Address,
                    Address2 = item.Address2,
                    City = item.City,
                    StateCode = item.StateCode.ToString(),
                    ZipCode = item.ZipCode,
                    PrimaryPhone = item.PrimaryPhone,
                    Active = item.Active ? "Y" : "N",
                    CreateUser = item.CreateUser,
                    CreateDate = item.CreateDate.ToString()
                };
                DisplayViewList.Add(entry);
                entry.LastName = CustomersDict.TryGetValue(item.CustomerID, out string LastName) ? LastName : "N/A";
                entry.StateCode = StatesDict.TryGetValue((int)item.StateCode, out string State) ? State : "N/A";
            }
        }

        private void SortData()
        {
            SortIcon = String.Empty;
            switch (SortOption)
            {
                case "1":
                    DisplayViewList = DisplayViewList.OrderBy(a => Convert.ToInt32(a.CustomerID)).ToList();
                    SortIcon = "bi-caret-up-fill";
                    SortColumn = "CustomerID";
                    break;
                case "2":
                    DisplayViewList = DisplayViewList.OrderByDescending(a => Convert.ToInt32(a.CustomerID)).ToList();
                    SortIcon = "bi-caret-down-fill";
                    SortColumn = "CustomerID";
                    break;
                case "3":
                    DisplayViewList = DisplayViewList.OrderBy(a => a.LastName).ToList();
                    SortIcon = "bi-caret-up-fill";
                    SortColumn = "LastName";
                    break;
                case "4":
                    DisplayViewList = DisplayViewList.OrderByDescending(a => a.LastName).ToList();
                    SortIcon = "bi-caret-down-fill";
                    SortColumn = "LastName";
                    break;
                case "5":
                    DisplayViewList = DisplayViewList.OrderBy(a => a.FirstName).ToList();
                    SortIcon = "bi-caret-up-fill";
                    SortColumn = "FirstName";
                    break;
                case "6":
                    DisplayViewList = DisplayViewList.OrderByDescending(a => a.FirstName).ToList();
                    SortIcon = "bi-caret-down-fill";
                    SortColumn = "FirstName";
                    break;
                case "7":
                    DisplayViewList = DisplayViewList.OrderBy(a => a.MI).ToList();
                    SortIcon = "bi-caret-up-fill";
                    SortColumn = "MI";
                    break;
                case "8":
                    DisplayViewList = DisplayViewList.OrderByDescending(a => a.MI).ToList();
                    SortIcon = "bi-caret-down-fill";
                    SortColumn = "MI";
                    break;
                case "9":
                    DisplayViewList = DisplayViewList.OrderBy(a => a.Address).ToList();
                    SortIcon = "bi-caret-up-fill";
                    SortColumn = "Address";
                    break;
                case "10":
                    DisplayViewList = DisplayViewList.OrderByDescending(a => a.Address).ToList();
                    SortIcon = "bi-caret-down-fill";
                    SortColumn = "Address";
                    break;
                case "11":
                    DisplayViewList = DisplayViewList.OrderBy(a => a.Address2).ToList();
                    SortIcon = "bi-caret-up-fill";
                    SortColumn = "Address2";
                    break;
                case "12":
                    DisplayViewList = DisplayViewList.OrderByDescending(a => a.Address2).ToList();
                    SortIcon = "bi-caret-down-fill";
                    SortColumn = "Address2";
                    break;
                case "13":
                    DisplayViewList = DisplayViewList.OrderBy(a => a.City).ToList();
                    SortIcon = "bi-caret-up-fill";
                    SortColumn = "City";
                    break;
                case "14":
                    DisplayViewList = DisplayViewList.OrderByDescending(a => a.City).ToList();
                    SortIcon = "bi-caret-down-fill";
                    SortColumn = "City";
                    break;
                case "15":
                    DisplayViewList = DisplayViewList.OrderBy(a => a.StateCode).ToList();
                    SortIcon = "bi-caret-up-fill";
                    SortColumn = "StateCode";
                    break;
                case "16":
                    DisplayViewList = DisplayViewList.OrderByDescending(a => a.StateCode).ToList();
                    SortIcon = "bi-caret-down-fill";
                    SortColumn = "StateCode";
                    break;
                case "17":
                    DisplayViewList = DisplayViewList.OrderBy(a => a.ZipCode).ToList();
                    SortIcon = "bi-caret-up-fill";
                    SortColumn = "ZipCode";
                    break;
                case "18":
                    DisplayViewList = DisplayViewList.OrderByDescending(a => a.ZipCode).ToList();
                    SortIcon = "bi-caret-down-fill";
                    SortColumn = "ZipCode";
                    break;
                case "19":
                    DisplayViewList = DisplayViewList.OrderBy(a => a.PrimaryPhone).ToList();
                    SortIcon = "bi-caret-up-fill";
                    SortColumn = "PrimaryPhone";
                    break;
                case "20":
                    DisplayViewList = DisplayViewList.OrderByDescending(a => a.PrimaryPhone).ToList();
                    SortIcon = "bi-caret-down-fill";
                    SortColumn = "PrimaryPhone";
                    break;
                case "21":
                    DisplayViewList = DisplayViewList.OrderBy(a => a.Active).ToList();
                    SortIcon = "bi-caret-up-fill";
                    SortColumn = "Active";
                    break;
                case "22":
                    DisplayViewList = DisplayViewList.OrderByDescending(a => a.Active).ToList();
                    SortIcon = "bi-caret-down-fill";
                    SortColumn = "Active";
                    break;
                case "23":
                    DisplayViewList = DisplayViewList.OrderBy(a => a.CreateUser).ToList();
                    SortIcon = "bi-caret-up-fill";
                    SortColumn = "CreateUser";
                    break;
                case "24":
                    DisplayViewList = DisplayViewList.OrderByDescending(a => a.CreateUser).ToList();
                    SortIcon = "bi-caret-down-fill";
                    SortColumn = "CreateUser";
                    break;
                case "25":
                    DisplayViewList = DisplayViewList.OrderBy(a => Convert.ToDateTime(a.CreateDate)).ToList();
                    SortIcon = "bi-caret-up-fill";
                    SortColumn = "CreateDate";
                    break;
                case "26":
                    DisplayViewList = DisplayViewList.OrderByDescending(a => Convert.ToDateTime(a.CreateDate)).ToList();
                    SortIcon = "bi-caret-down-fill";
                    SortColumn = "CreateDate";
                    break;
                default:
                    DisplayViewList = DisplayViewList.OrderBy(a => Convert.ToInt32(a.CustomerID)).ToList();
                    SortIcon = "bi-caret-up-fill";
                    SortColumn = "CustomerID";
                    break;
            }
        }
        private void LoadDictionaries(pmanuelEntities db)
        {
            CustomersDict = (from s in db.Customers select s).ToDictionary(a => a.CustomerID, a => a.LastName);
            StatesDict = (from s in db.States select s).ToDictionary(a => a.StateCode, a => a.StateAbbr);
        }

    }
}