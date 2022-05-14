using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinCry.Services
{
    public class ServiceWinCondition
    {
        public string Version { get; set; }
        public Service.ServiceRemovingCondition Condition { get; set; }
    }
}
