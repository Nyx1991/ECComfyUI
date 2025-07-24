using ECComfyUI;
using ECComfyUI.Models;
using System.Transactions;

ECComfyUIConnection connection = new ECComfyUIConnection();
connection.OnExecutionStart += Connection_OnExecutionStart;
void Connection_OnExecutionStart(ECCUIExecutionStart _statusMessage)
{
    Console.WriteLine("Started generation");
}

connection.OnExecuted += Connection_OnExecuted;
void Connection_OnExecuted(ECCUIExecuted _statusMessage)
{
    Console.WriteLine(_statusMessage.Data.Output.Images[0].Filename);
    connection.SaveFile(_statusMessage.Data.Output.Images[0].Filename, "C:\\Temp\\");
}

connection.Connect("127.0.0.1:8188", false);
Console.WriteLine(connection.Sid);
connection.CancelQueue(new string[] { "fsdfdfsds", "hkglbflkmnkblv", "cbvvbcxnm" }.ToList());

//ECCUIPrompt ret = connection.PromptFromWorkflowFile("C:\\Temp\\TestCUIWF.json");

var queue = connection.GetQueue();

Console.ReadKey();

connection.Disconnect();

void test()
{
    Console.WriteLine();
}