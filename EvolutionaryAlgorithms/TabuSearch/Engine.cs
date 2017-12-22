using Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TabuSearch
{
    public class Engine
    {
        private List<int[]> _tabuList = new List<int[]>();

        public SetCoveringProblemResult Solve(SetCoveringProblemData data)
        {
            var res = new GreedyEngine.Engine().Solve(data);
            var configuration = res.Solution;

            //int upperBound = 10000000;

            double tsFactor = 0.2;
            int tabuLength =  (int)Math.Ceiling(tsFactor
                * Utility.NumberOfSubsetsIncover(res.Solution, data.N)) + 1;

            int[] solution = new int[data.N];
            Array.Copy(res.Solution, solution, data.N);

            int solutionFitness = res.SolutionFitness;

            int iterations = 0;

            List<int> fitnessesHistory = new List<int>();
            while (iterations < 2000)
            {
                iterations++;

                List<int[]> neighborhood = new List<int[]>();
                List<ConfigurationEntry> sortedNeighborhood = new List<ConfigurationEntry>();

                for (int i = 0; i < data.N; ++i)
                {
                    int[] neighbor = new int[data.N];
                    Array.Copy(configuration, neighbor, data.N);

                    neighbor[i] = (neighbor[i] + 1) % 2;
                    neighborhood.Add(neighbor);
                    sortedNeighborhood.Add(new ConfigurationEntry
                    {
                        Index = i,
                        Fitness = Utility.CalculateConfigurationFitness(data, neighbor, data.N)
                    });
                }

                var configurations = sortedNeighborhood.OrderBy(x => x.Fitness).ToList();

                var acceptable = configurations.First(x => !BelongsToTabu(neighborhood[x.Index], data.N) ||
                        x.Fitness < solutionFitness);

                Array.Copy(neighborhood[acceptable.Index], configuration, data.N);

                if (acceptable.Fitness < solutionFitness && Utility.IsLegalSolution(data,neighborhood[acceptable.Index]))
                {
                    solution = neighborhood[acceptable.Index];
                    solutionFitness = acceptable.Fitness;
                }

                if (_tabuList.Count == tabuLength + 2)
                {
                    var first = _tabuList.First();
                    _tabuList.Remove(first);
                }

                _tabuList.Add(neighborhood[acceptable.Index]);

                fitnessesHistory.Add(solutionFitness);

                if (iterations > 100 && fitnessesHistory[iterations - 100] == fitnessesHistory[iterations - 1])
                    break;

                //Console.WriteLine("iterations: " + iterations);
                //Console.WriteLine("fitness: " + solutionFitness);
            }

            return new SetCoveringProblemResult()
            {
                Iterations = iterations,
                Solution = solution,
                SolutionFitness = solutionFitness
            };
        }

        private bool BelongsToTabu(int[] configuration, int n)
        {
            foreach (var entry in _tabuList)
            {
                if (Utility.AreConfigurationsIdentic(entry, configuration, n))
                    return true;
            }

            return false;
        }
    }
}
