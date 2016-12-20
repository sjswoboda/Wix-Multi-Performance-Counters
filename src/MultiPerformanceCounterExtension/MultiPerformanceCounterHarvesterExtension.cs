using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Tools.WindowsInstallerXml;
using Microsoft.Tools.WindowsInstallerXml.Extensions;
using Microsoft.Tools.WindowsInstallerXml.Serialize;
using Util = Microsoft.Tools.WindowsInstallerXml.Extensions.Serialize.Util;
using YesNoType = Microsoft.Tools.WindowsInstallerXml.Serialize.YesNoType;

namespace MultiPerformanceCounterExtension
{
    public sealed class MultiPerformanceCounterHarvesterExtension : HarvesterExtension
    {
        private static readonly IEnumerable<Util.PerformanceCounterTypesType> PerformanceCounterTypes 
            = Enum.GetValues(typeof(Util.PerformanceCounterTypesType)).Cast<Util.PerformanceCounterTypesType>();

        /// <summary>
        /// ComponentId for the generated Component.
        /// </summary>
        public string ComponentId { get; set; }

        /// <summary>
        /// DirectoryId for the Component tag.
        /// </summary>
        public string DirectoryId { get; set; }

        public override Fragment[] Harvest(string categoryName)
        {
            var component = new Component
            {
                KeyPath = YesNoType.yes,
                Id = string.IsNullOrEmpty(ComponentId) ? CompilerCore.GetIdentifierFromName(categoryName) : ComponentId.Trim(),
                Directory = string.IsNullOrEmpty(DirectoryId) ? "TARGETDIR" : DirectoryId.Trim()
            };
            component.AddChild(GetPerformanceCategory(categoryName));

            var fragment = new Fragment();
            fragment.AddChild(component);

            return new []{fragment};
        }

        private Util.PerformanceCategory GetPerformanceCategory(string categoryName)
        {
            Console.WriteLine($"Getting counters for CategoryName: {categoryName}");
            try
            {
                var category = PerformanceCounterCategory.GetCategories()
                    .Single(c => string.Equals(categoryName, c.CategoryName, StringComparison.OrdinalIgnoreCase));
                var isMultiInstance = category.CategoryType == PerformanceCounterCategoryType.MultiInstance;
                Trace.WriteLine($"Found category={categoryName}, MultiInstance={isMultiInstance}");

                // If it's multi-instance, check if there are any instances and get counters from there; else we get 
                // the counters straight up. For multi-instance, GetCounters() fails if there are any instances. If there
                // are no instances, then GetCounters(instance) can't be called since there is no instance. Instances
                // will exist for each counter even if only one of the counters was "intialized."
                var hasInstances = category.GetInstanceNames().Length > 0;
                var counters = hasInstances
                    ? category.GetCounters(category.GetInstanceNames().First())
                    : category.GetCounters();

                Trace.WriteLine($"Found {counters.Length} counters");

                var result = new Util.PerformanceCategory
                {
                    Id = CompilerCore.GetIdentifierFromName(category.CategoryName),
                    Name = category.CategoryName,
                    Help = category.CategoryHelp,
                    MultiInstance = isMultiInstance ? Util.YesNoType.yes : Util.YesNoType.no
                };

                foreach (var counter in counters)
                {
                    Console.WriteLine($"Counter={counter.CounterName}, Type={counter.CounterType}");
                    result.AddChild(new Util.PerformanceCounter
                    {
                        Name = counter.CounterName,
                        Type = CounterTypeToWix(counter.CounterType),
                        Help = counter.CounterHelp,
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                throw new WixException(UtilErrors.PerformanceCategoryNotFound(categoryName));
            }
        }

        /// <summary>
        /// Convert the PerformanceCounterType to the Wix type.
        /// </summary>
        /// <param name="performanceCounterType">Type to convert.</param>
        /// <returns>The wix performance counter type.</returns>
        private static Util.PerformanceCounterTypesType CounterTypeToWix(PerformanceCounterType performanceCounterType)
        {
            try
            {
                return PerformanceCounterTypes.Single(e => string.Equals(e.ToString(), performanceCounterType.ToString(), StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception)
            {
                throw new WixException(UtilErrors.UnsupportedPerformanceCounterType(performanceCounterType.ToString()));
            }
        }
    }
}