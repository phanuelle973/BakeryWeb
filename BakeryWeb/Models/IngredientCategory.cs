//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BakeryWeb.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class IngredientCategory
    {
        public System.Guid CategoryID { get; set; }
        public int CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string CreateUser { get; set; }
    }
}
