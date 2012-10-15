﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jace.Benchmark
{
    class Program
    {
        private const int NumberOfTests = 1000000;

        static void Main(string[] args)
        {
            ConsoleColor defaultForegroundColor = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Jace.NET Benchmark Application");
            Console.ForegroundColor = defaultForegroundColor;
            Console.WriteLine();

            Console.WriteLine("--------------------");
            Console.WriteLine("Function: {0}", "2+3*7/23");
            Console.WriteLine("Number Of Tests: {0}", NumberOfTests.ToString("N0"));
            Console.WriteLine();

            Console.WriteLine("Interpreted Mode:");
            CalculationEngine interpretedEngine = new CalculationEngine(CultureInfo.CurrentCulture, ExecutionMode.Interpreted);
            BenchMarkCalculationEngine(interpretedEngine, "2+3*7");

            Console.WriteLine("Compiled Mode:");
            CalculationEngine compiledEngine = new CalculationEngine(CultureInfo.CurrentCulture, ExecutionMode.Compiled);
            BenchMarkCalculationEngine(compiledEngine, "2+3*7");

            Console.WriteLine("--------------------");
            Console.WriteLine("Function: {0}", "(var1 + var2 * 3)/(2+3) - something");
            Console.WriteLine("Number Of Tests: {0}", NumberOfTests.ToString("N0"));
            Console.WriteLine();

            Console.WriteLine("Interpreted Mode:");
            BenchMarkCalculationEngineFunctionBuild(interpretedEngine, "(var1 + var2 * 3)/(2+3) - something");

            Console.WriteLine("Compiled Mode:");
            BenchMarkCalculationEngineFunctionBuild(compiledEngine, "(var1 + var2 * 3)/(2+3) - something");

            Console.WriteLine("--------------------");
            Console.WriteLine("Random Generated Functions: {0}", 1000.ToString("N0"));
            Console.WriteLine("Number Of Variables Of Each Function: {0}", 3);
            Console.WriteLine("Number Of Executions For Each Function: {0}", 10000.ToString("N0"));
            Console.WriteLine("Total Number Of Executions: {0}", (10000 * 1000).ToString("N0"));
            Console.WriteLine("Parallel: {0}", true);
            Console.WriteLine();

            List<string> functions = GenerateRandomFunctions(1000);

            Console.WriteLine("Interpreted Mode:");
            BenchMarkCalculationEngineRandomFunctionBuild(interpretedEngine, functions, 10000);

            Console.WriteLine("Compiled Mode:");
            BenchMarkCalculationEngineRandomFunctionBuild(compiledEngine, functions, 10000);

            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
        }

        private static void BenchMarkCalculationEngine(CalculationEngine engine, string functionText)
        {
            DateTime start = DateTime.Now;

            for (int i = 0; i < NumberOfTests; i++)
            {
                engine.Calculate(functionText);
            }

            DateTime end = DateTime.Now;

            Console.WriteLine("Total duration: {0}", end - start);
        }

        private static void BenchMarkCalculationEngineFunctionBuild(CalculationEngine engine, string functionText)
        {
            DateTime start = DateTime.Now;

            Func<int, int, int, double> function = (Func<int, int, int, double>)engine.Function(functionText)
                .Parameter("var1", DataType.Integer)
                .Parameter("var2", DataType.Integer)
                .Parameter("something", DataType.Integer)
                .Result(DataType.FloatingPoint)
                .Build();

            Random random = new Random();

            for (int i = 0; i < NumberOfTests; i++)
            {
                function(random.Next(), random.Next(), random.Next());
            }

            DateTime end = DateTime.Now;

            Console.WriteLine("Total duration: {0}", end - start);
        }

        private static List<string> GenerateRandomFunctions(int numberOfFunctions)
        {
            List<string> result = new List<string>();
            FunctionGenerator generator = new FunctionGenerator();

            for (int i = 0; i < numberOfFunctions; i++)
               result.Add(generator.Next());

            return result;
        }

        private static void BenchMarkCalculationEngineRandomFunctionBuild(CalculationEngine engine, List<string> functions, 
            int numberOfTests)
        {
            Random random = new Random();

            DateTime start = DateTime.Now;

            List<Task> tasks = new List<Task>();

            foreach (string functionText in functions)
            {
                Task task = new Task(() =>
                {
                    Func<int, int, int, double> function = (Func<int, int, int, double>)engine.Function(functionText)
                        .Parameter("var1", DataType.Integer)
                        .Parameter("var2", DataType.Integer)
                        .Parameter("var3", DataType.Integer)
                        .Result(DataType.FloatingPoint)
                        .Build();

                    for (int i = 0; i < numberOfTests; i++)
                    {
                        function(random.Next(), random.Next(), random.Next());
                    }
                });

                tasks.Add(task);
                task.Start();
            }

            Task.WaitAll(tasks.ToArray());

            DateTime end = DateTime.Now;

            Console.WriteLine("Total duration: {0}", end - start);
        }
    }
}
