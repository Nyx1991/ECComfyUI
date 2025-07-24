namespace ECComfyUI.Models
{
    public class ECCUIExecuted
    {
        public string Type { get; set; }
        public ExecutedData Data { get; set; }
    }

    public class ExecutedData
    {
        public string Node { get; set; }
        public string Display_Node { get; set; }
        public OutputData Output { get; set; }
        public string Prompt_Id { get; set; }
    }

    public class OutputData
    {
        public List<ImageOutput> Images { get; set; }
    }

    public class ImageOutput
    {
        public string Filename { get; set; }
        public string Subfolder { get; set; }
        public string Type { get; set; }
    }
}