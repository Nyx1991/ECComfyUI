namespace ECComfyUI.Models
{
    public class ECCUIExecuting
    {
        public string Type { get; set; }
        public ExecutingData Data { get; set; }
    }

    public class ExecutingData
    {
        public string? Node { get; set; }  // nullable string, da node null sein kann
        public string Prompt_Id { get; set; }
    }
}