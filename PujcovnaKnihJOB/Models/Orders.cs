//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PujcovnaKnihJOB.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Orders
    {
        public int ID { get; set; }
        public int CustomerID { get; set; }
        public int BookID { get; set; }
        public System.DateTime OrderDate { get; set; }
        public Nullable<System.DateTime> BorrowDate { get; set; }
        public Nullable<System.DateTime> ReturnDate { get; set; }
        public string State { get; set; }
        public string Invoiced { get; set; }
        public Nullable<System.DateTime> InvoiceDate { get; set; }
    
        public virtual Books Books { get; set; }
        public virtual Users Users { get; set; }
    }
}
