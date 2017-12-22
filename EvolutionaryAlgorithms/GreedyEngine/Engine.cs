using Shared;
using System.Collections.Generic;
using System.Linq;

namespace GreedyEngine
{
    public class Engine
    {
        public SetCoveringProblemResult Solve(SetCoveringProblemData data)
        {
            var universal = new List<int>();

            for (int i = 1; i <= data.M; ++i)
                universal.Add(i);

            int iterations = 0;
            int[] solution = new int[data.N];
            int solutionFitness = 0;

            while (universal.Any())
            {
                iterations++;

                int maxIndex = 0;
                int max = 0;
                for (int j = 0; j < data.N; ++j)
                    if (solution[j] != 1)
                    {
                        var intersecting = universal.Intersect(data.SubsetsWithBelongingElements[j]).Count();

                        if (intersecting > max)
                        {
                            max = intersecting;
                            maxIndex = j;
                        }
                    }

                universal = universal.Except(data.SubsetsWithBelongingElements[maxIndex]).ToList();
                solution[maxIndex] = 1;
                solutionFitness++;
            }

            return new SetCoveringProblemResult()
            {
                Iterations = iterations,
                Solution = solution,
                SolutionFitness = solutionFitness
            };
        }
    }
}
