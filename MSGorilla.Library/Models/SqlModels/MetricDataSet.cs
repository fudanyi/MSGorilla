//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MSGorilla.Library.Models.SqlModels
{
    using System;
    using System.Collections.Generic;
    
    public partial class MetricDataSet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string GroupID { get; set; }
        public string Creater { get; set; }
        public int RecordCount { get; set; }
    
        public virtual Group Group { get; set; }
    }
}