using System;
using Microsoft.Tools.WindowsInstallerXml.Tools;

namespace MultiPerformanceCounterExtension
{
    public class MultiPerformanceCounterHeatExtension : HeatExtension
    {
        private static readonly char[] CommandSeparators = { '-', '/' };
        private static readonly char[] CommandValueSeparator = { ' '};
        public override HeatCommandLineOption[] CommandLineTypes => new[]
        {
            new HeatCommandLineOption("perf2", "Performance counters with multi-instance support"),
            new HeatCommandLineOption("ci", "Component Id for the component, when component is not in a group")
        };

        /// <summary>
        /// Parse the command line options for this extension.
        /// </summary>
        /// <param name="type">The active harvester type.</param>
        /// <param name="args">The option arguments.</param>
        public override void ParseOptions(string type, string[] args)
        {
            var harvesterExtension = new MultiPerformanceCounterHarvesterExtension();

            // We only care about the options with - or / so just get to those
            var allArgs = string.Join(" ", args);
            var allOptionArgs = allArgs.Substring(allArgs.IndexOfAny(CommandSeparators));
            var allOptions = allOptionArgs.Split(CommandSeparators, StringSplitOptions.RemoveEmptyEntries);

            foreach (var option in allOptions)
            {
                var splitCmd = option.Split(CommandValueSeparator, 2);
                var cmd = splitCmd[0];
                if (splitCmd.Length > 1)
                {
                    var value = splitCmd[1];

                    if (string.Equals(cmd, "ci", StringComparison.OrdinalIgnoreCase))
                    {
                        harvesterExtension.ComponentId = value;
                    }
                    if (string.Equals(cmd, "dr", StringComparison.OrdinalIgnoreCase))
                    {
                        harvesterExtension.DirectoryId = value;
                    }
                }
            }
            Core.Harvester.Extension = harvesterExtension;
        }
    }
}
