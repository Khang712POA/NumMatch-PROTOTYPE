//using System;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//public class GemCollectorSolver
//{
//    private int[] board;
//    private int rows, cols;
//    public bool[] collected;
//    int totalMove = 0;
//    int collectedValue = 0;
//    int sumBestValue = 0;

//    public GemCollectorSolver(int[] board, int rows, int cols)
//    {
//        this.board = board;
//        this.rows = rows;
//        this.cols = cols;
//        collected = new bool[board.Length];
//        sumBestValue = SumBestValue();
//    }
//    private bool IsEnoughFivesCollected(int collected)
//    {
//        int total = SumBestValue();

//        int neededFives = (total / 2) * 2;

//        if (collected >= neededFives)
//        {
//            Debug.Log($"🎯 Đã thu thập đủ {neededFives} số 5, dừng lại.");
//            return true;
//        }
//        return false;
//    }
//    private int SumBestValue()
//    {
//        int count = 0;
//        for (int i = 0; i < board.Length; i++)
//        {
//            if (board[i] == 5)
//                count++;
//        }
//        return count;
//    }

//    public void Solve(int bestValue)
//    {
//        while (true)
//        {
//            var pairs = FindAllValidPairs(bestValue);
//            if (pairs.Count == 0) break;

//            var bestPair = pairs.OrderBy(p => p.move).First();

//            Debug.Log($"Cặp: ({bestPair.r1},{bestPair.c1}) ↔ ({bestPair.r2},{bestPair.c2}), move = {bestPair.move}");

//            var obstacles = FindObstacles(bestPair).Distinct().ToList();
//            obstacles = obstacles.OrderBy(o => o.move).ToList();

//            foreach (var obs in obstacles)
//            {
//                int row = obs.index / cols;
//                int col = obs.index % cols;
//                Debug.Log($"Obstacle at index {obs.value}=> row: {row}, col: {col}");
//            }

//            foreach (var obs in obstacles)
//            {
//                // Kiểm tra và tìm cặp match có thể loại bỏ vật cản obs.index
//                int matchIdx = FindMatchForObstacle(obs.index, ref totalMove);

//                if (matchIdx != -1)
//                {
//                    collected[obs.index] = true;

//                    int mr = matchIdx / cols;
//                    int mc = matchIdx % cols;

//                    totalMove += obs.move;

//                    Debug.Log($"🗑 Loại bỏ vật cản {obs.value} tại ({obs.r},{obs.c}) match với số {board[matchIdx]} tại ({mr},{mc}) move={obs.move}");
//                }
//                else
//                {
//                    Debug.LogWarning($"⚠️ Không thể loại bỏ vật cản {obs.value} tại ({obs.r},{obs.c}) vì còn vật cản khác chắn đường");
//                    return; // Bỏ qua cặp hiện tại, vì không thể loại bỏ vật cản này
//                }
//            }


//            collected[bestPair.i1] = true;
//            collected[bestPair.i2] = true;
//            totalMove += bestPair.move;
//            collectedValue = collectedValue + 2;
//            bool isEnoughFive = IsEnoughFivesCollected(collectedValue);
//            if (isEnoughFive)
//            {
//                Debug.Log("🎯 Đã thu thập đủ số 5, dừng lại.");
//                return;
//            }
//            else
//            {
//                int missing = sumBestValue - collectedValue;
//                Debug.Log($"⚠️ Thiếu {missing} số 5 để đủ cặp.");
//            }


//            Debug.Log($"✅ Match 5-5: ({bestPair.r1},{bestPair.c1}) ↔ ({bestPair.r2},{bestPair.c2}) move={bestPair.move}");
//        }

//        Debug.Log($"🎯 Hoàn thành! Tổng số lượt move: {totalMove}");
//    }

//    private int ShortestPath(int r1, int c1, int r2, int c2)
//    {
//        var obs = FindObstacles(r1, c1, r2, c2);
//        foreach (var ob in obs)
//        {

//        }
//        return 0;
//    }

//    private List<(int index, int value, int r, int c)> FindObstacles(int r1, int c1, int r2, int c2)
//    {
//        var obs = new List<(int, int, int, int)>();

//        if (r1 == r2) // ngang
//        {
//            int minC = Math.Min(c1, c2) + 1;
//            int maxC = Math.Max(c1, c2);
//            for (int c = minC; c < maxC; c++)
//            {
//                int idx = r1 * cols + c;
//                if (!collected[idx] && board[idx] != 0)
//                {
//                    obs.Add((idx, board[idx], r1, c));
//                }
//            }
//        }
//        else if (c1 == c2) // dọc
//        {
//            int minR = Math.Min(r1, r2) + 1;
//            int maxR = Math.Max(r1, r2);
//            for (int r = minR; r < maxR; r++)
//            {
//                int idx = r * cols + c1;
//                if (!collected[idx] && board[idx] != 0)
//                {
//                    obs.Add((idx, board[idx], r, c1));
//                }
//            }
//        }
//        else
//        {
//            int dr = r2 - r1;
//            int dc = c2 - c1;

//            if (Math.Abs(dr) == Math.Abs(dc)) // chéo chính hoặc chéo phụ
//            {
//                int stepR = dr > 0 ? 1 : -1;
//                int stepC = dc > 0 ? 1 : -1;
//                int steps = Math.Abs(dr);

//                for (int i = 1; i < steps; i++)
//                {
//                    int r = r1 + i * stepR;
//                    int c = c1 + i * stepC;
//                    int idx = r * cols + c;
//                    if (!collected[idx] && board[idx] != 0)
//                    {
//                        obs.Add((idx, board[idx], r, c));
//                    }
//                }
//            }
//        }
//        return obs;
//    }
//    private int FindMatchForObstacle(int obsIndex)
//    {
//        if (collected[obsIndex])
//            return -1;

//        int r = obsIndex / cols;
//        int c = obsIndex % cols;
//        int val = board[obsIndex];

//        for (int i = 0; i < board.Length; i++)
//        {
//            if (i == obsIndex || collected[i])
//                continue;

//            int r2 = i / cols;
//            int c2 = i % cols;
//            int val2 = board[i];

//            if (!IsMatch(val, val2))
//                continue;

//            int move = GetMoveIfMatch(r, c, r2, c2);
//            if (move < 0)
//                continue;

//            if (IsPathClear(r, c, r2, c2))
//            {
//                // Đường đi thông thoáng, trả về chỉ số match
//                return i;
//            }
//            else
//            {

//                bool canClear = TryClearObstacle(obsIndex, i, ref totalMove);
//                if (canClear)
//                {
//                    return i;
//                }
//            }
//        }

//        return -1;
//    }
//    // Hàm TryClearObstacle mới, dùng đệ quy DFS, tham số visited để tránh lặp vô hạn
//    private bool TryClearObstacle(int aIndex, int bIndex, ref int totalMove)
//    {
//        int r1 = aIndex / cols;
//        int c1 = aIndex % cols;
//        int r2 = bIndex / cols;
//        int c2 = bIndex % cols;

//        string pathType = GetPathType(r1, c1, r2, c2);
//        var pathCells = GetCellsOnPathIndices(r1, c1, r2, c2, pathType);
//        var valuesOnPath = pathCells.Select(idx => board[idx]).ToList();

//        foreach (var value in valuesOnPath)
//        {
//            var pairs = FindObstacles(value);
//            if (pairs.Count == 0)
//            {
//                break;
//            }
//            var bestPair = pairs.OrderBy(p => p.move).First();

//            Debug.Log($"Best Pair: i1={bestPair.i1}, r1={bestPair.r1}, c1={bestPair.c1}, i2={bestPair.i2}, r2={bestPair.r2}, c2={bestPair.c2}, move={bestPair.move}");

//            var obstacles = FindObstacles(bestPair).Distinct().ToList();
//            obstacles = obstacles.OrderBy(o => o.move).ToList();

//            foreach (var obs in obstacles)
//            {
//                int obsIdx = obs.index;

//                Debug.Log($"Obstacle at index {obs.index} => row: {obs.r}, col: {obs.c}");

//                int matchIdx = FindMatchForObstacle(obsIdx, ref totalMove);

//                if (matchIdx == -1)
//                {
//                    return false;
//                }

//                int rObs = obs.r;
//                int cObs = obs.c;
//                int rMatch = matchIdx / cols;
//                int cMatch = matchIdx % cols;

//                if (!IsPathClear(rObs, cObs, rMatch, cMatch))
//                {
//                    bool canClearSub = TryClearObstacle(obsIdx, matchIdx, ref totalMove);
//                    if (!canClearSub)
//                    {
//                        return false;
//                    }
//                }

//                collected[obsIdx] = true;
//                collected[matchIdx] = true;
//                totalMove += 1;

//                Debug.Log($"🗑 Phá vật cản {obs.value} tại ({rObs},{cObs}) bằng match {board[matchIdx]} tại ({rMatch},{cMatch})");
//            }
//            collected[bestPair.i1] = true;
//            collected[bestPair.i2] = true;
//            totalMove += bestPair.move;

//            Debug.Log($"✅ Match ({board[bestPair.i1]}-{board[bestPair.i2]}): ({bestPair.r1},{bestPair.c1}) ↔ ({bestPair.r2},{bestPair.c2}) move={bestPair.move}");

//            return true; // <-- Thêm dòng này
//        }
//        return false;
//    }
//    private bool IsMatch(int a, int b)
//    {
//        return (a == b) || (a + b == 10);
//    }

//    private int GetMoveIfMatch(int r1, int c1, int r2, int c2)
//    {
//        if (r1 == r2) return Math.Abs(c1 - c2) - 1;
//        if (c1 == c2) return Math.Abs(r1 - r2) - 1;

//        int dr = r2 - r1;
//        int dc = c2 - c1;

//        if (Math.Abs(dr) == Math.Abs(dc) && (dr * dc > 0))
//        {
//            return Math.Abs(dr) - 1;
//        }

//        if (Math.Abs(dr) == Math.Abs(dc) && (dr * dc < 0))
//        {
//            return Math.Abs(dr) - 1;
//        }

//        return -1;
//    }

//    // Kiểm tra đường đi giữa (r1,c1) và (r2,c2) có hợp lệ (theo 1 trong 3 kiểu: ngang, dọc, chéo)
//    private bool IsOnLine(int r1, int c1, int r2, int c2)
//    {
//        return (r1 == r2) || (c1 == c2) || (Math.Abs(r1 - r2) == Math.Abs(c1 - c2));
//    }

//    // Lấy loại đường đi giữa 2 ô: Horizontal, Vertical, Diagonal hoặc null nếu không hợp lệ
//    private string GetPathType(int r1, int c1, int r2, int c2)
//    {
//        if (r1 == r2)
//            return "Horizontal";
//        if (c1 == c2)
//            return "Vertical";
//        if (Math.Abs(r1 - r2) == Math.Abs(c1 - c2))
//            return "Diagonal";

//        return null; // Không phải 3 loại đường này
//    }

//    // Lấy tất cả các ô nằm giữa 2 ô (r1,c1) và (r2,c2) trên đường đi pathType
//    // Không bao gồm 2 điểm đầu cuối
//    private List<int> GetCellsOnPathIndices(int r1, int c1, int r2, int c2, string pathType)
//    {
//        var indices = new List<int>();

//        if (pathType == "Horizontal")
//        {
//            int minC = Math.Min(c1, c2) + 1;
//            int maxC = Math.Max(c1, c2) - 1;
//            for (int c = minC; c <= maxC; c++)
//            {
//                indices.Add(r1 * cols + c);
//            }
//        }
//        else if (pathType == "Vertical")
//        {
//            int minR = Math.Min(r1, r2) + 1;
//            int maxR = Math.Max(r1, r2) - 1;
//            for (int r = minR; r <= maxR; r++)
//            {
//                indices.Add(r * cols + c1);
//            }
//        }
//        else if (pathType == "Diagonal")
//        {
//            int dr = (r2 > r1) ? 1 : -1;
//            int dc = (c2 > c1) ? 1 : -1;

//            int r = r1 + dr;
//            int c = c1 + dc;
//            while (r != r2 && c != c2)
//            {
//                indices.Add(r * cols + c);
//                r += dr;
//                c += dc;
//            }
//        }

//        return indices;
//    }


//    // Kiểm tra đường đi giữa 2 ô (r1,c1) và (r2,c2) có thông suốt, không bị vật cản (collected == false)
//    private bool IsPathClear(int r1, int c1, int r2, int c2)
//    {
//        if (!IsOnLine(r1, c1, r2, c2))
//            return false;

//        string pathType = GetPathType(r1, c1, r2, c2);
//        if (pathType == null)
//            return false;

//        var betweenCells = GetCellsOnPath(r1, c1, r2, c2, pathType);

//        foreach (var (br, bc) in betweenCells)
//        {
//            int idx = br * cols + bc;
//            if (!collected[idx]) // Nếu có vật cản chưa phá
//                return false;
//        }

//        return true;
//    }
//    private List<(int r, int c)> GetCellsOnPath(int r1, int c1, int r2, int c2, string pathType)
//    {
//        var cells = new List<(int r, int c)>();

//        if (pathType == "Horizontal")
//        {
//            int minC = Math.Min(c1, c2) + 1;
//            int maxC = Math.Max(c1, c2) - 1;
//            for (int c = minC; c <= maxC; c++)
//            {
//                cells.Add((r1, c));
//            }
//        }
//        else if (pathType == "Vertical")
//        {
//            int minR = Math.Min(r1, r2) + 1;
//            int maxR = Math.Max(r1, r2) - 1;
//            for (int r = minR; r <= maxR; r++)
//            {
//                cells.Add((r, c1));
//            }
//        }
//        else if (pathType == "Diagonal")
//        {
//            int dr = (r2 > r1) ? 1 : -1;
//            int dc = (c2 > c1) ? 1 : -1;
//            int steps = Math.Abs(r2 - r1);

//            for (int i = 1; i < steps; i++)
//            {
//                int r = r1 + i * dr;
//                int c = c1 + i * dc;
//                cells.Add((r, c));
//            }
//        }

//        return cells;
//    }
//}
