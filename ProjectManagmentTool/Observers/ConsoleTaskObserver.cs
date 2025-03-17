using System;
using ProjectManagmentTool.Data;

namespace ProjectManagmentTool.Observers
{
    public class ConsoleTaskObserver : ITaskObserver
    {
        public void OnTaskCreated(ProjectTask task)
        {
            Console.WriteLine($"[TASK OBSERVER] Task Created: {task.TaskName} (ID: {task.TaskID})");

            // Optionally, send a notification to the frontend
            // For example, using SignalR (if implemented)
        }
    }
}
