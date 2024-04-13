using CommandLine;
using Walgelijk.AssetManager;

namespace Walgelijk.AssetPackageBuilder;

public class Program
{
    public static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
        {
            o.Output ??= o.Input! + ".waa";

            if (!o.Output!.EndsWith(".waa", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.Error.WriteLine("Output file must end in .waa");
                return;
            }

            var input = new DirectoryInfo(o.Input!);

            using var output = new FileStream(
                o.Output,
                o.Force ? FileMode.Create : FileMode.CreateNew);

            AssetPackageUtils.Build(input.Name, input, output);

            Console.WriteLine("Success. Written to {0}", output.Name);
        });
    }

    private class Options
    {
        [Option('i', "Input", Required = true, HelpText = "Path to input directory.")]
        public string? Input { get; set; }

        [Option('o', "Output", Required = false, HelpText = "Path to output archive. Will use input folder name if unspecified.")]
        public string? Output { get; set; }

        [Option('f', "Force", Required = false, HelpText = "Will overwrite if true.")]
        public bool Force { get; set; }
    }
}