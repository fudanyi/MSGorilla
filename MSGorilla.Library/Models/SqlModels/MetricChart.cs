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
    
    public partial class MetricChart
    {
        public MetricChart()
        {
            this.Chart_DataSet = new HashSet<Chart_DataSet>();
        }
    
        public string Name { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string GroupID { get; set; }
    
        public virtual ICollection<Chart_DataSet> Chart_DataSet { get; set; }
        public virtual Group Group { get; set; }
    }
}