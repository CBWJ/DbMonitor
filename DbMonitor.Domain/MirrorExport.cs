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
    
    public partial class MirrorExport
    {
        public long ID { get; set; }
        public Nullable<long> SCID { get; set; }
        public string MEUser { get; set; }
        public string MEPassword { get; set; }
        public string MEDirectory { get; set; }
        public string MESchemas { get; set; }
        public string MEFileName { get; set; }
        public string MELogFile { get; set; }
        public string MEExportTime { get; set; }
        public string MEStatus { get; set; }
        public Nullable<long> CreatorID { get; set; }
        public string CreationTime { get; set; }
        public string EditingTime { get; set; }
        public string MEImportStatus { get; set; }
        public string MEImportTime { get; set; }
        public string MEImportLogFile { get; set; }
    }
}
