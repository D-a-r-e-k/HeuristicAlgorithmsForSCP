using System.Collections.Generic;

namespace Shared
{
    public class SetCoveringProblemData
    {
        public int[,] A { get; set; }

        public int N { get; set; }
        public int M { get; set; }

        public List<List<int>> Subsets { get; set; }
        public List<List<int>> SubsetsWithBelongingElements { get; set; }

        public void Initialize()
        {
            A = new int[M, N];
            for (int i = 0; i < Subsets.Count; ++i)
                foreach (var covering in Subsets[i])
                    A[i,covering] = 1;

            SubsetsWithBelongingElements = new List<List<int>>();
            for (int i = 0; i < N; ++i)
                SubsetsWithBelongingElements.Add(new List<int>());

            for (int i = 0; i < M; ++i)
            {
                for (int j = 0; j < Subsets[i].Count; ++j)
                {
                    if (SubsetsWithBelongingElements[Subsets[i][j]] == null)
                        SubsetsWithBelongingElements[Subsets[i][j]] = new List<int>();
                    SubsetsWithBelongingElements[Subsets[i][j]].Add(i + 1);
                }
            }
        }

        public int ColumnCount(int symbol, int column)
        {
            int res = 0;
            for (int i = 0; i < M; ++i)
                res += (A[i, column] == symbol ? 1 : 0);

            return res;
        }
    }
}
