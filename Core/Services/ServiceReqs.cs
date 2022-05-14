using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinCry.Services
{
    public class ServiceReqs
    {
        public string[] VisibleOn { get; set; }
        public string[] CanDisableOn { get; set; }
        public string[] CanEnableOn { get; set; }
        public ServiceWinCondition[] CanDeleteOn { get; set; }
        public string[] CanRestoreOn { get; set; }
        public string[] CanBackupOn { get; set; }
    }
}