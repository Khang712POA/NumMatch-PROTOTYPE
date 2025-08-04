using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class MatchLogic
{
    private LevelData levelData;
    private List<TileNumber> tileNumbers;

    public MatchLogic(LevelData data)
    {
        levelData = data;
        LoadTiles();
    }
    private void LoadTiles()
    {
        tileNumbers = new List<TileNumber>();
        for (int i = 0; i < levelData.boardNumbers.Count; i++)
        {
            int value = levelData.boardNumbers[i];
            if (value == 5)
            {
                tileNumbers.Add(new TileNumber(i, value, levelData.columnCount));
            }
        }
    }

    public void Solve()
    {
        int pairCount = (tileNumbers.Count / 2) * 2;
        var selectedTiles = tileNumbers.Take(pairCount).ToList();

        List<string> allSolutions = new List<string>();
        var combinations = GetAllPairCombinations(selectedTiles);

        foreach (var solution in combinations.Take(10))
        {
            allSolutions.Add(FormatSolution(solution));
        }

        File.WriteAllLines("output.txt", allSolutions);
    }

    private string FormatSolution(List<(TileNumber, TileNumber)> pairs)
    {
        return string.Join("|", pairs.Select(p =>
            $"{p.Item1.Row},{p.Item1.Col},{p.Item2.Row},{p.Item2.Col}"
        ));
    }

    private List<List<(TileNumber, TileNumber)>> GetAllPairCombinations(List<TileNumber> tiles)
    {
        List<List<(TileNumber, TileNumber)>> results = new List<List<(TileNumber, TileNumber)>>();

        void Backtrack(List<TileNumber> remaining, List<(TileNumber, TileNumber)> current)
        {
            if (remaining.Count < 2)
            {
                results.Add(new List<(TileNumber, TileNumber)>(current));
                return;
            }

            for (int i = 1; i < remaining.Count; i++)
            {
                var pair = (remaining[0], remaining[i]);
                var nextRemaining = new List<TileNumber>(remaining);
                nextRemaining.RemoveAt(i);
                nextRemaining.RemoveAt(0);

                current.Add(pair);
                Backtrack(nextRemaining, current);
                current.RemoveAt(current.Count - 1);
            }
        }

        Backtrack(tiles, new List<(TileNumber, TileNumber)>());
        return results;
    }
}
