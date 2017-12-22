using System;
using System.Collections.Generic;
using System.Linq;

namespace Shared
{
    public static class Utility
    {
        public static Random Random = new Random();

        public static bool AreConfigurationsIdentic(int [] configuration, int [] configuration2, int n)
        {
            for (int i = 0; i < n; ++i)
                if (configuration[i] != configuration2[i])
                    return false;
            return true;
        }

        public static bool IsLegalSolution(SetCoveringProblemData data, int [] configuration)
        {
            var coveredElements = new List<int>();

            int coveringSetsCount = 0;
            for (int j = 0; j < data.N; ++j)
            {
                coveringSetsCount += configuration[j];
                if (configuration[j] == 1)
                    coveredElements = coveredElements.Union(data.SubsetsWithBelongingElements[j]).ToList();
            }

            return coveredElements.Count == data.M;
        }

        public static int NumberOfSubsetsIncover(int [] configuration, int n)
        {
            int numberOfSubsets = 0;
            for (int i = 0; i < n; ++i)
                numberOfSubsets += configuration[i];
            return numberOfSubsets;
        }

        public static int CalculateConfigurationFitness(SetCoveringProblemData data, int[] chromosome, int n)
        {
            var coveredElements = new List<int>();

            int coveringSetsCount = 0;
            for (int j = 0; j < n; ++j)
            {
                coveringSetsCount += chromosome[j];
                if (chromosome[j] == 1)
                    coveredElements = coveredElements.Union(data.SubsetsWithBelongingElements[j]).ToList();
            }

            return (data.M - coveredElements.Count) + coveringSetsCount;
        }

        public static int[] CountConfigurationCoverings(SetCoveringProblemData data, int[] chromosome)
        {
            var coveringsQuantity = new int[data.M];

            var chromosomeSubsets = new List<int>();
            for (int i = 0; i < data.N; ++i)
                if (chromosome[i] == 1)
                    chromosomeSubsets.Add(i);

            for (int i = 0; i < data.M; ++i)
                coveringsQuantity[i] = data.Subsets[i].Intersect(chromosomeSubsets).Count();

            return coveringsQuantity;
        }

        public static void AddMissingSetToCover(SetCoveringProblemData data, int[] chromosome, int n)
        {
            var coveringsQuantity = CountConfigurationCoverings(data, chromosome);

            int i1 = Array.FindIndex(coveringsQuantity, x => x == 0);
            while (i1 != -1)
            {
                var subset = data.Subsets[i1][Random.Next(data.Subsets[i1].Count)];
                chromosome[subset] = 1;

                for (int i = 0; i < data.M; ++i)
                    coveringsQuantity[i] += data.A[i, subset];

                i1 = Array.FindIndex(coveringsQuantity, i1 + 1, x => x == 0);
            }
        }

        public static void DeleteExcessiveSetsFromCover(SetCoveringProblemData data, int[] chromosome)
        {
            var coveringsQuantity = CountConfigurationCoverings(data, chromosome);

            int i1 = Array.FindIndex(coveringsQuantity, x => x > 1);
            while (i1 != -1)
            {
                for (int i = 0; i < data.N; ++i)
                    if (chromosome[i] == 1)
                    {
                        bool excessiveForAll = true;
                        for (int j = 0; j < data.M; ++j)
                            if (coveringsQuantity[j] - data.A[j, i] <= 0)
                                excessiveForAll = false;

                        if (excessiveForAll)
                        {
                            chromosome[i] = 0;
                            for (int j = 0; j < data.M; ++j)
                                coveringsQuantity[j] -= data.A[j, i];
                        }
                    }


                i1 = Array.FindIndex(coveringsQuantity, i1 + 1, x => x > 1);
            }
        }
    }
}
