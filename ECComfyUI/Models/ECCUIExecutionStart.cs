namespace ECComfyUI.Models
{
    public class ECCUIExecutionStart
    {
        public string Type { get; set; }
        public ExecutionStartData Data { get; set; }
    }

    public class ExecutionStartData
    {
        public string Prompt_Id { get; set; }
        public long Timestamp { get; set; }
    }
}