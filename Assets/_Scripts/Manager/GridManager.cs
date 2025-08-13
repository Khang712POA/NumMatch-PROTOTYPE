using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : SaiMonoBehaviour
{
    [Header("Singleton Instance")]
    private static GridManager instance;
    public static GridManager Instance => instance;

    [Header("Grid Info")]
    [SerializeField] private int lastRow;
    [SerializeField] private int numberTile;
    [SerializeField] private Sprite[] imageNumbers = new Sprite[9];
    private bool hasSetCurrentStageNext = false;

    [Space(2)]
    [Header("Tile Collections")]
    public List<TileNumber> tileNumbers;
    public List<TileNumber> tileNumbersShake;
    [SerializeField] private List<TileNumber> selectedTiles = new List<TileNumber>();
    public List<TileNumber> SelectedTiles => selectedTiles;
    private EquationType equationType = EquationType.None;


    [Space(2)]
    [Header("UI References")]
    [SerializeField] private RectTransform holderTile;
    [SerializeField] private GameObject tilePrefab; // Đổi từ TitlePrefab
    [SerializeField] private UILineConnector uiLineConnector;

    [Space(2)]
    [Header("Other Managers")]
    [SerializeField] private GridBoardCopy gridBoardCopy;
    [SerializeField] private MatchGenerator matchGenerator;
    [SerializeField] private GemSpawner gemSpawner;

    protected override void Awake()
    {
        base.Awake();
        if (instance != null)
        {
            Debug.LogError("Only 1 GridManager instance allowed!");
            return;
        }
        instance = this;
        numberTile = BoardConfig.COUNTTILE_DEFAULT;
    }
    protected override void Update()
    {
        if (Input.GetKeyDown(KeyCode.F)) //Find Match
        {
            // Lọc các tile có Value khác -1 và gọi hàm
            var filteredTiles = tileNumbers.Where(tile => tile.Value != -1).ToList();
            FindAndCheckMatches(filteredTiles);
        }
    }

    protected override void Start()
    {
        StartNewStage(GamePlayManager.Instance.CurrentStage, GamePlayManager.Instance.GameMode);
    }

    protected override void LoadComponents()
    {
        base.LoadComponents();
        this.LoadImageNumbers();
    }

    protected virtual void LoadImageNumbers()
    {
        if (imageNumbers.Length > 0) return;

        imageNumbers = Resources.LoadAll<Sprite>("Sprites/number");

        if (imageNumbers.Length == 0)
        {
            Debug.LogWarning("Không tìm thấy ảnh trong Resources/_Sprites/number");
        }
    }
    public void StartNewStage(int stage, GameMode gameMode)
    {
        CreateDisplay();
        tileNumbersShake.Clear();

        GamePlayManager.Instance.ResetNumberGenerate();
        UIManager.Instance.ResetTextGenerate();

        SetCurrentStageNextOnce(stage);

        int[] newData = matchGenerator.GenerateStage(GamePlayManager.Instance.CurrentStage); //Array Play

        matchGenerator.PrintGrid(newData); //Log
        matchGenerator.FindAndCheckMatches(newData); //Find Count Match

        SetTilesImages(stage, newData); //SetImage

        lastRow = GetLastRowWithValidTile();

        if (GamePlayManager.Instance.CurrentStage > GamePlayManager.Instance.CurrentStageNext)
        {
            AnimationGenerateCoroutine(0, 3);
            StartCoroutine(SetNumberTilesSequentially(0, newData.Length));
        }

        if (gameMode == GameMode.Gem)
        {
            SetupGemsForStage(stage);
        }
    }
    private void CreateDisplay()
    {
        tileNumbers.Clear();

        foreach (Transform child in holderTile)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < numberTile; i++)
        {
            var obj = Instantiate(tilePrefab, holderTile);

            int row = i / BoardConfig.ColumnCount;
            int col = i % BoardConfig.ColumnCount;

            obj.name = $"Row{row}_Col{col}";

            TileNumber tileNumber = obj.GetComponent<TileNumber>();
            tileNumber.SetNullImage(i);
            tileNumbers.Add(tileNumber);
        }

    }
    private void OnCheckLoseGame()
    {
        int firstEmptyRow = GetFirstEmptyRow();

        bool hasRemainingMatches = FindAndCheckMatches(
            tileNumbers.Where(tile => tile.Value != -1).ToList()
        );
        if(GamePlayManager.Instance.GameMode == GameMode.Gem) //Clear Gem
        {
            if(GamePlayManager.Instance.AreAllGemsDepleted())
            {
                return;
            }
        }

        if (hasRemainingMatches || GamePlayManager.Instance.NumberGenerate >= 1) //&& Gem
            return;

        gridBoardCopy.CopyTile(tileNumbers);
        UIManager.Instance.ActiveUILose();
        UIManager.Instance.ResetTextGenerate();
    }
    private void OnCheckWinGame()
    {
        if (GamePlayManager.Instance.GameMode == GameMode.Gem && GamePlayManager.Instance.AreAllGemsDepleted())
        {
            UIManager.Instance.ActiveUIWin();
            return;
        }

        if (tileNumbers.Any(tile => tile.IsInPlay)) return;

        StageCompeteAnimationCoroutine(() =>
        {
            GamePlayManager.Instance.IncreaseNumberAdd();
            StartNewStage(GamePlayManager.Instance.CurrentStage, GamePlayManager.Instance.GameMode);
            UIManager.Instance.UpdateTextStage(GamePlayManager.Instance.CurrentStage);
            UIManager.Instance.DeactiveStageCompleteAnimation();
        });
    }

    private bool CheckPointX2()
    {
        return UnityEngine.Random.value < 0.05f;
    }
    private void SetupGemsForStage(int stage)
    {
        GamePlayManager.Instance.LoadGemData(stage);

        var gemTypes = GamePlayManager.Instance.CurrentGemTypes
            .Select(g => new GemComponent(g.GemType, g.Count))
            .ToList();

        gemSpawner.SetAvailableGemTypes(gemTypes);

        UIManager.Instance.LoadUIGemTop();
    }
    public void OnTileSelected(TileNumber tile)
    {
        if (tile == null || selectedTiles.Contains(tile))
            return;

        selectedTiles.Add(tile);

        if (selectedTiles.Count != 2)
            return;

        var tileA = selectedTiles[0];
        var tileB = selectedTiles[1];

        bool isValidValue = (tileA.Value + tileB.Value == 10) || (tileA.Value == tileB.Value);
        bool isValidLine = IsClearPath(tileA, tileB, true);

        if (isValidValue && isValidLine)
        {
            HandleValidSelection(tileA, tileB);
        }
        else if (isValidValue && !isValidLine)
        {
            HandleInvalidLine(tileA, tileB);
        }
        else
        {
            HandleInvalidValue(tileA);
        }

        tileNumbersShake.Clear();
    }
    private void SetCurrentStageNextOnce(int currentStage)
    {
        if (!hasSetCurrentStageNext)
        {
            GamePlayManager.Instance.SetCurrentStageNext(currentStage);
            hasSetCurrentStageNext = true;
        }
    }
    #region Animations & Effects
    private IEnumerator SetNumberTilesSequentially(int indexStart, int indexEnd)
    {
        for (int i = indexStart; i < indexEnd; i++)
        {
            var targetTile = tileNumbers[i];

            targetTile.InitButton();

            if (i == indexStart)
            {
                yield return new WaitForSeconds(1.1f); // Đợi 1.2s sau ô đầu tiên

                targetTile.NumberImage.gameObject.SetActive(true);
                targetTile.NumberImage.color = new Color32(30, 91, 102, 255);
            }
            else
            {
                targetTile.NumberImage.gameObject.SetActive(true);
                targetTile.NumberImage.color = new Color32(30, 91, 102, 255);

                yield return new WaitForSeconds(0.08f); // Đợi 0.1s giữa các ô còn lại
            }
        }
    }
    private IEnumerator AnimateByRowAndColumn(int rowStart, int rowEnd)
    {
        yield return new WaitForSeconds(0.7f); // Number Spin delay

        for (int r = rowStart; r < rowEnd; r++)
        {
            yield return new WaitForSeconds((r - rowStart) * 0.55f);

            for (int c = 0; c < BoardConfig.ColumnCount; c++)
            {
                var tile = GetTile(r, c);
                if (tile == null) continue;

                float delay = c switch
                {
                    0 => 0f,
                    1 => 0f,
                    _ => 0.1f * c
                };

                StartCoroutine(AnimateTileWithDelay(tile, delay));

                if (c == 1)
                {
                    tile.DeselectBackground();
                    StartCoroutine(EnableBackgroundAfterDelay(tile, 0.1f));
                }
            }
        }
    }
    private IEnumerator EnableBackgroundAfterDelay(TileNumber tile, float delay)
    {
        yield return new WaitForSeconds(delay);
        tile.SelectBackground(); // hoặc phương thức bạn dùng để hiện ảnh lại
    }
    private IEnumerator AnimateTileWithDelay(TileNumber tile, float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        bool finished = false;
        tile.StartAnimateScaleCoroutine(() => finished = true);
        yield return new WaitUntil(() => finished);
    }
    private IEnumerator StageCompeteAnimation(Action onComplete)
    {
        UIManager.Instance.ActiveStageCompleteAnimation();

        yield return new WaitForSeconds(1.8f);

        if (CheckPointX2())
        {

            yield return new WaitForSeconds(0.5f);
            onComplete?.Invoke();

        }
        else
        {
            onComplete?.Invoke();
        }
    }
    private IEnumerator RemoveRowAndShiftDown(int rowStart, int indexRemove, Action callback)
    {
        yield return new WaitForSeconds(1.3f);

        int totalRows = tileNumbers.Count / BoardConfig.ColumnCount;

        for (int row = rowStart; row < totalRows; row++)
        {
            for (int col = 0; col < BoardConfig.ColumnCount; col++)
            {
                TileNumber tile = GetTile(row, col);
                TileNumber upperTile = GetTile(row + indexRemove, col);

                if (upperTile != null && upperTile.Value != -1)
                {
                    tile.CopyUpperTile(upperTile);
                    tile.MoveNumberImageUp(0.3f, indexRemove);
                }
                else
                {
                    tile.SetRemoveTile();
                }
            }
        }
        callback?.Invoke();
    }
    private IEnumerator RemoveRowCoroutine(int indexRowRemove, int rowLast, TileNumber tileA, TileNumber tileB, bool rowAClear, bool rowBClear, Action onComplete)
    {
        Debug.Log("Remove Row");

        int rowA = tileA.Row;
        int rowB = tileB.Row;
        int minRow = Mathf.Min(rowA, rowB);
        int maxRow = Mathf.Max(rowA, rowB);
        int indexRow = Mathf.Abs(rowA - rowB);

        bool isRowAGreater = rowA > rowB;
        bool isRowBGreater = rowB > rowA;
        int rowLastWithOffset = rowLast + indexRowRemove;

        // Helper gọi hàm xóa và gọi onComplete
        void RemoveAndComplete(int startRow, int count)
        {
            RemoveRowAndShiftDownCoroutine(startRow, count, () =>
            {
                RemoveRowIndex(count, rowLast);
                onComplete?.Invoke();
            });
        }

        // TH1 & TH2 dạng đặc biệt
        if ((maxRow == rowLastWithOffset && indexRowRemove == 1) ||
            (maxRow == rowLastWithOffset && indexRowRemove == 2 && indexRow == 1))
        {
            onComplete?.Invoke();
            yield break;
        }

        if (indexRow == 1 && indexRowRemove == 2)
        {
            if (minRow + indexRowRemove != rowLastWithOffset + 1)
            {
                // TH2 Type 1
                RemoveAndComplete(minRow, indexRowRemove);
                yield break;
            }
            else if (maxRow + indexRowRemove == rowLastWithOffset)
            {
                yield break;
            }
        }

        if (indexRowRemove == 2 && indexRow > 1)
        {
            if (maxRow + 1 == rowLastWithOffset && isRowBGreater)
            {
                RemoveAndComplete(rowA, 1);
                yield break;
            }
            if (maxRow + 1 == rowLastWithOffset && isRowAGreater)
            {
                RemoveAndComplete(rowB, 1);
                yield break;
            }

            if (isRowAGreater)
            {
                RemoveRowAndShiftDownBreakCoroutine(rowB, rowA - 1, 1, () =>
                {
                    RemoveRowsInRange(rowB, rowA - 1);
                });
                RemoveAndComplete(rowA - 1, indexRowRemove);
                yield break;
            }

            if (isRowBGreater)
            {
                RemoveRowAndShiftDownBreakCoroutine(rowA, rowB - 1, 1, () =>
                {
                    RemoveRowsInRange(rowA, rowB - 1);
                });
                RemoveAndComplete(rowB - 1, indexRowRemove);
                yield break;
            }
        }

        // Các trường hợp còn lại indexRowRemove == 1 hoặc rowAClear/rowBClear xử lý tương tự
        if (indexRowRemove == 1)
        {
            if (rowAClear)
            {
                RemoveAndComplete(rowA, indexRowRemove);
                yield break;
            }

            if (rowBClear)
            {
                RemoveAndComplete(rowB, indexRowRemove);
                yield break;
            }
        }

        // Mặc định xóa rowA
        RemoveAndComplete(rowA, indexRowRemove);
    }
    private IEnumerator RemoveRowAndShiftDownBreak(int rowStart, int rowEnd, int indexRemove, Action callback)
    {
        yield return new WaitForSeconds(1.3f);

        for (int row = rowStart; row < rowEnd; row++)
        {
            for (int col = 0; col < BoardConfig.ColumnCount; col++)
            {
                TileNumber tile = GetTile(row, col);
                TileNumber upperTile = GetTile(row + indexRemove, col);

                if (upperTile != null && upperTile.Value != -1)
                {
                    tile.CopyUpperTile(upperTile);
                    tile.MoveNumberImageUp(0.3f, indexRemove);
                }
            }
        }
        callback?.Invoke();
    }
    private IEnumerator TriggerAnimationRemoveSequence(int row)
    {
        for (int col = 0; col < BoardConfig.ColumnCount; col++)
        {
            TileNumber tile = GetTile(row, col);

            tile.OnTileRemoveAnimation();

            yield return new WaitForSeconds(0.1f);

            tile.FadeImage(tile.CircleRemove);
        }
    }
    private void StageCompeteAnimationCoroutine(Action onComplete)
    {
        StartCoroutine(StageCompeteAnimation(onComplete));
    }
    private void RemoveRowAndShiftDownCoroutine(int rowStart, int indexRowRemove, Action action)
    {
        StartCoroutine(RemoveRowAndShiftDown(rowStart, indexRowRemove, action));
    }
    private void RemoveRowAndShiftDownBreakCoroutine(int rowStart, int rowEnd, int indexRowRemove, Action action)
    {
        StartCoroutine(RemoveRowAndShiftDownBreak(rowStart, rowEnd, indexRowRemove, action));
    }
    private void AnimationGenerateCoroutine(int rowStart, int rowEnd)
    {
        StartCoroutine(AnimateByRowAndColumn(rowStart, rowEnd));
    }
    private void TriggerAnimationCoroutine(int row)
    {
        StartCoroutine(TriggerAnimationRemoveSequence(row));
    }
    public void AnimationGenerate()
    {
        int totalRows = tileNumbers.Count / BoardConfig.ColumnCount;

        for (int row = 0; row < totalRows; row++)
        {
            for (int col = 0; col < BoardConfig.ColumnCount; col++)
            {
                TileNumber tile = GetTile(row, col);
                if (tile.IsMatch) continue;
                if (tile.Value == -1) return;
                tile.SpinCircle();
            }
        }
    }

    #endregion
    private void SetTilesImages(int stage, int[] newData)
    {
        for (int i = 0; i < newData.Length; i++)
        {
            bool activateAnimation = (stage == 1);
            SetTileImage(tileNumbers[i], newData[i], i, activateAnimation);

            if (activateAnimation)
            {
                tileNumbers[i].gameObject.SetActive(true);
            }
        }
    }
    private void SetTileImage(TileNumber tile, int value, int index, bool activeAnimation)
    {
        if (value > 0 && value <= imageNumbers.Length)
        {
            tile.SetTileNumber(index, value, imageNumbers[value - 1], activeAnimation);
        }
    }
    private int GetFirstEmptyRow()
    {
        foreach (var tile in tileNumbers)
        {
            if (tile?.Value == -1)
                return tile.Row;
        }
        return -1;
    }
    private int GetFirstEmptyIndex()
    {
        foreach (var tile in tileNumbers)
        {
            if (tile?.Value == -1)
                return tile.Index;
        }
        return -1;
    }
    private int GetLastRowWithValidTile()
    {
        int totalRows = tileNumbers.Count / BoardConfig.ColumnCount;

        for (int row = totalRows - 1; row >= 0; row--)
        {
            for (int col = 0; col < BoardConfig.ColumnCount; col++)
            {
                TileNumber tile = GetTile(row, col);
                if (tile != null && tile.Value != -1)
                {
                    return row;
                }
            }
        }
        return -1; // Không có hàng nào hợp lệ
    }

    private TileNumber GetTile(int row, int col)
    {
        int index = row * BoardConfig.ColumnCount + col;
        if (index >= 0 && index < tileNumbers.Count)
            return tileNumbers[index];
        return null;
    }
    private EquationType GetEquationType(TileNumber a, TileNumber b)
    {
        if (a == null || b == null) return EquationType.None;
        if (a.Row == b.Row)
        {
            return EquationType.StraighType1;
        }
        return EquationType.None;
    }

    private void HandleValidSelection(TileNumber tileA, TileNumber tileB)
    {
        equationType = GetEquationType(tileA, tileB);

        int rowA = tileA.Row;
        int rowB = tileB.Row;
        int colA = tileA.Col;
        int colB = tileB.Col;

        int rowDiff = Mathf.Abs(rowA - rowB);
        int colDiff = Mathf.Abs(colA - colB);
        int minRow = Mathf.Min(rowA, rowB);
        int maxRow = Mathf.Max(rowA, rowB);
        int minCol = Mathf.Min(colA, colB);
        int maxCol = Mathf.Max(colA, colB);

        uiLineConnector.StartDrawing(rowDiff, colDiff, minRow, maxRow, minCol, maxCol, rowA, rowB, equationType);

        GamePlayManager.Instance.DeDuctAvailableGemTypes(tileA.GemType, tileA.IsGem);
        GamePlayManager.Instance.DeDuctAvailableGemTypes(tileB.GemType, tileB.IsGem);

        tileA.ClearImage();
        tileB.ClearImage();

        bool rowACleared = false;
        bool rowBCleared = false;
        int clearedRowsCount = 0;

        if (AreRowsMatched(tileA, tileB))
        {
            if (rowA == rowB)
            {
                if (IsRowFullyMatched(rowA))
                {
                    rowACleared = true;
                    HandleRowClear(rowA, UIManager.Instance.AnimationRemoveColumn1);
                    clearedRowsCount++;
                }
            }
            else
            {
                if (IsRowFullyMatched(rowA))
                {
                    rowACleared = true;
                    HandleRowClear(rowA, UIManager.Instance.AnimationRemoveColumn1);
                    clearedRowsCount++;
                }
                if (IsRowFullyMatched(rowB))
                {
                    rowBCleared = true;
                    HandleRowClear(rowB, UIManager.Instance.AnimationRemoveColumn2);
                    clearedRowsCount++;
                }
            }

            AudioManager.Instance.PlaySFX("sfx_row_clear");
            this.lastRow = GetLastRowWithValidTile();

            StartCoroutine(RemoveRowCoroutine(clearedRowsCount, lastRow, tileA, tileB, rowACleared, rowBCleared, () =>
            {
                OnCheckLoseGame();
            }));

        }
        else
        {
            OnCheckLoseGame();
        }

        AudioManager.Instance.PlaySFX("sfx_pair_clear");

        OnCheckWinGame();

        selectedTiles.Clear();
    }
    private void HandleInvalidLine(TileNumber tileA, TileNumber tileB)
    {
        tileA.DeselectBackground();
        tileB.DeselectBackground();

        foreach (var t in tileNumbersShake)
            t.ObjectShake.ShakeAndRecover();

        selectedTiles.Clear();
    }
    private void HandleInvalidValue(TileNumber tileA)
    {
        tileA.DeselectBackground();
        selectedTiles.RemoveAt(0);
    }
    private void HandleRowClear(int row, RectTransform animationObj)
    {
        Debug.Log($"[Clear Row] {row}");
        animationObj.gameObject.SetActive(true);
        Animator animator = animationObj.GetComponent<Animator>();
        animator.Play(animator.GetCurrentAnimatorStateInfo(0).shortNameHash, 0, 0f);
        animationObj.anchoredPosition = new Vector2(-500f, 565 - row * 112 /*- contTent.anchoredPosition.y*/);

        FadeNumberAndSetNull(row);
        TriggerAnimationCoroutine(row);
    }
    private void RemoveRowIndex(int rowIndex, int lastRow)
    {
        for (int row = lastRow; row > lastRow - rowIndex; row--)
        {
            for (int col = 0; col < BoardConfig.ColumnCount; col++)
            {
                TileNumber tile = GetTile(row, col);
                if (tile != null && tile.Value != -1)
                {
                    tile.SetRemoveTile();
                }
            }
        }
    }
    private void RemoveRowsInRange(int startRow, int lastEnd)
    {
        for (int row = lastEnd; row > startRow + 1; row--)
        {
            for (int col = 0; col < BoardConfig.ColumnCount; col++)
            {
                TileNumber tile = GetTile(row, col);
                if (tile != null && tile.Value != -1)
                {
                    tile.SetRemoveTile();
                    tile.SetValueNull();
                }
            }
        }
    }
    private bool AreRowsMatched(TileNumber tileA, TileNumber tileB)
    {
        int rowA = tileA.Row;
        int rowB = tileB.Row;

        bool resultA = IsRowFullyMatched(rowA);
        bool resultB = IsRowFullyMatched(rowB);

        return resultA || resultB;
    }
    private bool IsRowFullyMatched(int row)
    {
        for (int col = 0; col < BoardConfig.ColumnCount; col++)
        {
            TileNumber tile = GetTile(row, col);
            if (!tile.IsMatch && tile.IsInPlay)
            {
                return false;
            }
        }
        return true;
    }
    private void FadeNumberAndSetNull(int row)
    {
        for (int col = 0; col < BoardConfig.ColumnCount; col++)
        {
            TileNumber tile = GetTile(row, col);

            tile.FadeImage(tile.NumberImage);
            tile.SetValueNull();
        }
    }
    private bool FindAndCheckMatches(List<TileNumber> tileNumbers)
    {
        bool hasMatch = false;
        int countMatch = 0;
        HashSet<int> matched = new HashSet<int>();

        for (int i = 0; i < tileNumbers.Count; i++)
        {
            TileNumber a = tileNumbers[i];
            if (a == null || a.Value == 0 || a.IsMatch || matched.Contains(i)) continue;

            for (int j = 0; j < tileNumbers.Count; j++)
            {
                if (i == j) continue;

                TileNumber b = tileNumbers[j];
                if (b == null || b.Value == 0 || b.IsMatch || matched.Contains(j)) continue;

                bool isSame = a.Value == b.Value;
                bool isSumTen = a.Value + b.Value == 10;

                if (!isSame && !isSumTen) continue;

                // Kiểm tra đường đi giữa 2 tile
                if (IsClearPath(a, b, false))
                {
                    matched.Add(i);
                    matched.Add(j);
                    countMatch++;
                    hasMatch = true;

                    Debug.Log($"✅ Match: {a.Value} ({a.Row}, {a.Col}) <-> {b.Value} ({b.Row}, {b.Col}) :" + " ⏰ Thời gian: " + DateTime.Now.ToString("HH:mm:ss.fff"));

                    break; // không cần tìm thêm j cho i
                }
            }
        }

        if (!hasMatch)
        {
            Debug.Log("❌ Không tìm thấy cặp nào.");
        }
        else
        {
            Debug.Log("🔍 Tổng cặp match được: " + countMatch + " ⏰ Thời gian: " + DateTime.Now.ToString("HH:mm:ss.fff"));
        }
        return hasMatch;
    }
    private bool IsClearPath(TileNumber a, TileNumber b, bool Shake)
    {
        int rowA = a.Row, colA = a.Col;
        int rowB = b.Row, colB = b.Col;
        int dr = rowB - rowA, dc = colB - colA;

        bool checkMatch = true;

        if (dr == 0)
        {
            for (int c = Mathf.Min(colA, colB) + 1; c < Mathf.Max(colA, colB); c++)
                CheckTile(rowA, c, ref checkMatch, Shake);
            return checkMatch;
        }

        if (dc == 0)
        {
            for (int r = Mathf.Min(rowA, rowB) + 1; r < Mathf.Max(rowA, rowB); r++)
                CheckTile(r, colA, ref checkMatch, Shake);
            return checkMatch;
        }

        if (Mathf.Abs(dr) == Mathf.Abs(dc))
        {
            int steps = Mathf.Abs(dr), stepR = dr > 0 ? 1 : -1, stepC = dc > 0 ? 1 : -1;
            for (int i = 1; i < steps; i++)
                CheckTile(rowA + i * stepR, colA + i * stepC, ref checkMatch, Shake);
            return checkMatch;
        }

        int columns = BoardConfig.ColumnCount;
        if (rowA < rowB)
        {
            for (int c = colA + 1; c < columns; c++) CheckTile(rowA, c, ref checkMatch, Shake);
            for (int c = 0; c < colB; c++) CheckTile(rowB, c, ref checkMatch, Shake);
            if (rowB - rowA > 1)
            {
                for (int r = rowA + 1; r < rowB; r++)
                {
                    for (int c = 0; c < columns; c++)
                        CheckTile(r, c, ref checkMatch, Shake);
                }
            }
            return checkMatch;
        }
        else if (rowA > rowB)
        {
            for (int c = colB + 1; c < columns; c++) CheckTile(rowB, c, ref checkMatch, Shake);
            for (int c = colA - 1; c >= 0; c--) CheckTile(rowA, c, ref checkMatch, Shake);
            if (rowA - rowB > 1)
            {
                for (int r = rowB + 1; r < rowA; r++)
                {
                    for (int c = 0; c < columns; c++)
                        CheckTile(r, c, ref checkMatch, Shake);
                }
            }
            return checkMatch;
        }

        return false;
    }
    private void CheckTile(int row, int col, ref bool checkMatch, bool Shake)
    {
        TileNumber tile = GetTile(row, col);
        if (tile != null && tile.gameObject.activeSelf && !tile.IsMatch)
        {
            if (Shake) tileNumbersShake.Add(tile);
            checkMatch = false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [ContextMenu("Next Stage")]
    public void NextStage()
    {
        for (int row = 0; row < tileNumbers.Count / BoardConfig.ColumnCount; row++)
        {
            for (int col = 0; col < BoardConfig.ColumnCount; col++)
            {
                TileNumber tile = GetTile(row, col);
                if (tile.Value != -1)
                {
                    tile.SetRemoveTile();
                }
            }
        }
        GamePlayManager.Instance.ResetAllGemCounts();

        this.OnCheckWinGame();
    }
    [ContextMenu("Clone Remaining Tiles to Bottom")]
    public void CloneRemainingTilesToBottom()
    {
        var remainingTiles = tileNumbers
            .Where(tile => tile.Value != -1 && !tile.IsMatch)
            .ToList();

        int indexStart = GetFirstEmptyIndex();
        int indexEnd = indexStart + remainingTiles.Count;

        if (indexEnd > tileNumbers.Count)
        {
            CreateGenerate(indexEnd - tileNumbers.Count, indexEnd);
        }

        for (int offset = 0; offset < remainingTiles.Count; offset++)
        {
            int targetIndex = indexStart + offset;

            if (targetIndex >= tileNumbers.Count || tileNumbers[targetIndex] == null)
            {
                Debug.LogWarning($"tileNumbers[{targetIndex}] is null or out of range.");
                continue;
            }

            var targetTile = tileNumbers[targetIndex];
            var sourceTile = remainingTiles[offset];
            targetTile.SetTileNumber(targetIndex, sourceTile.Value, sourceTile.NumberImage.sprite, false);
        }

        if (GamePlayManager.Instance.GameMode == GameMode.Gem)
        {
            var tilesToSpawnGems = tileNumbers
                .Skip(indexStart)
                .Take(indexEnd - indexStart)
                .Where(t => t != null)
                .ToList();

            gemSpawner.GenerateGemsInSpawn(tilesToSpawnGems);
        }

        StartCoroutine(CheckLoseGame(indexStart, indexEnd, () =>
        {
            Debug.Log("Done checking lose game!");
            OnCheckLoseGame();
        }));

        tileNumbersShake.Clear();
    }
    private IEnumerator CheckLoseGame(int indexStart, int indexEnd, Action onComplete)
    {
        int startRow = indexStart / BoardConfig.ColumnCount;
        int endRow = indexEnd / BoardConfig.ColumnCount;

        AnimationGenerateCoroutine(startRow, endRow);

        yield return StartCoroutine(SetNumberTilesSequentially(indexStart, indexEnd));

        // Gọi callback nếu có
        onComplete?.Invoke();
    }
    private void CreateGenerate(int indexStart, int indexEnd)
    {
        for (int i = indexStart; i < indexEnd; i++)
        {
            var obj = Instantiate(tilePrefab, holderTile);

            int row = i / BoardConfig.ColumnCount;
            int col = i % BoardConfig.ColumnCount;

            TileNumber tileNumber = obj.GetComponent<TileNumber>();
            tileNumber.SetNullImage(i);
            tileNumbers.Add(tileNumber);
        }
    }
    [System.Serializable]
    public struct TestRowCase
    {
        public int startRow;
        public int endRow;
        public int indexRowRemove; //Max 2
        public int rowA; //must be in the delete queue
        public int rowB; //must be in the delete queue
    }
    [SerializeField] private TestRowCase testCase;
    //Xóa 2 hàng đặt 
    // start Row 0 - End row 1 - index row 2 - row a 0 row b 1
    //Xóa 1 hàng đặt 
    // start Row 0 - End row 0 - index row 1 - row a 0 row b 0
    //row a - b phải bằng nhau nếu là 1 hàng khác nhau nếu 2 hàng
    [ContextMenu("Force Clear Two Rows")]
    public void TestRemoveRowCases()
    {
        ForceClearRows(testCase.startRow, testCase.endRow, testCase.indexRowRemove, testCase.rowA, testCase.rowB);
    }
    private void ForceClearRows(int startRow, int endRow, int indexRow, int rowA, int rowB)
    {
        TileNumber tileA = GetTile(rowA, 1);
        TileNumber tileB = GetTile(rowB, 2);

        for (int row = startRow; row <= endRow; row++)
        {
            for (int col = 0; col < BoardConfig.ColumnCount; col++)
            {
                TileNumber tile = GetTile(row, col);
                tile.CircleRemove.gameObject.SetActive(false);
            }
            Debug.Log($"[FORCE CLEAR] Xoá hàng {row}");
            RectTransform animTarget = (row == startRow) ? UIManager.Instance.AnimationRemoveColumn1 : UIManager.Instance.AnimationRemoveColumn2;
            HandleRowClear(row, animTarget);
        }
        this.lastRow = GetLastRowWithValidTile();
        StartCoroutine(RemoveRowCoroutine(indexRow, lastRow, tileA, tileB, false, false, () =>
        {

            if (endRow < lastRow)
            {
                for (int row = endRow; row <= lastRow; row++)
                {
                    for (int col = 0; col < BoardConfig.ColumnCount; col++)
                    {
                        TileNumber tile = GetTile(row, col);
                        tile.SetIsInPlayFalse();
                    }
                }
            }
            else
            {
                for (int row = 0; row <= 0; row++)
                {
                    for (int col = 0; col < BoardConfig.ColumnCount; col++)
                    {
                        TileNumber tile = GetTile(row, col);
                        tile.SetIsInPlayFalse();
                    }
                }
            }
            OnCheckWinGame();
        }));
        AudioManager.Instance.PlaySFX("sfx_row_clear");
    }
}
