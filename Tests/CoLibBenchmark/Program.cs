// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using CoLibBenchmark.Container;
using CoLibBenchmark.Logging;
using CoLibBenchmark.Misc;
using CoLibBenchmark.ObjectPool;

BenchmarkRunner.Run<LogBenchmark>();


// var benchmark = new LogBenchmark();
// benchmark.Setup();
// benchmark.TestSerilog();
// benchmark.TestCoLog();
// benchmark.Cleanup();