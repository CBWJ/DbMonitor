using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbMonitor.WebUI.Infrastructure.Abstract
{
    public interface IMirrorExport
    {
        void ExecuteExport(long id);

        void ExecuteImport(long id);
    }
}
