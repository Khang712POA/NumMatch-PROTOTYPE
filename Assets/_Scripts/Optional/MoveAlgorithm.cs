using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class MoveAlgorithm
{
    private int[] board;
    private int rows, cols;
    private List<(int r, int c)> positions;
    private int countToCollect;

    // Lưu 10 lời giải tốt nhất: (totalCost, list pairs)
    private SortedSet<(int, List<(int, int, int, int)>)> bestSolutions;

    public MoveAlgorithm(int[] board, int rows, int cols)
    {
        this.board = board;
        this.rows = rows;
        this.cols = cols;
        positions = new List<(int, int)>();
        bestSolutions = new SortedSet<(int, List<(int, int, int, int)>)>(Comparer<(int, List<(int, int, int, int)>)>.Create(
            (a, b) =>
            {
                int cmp = a.Item1.CompareTo(b.Item1);
                if (cmp != 0) return cmp;
                return 1; // tránh trùng key SortedSet
            }
        ));

        CollectPositions();
    }

    private void CollectPositions()
    {
        for (int i = 0; i < board.Length; i++)
        {
            if (board[i] == 5)
            {
                int r = i / cols;
                int c = i % cols;
                positions.Add((r, c));
            }
        }

        int count = positions.Count;
        countToCollect = (count / 2) * 2; // phần chẵn lớn nhất

        if (positions.Count > countToCollect)
            positions = positions.GetRange(0, countToCollect);
    }

    private int GetMoveCost((int r, int c) a, (int r, int c) b)
    {
        if (a.r == b.r) return Math.Abs(a.c - b.c) - 1;
        if (a.c == b.c) return Math.Abs(a.r - b.r) - 1;
        return int.MaxValue / 2;
    }

    public void SolveAndSaveTop10(string outputFile)
    {
        int n = positions.Count;
        bool[] used = new bool[n];
        List<(int, int, int, int)> currentPairs = new List<(int, int, int, int)>();

        DFS(used, currentPairs, 0, 0);

        using (StreamWriter writer = new StreamWriter(outputFile))
        {
            foreach (var sol in bestSolutions.Take(10))
            {
                var line = string.Join("|", sol.Item2.Select(p => $"{p.Item1},{p.Item2},{p.Item3},{p.Item4}"));
                writer.WriteLine(line);
            }
        }
    }

    private void DFS(bool[] used, List<(int, int, int, int)> currentPairs, int startIndex, int currentCost)
    {
        if (currentPairs.Count == countToCollect / 2)
        {
            if (bestSolutions.Count < 10)
            {
                bestSolutions.Add((currentCost, new List<(int, int, int, int)>(currentPairs)));
            }
            else
            {
                var maxCost = bestSolutions.Max.Item1;
                if (currentCost < maxCost)
                {
                    bestSolutions.Remove(bestSolutions.Max);
                    bestSolutions.Add((currentCost, new List<(int, int, int, int)>(currentPairs)));
                }
            }
            return;
        }

        int firstUnused = -1;
        for (int i = startIndex; i < used.Length; i++)
        {
            if (!used[i])
            {
                firstUnused = i;
                break;
            }
        }

        if (firstUnused == -1) return;

        used[firstUnused] = true;

        for (int j = firstUnused + 1; j < used.Length; j++)
        {
            if (!used[j])
            {
                int cost = GetMoveCost(positions[firstUnused], positions[j]);
                if (cost == int.MaxValue / 2)
                    continue;

                int newCost = currentCost + cost;

                if (bestSolutions.Count == 10 && newCost >= bestSolutions.Max.Item1)
                    continue;

                used[j] = true;
                currentPairs.Add((positions[firstUnused].r, positions[firstUnused].c, positions[j].r, positions[j].c));
                DFS(used, currentPairs, firstUnused + 1, newCost);
                currentPairs.RemoveAt(currentPairs.Count - 1);
                used[j] = false;
            }
        }
        used[firstUnused] = false;
    }
}
