using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GemCollectorSolver
{
    private int[] board;
    private int rows, cols;
    public bool[] collected;
    int totalMove = 0;
    int collectedValue = 0;
    int sumBestValue = 0;

    public GemCollectorSolver(int[] board, int rows, int cols)
    {
        this.board = board;
        this.rows = rows;
        this.cols = cols;
        collected = new bool[board.Length];
        sumBestValue = SumBestValue();
    }
    private bool IsEnoughFivesCollected(int collected)
    {
        int total = SumBestValue();

        int neededFives = (total / 2) * 2;

        if (collected >= neededFives)
        {
            Debug.Log($"🎯 Đã thu thập đủ {neededFives} số 5, dừng lại.");
            return true;
        }
        return false;
    }
    private int SumBestValue()
    {
        int count = 0;
        for (int i = 0; i < board.Length; i++)
        {
            if (board[i] == 5)
                count++;
        }
        return count;
    }

    public void Solve(int bestValue)
    {
        int countBestValue = board.Count(x => x == bestValue);
        Console.WriteLine($"Số lượng phần tử bằng {bestValue}: {countBestValue}");

        var allPairs = FindAllPairs(bestValue);
        foreach (var p in allPairs)
        {
            //ShortestPath(p.i1 / cols, p.i1 % cols, p.i2 / cols, p.i2 % cols);
            Console.WriteLine($"Cặp giá trị 5 tại index: {p.i1} - {p.i2}");
        }

    }
    public List<(int i1, int i2)> FindAllPairs(int bestValue)
    {
        var pairs = new List<(int i1, int i2)>();
        int n = board.Length;

        for (int i = 0; i < n - 1; i++)
        {
            if (board[i] != bestValue) continue;

            for (int j = i + 1; j < n; j++)
            {
                if (board[j] != bestValue) continue;

                pairs.Add((i, j));
            }
        }
        Console.WriteLine($"Tổng số cặp tìm được: {pairs.Count}");

        return pairs;
    }

    private int ShortestPath(int r1, int c1, int r2, int c2)
    {
        int sumPath = 0;
        var obs = FindObstacles(r1, c1, r2, c2);
        foreach (var ob in obs)
        {
            int indexMatch = FindMoveMatchForObstacle(ob.index); //4-2
            if (indexMatch != -1)
            {
                collected[indexMatch] = true;
                collected[ob.index] = true;

                Debug.Log($"🗑 Loại bỏ vật cản {ob.value} tại ({ob.r},{ob.c}) match với số {board[indexMatch]} tại ({indexMatch/cols},{indexMatch%cols})"); //move

                sumPath++;
            }
        }
        return 0;
    }

    private List<(int index, int value, int r, int c)> FindObstacles(int r1, int c1, int r2, int c2)
    {
        var obs = new List<(int, int, int, int)>();

        if (r1 == r2) // ngang
        {
            int minC = Math.Min(c1, c2) + 1;
            int maxC = Math.Max(c1, c2);
            for (int c = minC; c < maxC; c++)
            {
                int idx = r1 * cols + c;
                if (!collected[idx] && board[idx] != 0)
                {
                    obs.Add((idx, board[idx], r1, c));
                }
            }
        }
        else if (c1 == c2) // dọc
        {
            int minR = Math.Min(r1, r2) + 1;
            int maxR = Math.Max(r1, r2);
            for (int r = minR; r < maxR; r++)
            {
                int idx = r * cols + c1;
                if (!collected[idx] && board[idx] != 0)
                {
                    obs.Add((idx, board[idx], r, c1));
                }
            }
        }
        else
        {
            int dr = r2 - r1;
            int dc = c2 - c1;

            if (Math.Abs(dr) == Math.Abs(dc)) // chéo chính hoặc chéo phụ
            {
                int stepR = dr > 0 ? 1 : -1;
                int stepC = dc > 0 ? 1 : -1;
                int steps = Math.Abs(dr);

                for (int i = 1; i < steps; i++)
                {
                    int r = r1 + i * stepR;
                    int c = c1 + i * stepC;
                    int idx = r * cols + c;
                    if (!collected[idx] && board[idx] != 0)
                    {
                        obs.Add((idx, board[idx], r, c));
                    }
                }
            }
        }
        return obs;
    }
    private List<(int index, int r, int c, int value)> SortObsByMove(List<(int index, int r, int c, int value)> obs, int currentR, int currentC)
    {
        var obsWithMove = new List<((int index, int r, int c, int value) ob, int move)>();

        foreach (var ob in obs)
        {
            int move = GetMoveIfMatch(ob.r, ob.c, currentR, currentC);
            if (move >= 1)
            {
                obsWithMove.Add((ob, move));
            }
        }

        obsWithMove.Sort((a, b) => a.move.CompareTo(b.move));

        return obsWithMove.Select(x => x.ob).ToList();
    }
    private int FindMoveMatchForObstacle(int obsIndex)
    {
        int currentR = obsIndex / cols;
        int currentC = obsIndex % cols;
        var obs = GetValidCellsExcept(obsIndex);

        var sortedObs = SortObsByMove(obs, currentR, currentC);

        foreach (var ob in sortedObs)
        {
            int baseMove = GetMoveIfMatch(ob.r, ob.c, currentR, currentC);

            if (baseMove == 1)
            {
                return ob.index; // trực tiếp trả về khi move=1
            }
            else if (baseMove > 1)
            {
                int clearedMoves = TryClearObstacle(ob.r, ob.c, currentR, currentC);
                if (clearedMoves != -1)
                {
                    int totalMove = baseMove + clearedMoves;
                    if (totalMove == 0)
                    {
                        return ob.index;
                    }
                    else
                    {
                        // Nếu chưa đạt move=1, tiếp tục đệ quy để dọn tiếp
                        int nextMatch = FindMoveMatchForObstacle(obsIndex);
                        if (nextMatch != -1)
                            return nextMatch;
                    }
                }
            }
        }

        return -1;
    }

    private int TryClearObstacle(int r1, int c1, int r2, int c2)
    {
        var cellsOnPath = GetCellsOnPathIndices(r1, c1, r2, c2, GetPathType(r1, c1, r2, c2));
        int clearedCount = 0;

        foreach (var cell in cellsOnPath)
        {
            int index = cell.index;
            if (IsObstacle(index))
            {
                int nextIndex = FindMoveMatchForObstacle(index);
                if (nextIndex == -1)
                {
                    return -1;
                }
                else
                {
                    ClearObstacleAt(nextIndex);
                    clearedCount++; 
                }
            }
        }

        return clearedCount; 
    }
    private bool IsObstacle(int index)
    {
        return !collected[index];
    }
    private void ClearObstacleAt(int index)
    {
        collected[index] = true; 
    }
    private bool IsMatch(int a, int b)
    {
        return (a == b) || (a + b == 10);
    }
    private List<(int index, int r, int c, int value)> GetValidCellsExcept(int obsIndex)
    {
        var result = new List<(int, int, int, int)>();

        for (int i = 0; i < board.Length; i++)
        {
            if (i == obsIndex || collected[i])
                continue;

            if (!IsMatch(GetValueAtIndex(i), GetValueAtIndex(obsIndex)))
                continue;

            int r = i / cols;
            int c = i % cols;
            int val = board[i];

            result.Add((i, r, c, val));
        }

        return result;
    }
    private int GetValueAtIndex(int index)
    {
        if (index < 0 || index >= board.Length)
            throw new ArgumentOutOfRangeException(nameof(index), "Index ngoài phạm vi mảng board.");

        return board[index];
    }
    private int GetMoveIfMatch(int r1, int c1, int r2, int c2)
    {
        // Hàm phụ lấy các chỉ số ô nằm giữa 2 điểm
        List<int> indicesBetween = GetCellsOnPathIndices(r1, c1, r2, c2, GetPathType(r1, c1, r2, c2))
                                    .Select(cell => cell.index).ToList();

        // Kiểm tra tất cả ô giữa có collected == true không
        bool allCollected = indicesBetween.All(index => collected[index]);

        int distance = -1;

        if (r1 == r2)
            distance = Math.Abs(c1 - c2) - 1;
        else if (c1 == c2)
            distance = Math.Abs(r1 - r2) - 1;
        else
        {
            int dr = r2 - r1;
            int dc = c2 - c1;

            if (Math.Abs(dr) == Math.Abs(dc))
                distance = Math.Abs(dr) - 1;
        }

        if (distance < 0)
            return -1;

        if (allCollected)
            return 1; // Nếu tất cả ô giữa đã collected, coi như move = 1

        return distance;
    }
    private string GetPathType(int r1, int c1, int r2, int c2)
    {
        if (r1 == r2)
            return "Horizontal";
        if (c1 == c2)
            return "Vertical";
        if (Math.Abs(r1 - r2) == Math.Abs(c1 - c2))
            return "Diagonal";

        return null; // Không phải 3 loại đường này
    }
    // Không bao gồm 2 điểm đầu cuối
    private List<(int index, int value, int r, int c)> GetCellsOnPathIndices(int r1, int c1, int r2, int c2, string pathType)
    {
        var result = new List<(int, int, int, int)>();

        if (pathType == "Horizontal")
        {
            int minC = Math.Min(c1, c2) + 1;
            int maxC = Math.Max(c1, c2) - 1;
            for (int c = minC; c <= maxC; c++)
            {
                int index = r1 * cols + c;
                int value = board[index]; // Giả sử bạn có mảng board lưu giá trị tại mỗi index
                result.Add((index, value, r1, c));
            }
        }
        else if (pathType == "Vertical")
        {
            int minR = Math.Min(r1, r2) + 1;
            int maxR = Math.Max(r1, r2) - 1;
            for (int r = minR; r <= maxR; r++)
            {
                int index = r * cols + c1;
                int value = board[index];
                result.Add((index, value, r, c1));
            }
        }
        else if (pathType == "Diagonal")
        {
            int dr = (r2 > r1) ? 1 : -1;
            int dc = (c2 > c1) ? 1 : -1;

            int r = r1 + dr;
            int c = c1 + dc;
            while (r != r2 && c != c2)
            {
                int index = r * cols + c;
                int value = board[index];
                result.Add((index, value, r, c));
                r += dr;
                c += dc;
            }
        }

        return result;
    }
}
