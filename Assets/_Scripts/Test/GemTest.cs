using System.Linq;
using UnityEngine;

public class GemTest : MonoBehaviour
{
    [SerializeField] string input;

    [ContextMenu("TestGemCollectorSolver")]
    public void TestGemCollectorSolver()
    {
        int[] board = ConvertStringToBoard(input);
        PrintGrid(board);
        var solver = new GemCollectorSolver(board, 10, 9);
        solver.Solve();

        //var moveAlgorithm = new MoveAlgorithm(board, 5, 9);
        //moveAlgorithm.SolveAndSaveTop10("Assets/Data/output.txt");
    }
    public int[] ConvertStringToBoard(string input)
    {
        // Loại bỏ ký tự không phải số (nếu có)
        input = new string(input.Where(char.IsDigit).ToArray());

        int[] board = new int[input.Length];

        for (int i = 0; i < input.Length; i++)
        {
            board[i] = input[i] - '0'; // chuyển char số thành int
        }

        return board;
    }

    private void PrintGrid(int[] grid)
    {
        int cols = 9; // Số cột mặc định là 9
        int rows = grid.Length / cols;

        string output = "🎮 Stage Grid:\n";
        int[] counts = new int[10]; // Chỉ số từ 1 đến 9

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int value = grid[r * cols + c];
                output += value + "  ";

                if (value >= 1 && value <= 9)
                    counts[value]++;
            }
            output += "\n";
        }
        Debug.Log(output);
    }
}
