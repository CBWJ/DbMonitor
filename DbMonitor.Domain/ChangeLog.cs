//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace DbMonitor.Domain
{
    using System;
    using System.Collections.Generic;
    
    public partial class ChangeLog
    {
        public long ID { get; set; }
        public Nullable<long> SCID { get; set; }
        public string CLChangeEvent { get; set; }
        public string CLContent { get; set; }
        public string CLObjectName { get; set; }
        public string CLSchema { get; set; }
        public string CLObjectType { get; set; }
        public string CLSQL_Text { get; set; }
        public string CLOperator { get; set; }
        public string CLChangeTime { get; set; }
        public string CLGrabTime { get; set; }
        public string CLOldData { get; set; }
        public string CLNewData { get; set; }
        public string CLChangeType { get; set; }
    }
}
