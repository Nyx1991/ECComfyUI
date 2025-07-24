namespace ECComfyUI.Models
{
    public class ECCUIExecutionCached
    {
        public string Type { get; set; }
        public ExecutionCachedData Data { get; set; }
    }

    public class ExecutionCachedData
    {
        public List<string> Nodes { get; set; }
        public string Prompt_Id { get; set; }
        public long Timestamp { get; set; }
    }
}