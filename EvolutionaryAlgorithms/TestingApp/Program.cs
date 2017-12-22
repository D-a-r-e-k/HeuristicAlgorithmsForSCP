using Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace TestingApp
{
    class Program
    {
        const int TestCases = 5;

        static GeneticEngine.Engine _geneticEngine = new GeneticEngine.Engine();

        static void Main(string[] args)
        {
            for (int i = 0; i <= TestCases; ++i)
            {
                var problemData = ReadData($"test{i}.txt");

                problemData.Initialize();

                Console.WriteLine();
                Console.WriteLine("TEST CASE #" + i);
                Console.WriteLine();

                RunHeuristic("Greedy", new GreedyEngine.Engine().Solve, problemData);
                RunHeuristic("Tabu", new TabuSearch.Engine().Solve, problemData);
                RunHeuristic("SA", new SimulatedAnnealingEngine.Engine().Solve, problemData);
                RunHeuristic("Genetic", new GeneticEngine.Engine().Solve, problemData);
            }

            Console.ReadKey();
            Console.ReadKey();
        }

        static void RunHeuristic(string name, Func<SetCoveringProblemData, SetCoveringProblemResult> logic, SetCoveringProblemData data)
        {
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();
            var result = logic(data);
            stopwatch.Stop();

            Console.WriteLine(name);
            Console.WriteLine("Iterations: " + result.Iterations);
            Console.WriteLine("Solution: " + result.SolutionFitness);
            Console.WriteLine("Time (s): " + stopwatch.ElapsedMilliseconds / 1000.0);
            Console.WriteLine();
        }

        static SetCoveringProblemData ReadData(string file)
        {
            var result = new SetCoveringProblemData();

            string content = File.ReadAllText(file);
            List<int> nums = content
                .Split(new[] { " ", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => int.Parse(x))
                .ToList();

            int counter = 2;
            int m = nums[0]; // number of rows
            int n = nums[1]; // number of columns

            counter += n;

            result.Subsets = new List<List<int>>(m);
            for (int j = 0; j < m; ++j)
            {
                int capacity = nums[counter++];

                var subset = new List<int>(capacity);
                for (int k = 0; k < capacity; ++k)
                    subset.Add(nums[counter++] - 1);

                result.Subsets.Add(subset);
            }

            result.N = n;
            result.M = m;

            return result;
        }
    }
}
