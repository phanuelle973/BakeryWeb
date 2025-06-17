using BakeryWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace BakeryWeb.Controllers
{
    public class InventoryController : Controller
    {
        // GET: Inventory

        string Exception;
        string ExceptionMessage;
        string UserName;
        string Action;
        string ApplicationMessage;
        string SortOption = String.Empty;
        string SortColumn = String.Empty;
        string SortIcon = String.Empty;
        Dictionary<string, string> DataObjDict = new Dictionary<string, string>();
        Dictionary<Guid, string> InventoryDict = new Dictionary<Guid, string>();
        Dictionary<int, string> UOMDict = new Dictionary<int, string>();
        Dictionary<int, string> LocationsDict = new Dictionary<int, string>();
        Dictionary<int, string> CategoryDict = new Dictionary<int, string>();
        Dictionary<int, string> SupplierDict = new Dictionary<int, string>();
        List<IngredientInventory> InventoryList = new List<IngredientInventory>();
        List<DisplayView> DisplayViewList = new List<DisplayView>();

        public class DisplayView
        {
            public string IngredientID { get; set; }
            public string ItemNumber { get; set; }
            public string IngredientDescription { get; set; }
            public string IngredientCategory  { get; set; }
            public string StockCount { get; set; }
            public string UOM { get; set; }
            public string CostPerUnit { get; set; }
            public string Location { get; set; }
            public string ReorderLevel { get; set; }
            public string ReorderQty { get; set; }
            public string Active { get; set; }
            public string Deleted { get; set; }
            public string LastReorderDate { get; set; }
            public string Supplier { get; set; }
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
            ViewBag.SubTitle = "Inventory";
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
            InventoryList = (from s in db.IngredientInventories
                             where s.Active == true
                             && s.Deleted == false
                             select s).ToList();
            if (!InventoryList.Any())
            {
                Exception = "Y";
                ExceptionMessage = "There were no Ingredient records.";
                return;
            }
        }

        private void FormatData()
        {
            foreach (var item in InventoryList)
            {
                var entry = new DisplayView()
                {
                    IngredientID = item.IngredientID.ToString(),
                    ItemNumber = item.ItemNumber,
                    IngredientDescription = item.IngredientDescription,
                    IngredientCategory  = item.IngredientCategory.ToString(),
                    StockCount = item.StockCount.ToString(),
                    UOM = item.UOM.ToString(),
                    CostPerUnit = item.CostPerUnit.ToString(),
                    Location = item.Location.ToString(),
                    ReorderLevel = item.ReorderLevel.ToString(),
                    ReorderQty = item.ReorderQty.ToString(),
                    Active = item.Active ? "Y" : "N",
                    Deleted = item.Deleted ? "Y" : "N",
                    LastReorderDate = item.LastReorderDate.ToString(),
                    Supplier = item.SupplierID.ToString()
                };
                DisplayViewList.Add(entry);
                entry.IngredientDescription = InventoryDict.TryGetValue(item.IngredientID, out string IngredientDescription) ? IngredientDescription : "N/A";
                entry.UOM = UOMDict.TryGetValue(item.UOM, out string UOMCode) ? UOMCode : "N/A";
                entry.Location = LocationsDict.TryGetValue(item.Location ?? 0, out string LocationShortName) ? LocationShortName : "N/A";
                entry.IngredientCategory = CategoryDict.TryGetValue(item.IngredientCategory ?? 0, out string CategoryName) ? CategoryName : "N/A";
                entry.Supplier = SupplierDict.TryGetValue(item.SupplierID, out string SupplierName) ? SupplierName : "N/A";

            }
        }

        private void SortData()
        {
            SortIcon = String.Empty;
            switch (SortOption)
            {
                //case "1":
                //    DisplayViewList = DisplayViewList.OrderBy(a => Convert.ToInt32(a.IngredientID)).ToList();
                //    SortIcon = "bi-caret-up-fill";
                //    SortColumn = "IngredientID";
                //    break;
                //case "2":
                //    DisplayViewList = DisplayViewList.OrderByDescending(a => Convert.ToInt32(a.IngredientID)).ToList();
                //    SortIcon = "bi-caret-down-fill";
                //    SortColumn = "IngredientID";
                //    break;
                case "1":
                    DisplayViewList = DisplayViewList.OrderBy(a => a.ItemNumber).ToList();
                    SortIcon = "bi-caret-up-fill";
                    SortColumn = "ItemNumber";
                    break;
                case "2":
                    DisplayViewList = DisplayViewList.OrderByDescending(a => a.ItemNumber).ToList();
                    SortIcon = "bi-caret-down-fill";
                    SortColumn = "ItemNumber";
                    break;
                case "3":
                    DisplayViewList = DisplayViewList.OrderBy(a => a.IngredientDescription).ToList();
                    SortIcon = "bi-caret-up-fill";
                    SortColumn = "IngredientDescription";
                    break;
                case "4":
                    DisplayViewList = DisplayViewList.OrderByDescending(a => a.IngredientDescription).ToList();
                    SortIcon = "bi-caret-down-fill";
                    SortColumn = "IngredientDescription";
                    break;
                case "5":
                    DisplayViewList = DisplayViewList.OrderBy(a => a.IngredientCategory).ToList();
                    SortIcon = "bi-caret-up-fill";
                    SortColumn = "IngredientCategory";
                    break;
                case "6":
                    DisplayViewList = DisplayViewList.OrderByDescending(a => a.IngredientCategory).ToList();
                    SortIcon = "bi-caret-down-fill";
                    SortColumn = "IngredientCategory";
                    break;
                case "7":
                    DisplayViewList = DisplayViewList.OrderBy(a => Convert.ToInt32(a.StockCount)).ToList();
                    SortIcon = "bi-caret-up-fill";
                    SortColumn = "StockCount";
                    break;
                case "8":
                    DisplayViewList = DisplayViewList.OrderByDescending(a => Convert.ToInt32(a.StockCount)).ToList();
                    SortIcon = "bi-caret-down-fill";
                    SortColumn = "StockCount";
                    break;
                case "9":
                    DisplayViewList = DisplayViewList.OrderBy(a => a.UOM).ToList();
                    SortIcon = "bi-caret-up-fill";
                    SortColumn = "UOM";
                    break;
                case "10":
                    DisplayViewList = DisplayViewList.OrderByDescending(a => a.UOM).ToList();
                    SortIcon = "bi-caret-down-fill";
                    SortColumn = "UOM";
                    break;
                case "11":
                    DisplayViewList = DisplayViewList.OrderBy(a => Convert.ToDouble(a.CostPerUnit)).ToList();
                    SortIcon = "bi-caret-up-fill";
                    SortColumn = "CostPerUnit";
                    break;
                case "12":
                    DisplayViewList = DisplayViewList.OrderByDescending(a => Convert.ToDouble(a.CostPerUnit)).ToList();
                    SortIcon = "bi-caret-down-fill";
                    SortColumn = "CostPerUnit";
                    break;
                case "13":
                    DisplayViewList = DisplayViewList.OrderBy(a => a.Location).ToList();
                    SortIcon = "bi-caret-up-fill";
                    SortColumn = "Location";
                    break;
                case "14":
                    DisplayViewList = DisplayViewList.OrderByDescending(a => a.Location).ToList();
                    SortIcon = "bi-caret-down-fill";
                    SortColumn = "Location";
                    break;
                case "15":
                    DisplayViewList = DisplayViewList.OrderBy(a => Convert.ToInt32(a.ReorderLevel)).ToList();
                    SortIcon = "bi-caret-up-fill";
                    SortColumn = "ReorderLevel";
                    break;
                case "16":
                    DisplayViewList = DisplayViewList.OrderByDescending(a => Convert.ToInt32(a.ReorderLevel)).ToList();
                    SortIcon = "bi-caret-down-fill";
                    SortColumn = "ReorderLevel";
                    break;
                case "17":
                    DisplayViewList = DisplayViewList.OrderBy(a => Convert.ToInt32(a.ReorderQty)).ToList();
                    SortIcon = "bi-caret-up-fill";
                    SortColumn = "ReorderQty";
                    break;
                case "18":
                    DisplayViewList = DisplayViewList.OrderByDescending(a => Convert.ToInt32(a.ReorderQty)).ToList();
                    SortIcon = "bi-caret-down-fill";
                    SortColumn = "ReorderQty";
                    break;
                //case "19":
                //    DisplayViewList = DisplayViewList.OrderBy(a => a.Active).ToList();
                //    SortIcon = "bi-caret-up-fill";
                //    SortColumn = "Active";
                //    break;
                //case "20":
                //    DisplayViewList = DisplayViewList.OrderByDescending(a => a.Active).ToList();
                //    SortIcon = "bi-caret-down-fill";
                //    SortColumn = "Active";
                //    break;
                case "19":
                    DisplayViewList = DisplayViewList.OrderBy(a => Convert.ToDateTime(a.LastReorderDate)).ToList();
                    SortIcon = "bi-caret-up-fill";
                    SortColumn = "LastReorderDate";
                    break;
                case "20":
                    DisplayViewList = DisplayViewList.OrderByDescending(a => Convert.ToDateTime(a.LastReorderDate)).ToList();
                    SortIcon = "bi-caret-down-fill";
                    SortColumn = "LastReorderDate";
                    break;
                case "21":
                    DisplayViewList = DisplayViewList.OrderBy(a => a.Supplier).ToList();
                    SortIcon = "bi-caret-up-fill";
                    SortColumn = "Supplier";
                    break;
                case "22":
                    DisplayViewList = DisplayViewList.OrderByDescending(a => a.Supplier).ToList();
                    SortIcon = "bi-caret-down-fill";
                    SortColumn = "Supplier";
                    break;
                default:
                    DisplayViewList = DisplayViewList.OrderBy(a => Convert.ToInt32(a.ItemNumber)).ToList();
                    SortIcon = "bi-caret-up-fill";
                    SortColumn = "ItemNumber";
                    break;
            }
        }
        private void LoadDictionaries(pmanuelEntities db)
        {
            InventoryDict = (from s in db.IngredientInventories select s).ToDictionary(a => a.IngredientID, a => a.IngredientDescription);
            //StatesDict = (from s in db.States select s).ToDictionary(a => a.Location, a => a.StateAbbr);
            UOMDict = (from s in db.UOMs select s).ToDictionary(a => a.UOMCode, a => a.UOMShortName);
            LocationsDict = (from s in db.Locations select s).ToDictionary(a => a.LocationID, a => a.LocationShortName);
            CategoryDict = (from s in db.IngredientCategories select s).ToDictionary(a => a.CategoryCode, a => a.CategoryName);
            SupplierDict = (from s in db.IngredientSuppliers select s).ToDictionary(a => a.SupplierCode, a => a.SupplierName);
        }

    }
}