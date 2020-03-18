using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Creator.Messaging
{
    public class ShowReportMessage:BaseMessage
    {
        string _report;

        public string Report { get => _report; set => _report = value; }
    }
}
