using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console_monitoring_utility.Interfaces
{
    public interface ICheckConnections
    {
        Task CheckAvailability(List<string> values);
        void Log(string value);
        List<string> Logger { get; set; }
    }
}
