using Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticEngine
{
    public class Engine
    {
        private const int PopulationSize = 100;

        private int[,] _l; // population matrix
        private int[] _r;

        public SetCoveringProblemResult Solve(SetCoveringProblemData data)
        {
            Initialize(data);

            int iterations = 0;
            List<int> fitnessesHistory = new List<int>();
            int solution = 1000000000;
            int solutionIndex = 0;

            while (iterations < 3000)
            {
                //Console.WriteLine("Iteration: " + iterations);

                var fitnesses = new List<PopulationEntry>();
                for (int i = 0; i < PopulationSize; ++i)
                {
                    var fitness = Utility.CalculateConfigurationFitness(data,GetChromosomeFromPopulation(i,data.N),data.N);

                    fitnesses.Add(new PopulationEntry
                    {
                        EntryFitness = fitness,
                        EntryIndex = i
                    });
                }

                fitnesses = fitnesses.OrderBy(x => x.EntryFitness).ToList();

                var offSpring = new Offspring();
                if (iterations % 2 == 0)
                    offSpring = Crossover(data, GetChromosomeFromPopulation(fitnesses[0].EntryIndex, data.N), GetChromosomeFromPopulation(fitnesses[PopulationSize - 1].EntryIndex, data.N), data.N);
                else
                    offSpring = Crossover(data, GetChromosomeFromPopulation(fitnesses[0].EntryIndex, data.N), GetChromosomeFromPopulation(fitnesses[1].EntryIndex, data.N), data.N);

                var fitness1 = Utility.CalculateConfigurationFitness(data,offSpring.Chromosome1, data.N);
                var fitness2 = Utility.CalculateConfigurationFitness(data,offSpring.Chromsome2, data.N);

                if (fitness1 > fitness2)
                    for (int i = 0; i < data.N; ++i)
                        _l[fitnesses.Last().EntryIndex, i] = offSpring.Chromsome2[i];
                else
                    for (int i = 0; i < data.N; ++i)
                        _l[fitnesses.Last().EntryIndex, i] = offSpring.Chromosome1[i];

                var totalFitness = 0;
                for (int i = 0; i < PopulationSize; ++i)
                {
                    var fitness = Utility.CalculateConfigurationFitness(data,GetChromosomeFromPopulation(i, data.N), data.N);
                    totalFitness += fitness;
                }

                fitnessesHistory.Add(totalFitness);

                //Console.WriteLine("Population fitness: " + totalFitness);

                iterations++;

                solution = fitnesses[0].EntryFitness;
                solutionIndex = fitnesses[0].EntryIndex;

                if (iterations > 50 && fitnessesHistory[iterations - 50] == fitnessesHistory[iterations - 1])
                    break;
            }

            //Console.WriteLine("solution: " + solution);

            return new SetCoveringProblemResult()
            {
                Iterations = iterations,
                Solution = GetChromosomeFromPopulation(solutionIndex, data.N),
                SolutionFitness = solution
            };
        }

        private int[] GetChromosomeFromPopulation(int i, int n)
        {
            var chromosome = new int[n];
            for (int j = 0; j < n; ++j)
                chromosome[j] = _l[i, j];

            return chromosome;
        }

        private void Initialize(SetCoveringProblemData data)
        {
            _l = new int[PopulationSize, data.N];
            _r = new int[data.M];

            int t = 0;

            var res = new GreedyEngine.Engine().Solve(data);
            for (int i = 0; i < data.N; ++i)
                _l[t, i] = res.Solution[i];
            //FindNextCovering(data, t, (d, r) => d.Subsets[r].First());
            
            while (t < PopulationSize - 1)
            {
                t++;

                for (int j = 0; j < data.M; ++j)
                    _r[j] = 0;

                FindNextCovering(data, t, (d, r) => d.Subsets[r][Utility.Random.Next(data.Subsets[r].Count)]);
            }

            DeleteExcessiveColumns(data);
        }

        private Offspring Crossover(SetCoveringProblemData data, int [] chromosome, int [] chromosome2, int n)
        {
            var offspring1 = new int[n];
            var offspring2 = new int[n];

            int k = Utility.Random.Next(n);

            for (int i = 0; i < k; ++i)
            {
                offspring1[i] = chromosome[i];
                offspring2[i] = chromosome2[i];
            }

            for (int i = k; i < n; ++i)
            {
                offspring1[i] = chromosome2[i];
                offspring2[i] = chromosome[i];
            }

          //  for (int i = 0; i < 3; ++i)
                MutateChromosome(data, offspring1, n);
            //Uti(data, offspring1, n);

           // for (int i = 0; i < 3; ++i)
                MutateChromosome(data, offspring2, n);
            //AddMissingGenees(data, offspring2, n);

            return FixOffSpringChromosomes(data, offspring1, offspring2, n);
        }

        private void MutateChromosome(SetCoveringProblemData data, int [] chromosome, int n)
        {
            int k = Utility.Random.Next(n);

            var probabilityOfMutation = 1.0 / E(data, k);

            double probabilityDivistionFactor = 0;
            for (int i = 0; i < n; ++i)
                probabilityDivistionFactor += 1.0 / E(data, i);

            probabilityOfMutation /= probabilityDivistionFactor;

            if (probabilityOfMutation > 1.0 / n && data.ColumnCount(0, k) > data.ColumnCount(1, k))
                chromosome[k] = (chromosome[k] + 1) % 2;
        }

        private double E(SetCoveringProblemData data, int j)
        {
            return -data.ColumnCount(0, j) * Math.Log(data.ColumnCount(0, j)) - data.ColumnCount(1, j) * Math.Log(data.ColumnCount(1, j));
        }

        private Offspring FixOffSpringChromosomes(SetCoveringProblemData data, int [] chromosome, int [] chromosome2, int n)
        {
            Utility.AddMissingSetToCover(data, chromosome, n);
            Utility.DeleteExcessiveSetsFromCover(data, chromosome);

            Utility.AddMissingSetToCover(data, chromosome2, n);
            Utility.DeleteExcessiveSetsFromCover(data, chromosome2);

            return new Offspring()
            {
                Chromosome1 = chromosome,
                Chromsome2 = chromosome2
            };
        }

        private void DeleteExcessiveColumns(SetCoveringProblemData data)
        {
            for (int k = 0; k < PopulationSize; ++k)
            {
                var chromosome = new int[data.N];
                for (int i = 0; i < data.N; ++i)
                    chromosome[i] = _l[k,i];

                Utility.DeleteExcessiveSetsFromCover(data, chromosome);

                for (int i = 0; i < data.N; ++i)
                    _l[k, i] = chromosome[i];
            }
        }

        private void FindNextCovering(SetCoveringProblemData data, int t,
            Func<SetCoveringProblemData, int, int> nextCoveringFunction)
        {
            int r = 0;
            while (r != -1)
            {
                int j1 = nextCoveringFunction(data, r);
                _l[t, j1] = 1;
                for (int l = 0; l < data.M; ++l)
                    _r[l] = _r[l] + data.A[l, j1];

                r = Array.IndexOf(_r, 0);
            }
        }
    }
}
