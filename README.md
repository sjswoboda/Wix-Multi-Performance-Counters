# Wix-Multi-Performance-Counters
Better support for multi-instance performance counters in Wix

Fixes the following issues:
- For counters requiring a base type such as AverageBase and AverageTimer32, will harvest both the counters and add them properly in order.
- Support for multi-instance counters (uses the correct constructor, can harvest whether or not an instance exists)

# Usage
`heat perf2 "Your Performance Counter Category" -ext "c:\your\path\to\MultiPerformanceCounterExtension.dll" <harvester arguments> -out sourceFile.wxs`


Supports all the parameters supported by 'perf' as shown on: http://wixtoolset.org/documentation/manual/v3/overview/heat.html
