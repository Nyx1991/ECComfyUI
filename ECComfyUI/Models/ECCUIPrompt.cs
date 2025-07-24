using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECComfyUI.Models
{
    public class ECCUIPrompt
    {
        public string Prompt_Id { get; set; }
        public int Number { get; set; }
        public Dictionary<string, object> Node_Errors { get; set; }
    }
}
