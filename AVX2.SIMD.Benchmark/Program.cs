using AVX2.SIMD.Benchmark;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

Summary summary = BenchmarkRunner.Run<BenchmarkSimd>();