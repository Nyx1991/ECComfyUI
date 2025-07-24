# Simple .NET Library to call ComfyUI API
Currently only supports the most basic functions

## Connect
```
ECComfyUIConnection connection = new ECComfyUIConnection();
connection.Connect("127.0.0.1:8188");
```

## Prompt
```
ECCUIPrompt prompt = connection.PromptFromWorkflowFile("C:\\Temp\\TestCUIWF.json");
```
## Queue

```
//Get queue
ECCUIQueue queue = connection.GetQueue();
//Cancel queue
connection.CancelQueue(prompt.Prompt_Id);
//Cancel all queued
connection.CancelQueued(prompt.Prompt_Id);
```

## Download file

```
connection.OnExecuted += Connection_OnExecuted;
void Connection_OnExecuted(ECCUIExecuted _statusMessage)
{
    Console.WriteLine(_statusMessage.Data.Output.Images[0].Filename);
    connection.SaveFile(_statusMessage.Data.Output.Images[0].Filename, "C:\\Temp\\");
}
```

## Events
- OnConnected
- OnStatusUpdate
- OnExecutionStart
- OnExecutionCached
- OnExecuted
- OnExecutionSuccess
- OnExecuting
- OnProgress
