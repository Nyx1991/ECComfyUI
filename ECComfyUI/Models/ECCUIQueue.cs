using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ECComfyUI.Models
{
    public class ECCUIQueue
    {
        public List<QueueEntry> queue_running = new List<QueueEntry>();
        public List<QueueEntry> queue_pending = new List<QueueEntry>();

        internal static ECCUIQueue Parse(string _input)
        {
            ECCUIQueue ret = new ECCUIQueue();

            JObject json = JObject.Parse(_input);
            
            foreach (var item in json["queue_running"])
            {
                ret.queue_running.Add(QueueEntry.Parse(item));
            }

            foreach (var item in json["queue_pending"])
            {
                ret.queue_pending.Add(QueueEntry.Parse(item));
            }

            return ret;
        }
    }

    public class QueueEntry
    {
        public int id;
        public string prompt_id;
        public string client_id;
        public object workflow;
        public object extra_pnginfo;
        public List<string> outputNodes = new List<string>();

        internal static QueueEntry Parse(JToken _input)
        {
            QueueEntry ret = new QueueEntry();
            ret.id = _input.ElementAt(0).Value<int>();
            ret.prompt_id = _input.ElementAt(1).Value<string>();
            ret.workflow = _input.ElementAt(2).ToObject<object>();
            ret.extra_pnginfo = _input.ElementAt(3).SelectToken("extra_pnginfo");
            ret.client_id = _input.ElementAt(3).SelectToken("client_id").ToString();

            //ret.client_id_object = new ClientIdObject() { client_id = _input.ElementAt(4).ToString() };
            ret.outputNodes = _input.ElementAt(4).ToObject<List<string>>();
            return ret;
        }
    }
}
