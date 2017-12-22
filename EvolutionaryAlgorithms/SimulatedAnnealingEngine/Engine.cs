using Shared;
using System;
using System.Linq;

namespace SimulatedAnnealingEngine
{
    public class Engine
    {
        public SetCoveringProblemResult Solve(SetCoveringProblemData data)
        {
            double t = 1000000;
            double tStop = 0.02;
            double alfa = 0.98;

            int iterations = 0;

            //var res = new GreedyEngine.Engine().Solve(data);
            var configuration = Initialize(data);

            while (t > tStop)
            {
                iterations++;

                int [] oldConfiguration = new int[data.N];
                Array.Copy(configuration, oldConfiguration, data.N);

                int oldFitness = Utility.CalculateConfigurationFitness(data,configuration, data.N);

                if (Utility.Random.NextDouble() >= 0.5)
                {
                    int k1 = Utility.Random.Next(data.N);
                    int k2 = Utility.Random.Next(data.N);

                    int temp = configuration[k1];
                    configuration[k1] = configuration[k2];
                    configuration[k2] = temp;
                }
                else
                {
                    int k1 = Utility.Random.Next(data.N);

                    configuration[k1] = (configuration[k1] + 1) % 2;
                }

                Utility.AddMissingSetToCover(data, configuration, data.N);
                Utility.DeleteExcessiveSetsFromCover(data, configuration);

                int newFitness = Utility.CalculateConfigurationFitness(data,configuration, data.N);

                //Console.WriteLine("iteration: " + iterations);
                //Console.WriteLine("fitness: " + newFitness);

                if (newFitness > oldFitness)
                {
                    double acceptProbability = 1.0 / Math.Exp((newFitness - oldFitness) / t);

                    if (Utility.Random.NextDouble() > acceptProbability)
                        Array.Copy(oldConfiguration, configuration, data.N);
                }

                t *= alfa;
            }

            int finalFitness = Utility.CalculateConfigurationFitness(data,configuration, data.N);

            return new SetCoveringProblemResult()
            {
                Iterations = iterations,
                Solution = configuration,
                SolutionFitness = finalFitness
            };
        }
        
        private int [] Initialize(SetCoveringProblemData data)
        {
            int [] solution = new int[data.N];
            int[] rArray = new int[data.M];

            int r = 0;
            while (r != -1)
            {
                int j1 = data.Subsets[r].First();
                solution[j1] = 1;
                for (int l = 0; l < data.M; ++l)
                    rArray[l] = rArray[l] + data.A[l, j1];

                r = Array.IndexOf(rArray, 0);
            }

            return solution;
        }
    }
}
