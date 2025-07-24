namespace ECComfyUI.Models
{
    public class ECCUIProgress
    {
        public string Type { get; set; }
        public ProgressData Data { get; set; }
    }

    public class ProgressData
    {
        public int Value { get; set; }
        public int Max { get; set; }
        public string PromptId { get; set; }
        public string Node { get; set; }
    }

}