using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GemCollectorSolver
{
    private int[] board;
    private int rows, cols;
    private bool[] collected;

    public GemCollectorSolver(int[] board, int rows, int cols)
    {
        this.board = board;
        this.rows = rows;
        this.cols = cols;
        collected = new bool[board.Length];
    }

    public void Solve()
    {
        int totalMove = 0;

        while (true)
        {
            var pairs = FindAllValidPairs();
            if (pairs.Count == 0) break;

            var bestPair = pairs.OrderBy(p => p.move).First();

            var obstacles = FindObstacles(bestPair).Distinct().ToList();
            obstacles = obstacles.OrderBy(o => o.move).ToList();

            foreach (var obs in obstacles)
            {
                // Khởi tạo tập kiểm tra để tránh lặp vô hạn
                var visiting = new HashSet<int>();

                // Kiểm tra và tìm cặp match có thể loại bỏ vật cản obs.index
                int matchIdx = FindMatchForObstacle(obs.index, visiting);

                if (matchIdx != -1)
                {
                    collected[obs.index] = true;

                    int mr = matchIdx / cols;
                    int mc = matchIdx % cols;

                    totalMove += obs.move;

                    Debug.Log($"🗑 Loại bỏ vật cản {obs.value} tại ({obs.r},{obs.c}) match với số {board[matchIdx]} tại ({mr},{mc}) move={obs.move}");
                }
                else
                {
                    Debug.LogWarning($"⚠️ Không thể loại bỏ vật cản {obs.value} tại ({obs.r},{obs.c}) vì còn vật cản khác chắn đường");
                    return; // Bỏ qua cặp hiện tại, vì không thể loại bỏ vật cản này
                }
            }


            collected[bestPair.i1] = true;
            collected[bestPair.i2] = true;
            totalMove += bestPair.move;

            Debug.Log($"✅ Match 5-5: ({bestPair.r1},{bestPair.c1}) ↔ ({bestPair.r2},{bestPair.c2}) move={bestPair.move}");
        }

        Debug.Log($"🎯 Hoàn thành! Tổng số lượt move: {totalMove}");
    }

    private List<(int i1, int r1, int c1, int i2, int r2, int c2, int move)> FindAllValidPairs()
    {
        var result = new List<(int, int, int, int, int, int, int)>();

        for (int i1 = 0; i1 < board.Length; i1++)
        {
            if (collected[i1] || board[i1] != 5) continue;

            int r1 = i1 / cols;
            int c1 = i1 % cols;

            for (int i2 = i1 + 1; i2 < board.Length; i2++)
            {
                if (collected[i2] || board[i2] != 5) continue;

                int r2 = i2 / cols;
                int c2 = i2 % cols;

                int move = GetMoveIfMatch(r1, c1, r2, c2);
                if (move >= 0)
                {
                    result.Add((i1, r1, c1, i2, r2, c2, move));
                }
            }
        }

        return result;
    }

    private List<(int index, int value, int r, int c, int move)> FindObstacles((int i1, int r1, int c1, int i2, int r2, int c2, int move) pair)
    {
        var obs = new List<(int, int, int, int, int)>();

        if (pair.r1 == pair.r2) // ngang
        {
            int minC = Math.Min(pair.c1, pair.c2) + 1;
            int maxC = Math.Max(pair.c1, pair.c2);
            for (int c = minC; c < maxC; c++)
            {
                int idx = pair.r1 * cols + c;
                if (!collected[idx] && board[idx] != 0)
                {
                    obs.Add((idx, board[idx], pair.r1, c, Math.Abs(c - pair.c1)));
                }
            }
        }
        else if (pair.c1 == pair.c2) // dọc
        {
            int minR = Math.Min(pair.r1, pair.r2) + 1;
            int maxR = Math.Max(pair.r1, pair.r2);
            for (int r = minR; r < maxR; r++)
            {
                int idx = r * cols + pair.c1;
                if (!collected[idx] && board[idx] != 0)
                {
                    obs.Add((idx, board[idx], r, pair.c1, Math.Abs(r - pair.r1)));
                }
            }
        }
        else
        {
            int dr = pair.r2 - pair.r1;
            int dc = pair.c2 - pair.c1;

            if (Math.Abs(dr) == Math.Abs(dc)) // chéo chính hoặc chéo phụ
            {
                int stepR = dr > 0 ? 1 : -1;
                int stepC = dc > 0 ? 1 : -1;
                int steps = Math.Abs(dr);

                for (int i = 1; i < steps; i++)
                {
                    int r = pair.r1 + i * stepR;
                    int c = pair.c1 + i * stepC;
                    int idx = r * cols + c;
                    if (!collected[idx] && board[idx] != 0)
                    {
                        obs.Add((idx, board[idx], r, c, i));
                    }
                }
            }
        }

        return obs;
    }

    private List<(int index, int value, int r, int c, int move)> FindObstaclesOnPath(int r1, int c1, int r2, int c2)
    {
        var obs = new List<(int, int, int, int, int)>();

        if (r1 == r2) // ngang
        {
            int minC = Math.Min(c1, c2) + 1;
            int maxC = Math.Max(c1, c2);
            for (int c = minC; c < maxC; c++)
            {
                int idx = r1 * cols + c;
                if (!collected[idx] && board[idx] != 0)
                {
                    obs.Add((idx, board[idx], r1, c, Math.Abs(c - c1)));
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
                    obs.Add((idx, board[idx], r, c1, Math.Abs(r - r1)));
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
                        obs.Add((idx, board[idx], r, c, i));
                    }
                }
            }
        }

        return obs;
    }

    private int FindMatchForObstacle(int obsIndex, HashSet<int> visiting)
    {
        if (collected[obsIndex]) return -1;
        if (visiting.Contains(obsIndex))
            return -1;

        visiting.Add(obsIndex);

        int r = obsIndex / cols;
        int c = obsIndex % cols;
        int val = board[obsIndex];

        for (int i = 0; i < board.Length; i++)
        {
            if (i == obsIndex || collected[i]) continue;

            int r2 = i / cols;
            int c2 = i % cols;
            int val2 = board[i];

            if (IsMatch(val, val2) && IsOnLine(r, c, r2, c2))
            {
                int move = GetMoveIfMatch(r, c, r2, c2);
                if (move >= 0)
                {
                    var obstacles = FindObstaclesOnPath(r, c, r2, c2);

                    bool canRemoveAll = true;

                    // Sắp xếp vật cản theo vị trí trên đường đi (từ gần đến xa) để loại bỏ đúng thứ tự
                    obstacles = obstacles.OrderBy(o => GetDistanceAlongPath(r, c, o.r, o.c)).ToList();

                    foreach (var obs in obstacles)
                    {
                        // Đệ quy kiểm tra vật cản con, phải loại bỏ trước vật cản chính
                        if (!CanRemoveObstacle(obs.index, visiting))
                        {
                            canRemoveAll = false;
                            break;
                        }
                    }

                    if (canRemoveAll)
                    {
                        visiting.Remove(obsIndex);
                        return i;
                    }
                }
            }
        }

        visiting.Remove(obsIndex);
        return -1;
    }
    // Hàm phụ tính khoảng cách điểm (r1,c1) đến (r2,c2) trên đường đi (để sắp xếp vật cản đúng thứ tự)
    private int GetDistanceAlongPath(int r1, int c1, int r2, int c2)
    {
        if (r1 == r2)
            return Math.Abs(c2 - c1);
        if (c1 == c2)
            return Math.Abs(r2 - r1);

        int dr = r2 - r1;
        int dc = c2 - c1;

        if (Math.Abs(dr) == Math.Abs(dc))
            return Math.Abs(dr);

        // Trường hợp không thẳng hàng (nếu có) trả về lớn để ưu tiên thấp
        return int.MaxValue;
    }
    private bool CanRemoveObstacle(int obsIndex, HashSet<int> visiting)
    {
        int matchIdx = FindMatchForObstacle(obsIndex, visiting);
        return matchIdx != -1;
    }
    private bool IsMatch(int a, int b)
    {
        return (a == b) || (a + b == 10);
    }

    private bool IsOnLine(int r1, int c1, int r2, int c2)
    {
        if (r1 == r2) return true;
        if (c1 == c2) return true;
        int dr = r2 - r1;
        int dc = c2 - c1;
        if (Math.Abs(dr) == Math.Abs(dc)) return true;
        return false;
    }

    private int GetMoveIfMatch(int r1, int c1, int r2, int c2)
    {
        if (r1 == r2) return Math.Abs(c1 - c2) - 1;
        if (c1 == c2) return Math.Abs(r1 - r2) - 1;

        int dr = r2 - r1;
        int dc = c2 - c1;

        if (Math.Abs(dr) == Math.Abs(dc) && (dr * dc > 0))
        {
            return Math.Abs(dr) - 1;
        }

        if (Math.Abs(dr) == Math.Abs(dc) && (dr * dc < 0))
        {
            return Math.Abs(dr) - 1;
        }

        return -1;
    }
}
