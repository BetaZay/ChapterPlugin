using System;
using MediaBrowser.Model.Tasks;

public class Program
{
    public static void Main()
    {
        Console.WriteLine("TaskTriggerInfoType Members:");
        foreach (var name in Enum.GetNames(typeof(TaskTriggerInfoType)))
        {
            Console.WriteLine(name);
        }
    }
}
