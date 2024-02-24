using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace microsoft.taskmaster;

internal class Program
{
    static void Main(string[] args){

    var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("app.json", optional: true, reloadOnChange: true)
            .Build();

    if (args.Length == 0){
        Program.DisplayHelp();
        return;
    }
    else if (args.Length > 0){

        string command = args[0];
        switch (command.ToLower())
        {
            case "create":
                if (args.Length != 4){
                    Console.WriteLine("Invalid number of arguments");
                    Program.DisplayHelp();
                    return;
                }
                string tasksfile = Path.Combine(Directory.GetCurrentDirectory(), config.GetSection("TasksFile").Value);

                if (!File.Exists(tasksfile)){
                    File.Create(tasksfile).Close();
                }
                string dataToWrite = $@"{{""Title"":""{args[1]}"", ""Description"":""{args[2]}"", ""Date"":""{args[3]}""}}";
                JObject jsonData = JObject.Parse(dataToWrite);
                string fileContent = File.ReadAllText(tasksfile) == "" ? "[]" : File.ReadAllText(tasksfile);
                JArray tasksArray = JArray.Parse(fileContent);
                if (tasksArray.Any(task => task["Title"].ToString() == args[1])){
                    Console.WriteLine("Task with same title already exists.");
                    return;
                }
                tasksArray.Add(jsonData);
                File.WriteAllText(tasksfile, tasksArray.ToString());
                break;

            case "list":
                if (args.Length != 1){
                    Console.WriteLine("Invalid number of arguments");
                    Program.DisplayHelp();
                    return;
                }
                tasksfile = Path.Combine(Directory.GetCurrentDirectory(), config.GetSection("TasksFile").Value);
                if (!File.Exists(tasksfile)){
                    Console.WriteLine("No tasks file found");
                    return;
                }
                string tasks = File.ReadAllText(tasksfile);
                tasksArray = JArray.Parse(tasks);
                foreach (JObject task in tasksArray)
                {
                    Console.WriteLine($"Title: {task["Title"]}, Description: {task["Description"]}, Date: {task["Date"]}");
                }
                break;

            case "delete":
                if (args.Length != 2){
                    Console.WriteLine("Invalid number of arguments");
                    Program.DisplayHelp();
                    return;
                }
                tasksfile = Path.Combine(Directory.GetCurrentDirectory(), config.GetSection("TasksFile").Value);
                string taskTitleToDelete = args[1];
                tasks = File.ReadAllText(tasksfile);
                tasksArray = JArray.Parse(tasks);

                var taskToDelete = tasksArray.Select((task, index) => new { Task = task, Index = index})
                    .FirstOrDefault(t => t.Task["Title"].ToString() == taskTitleToDelete);

                if (taskToDelete != null){
                    tasksArray.RemoveAt(taskToDelete.Index);
                    File.WriteAllText(tasksfile, tasksArray.ToString());
                }
                break;
            case "edit":
                if (args.Length != 3){
                    Console.WriteLine("Invalid number of arguments");
                    Program.DisplayHelp();
                    return;
                }
                tasksfile = Path.Combine(Directory.GetCurrentDirectory(), config.GetSection("TasksFile").Value);
                string taskTitleToEdit = args[1];
                string newDescription = args[2];
                tasks = File.ReadAllText(tasksfile);
                tasksArray = JArray.Parse(tasks);

                var taskToEdit = tasksArray.Select((task, index) => new { Task = task, Index = index})
                    .FirstOrDefault(t => t.Task["Title"].ToString() == taskTitleToEdit);

                if (taskToEdit != null){
                    tasksArray[taskToEdit.Index]["Description"] = newDescription;
                    File.WriteAllText(tasksfile, tasksArray.ToString());
                }
                break;
            default:
                Console.WriteLine("Invalid command");
                Program.DisplayHelp();
                break;
        }
    }
}
static void DisplayHelp(){
    Console.WriteLine("taskmaster v1.0.0");
    Console.WriteLine("-------------");
    Console.WriteLine("\nUsage:");
    Console.WriteLine("  taskmaster create <title> <description> <date>");
    Console.WriteLine("  taskmaster list");
    Console.WriteLine("  taskmaster delete <title>");
    Console.WriteLine("  taskmaster edit <title> <new description>");
    return;
}
}