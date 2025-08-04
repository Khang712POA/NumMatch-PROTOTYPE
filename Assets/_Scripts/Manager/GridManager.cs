using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : SaiMonoBehaviour
{
    private static GridManager instance;
    public static GridManager Instance => instance;

    const int COUNTTILE_DEFAULT = 90;

    public int[,] gridMatrix;
    [SerializeField] int lastRow;

    public List<TileNumber> tileNumbers;
    public List<TileNumber> tileNumbersShake;
    [SerializeField] private List<TileNumber> selectedTiles = new List<TileNumber>();
    public List<TileNumber> SelectedTiles => selectedTiles;

    private int numberTile;
    private int currentStage = 1;

    [SerializeField] Sprite[] imageNumbers = new Sprite[9];
    [SerializeField] RectTransform holderTile;
    [SerializeField] GameObject TitlePrefab;
    [SerializeField] RectTransform AnimationRemoveColumn1;
    [SerializeField] RectTransform AnimationRemoveColumn2;
    [SerializeField] UILineConnector uILineConnector;

    protected override void Awake()
    {
        base.Awake();
        if (instance != null)
        {
            Debug.LogError("Only 1 GridManager instance allowed!");
            return;
        }
        instance = this;
        numberTile = COUNTTILE_DEFAULT;
        CreateDisplay();
    }

    protected override void Start()
    {
        StartNewStage();
    }

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadImageNumbers();
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
#if UNITY_EDITOR
    [ContextMenu("Clone Remaining Tiles to Bottom")]
    public void CloneRemainingTilesToBottom()
    {
        List<(int value, Sprite sprite, bool isMatch)> remainingTiles = new();

        foreach (var tile in tileNumbers)
        {
            if (tile.Value != -1 && !tile.IsMatch)
            {
                remainingTiles.Add((tile.Value, tile.NumberImage.sprite, tile.IsMatch));
            }
        }

        int indexStart = FindIndexLast() + 1;
        int indexEnd = FindIndexLast() + 1 + remainingTiles.Count;
        for (int j = 0; j < remainingTiles.Count; j++)
        {
            int i = indexStart + j;
            var targetTile = tileNumbers[i];
            var data = remainingTiles[j];
            targetTile.SetTileNumber(i, data.value, data.sprite, true);
        }
        AnimationGenerate(indexStart / BoardConfig.ColumnCount, indexEnd / BoardConfig.ColumnCount);
        StartCoroutine(SetNumberTilesSequentially(indexStart, indexEnd));
    }
#endif
    private IEnumerator SetNumberTilesSequentially(int indexStart, int indexEnd)
    {
        for (int i = indexStart; i < indexEnd; i++)
        {
            var targetTile = tileNumbers[i];

            targetTile.InitButton();

            if (i== indexStart)
            {
                yield return new WaitForSeconds(1.1f);
                targetTile.NumberImage.gameObject.SetActive(true);
                targetTile.NumberImage.color = new Color32(30, 91, 102, 255);

                // Đợi 1.2s sau ô đầu tiên
            }
            else
            {
                targetTile.NumberImage.gameObject.SetActive(true);
                targetTile.NumberImage.color = new Color32(30, 91, 102, 255);

                // Đợi 0.1s giữa các ô còn lại
                yield return new WaitForSeconds(0.08f);
            }
        }
    }
    private void AnimationGenerate(int rowStart, int rowEnd)
    {
        StartCoroutine(AnimateByRowAndColumn(rowStart, rowEnd));
    }
    private IEnumerator AnimateByRowAndColumn(int rowStart, int rowEnd)
    {
        yield return new WaitForSeconds(0.7f); //Number Spin

        for (int r = rowStart; r < rowEnd; r++)
        {
            float rowDelay = (r - rowStart) * 0.55f;
            yield return new WaitForSeconds(rowDelay);

            for (int c = 0; c < BoardConfig.ColumnCount; c++)
            {
                TileNumber tile = GetTile(r, c);
                if (tile == null) continue;

                if (c == 0)
                {
                    // Cột 0 chạy ngay
                    StartCoroutine(AnimateTileWithDelay(tile, 0f));
                }
                else if (c == 1)
                {
                    // Bắt đầu animation ngay
                    StartCoroutine(AnimateTileWithDelay(tile, 0f));

                    // Ẩn hình ngay
                    tile.DeselectBackground();

                    // Sau 0.15s thì hiện lại ảnh (bật hình)
                    StartCoroutine(EnableBackgroundAfterDelay(tile, 0.1f));
                }

                else
                {
                    // Các cột sau đợi 0.15s so với cột trước
                    float colDelay = 0.1f * c;
                    StartCoroutine(AnimateTileWithDelay(tile, colDelay));
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

    private int FindIndexLast()
    {
        foreach (var tile in tileNumbers)
        {
            if (tile.Value == -1)
            {
                Debug.Log("FindIndexLast: " + tile.Index);
                return tile.Index - 1;
            }
        }
        return -1;
    }
    public void CreateDisplay()
    {
        tileNumbers.Clear();

        foreach (Transform child in holderTile)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < numberTile; i++)
        {
            var obj = Instantiate(TitlePrefab, holderTile);

            int row = i / BoardConfig.ColumnCount;
            int col = i % BoardConfig.ColumnCount;

            obj.name = $"Row{row}_Col{col}";

            TileNumber itemTooltip = obj.GetComponent<TileNumber>();
            itemTooltip.SetNullImage(i);
            tileNumbers.Add(itemTooltip);
        }

    }

    public int[] GenerateStage(int stage)
    {
        int totalTiles = 27;
        int[] result = new int[totalTiles];
        List<int> pool = new List<int>();

        int[] distribution = new int[9];
        for (int i = 0; i < totalTiles; i++)
        {
            distribution[i % 9]++;
        }

        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < distribution[i]; j++)
            {
                pool.Add(i + 1);
            }
        }

        for (int i = 0; i < pool.Count; i++)
        {
            int rand = UnityEngine.Random.Range(i, pool.Count);
            (pool[i], pool[rand]) = (pool[rand], pool[i]);
        }

        int numberOfPairs = stage == 1 ? 3 : stage == 2 ? 2 : 1;
        List<(int, int)> validPairs = GenerateValidPairs();
        for (int i = 0; i < numberOfPairs; i++)
        {
            var pair = validPairs[UnityEngine.Random.Range(0, validPairs.Count)];
            pool[i * 2] = pair.Item1;
            pool[i * 2 + 1] = pair.Item2;
        }

        return pool.ToArray();
    }

    private List<(int, int)> GenerateValidPairs()
    {
        List<(int, int)> pairs = new List<(int, int)>();
        for (int i = 1; i <= 9; i++) pairs.Add((i, i));
        for (int i = 1; i <= 9; i++)
        {
            int j = 10 - i;
            if (i <= j && j <= 9) pairs.Add((i, j));
        }
        return pairs;
    }

    public void StartNewStage()
    {
        int[] newData = GenerateStage(currentStage);
        for (int i = 0; i < newData.Length; i++)
        {
            SetTileImage(tileNumbers[i], newData[i], i);
            tileNumbers[i].gameObject.SetActive(true);
        }

        this.lastRow = GetLastRowWithValidTile();
        Debug.Log("Hàng cuối cùng có tile hợp lệ: " + lastRow);
    }

    private void SetTileImage(TileNumber tile, int value, int index)
    {
        if (value > 0 && value <= imageNumbers.Length)
        {
            tile.SetTileNumber(index, value, imageNumbers[value - 1], false);
        }
    }

    public void OnCheckClearBoard()
    {
        foreach (var tile in tileNumbers)
        {
            if (tile.gameObject.activeSelf) return;
        }
        currentStage++;
        StartNewStage();
    }
    private EquationType equationType = EquationType.None;
    public void OnTileSelected(TileNumber tile)
    {
        if (tile == null || selectedTiles.Contains(tile)) return;

        selectedTiles.Add(tile);

        if (selectedTiles.Count != 2) return;

        var tileA = selectedTiles[0];
        var tileB = selectedTiles[1];

        bool validValue = (tileA.Value + tileB.Value == 10) || (tileA.Value == tileB.Value);
        bool validLine = IsClearPath(tileA, tileB);

        if (validValue && validLine)
        {
            equationType = GetEquationType(tileA, tileB);
            int rowA = tileA.Row;
            int rowB = tileB.Row;
            int colA = tileA.Col;
            int colB = tileB.Col;
            int indexRow = Mathf.Abs(rowA - rowB);
            int indexColumn = Mathf.Abs(colA - colB);
            int minRow = Mathf.Min(rowA, rowB);
            int maxRow = Mathf.Max(rowA, rowB);
            int minColumn = Mathf.Min(colA, colB);
            int maxColumn = Mathf.Max(colA, colB);
            uILineConnector.StartDrawing(indexRow, indexColumn, minRow, maxRow, minColumn, maxColumn, rowA, rowB, equationType);

            tileA.ClearImage();
            tileB.ClearImage();

            if (AreRowsMatched(tileA, tileB))
            {
                bool rowAClear = false;
                bool rowBClear = false;
                int indexRowRemove = 0;

                if (rowA == rowB)
                {
                    if (IsRowFullyMatched(rowA))
                    {
                        rowAClear = true;
                        HandleRowClear(rowA, AnimationRemoveColumn1);
                        indexRowRemove++;
                    }
                }
                else
                {
                    if (IsRowFullyMatched(rowA))
                    {
                        rowAClear = true;
                        HandleRowClear(rowA, AnimationRemoveColumn1);
                        indexRowRemove++;
                    }
                    if (IsRowFullyMatched(rowB))
                    {
                        rowBClear = true;
                        HandleRowClear(rowB, AnimationRemoveColumn2);
                        indexRowRemove++;
                    }
                }
                AudioManager.Instance.PlaySFX("sfx_row_clear");

                Debug.Log("Index Row " + indexRowRemove);
                this.lastRow = GetLastRowWithValidTile();
                RemoveRow(indexRowRemove, lastRow, tileA, tileB, rowAClear, rowBClear); //Fix
            }
            AudioManager.Instance.PlaySFX("sfx_pair_clear");

            OnCheckClearBoard();
            selectedTiles.Clear();
        }
        else if (validValue && !validLine)
        {
            tileA.DeselectBackground();
            tileB.DeselectBackground();
            foreach (var t in tileNumbersShake) t.ObjectShake.ShakeAndRecover();
            selectedTiles.Clear();
        }
        else
        {
            tileA.DeselectBackground();
            selectedTiles.RemoveAt(0);
        }

        tileNumbersShake.Clear();
    }
    private EquationType GetEquationType(TileNumber a, TileNumber b)
    {
        if(a == null || b == null) return EquationType.None;
        if(a.Row == b.Row)
        {
            return EquationType.StraighType1;
        }
        return EquationType.None;
    }
    [SerializeField]
    int endRow = 0;
    [SerializeField]
    int indexRow = 0;
#if UNITY_EDITOR
    [ContextMenu("Force Clear Two Rows")]
    public void OnForceClearTwoRows()
    {
        ForceClearTwoRows(endRow, indexRow);
    }
    private void ForceClearTwoRows(int startRow, int indexRow)
    {
        for (int row = 0; row <= startRow; row++)
        {
            Debug.Log($"[FORCE CLEAR] Xoá hàng {row}");

            for (int col = 0; col < BoardConfig.ColumnCount; col++)
            {
                TileNumber tile = GetTile(row, col);
                if (tile == null) continue;

                tile.ClearImage(); // Xoá sprite / ẩn tile
            }

            // Hiệu ứng xóa hàng
            RectTransform animTarget = (row == startRow) ? AnimationRemoveColumn1 : AnimationRemoveColumn2;
            HandleRowClear(row, animTarget);
        }

        lastRow = GetLastRowWithValidTile();
        //RemoveRow(indexRow, lastRow, tileA, tileB);
        AudioManager.Instance.PlaySFX("sfx_row_clear");
    }
#endif
    private void RemoveRow(int indexRowRemove, int rowLast, TileNumber tileA, TileNumber tileB, bool rowAClear, bool rowBClear) 
    {
        Debug.Log("Remove Row");
        bool doubleClear = rowAClear && rowBClear;
        int rowA = tileA.Row;
        int rowB = tileB.Row;
        int minRow = Mathf.Min(rowA, rowB);
        int maxRow = Mathf.Max(rowA, rowB);
        bool isRowAGreater = rowA > rowB;
        bool isRowBGreater = rowB > rowA; 
        int indexRow = Mathf.Abs(rowA - rowB);

        Debug.Log("IndexRow: "+ indexRow +" MinRow:" + minRow + "MaxRow:" + maxRow + "IndexrowRemove: " + indexRowRemove + " LastRow:" +lastRow + " rowA: "+ rowA+ " rowB: "+ rowB + " RowAClear: "+rowAClear+" RowBClear: "+rowBClear) ;

        //TH1
        if (maxRow == rowLast + indexRowRemove && indexRowRemove == 1)
        {
            return;
        }
        else if(maxRow == rowLast +indexRowRemove && indexRowRemove == 2 && indexRow == 1)
        {
            return;
        }
        else if (indexRow == 1 && indexRowRemove == 2 && minRow + indexRowRemove != rowLast + indexRowRemove +1)//TH2 Type 1
        {
            Debug.Log("TH2: Type1");
            RemoveRowAndShiftDownCoroutine(minRow, indexRowRemove, () =>
            {
                Debug.Log("RemoveRowIndex");
                RemoveRowIndex(indexRowRemove, rowLast);
            });
        }
        else if(indexRow == 1 && indexRowRemove == 2 && maxRow + indexRowRemove == rowLast + indexRowRemove )
        {
            return;
        }
        else if (indexRowRemove == 2 && indexRow >1)//TH 2 Type2
        {
            if (maxRow + 1 == rowLast + indexRowRemove && isRowBGreater)
            {
                Debug.Log("AAAA + --->>>>");
                //1 Hang khong lam gi
                RemoveRowAndShiftDownCoroutine(rowA, 1, () =>
                {
                    RemoveRowIndex(1, rowLast);
                });
            }
            else if (maxRow + 1 == rowLast + indexRowRemove && isRowAGreater)
            {
                Debug.Log("BBBB + --->>>>");
                //1 Hang khong lam gi
                RemoveRowAndShiftDownCoroutine(rowB, 1, () =>
                {
                    RemoveRowIndex(1, rowLast);
                });
            }
            else if (isRowAGreater)
            {
                Debug.Log("CCCC + --->>>>");

                RemoveRowAndShiftDownBreakCoroutine(rowB, rowA - 1, 1, () =>
                {
                    RemoveRowsInRange(rowB, rowA - 1);
                });
                RemoveRowAndShiftDownCoroutine(rowA -1, indexRowRemove, () =>
                {
                    RemoveRowIndex(indexRowRemove, rowLast);
                });

            }
            else if (isRowBGreater)
            {
                Debug.Log("DDDD + --->>>>");

                RemoveRowAndShiftDownBreakCoroutine(rowA, rowB - 1, 1, () =>
                {
                    RemoveRowsInRange(rowA, rowB - 1);
                });
                RemoveRowAndShiftDownCoroutine(rowB - 1, indexRowRemove, () =>
                {
                    RemoveRowIndex(indexRowRemove, rowLast);
                });
            }
        }
        //Hang 1 
        else if (rowAClear && indexRowRemove == 1 && isRowBGreater)
        {
            RemoveRowAndShiftDownCoroutine(rowA, indexRowRemove, () =>
            {
                Debug.Log("RemoveRowIndex A");
                RemoveRowIndex(indexRowRemove, rowLast);
            });
        }
        else if(rowAClear && indexRowRemove == 1 && isRowAGreater)// tren duoi
        {
            RemoveRowAndShiftDownCoroutine(rowA, indexRowRemove, () =>
            {
                Debug.Log("RemoveRowIndex A");
                RemoveRowIndex(indexRowRemove, rowLast);
            });
        }
        else if (rowBClear && indexRowRemove == 1 && isRowBGreater)// tren duoi
        {
            RemoveRowAndShiftDownCoroutine(rowB, indexRowRemove, () =>
            {
                Debug.Log("RemoveRowIndex B");
                RemoveRowIndex(indexRowRemove, rowLast);
            });
        }
        else if (rowBClear && indexRowRemove == 1 && isRowAGreater)// tren duoi
        {
            RemoveRowAndShiftDownCoroutine(rowB, indexRowRemove, () =>
            {
                Debug.Log("RemoveRowIndex A");
                RemoveRowIndex(indexRowRemove, rowLast);
            });
        }
        else
        {
            RemoveRowAndShiftDownCoroutine(rowA, indexRowRemove, () =>
            {
                Debug.Log("RemoveRowIndex A");
                RemoveRowIndex(indexRowRemove, rowLast);
            });
        }

    }
    private void RemoveRowAndShiftDownCoroutine(int rowStart, int indexRowRemove, Action action)
    {
        StartCoroutine(RemoveRowAndShiftDown(rowStart, indexRowRemove, action));
    }

    private IEnumerator RemoveRowAndShiftDown(int rowStart, int indexRemove, Action callback)
    {
        Debug.Log("AAAARemoveRowAndShiftDown");
        yield return new WaitForSeconds(0.95f);

        int totalRows = tileNumbers.Count / BoardConfig.ColumnCount;

        for (int row = rowStart; row < totalRows; row++)
        {
            for (int col = 0; col < BoardConfig.ColumnCount; col++)
            {
                TileNumber tile = GetTile(row, col);
                TileNumber upperTile = GetTile(row + indexRemove, col);

                if (upperTile != null && upperTile.Value != -1)
                {
                    Debug.Log("indexRemove: " + indexRemove + "Index:" + tile.Index);
                    tile.CopyTile(upperTile);
                    tile.MoveNumberImageUp(0.3f, indexRemove);
                }
            }
        }
        callback?.Invoke();
    }
    private void RemoveRowAndShiftDownBreakCoroutine(int rowStart, int rowEnd, int indexRowRemove, Action action)
    {
        StartCoroutine(RemoveRowAndShiftDownBreak(rowStart, rowEnd, indexRowRemove, action));
    }
    private IEnumerator RemoveRowAndShiftDownBreak(int rowStart, int rowEnd, int indexRemove, Action callback)
    {
        yield return new WaitForSeconds(0.95f);

        Debug.Log("Break RowStart: " + rowStart + "|" + "RowEnd: " + rowEnd);

        for (int row = rowStart; row < rowEnd ; row++)
        {
            Debug.Log("Break RowStart:"+ row);
            for (int col = 0; col < BoardConfig.ColumnCount; col++)
            {
                TileNumber tile = GetTile(row, col);
                TileNumber upperTile = GetTile(row + indexRemove, col);

                if (upperTile != null && upperTile.Value != -1)
                {
                    Debug.Log("indexRemove: " + indexRemove + "Index:" + tile.Index);
                    tile.CopyTile(upperTile);
                    tile.MoveNumberImageUp(0.3f, indexRemove);
                }
            }
        }
        callback?.Invoke();
    }
    private void RemoveRowIndex(int rowIndex, int lastRow)
    {
        for (int row = lastRow; row > lastRow - rowIndex; row--)
        {
            Debug.Log("RowRemoveRowIndex: " + row);
            for (int col = 0; col < BoardConfig.ColumnCount; col++)
            {
                TileNumber tile = GetTile(row, col);
                if (tile != null && tile.Value != -1)
                {
                    tile.SetRemoveTile();
                    Debug.Log("SetRemoveTile: " + row + "SetRemoveTile: "+col);
                }
            }
        }
    }
    private void RemoveRowsInRange(int startRow, int lastEnd)
    {
        for (int row = lastEnd; row > startRow + 1; row--)
        {
            Debug.Log("RowRemoveRowIndex: " + row);
            for (int col = 0; col < BoardConfig.ColumnCount; col++)
            {
                TileNumber tile = GetTile(row, col);
                if (tile != null && tile.Value != -1)
                {
                    tile.SetRemoveTile();
                    tile.SetValueNotNull();
                    Debug.Log("SetRemoveTile: " + row + "SetRemoveTile: " + col);
                }
            }
        }
    }
    private void HandleRowClear(int row, RectTransform animationObj)
    {
        Debug.Log($"[Clear Row] {row}");

        animationObj.gameObject.SetActive(true);
        Animator animator = animationObj.GetComponent<Animator>();
        animator.Play(animator.GetCurrentAnimatorStateInfo(0).shortNameHash, 0, 0f);
        animationObj.anchoredPosition = new Vector2(-500f, 565 - row * 112);

        FadeNumberAndSetNull(row);
        TriggerAnimationRemove(row);
    }
    private bool IsClearPath(TileNumber a, TileNumber b)
    {
        int rowA = a.Row, colA = a.Col;
        int rowB = b.Row, colB = b.Col;
        int dr = rowB - rowA, dc = colB - colA;

        bool checkMatch = true;

        if (dr == 0)
        {
            for (int c = Mathf.Min(colA, colB) + 1; c < Mathf.Max(colA, colB); c++)
                CheckTile(rowA, c, ref checkMatch);
            return checkMatch;
        }

        if (dc == 0)
        {
            for (int r = Mathf.Min(rowA, rowB) + 1; r < Mathf.Max(rowA, rowB); r++)
                CheckTile(r, colA, ref checkMatch);
            return checkMatch;
        }

        if (Mathf.Abs(dr) == Mathf.Abs(dc))
        {
            int steps = Mathf.Abs(dr), stepR = dr > 0 ? 1 : -1, stepC = dc > 0 ? 1 : -1;
            for (int i = 1; i < steps; i++)
                CheckTile(rowA + i * stepR, colA + i * stepC, ref checkMatch);
            return checkMatch;
        }

        int columns = BoardConfig.ColumnCount;
        if (rowA < rowB)
        {
            for (int c = colA + 1; c < columns; c++) CheckTile(rowA, c, ref checkMatch);
            for (int c = 0; c < colB; c++) CheckTile(rowB, c, ref checkMatch);
            if (rowB - rowA > 1)
            {
                for (int r = rowA + 1; r < rowB; r++)
                {
                    for (int c = 0; c < columns; c++)
                        CheckTile(r, c, ref checkMatch);
                }
            }
            return checkMatch;
        }
        else if (rowA > rowB)
        {
            for (int c = colB + 1; c < columns; c++) CheckTile(rowB, c, ref checkMatch);
            for (int c = colA - 1; c >= 0; c--) CheckTile(rowA, c, ref checkMatch);
            if (rowA - rowB > 1)
            {
                for (int r = rowB + 1; r < rowA; r++)
                {
                    for (int c = 0; c < columns; c++)
                        CheckTile(r, c, ref checkMatch);
                }
            }
            return checkMatch;
        }

        return false;
    }

    private void CheckTile(int row, int col, ref bool checkMatch)
    {
        TileNumber tile = GetTile(row, col);
        if (tile != null && tile.gameObject.activeSelf && !tile.IsMatch)
        {
            tileNumbersShake.Add(tile);
            checkMatch = false;
        }
    }
    private bool AreRowsMatched(TileNumber tileA, TileNumber tileB)
    {
        int rowA = tileA.Row;
        int rowB = tileB.Row;

        Debug.Log("rowA:" + rowA + "rowB: " + rowB);

        bool resultA = IsRowFullyMatched(rowA);
        bool resultB = IsRowFullyMatched(rowB);

        Debug.Log("resultA: " + resultA + "resultB: " + resultB);

        return resultA || resultB;
    }

    private bool IsRowFullyMatched(int row)
    {
        for (int col = 0; col < BoardConfig.ColumnCount; col++)
        {
            TileNumber tile = GetTile(row, col);
            if (tile != null && tile.gameObject.activeSelf && !tile.IsMatch)
            {
                return false;
            }
        }
        return true;
    }
    private void TriggerAnimationRemove(int row)
    {
        StartCoroutine(TriggerAnimationRemoveSequence(row));
    }

    private IEnumerator TriggerAnimationRemoveSequence(int row)
    {
        for (int col = 0; col < BoardConfig.ColumnCount; col++)
        {
            TileNumber tile = GetTile(row, col);
            Debug.Log("TileNumber: " + row + "|" + col);

            tile.OnTileRemoveAnimation();

            yield return new WaitForSeconds(0.1f);

            tile.FadeImage(tile.CircleRemove);
        }
    }
    private void FadeNumberAndSetNull(int row)
    {
        for (int col = 0; col < BoardConfig.ColumnCount; col++)
        {
            TileNumber tile = GetTile(row, col);

            tile.FadeImage(tile.NumberImage);
            tile.SetNullValue();
        }
    }
    private void RemoveLastRow(int row)
    {
        Debug.Log("RemoveLastRow: " + row);
        for (int c = 0; c < BoardConfig.ColumnCount; c++)
        {
            TileNumber lastTile = GetTile(row, c);
            lastTile.SetRemoveTile();
        }
    }
    private TileNumber GetTile(int row, int col)
    {
        int index = row * BoardConfig.ColumnCount + col;
        if (index >= 0 && index < tileNumbers.Count)
            return tileNumbers[index];
        return null;
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
    //Generate
    public void AnimationGenerate()
    {
        int totalRows = tileNumbers.Count / BoardConfig.ColumnCount;

        for (int row = 0 ; row < totalRows; row++)
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
}
