using CommandLine;
using Walgelijk.AssetManager;

namespace Walgelijk.AssetPackageBuilder;

public class Program
{
    public static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
        {
            // add extracting

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

            AssetPackageUtils.Build(o.Id ?? input.Name, input, output);

            Console.WriteLine("Success. Written to {0}", output.Name);
        });
    }

    public enum Mode
    {
        Extract,
        Pack
    }

    private class Options
    {
        [Option('m', "Mode", Required = false, Default = Mode.Pack, HelpText = "Program mode (pack or extract).")]
        public Mode Mode { get; set; }

        [Option('i', "Input", Required = true, HelpText = "Path to input directory.")]
        public string? Input { get; set; }

        [Option('o', "Output", Required = false, HelpText = "Path to output archive. Will use input folder name if unspecified.")]
        public string? Output { get; set; }

        [Option('f', "Force", Required = false, HelpText = "Will overwrite if true.")]
        public bool Force { get; set; }

        [Option('d', "Id", Required = false, HelpText = "Determines package ID. Falls back to directory name if not provided.")]
        public string? Id { get; set; }
    }
}