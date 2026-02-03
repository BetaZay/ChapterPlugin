using System;

public class Program
{
    public static void Main()
    {
         // Try to find NetworkConfiguration by name in loaded assemblies (since we ref Controller/Model)
         foreach(var asm in AppDomain.CurrentDomain.GetAssemblies())
         {
             foreach(var type in asm.GetTypes())
             {
                 if (type.Name == "NetworkConfiguration")
                 {
                     Console.WriteLine("Found: " + type.FullName);
                 }
             }
         }
    }
}
