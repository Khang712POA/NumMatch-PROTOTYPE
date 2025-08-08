using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MatchGenerator : MonoBehaviour
{
    public int[] flatGrid;
    private void Start()
    {
        //flatGrid = GenerateStage(3);
        //FindAndCheckMatches(flatGrid, 3, 9);
        //PrintGrid(flatGrid, 3, 9);
    }

    public int[] GenerateStage(int stage)
    {
        const int rows = 3;
        const int cols = 9;
        const int totalTiles = rows * cols;

        int numberOfPairs = stage == 1 ? 3 : stage == 2 ? 2 : 1;

        System.Random rand = new System.Random();
        int attempts = 0;
        int[] flatGrid;

        do
        {
            attempts++;

            flatGrid = new int[totalTiles];
            List<(int, int)> validPairs = GenerateValidPairs();
            HashSet<int> usedIndices = new();
            Dictionary<int, int> frequency = new();

            for (int i = 1; i <= 9; i++) frequency[i] = 0;

            List<List<int>> directions = new()
        {
            new() { 0, 1 },     // ngang
            new() { 1, 0 },     // dọc
            new() { 1, 1 },     // chéo chính
            new() { 1, -1 }     // chéo phụ
        };

            int placedPairs = 0;

            // Đặt đúng số lượng cặp match cần
            int pairAttempts = 0;
            while (placedPairs < numberOfPairs && pairAttempts++ < 1000)
            {
                int r = rand.Next(rows);
                int c = rand.Next(cols);
                int index1 = r * cols + c;
                if (flatGrid[index1] != 0) continue;

                foreach (var dir in directions)
                {
                    int r2 = r + dir[0];
                    int c2 = c + dir[1];
                    if (r2 < 0 || r2 >= rows || c2 < 0 || c2 >= cols) continue;

                    int index2 = r2 * cols + c2;
                    if (flatGrid[index2] != 0) continue;

                    var pair = validPairs[rand.Next(validPairs.Count)];

                    if (frequency[pair.Item1] >= 4 || frequency[pair.Item2] >= 4) continue;

                    flatGrid[index1] = pair.Item1;
                    flatGrid[index2] = pair.Item2;
                    frequency[pair.Item1]++;
                    frequency[pair.Item2]++;
                    usedIndices.Add(index1);
                    usedIndices.Add(index2);
                    placedPairs++;
                    break;
                }
            }

            if (placedPairs != numberOfPairs) continue;

            // Điền số còn lại
            for (int i = 0; i < flatGrid.Length; i++)
            {
                if (flatGrid[i] != 0) continue;

                int tryVal = 0;
                int retry = 0;
                do
                {
                    tryVal = rand.Next(1, 10);
                    retry++;
                } while (retry < 100 && (frequency[tryVal] >= 4 || HasMatch(flatGrid, i, tryVal, rows, cols)));

                flatGrid[i] = tryVal;
                frequency[tryVal]++;
            }

            // Xác nhận số lần xuất hiện hợp lệ (tùy chọn)
            bool freqValid = frequency.Values.All(count => count >= 2 && count <= 4);
            int matchCount = CountValidMatches(flatGrid, rows, cols);

            if (matchCount == numberOfPairs && freqValid)
            {
                Debug.Log($"✅ Generated stage in {attempts} attempt(s)");
                return flatGrid;
            }

        } while (true); // Lặp lại nếu chưa đạt yêu cầu
    }
    private int CountValidMatches(int[] flatGrid, int rows, int cols)
    {
        HashSet<int> matched = new HashSet<int>();
        int totalMatches = 0;

        for (int i = 0; i < flatGrid.Length; i++)
        {
            if (flatGrid[i] == 0 || matched.Contains(i)) continue;

            int r1 = i / cols;
            int c1 = i % cols;

            for (int j = i + 1; j < flatGrid.Length; j++)
            {
                if (flatGrid[j] == 0 || matched.Contains(j)) continue;

                int r2 = j / cols;
                int c2 = j % cols;

                int val1 = flatGrid[i];
                int val2 = flatGrid[j];

                bool isSame = val1 == val2;
                bool isSumTen = val1 + val2 == 10;

                if ((isSame || isSumTen) && IsPathClear1D(flatGrid, r1, c1, r2, c2, rows, cols))
                {
                    matched.Add(i);
                    matched.Add(j);
                    totalMatches++;
                    break; // để tránh đếm một ô nhiều lần
                }
            }
        }

        return totalMatches;
    }
    private bool HasMatch(int[] grid, int index, int value, int rows, int cols)
    {
        int r = index / cols;
        int c = index % cols;

        for (int dr = -1; dr <= 1; dr++)
        {
            for (int dc = -1; dc <= 1; dc++)
            {
                if (dr == 0 && dc == 0) continue;

                int nr = r + dr;
                int nc = c + dc;

                if (nr < 0 || nr >= rows || nc < 0 || nc >= cols) continue;

                int ni = nr * cols + nc;
                int other = grid[ni];
                if (other == 0) continue;

                if (value == other || value + other == 10)
                    return true;
            }
        }

        return false;
    }

    private List<(int, int)> GenerateValidPairs()
    {
        List<(int, int)> pairs = new();
        for (int i = 1; i <= 9; i++) pairs.Add((i, i));
        for (int i = 1; i <= 4; i++) pairs.Add((i, 10 - i)); // (1,9), (2,8), (3,7), (4,6)
        return pairs;
    }

    public void FindAndCheckMatches(int[] flatGrid, int rows, int cols)
    {
        HashSet<int> matched = new HashSet<int>();
        int totalMatches = 0;

        for (int i = 0; i < flatGrid.Length; i++)
        {
            if (flatGrid[i] == 0 || matched.Contains(i)) continue;

            int r1 = i / cols;
            int c1 = i % cols;

            for (int j = 0; j < flatGrid.Length; j++)
            {
                if (i == j || flatGrid[j] == 0 || matched.Contains(j)) continue;

                int r2 = j / cols;
                int c2 = j % cols;

                int val1 = flatGrid[i];
                int val2 = flatGrid[j];

                bool isSame = val1 == val2;
                bool isSumTen = val1 + val2 == 10;

                if ((isSame || isSumTen) && IsPathClear1D(flatGrid, r1, c1, r2, c2, rows, cols))
                {
                    matched.Add(i);
                    matched.Add(j);
                    totalMatches++;
                    Debug.Log($"✅ Match: {val1} ({r1}, {c1}) <-> {val2} ({r2}, {c2})");

                    CheckSurroundingForPotentialMatch1D(flatGrid, r1, c1, val1, matched, rows, cols);
                    CheckSurroundingForPotentialMatch1D(flatGrid, r2, c2, val2, matched, rows, cols);
                    break;
                }
            }
        }

        Debug.Log($"🔍 Tổng số cặp match được: {totalMatches}");
    }


    void CheckSurroundingForPotentialMatch1D(int[] grid, int r, int c, int currentValue, HashSet<int> matched, int rows, int cols)
    {
        int[,] directions = {
        {-1, -1}, {-1, 0}, {-1, 1},
        { 0, -1},          { 0, 1},
        { 1, -1}, { 1, 0}, { 1, 1}
    };

        for (int i = 0; i < directions.GetLength(0); i++)
        {
            int nr = r + directions[i, 0];
            int nc = c + directions[i, 1];
            if (nr < 0 || nc < 0 || nr >= rows || nc >= cols) continue;

            int neighborIndex = nr * cols + nc;
            if (grid[neighborIndex] == 0 || matched.Contains(neighborIndex)) continue;

            int neighborValue = grid[neighborIndex];
            bool isSame = neighborValue == currentValue;
            bool isSumTen = neighborValue + currentValue == 10;

            if (isSame || isSumTen)
            {
                bool canMatchFurther = false;

                for (int j = 0; j < directions.GetLength(0); j++)
                {
                    int rr = nr + directions[j, 0];
                    int cc = nc + directions[j, 1];

                    if (rr < 0 || cc < 0 || rr >= rows || cc >= cols) continue;

                    int aroundIdx = rr * cols + cc;
                    if ((rr == r && cc == c) || grid[aroundIdx] == 0 || matched.Contains(aroundIdx)) continue;

                    int valAround = grid[aroundIdx];
                    bool valid = (valAround == neighborValue || valAround + neighborValue == 10) &&
                                 IsPathClear1D(grid, nr, nc, rr, cc, rows, cols);

                    if (valid)
                    {
                        canMatchFurther = true;
                        break;
                    }
                }

                if (!canMatchFurther)
                {
                    Debug.Log($"🚫 Ô ({nr},{nc})[{neighborValue}] gần ô ({r},{c})[{currentValue}] nhưng không thể tạo match vì bị chặn.");
                }
            }
        }
    }

    bool IsPathClear1D(int[] grid, int r1, int c1, int r2, int c2, int rows, int cols)
    {
        int dr = r2 - r1;
        int dc = c2 - c1;

        int absDr = Mathf.Abs(dr);
        int absDc = Mathf.Abs(dc);

        // Ngang
        if (r1 == r2)
        {
            int step = dc > 0 ? 1 : -1;
            for (int c = c1 + step; c != c2; c += step)
            {
                int idx = r1 * cols + c;
                if (grid[idx] != 0) return false;
            }
            return true;
        }

        // Dọc
        if (c1 == c2)
        {
            int step = dr > 0 ? 1 : -1;
            for (int r = r1 + step; r != r2; r += step)
            {
                int idx = r * cols + c1;
                if (grid[idx] != 0) return false;
            }
            return true;
        }

        // Chéo
        if (absDr == absDc)
        {
            int stepR = dr > 0 ? 1 : -1;
            int stepC = dc > 0 ? 1 : -1;
            int steps = absDr;

            for (int i = 1; i < steps; i++)
            {
                int r = r1 + i * stepR;
                int c = c1 + i * stepC;
                int idx = r * cols + c;
                if (grid[idx] != 0) return false;
            }
            return true;
        }

        return false;
    }


}
