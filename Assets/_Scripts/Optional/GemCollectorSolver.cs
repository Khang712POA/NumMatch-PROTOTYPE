using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

public class GemCollectorSolver
{
    private int[] board;
    private int rows, cols;
    public bool[] collected;
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
    private List<(int index, int r, int c, int value)> FindSameValueCells(int index)
    {
        List<(int, int, int, int)> result = new List<(int, int, int, int)>();

        if (index < 0 || index >= board.Length)
            return result;

        int targetValue = board[index];  // giá trị cần tìm
        int targetR = index / cols;
        int targetC = index % cols;

        for (int i = 0; i < board.Length; i++)
        {
            if (board[i] == targetValue && !collected[i]) // chỉ lấy ô chưa collected
            {
                int r = i / cols;
                int c = i % cols;
                result.Add((i, r, c, board[i]));
            }
        }

        return result;
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
        //int countBestValue = board.Count(x => x == bestValue);
        //Debug.Log("Số lượng phần tử bằng " + bestValue + " | " + countBestValue);

        //var allPairs = FindAllPairs(bestValue);
        //foreach (var p in allPairs)
        //{
        ShortestPath(9, 3, 6, 6);
        //Debug.Log($"Cặp giá trị {bestValue} tại index: {p.i1} - {p.i2}");
        //}
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
 
    public int totalMove = 0;
    private HashSet<int> clearedObstacles = new HashSet<int>();

    private int ShortestPath(int r1, int c1, int r2, int c2)
    {
        var obs = FindObstacles(r1, c1, r2, c2).ToList();
        if (obs.Count == 0) return 0;

        var sortedObsList = obs.Select(o =>
        {
            var obsExcepts = GetValidCellsExcept(o.index);
            return GetSortedObsForIndex(o.index, obsExcepts, "");
        }).ToList();

        var lens = sortedObsList.Select(list => list.Count).ToArray();
        Debug.Log($"lens = [{string.Join(",", lens)}]");
        if (lens.Any(n => n == 0)) return 0;

        int n = obs.Count;
        int[] pos = new int[n];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < lens[i]; j++)
            {
                int tempMove = 0;

                var sob = sortedObsList[i][j];

                Debug.Log("(i,j)=" + i + "," + j);

                int indexMatch = FindMoveMatchForObstacle(sob, obs[i].index, ref tempMove);
                if (indexMatch != -1)
                {
                    int rowMatch = indexMatch / cols;   // nhớ dùng cols chứ không phải rows
                    int colMatch = indexMatch % cols;
                    int rowObs = obs[i].index / cols;
                    int colObs = obs[i].index % cols;

                    // Log 1: vị trí của indexMatch và obs
                    Debug.Log($"[MatchPos] indexMatch=({rowMatch},{colMatch}) ↔ obs=({rowObs},{colObs}) | obsIndex={obs[i].index}");

                    // Log 2: trạng thái tempMove, i, j
                    Debug.Log($"[MatchInfo] i={i}, j={j}, tempMove={tempMove}");

                    ClearObstacleAt(sob.index);
                    ClearObstacleAt(obs[i].index);

                    break;
                }
                //ResetObstacleAt();
            }
        }
        return 0;
    }
    private List<(int index, int r, int c, int value, int move)> GetSortedObsForIndex(int obsIndex, List<(int index, int r, int c, int value)> obs, string pathRemove)
    {
        int currentR = obsIndex / cols;
        int currentC = obsIndex % cols;

        var sortedObs = SortObsByMove(obs, currentR, currentC, pathRemove);

        return sortedObs;
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
    private List<(int index, int r, int c, int value, int move)> SortObsByMove(
    List<(int index, int r, int c, int value)> obs,
    int currentR, int currentC, string pathRemove)
    {
        var obsWithMove = new List<(int index, int r, int c, int value, int move)>();

        foreach (var ob in obs)
        {
            int move = GetMoveIfMatch(ob.r, ob.c, currentR, currentC, pathRemove);
            if (move >= 1)
            {
                obsWithMove.Add((ob.index, ob.r, ob.c, ob.value, move));
            }
        }

        // Sắp xếp theo move
        obsWithMove.Sort((a, b) => a.move.CompareTo(b.move));

        return obsWithMove;
    }
    private int FindMoveMatchForObstacle((int index, int r, int c, int value, int move) ob, int obsIndex, ref int sumPath)
    {
        if (!IsObstacle(ob.index)) return -1;
        int currentR = obsIndex / cols;
        int currentC = obsIndex % cols;

        Debug.Log($"[FindMoveMatchForObstacle] ob=({ob.r}, {ob.c}), obsIndex=({currentR}, {currentC})");


        int move = ob.move;
        if (move == 1)
        {
            //ClearObstacleAt(ob.index);
            sumPath++;
            return ob.index; // trực tiếp trả về khi move=1
        }
        else if (move > 1)
        {
            if (TryClearObstacle(ob.r, ob.c, currentR, currentC, ref sumPath))
            {
                sumPath++;
                return ob.index;
            }
            else
            {
                return -1;
            }
        }

        return -1;
    }
    private bool TryClearObstacle(int r1, int c1, int r2, int c2, ref int sumPath)
    {
        string pathType = GetPathType(r1, c1, r2, c2);
        var cells = GetCellsOnPathIndices(r1, c1, r2, c2, pathType, "");
        List<(int r, int c)> deductPath = new List<(int, int)>();

        // thêm điểm đầu
        deductPath.Add((r1, c1));
        deductPath.Add((r2, c2));

        // Debug cellsOnPath
        Debug.Log($"[PathInfo] Start=({r1},{c1}), End=({r2},{c2}), PathType={pathType}");
        foreach (var cell in cells)
        {
            //Duong nam tren Duong (Doc, Ngang, Cheo)
            Debug.Log($"Index: {cell.index} | Row: {cell.r} | Col: {cell.c} | Value: {board[cell.index]} | Collected: {collected[cell.index]}");
        }

        foreach (var cell in cells)
        {
            //if()

            int index = cell.index;

            if (IsObstacle(index))
            {
                bool matched = ProcessObstaclesForCell(index, deductPath, pathType);
                if (matched)
                {
                    return true;
                }

                Debug.Log("Value: " + board[index]);
            }

            // xử lý các logic khác của cell...
        }


        return false; 
    }
    private bool ProcessObstaclesForCell(int index, List<(int r, int c)> deductPath, string path)
    {
        var obs = GetValidCellsExcept(index);
        // tạo list tạm lưu obs + move
        var paths = GetSortedObsForIndex(index, obs, path); // tất cả các đường đi từ ob.index

        var pathsRemove = RemoveException(paths, deductPath);

        // sắp xếp theo minMove tăng dần
        var sortedObs = pathsRemove.OrderBy(o => o.move).ToList();

        int rowObs = index / cols;
        int colObs = index % cols;
        foreach (var ob in sortedObs)
        {
            Debug.Log($"Ob => Index: {ob.index}, Row: {ob.r}, Col: {ob.c}, Value: {ob.value}, Move: {ob.move}" +"<>Index-"+ "Row: " + rowObs + "- Col: " + colObs);
        }

        foreach (var ob in sortedObs)
        {
            Debug.Log($"Ob => Index: {ob.index}, Row: {ob.r}, Col: {ob.c}, Value: {ob.value}, Move: {ob.move}");
            int tempMove = 0;
            int indexMatch = FindMoveMatchForObstacle(ob, index, ref tempMove);
            if (indexMatch != -1)
            {
                int rowMatch = indexMatch / cols;
                int colMatch = indexMatch % cols;
                int rowObs1 = index / cols;
                int colObs1 = index % cols;

                // Log 1
                Debug.Log($"[MatchPos] indexMatch=({rowMatch},{colMatch}) ↔ obs=({rowObs1},{colObs1}) | obsIndex={index}");

                // 👉 Có match → return true để báo về vòng duyệt cell
                return true;
            }
            break;
        }

        return false; // không có match
    }
    private List<(int index, int r, int c, int value, int move)> RemoveException( //Dedect two Head
      List<(int index, int r, int c, int value, int move)> paths,
      List<(int r, int c)> removes)
    {
        var result = new List<(int, int, int, int, int)>();

        foreach (var p in paths)
        {
            // chỉ so sánh r và c
            if (!removes.Contains((p.r, p.c)))
            {
                result.Add(p);
            }
        }

        return result;
    }


    private bool IsObstacle(int index)
    {
        return !collected[index] || board[index] != 5;
    }
    private void ClearObstacleAt(int index)
    {
        clearedObstacles.Add(index); //Save Stay
        collected[index] = true; 
    }
    private void ResetObstacleAt()
    {
        foreach(int obStacle in clearedObstacles)
        {
            collected[obStacle] = false;
        }
        clearedObstacles.Clear();
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
    private int GetMoveIfMatch(int r1, int c1, int r2, int c2, string pathRemove)
    {
        // Hàm phụ lấy các chỉ số ô nằm giữa 2 điểm
        List<int> indicesBetween = GetCellsOnPathIndices(r1, c1, r2, c2, GetPathType(r1, c1, r2, c2), pathRemove)
                                    .Select(cell => cell.index).ToList();

        // Kiểm tra tất cả ô giữa có collected == true không
        bool allCollected = indicesBetween.All(index => collected[index]);

        int distance = -1;

        if (r1 == r2)
            distance = Math.Abs(c1 - c2);
        else if (c1 == c2)
            distance = Math.Abs(r1 - r2);
        else
        {
            int dr = r2 - r1;
            int dc = c2 - c1;

            if (Math.Abs(dr) == Math.Abs(dc))
                distance = Math.Abs(dr);
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
    // (Xu Ly su kien Index Nam Ngoai Path)
    private List<(int index, int value, int r, int c)> GetCellsOnPathIndices(int r1, int c1, int r2, int c2, string pathType, string pathRemove)
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

        if (pathRemove == null || pathRemove == "")
        {
            //Khong lam gi
        }

        return result;
    }
}
