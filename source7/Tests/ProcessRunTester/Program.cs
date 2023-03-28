using System.Reflection;
using System.Text.Json;

namespace ProcessRunTester;

internal class Program
{
    static int Main(string[] args)
    {
        var appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var processPropsFile = Path.Combine(appPath, "processargs.json");
        if (File.Exists(processPropsFile))
        {
            File.Delete(processPropsFile);
        }

        var props = new ProcessProperties
        {
            WorkingDirectory = Environment.CurrentDirectory,
            Arguments = args
        };

        var propStr = JsonSerializer.Serialize(props);
        Console.WriteLine(propStr);

        File.WriteAllText(processPropsFile, propStr);

        if (args.Contains("--keeprunning"))
        {
            Console.ReadLine();
        }

        return 0;
    }
}