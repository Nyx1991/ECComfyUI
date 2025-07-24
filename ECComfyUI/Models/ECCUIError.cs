using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECComfyUI.Models
{
    public class ECCUIErrorMessage
    {
        public ErrorDetail Error { get; set; }
        public Dictionary<string, NodeErrorDetail> Node_Errors { get; set; }
    }

    public class ErrorDetail
    {
        public string Type { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
        public Dictionary<string, object> Extra_Info { get; set; }
    }

    public class NodeErrorDetail
    {
        public List<InnerError> Errors { get; set; }
        public List<string> Dependent_Outputs { get; set; }
        public string Class_Type { get; set; }
    }

    public class InnerError
    {
        public string Type { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
        public ExtraInfo Extra_Info { get; set; }
    }

    public class ExtraInfo
    {
        public string Input_Name { get; set; }
        public List<object> Input_Config { get; set; }
        public string Exception_Message { get; set; }
        public string Exception_Type { get; set; }
        public List<string> Traceback { get; set; }
        public List<object> Linked_Node { get; set; }
    }

}
