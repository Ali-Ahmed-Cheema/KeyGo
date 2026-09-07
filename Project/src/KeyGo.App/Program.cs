using KeyGo.Core.Services;

namespace KeyGo.App;

public static class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("KeyGo App started.");
        Console.WriteLine("Core Alpha shell is ready.");

        if (args.Length == 0)
        {
            Console.WriteLine("Open a project by launching KeyGo with a folder path.");
            return;
        }

        var workspace = new ProjectWorkspaceService();
        var snapshot = await workspace.IndexAsync(args[0]);
        Console.WriteLine($"Project: {snapshot.Project.Name}");
        Console.WriteLine($"Files indexed: {snapshot.Files.Count}");
        Console.WriteLine($"Technologies: {string.Join(", ", snapshot.Project.Technologies)}");
    }
}
