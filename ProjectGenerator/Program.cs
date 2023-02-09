namespace ProjectGenerator;

public class Program
{
    public static void Main(string[] args)
    {
        var command = Args.Configuration.Configure<Options>().CreateAndBind(args);
    }
}

public class Options
{
    [System.ComponentModel.Description("Project name. May not contain spaces or other special characters")]
    public string Name { get; set; }
    [System.ComponentModel.Description("Requested game engine branch")]
    public string EngineBranch { get; set; } = "master";
    public s
}