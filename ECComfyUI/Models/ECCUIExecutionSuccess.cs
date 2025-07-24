namespace ECComfyUI.Models
{
    public class ECCUIExecutionSuccess
    {
        public string Type { get; set; }
        public ExecutionSuccessData Data { get; set; }
    }

    public class ExecutionSuccessData
    {
        public string Prompt_Id { get; set; }
        public long Timestamp { get; set; }
    }
}