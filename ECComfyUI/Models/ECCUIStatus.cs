using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECComfyUI.Models
{
    public class ECCUIStatus
    {
        public string Type { get; set; }
        public StatusData Data { get; set; }
    }

    public class StatusData
    {
        public StatusInfo Status { get; set; }
        public string Sid { get; set; }
    }

    public class StatusInfo
    {
        public ExecInfo Exec_Info { get; set; }
    }

    public class ExecInfo
    {
        public int Queue_Remaining { get; set; }
    }
}
