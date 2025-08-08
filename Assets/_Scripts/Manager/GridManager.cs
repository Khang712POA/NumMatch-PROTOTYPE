using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : SaiMonoBehaviour
{
    private static GridManager instance;
    public static GridManager Instance => instance;


    [SerializeField] int lastRow;
    public List<TileNumber> tileNumbers;
    public List<TileNumber> tileNumbersShake;
    [SerializeField] private List<TileNumber> selectedTiles = new List<TileNumber>();
    public List<TileNumber> SelectedTiles => selectedTiles;

    private int numberTile;

    [SerializeField] Sprite[] imageNumbers = new Sprite[9];

    [SerializeField] RectTransform holderTile;
    [SerializeField] GameObject TitlePrefab;
    [SerializeField] RectTransform AnimationRemoveColumn1;
    [SerializeField] RectTransform AnimationRemoveColumn2;
    [SerializeField] UILineConnector uILineConnector;
    [SerializeField] GridBoardCopy gridBoardCopy;
    [SerializeField] MatchGenerator matchGenerator;
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
        if (Input.GetKeyDown(KeyCode.T))
        {
            // Lọc các tile có Value khác -1 và gọi hàm
            var filteredTiles = tileNumbers.Where(tile => tile.Value != -1).ToList();
            FindAndCheckMatches(filteredTiles);
        }
    }

    protected override void Start()
    {
        StartNewStage(GameManager.Instance.CurrentStage);
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

    //private int FindIndexLast()
    //{
    //    foreach (var tile in tileNumbers)
    //    {
    //        if (tile.Value == -1)
    //        {
    //            Debug.Log("FindIndexLast: " + tile.Index);
    //            return tile.Index - 1;
    //        }
    //    }
    //    return -1;
    //}
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

            TileNumber tileNumber = obj.GetComponent<TileNumber>();
            tileNumber.SetNullImage(i);
            tileNumbers.Add(tileNumber);
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
    private bool hasSetCurrentStageNext = false;

    public void SetCurrentStageNextOnce(int currentStage)
    {
        if (!hasSetCurrentStageNext)
        {
            GameManager.Instance.SetCurrentStageNext(currentStage);
            hasSetCurrentStageNext = true;
        }
    }
    public void StartNewStage(int stage)
    {
        CreateDisplay();
        tileNumbersShake.Clear();
        GameManager.Instance.ResetNumberGenerate();
        UIManager.Instance.ResetTextGenerate();
        SetCurrentStageNextOnce(stage);
        int[] newData = matchGenerator.GenerateStage(GameManager.Instance.CurrentStage);
        PrintGrid(newData);
        FindAndCheckMatches(newData);
        for (int i = 0; i < newData.Length; i++)
        {
            if(stage == 1)
            {
                SetTileImage(tileNumbers[i], newData[i], i , true);
                tileNumbers[i].gameObject.SetActive(true);
            }
            else
            {
                SetTileImage(tileNumbers[i], newData[i], i, false);
            }
        }

        this.lastRow = GetLastRowWithValidTile();
        if(GameManager.Instance.CurrentStage > GameManager.Instance.CurrentStageNext)
        {
            AnimationGenerate(0, 3);
            StartCoroutine(SetNumberTilesSequentially(0, newData.Length));
        }

        if(stage >= 3)
        {
            GameManager.Instance.LoadGemData(stage);
            availableGemTypes = GameManager.Instance.CurrentGemTypes
                .Select(g => new GemComponent(g.GemType, g.Count))
                .ToList();
            UIManager.Instance.LoadUIGemTop();
        }
        Debug.Log("Hàng cuối cùng có tile hợp lệ: " + lastRow);
    }

    private void SetTileImage(TileNumber tile, int value, int index, bool activeAnimation)
    {
        if (value > 0 && value <= imageNumbers.Length)
        {
            tile.SetTileNumber(index, value, imageNumbers[value - 1], activeAnimation);
        }
    }
    public void OnLoseGame()
    {
        //int[] currentTiles = GetAllTileValues();
        int firstEmptyRow = GetFirstEmptyRow();

        bool hasRemainingMatches = FindAndCheckMatches(
            tileNumbers.Where(tile => tile.Value != -1).ToList()
        );


        //Debug.Log("Array: " + currentTiles);
        //Debug.Log("hasRemainingMatches: " + hasRemainingMatches);
        //Debug.Log("NumberGenerate: " + GameManager.Instance.NumberGenerate);

        if (hasRemainingMatches || GameManager.Instance.NumberGenerate >= 1) //&& Gem)
            return;

        gridBoardCopy.CopyTile(tileNumbers);
        UIManager.Instance.ActiveUILose();
        UIManager.Instance.ResetTextGenerate();
    }
    private int[] GetAllTileValues()
    {
        return tileNumbers
            .Where(tile => tile.Value != -1)
            .Select(tile => tile.Value)
            .ToArray();
    }

    private int GetFirstEmptyRow()
    {
        for (int row = 0; row < tileNumbers.Count; row++) 
        {
            for (int col = 0; col < BoardConfig.ColumnCount; col++)
            {
                TileNumber tile = GetTile(row, col);
                if (tile.Value == -1)
                {
                    return row;
                }
            }
        }

        return -1;
    }
    private int GetFirstEmptyIndex()
    {
        for (int row = 0; row < tileNumbers.Count; row++)
        {
            for (int col = 0; col < BoardConfig.ColumnCount; col++)
            {
                TileNumber tile = GetTile(row, col);
                if (tile.Value == -1)
                {
                    return tile.Index;
                }
            }
        }

        return -1;
    }
    public void OnCheckClearBoard()
    {
        if(GameManager.Instance.CurrentStage < 3)
        {

            Debug.Log("OnCheckClearBoard");
            foreach (var tile in tileNumbers)
            {
                if (tile.isInPlay) return;
            }

            StageCompeteAnimationCoroutine(() =>
            {
                GameManager.Instance.IncreaseNumberAdd();
                StartNewStage(GameManager.Instance.CurrentStage);
                UIManager.Instance.UpdateTextStage(GameManager.Instance.CurrentStage);
                UIManager.Instance.StageCompeteAnimation.SetActive(false);
            });
        }
        else
        {

            bool AreAllGemsDepleted = GameManager.Instance.AreAllGemsDepleted();
            Debug.Log("AreAllGemsDepleted: " + AreAllGemsDepleted);
            if(AreAllGemsDepleted) UIManager.Instance.ActiveUIWin();
        }
    }
    private void StageCompeteAnimationCoroutine(Action onComplete)
    {
        StartCoroutine(StageCompeteAnimation(onComplete));
    }

    private IEnumerator StageCompeteAnimation(Action onComplete)
    {
        UIManager.Instance.StageCompeteAnimation.SetActive(true);

        yield return new WaitForSeconds(1.8f);

        if(CheckPointX2())
        {

            yield return new WaitForSeconds(0.5f);
            onComplete?.Invoke();

        }
        else
        {
            onComplete?.Invoke();
        }
    }
    private bool CheckPointX2()
    {
        return UnityEngine.Random.value < 0.05f; 
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
        bool validLine = IsClearPath(tileA, tileB, true);

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
            bool rowAClear = false;
            bool rowBClear = false;
            uILineConnector.StartDrawing(indexRow, indexColumn, minRow, maxRow, minColumn, maxColumn, rowA, rowB, equationType);

            GameManager.Instance.DeDuctAvailableGemTypes(tileA.GemType, tileA.IsGem);
            GameManager.Instance.DeDuctAvailableGemTypes(tileB.GemType, tileB.IsGem);

            tileA.ClearImage();
            tileB.ClearImage();

            if (AreRowsMatched(tileA, tileB))
            {
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
                this.lastRow = GetLastRowWithValidTile();
                StartCoroutine(RemoveRowCoroutine(indexRowRemove, lastRow, tileA, tileB, rowAClear, rowBClear, () =>
                {
                    OnLoseGame(); //Check Lose
                    Debug.Log("✅ RemoveRow hoàn tất, thực hiện logic tiếp theo..." + " ⏰ Thời gian: " + DateTime.Now.ToString("HH:mm:ss.fff"));
                }));

                if(GameManager.Instance.CurrentStage < 3)
                {
                    OnCheckClearBoard();
                }
            }
            else
            {
                OnLoseGame();
            }
            AudioManager.Instance.PlaySFX("sfx_pair_clear");
            if(GameManager.Instance.CurrentStage >=3)
            {
                OnCheckClearBoard();
            }
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
    private IEnumerator RemoveRowCoroutine(int indexRowRemove, int rowLast, TileNumber tileA, TileNumber tileB, bool rowAClear, bool rowBClear, Action onComplete)
    {
        Debug.Log("Remove Row");
        int rowA = tileA.Row;
        int rowB = tileB.Row;
        int minRow = Mathf.Min(rowA, rowB);
        int maxRow = Mathf.Max(rowA, rowB);
        bool isRowAGreater = rowA > rowB;
        bool isRowBGreater = rowB > rowA; 
        int indexRow = Mathf.Abs(rowA - rowB);

        //Debug.Log("IndexRow: "+ indexRow +" MinRow:" + minRow + "MaxRow:" + maxRow + "IndexrowRemove: " + indexRowRemove + " LastRow:" +lastRow + " rowA: "+ rowA+ " rowB: "+ rowB ) ;

        //TH1
        if (maxRow == rowLast + indexRowRemove && indexRowRemove == 1)
        {
            onComplete?.Invoke();

            yield break;
        }
        else if(maxRow == rowLast +indexRowRemove && indexRowRemove == 2 && indexRow == 1)
        {
            onComplete?.Invoke();
            yield break;
        }
        else if (indexRow == 1 && indexRowRemove == 2 && minRow + indexRowRemove != rowLast + indexRowRemove +1)//TH2 Type 1
        {
            Debug.Log("TH2: Type1");
            RemoveRowAndShiftDownCoroutine(minRow, indexRowRemove, () =>
            {
                RemoveRowIndex(indexRowRemove, rowLast);

                onComplete?.Invoke();

            });
        }
        else if(indexRow == 1 && indexRowRemove == 2 && maxRow + indexRowRemove == rowLast + indexRowRemove )
        {
            yield break;
        }
        else if (indexRowRemove == 2 && indexRow >1)//TH 2 Type2
        {
            if (maxRow + 1 == rowLast + indexRowRemove && isRowBGreater)
            {
                //1 Hang khong lam gi
                RemoveRowAndShiftDownCoroutine(rowA, 1, () =>
                {
                    RemoveRowIndex(1, rowLast);

                    onComplete?.Invoke();

                });
            }
            else if (maxRow + 1 == rowLast + indexRowRemove && isRowAGreater)
            {
                //1 Hang khong lam gi
                RemoveRowAndShiftDownCoroutine(rowB, 1, () =>
                {
                    RemoveRowIndex(1, rowLast);

                    onComplete?.Invoke();

                });
            }
            else if (isRowAGreater)
            {
                RemoveRowAndShiftDownBreakCoroutine(rowB, rowA - 1, 1, () =>
                {
                    RemoveRowsInRange(rowB, rowA - 1);
                });
                RemoveRowAndShiftDownCoroutine(rowA -1, indexRowRemove, () =>
                {
                    RemoveRowIndex(indexRowRemove, rowLast);

                    onComplete?.Invoke();

                });

            }
            else if (isRowBGreater)
            {
                RemoveRowAndShiftDownBreakCoroutine(rowA, rowB - 1, 1, () =>
                {
                    RemoveRowsInRange(rowA, rowB - 1);
                });
                RemoveRowAndShiftDownCoroutine(rowB - 1, indexRowRemove, () =>
                {
                    RemoveRowIndex(indexRowRemove, rowLast);

                    onComplete?.Invoke();

                });
            }
        }
        //Hang 1 
        else if (rowAClear && indexRowRemove == 1 && isRowBGreater)
        {
            RemoveRowAndShiftDownCoroutine(rowA, indexRowRemove, () =>
            {
                RemoveRowIndex(indexRowRemove, rowLast);

                onComplete?.Invoke();

            });
        }
        else if(rowAClear && indexRowRemove == 1 && isRowAGreater)// tren duoi
        {
            RemoveRowAndShiftDownCoroutine(rowA, indexRowRemove, () =>
            {
                RemoveRowIndex(indexRowRemove, rowLast);

                onComplete?.Invoke();

            });
        }
        else if (rowBClear && indexRowRemove == 1 && isRowBGreater)// tren duoi
        {
            RemoveRowAndShiftDownCoroutine(rowB, indexRowRemove, () =>
            {
                RemoveRowIndex(indexRowRemove, rowLast);

                onComplete?.Invoke();

            });
        }
        else if (rowBClear && indexRowRemove == 1 && isRowAGreater)// tren duoi
        {
            RemoveRowAndShiftDownCoroutine(rowB, indexRowRemove, () =>
            {
                RemoveRowIndex(indexRowRemove, rowLast);

                onComplete?.Invoke();

            });
        }
        else
        {
            RemoveRowAndShiftDownCoroutine(rowA, indexRowRemove, () =>
            {
                RemoveRowIndex(indexRowRemove, rowLast);

                onComplete?.Invoke();
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
    private void RemoveRowAndShiftDownBreakCoroutine(int rowStart, int rowEnd, int indexRowRemove, Action action)
    {
        StartCoroutine(RemoveRowAndShiftDownBreak(rowStart, rowEnd, indexRowRemove, action));
    }
    private IEnumerator RemoveRowAndShiftDownBreak(int rowStart, int rowEnd, int indexRemove, Action callback)
    {
        yield return new WaitForSeconds(1.3f);

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
                    tile.CopyUpperTile(upperTile);
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
        animationObj.anchoredPosition = new Vector2(-500f, 565 - row * 112 - contTent.anchoredPosition.y);

        FadeNumberAndSetNull(row);
        TriggerAnimationRemove(row);
    }
    public int countMatch = 0;
    public int countCheck = 0;
    public bool FindAndCheckMatches(List<TileNumber> tileNumbers)
    {
        bool hasMatch = false;
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
                    countCheck++;
                    hasMatch = true;

                    Debug.Log($"✅ Match: {a.Value} ({a.Row}, {a.Col}) <-> {b.Value} ({b.Row}, {b.Col}) : {countCheck}" + " ⏰ Thời gian: " + DateTime.Now.ToString("HH:mm:ss.fff"));

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

        countMatch = 0;
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
            if(Shake) tileNumbersShake.Add(tile);
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
            if (!tile.IsMatch && tile.isInPlay)
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
            //Debug.Log("TileNumber: " + row + "|" + col);

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
//#if UNITY_EDITOR
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
    //[ContextMenu("Force Clear Two Rows")]
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
            RectTransform animTarget = (row == startRow) ? AnimationRemoveColumn1 : AnimationRemoveColumn2;
            HandleRowClear(row, animTarget);
        }
        this.lastRow = GetLastRowWithValidTile();
        StartCoroutine(RemoveRowCoroutine(indexRow, lastRow, tileA, tileB, false, false, () =>
        {

            Debug.Log("EndRow: " + endRow + "|LastRow: " + lastRow);
            if (endRow < lastRow)
            {
                for (int row = endRow; row <= lastRow; row++)
                {
                    for (int col = 0; col < BoardConfig.ColumnCount; col++)
                    {
                        TileNumber tile = GetTile(row, col);
                        tile.TestCase();
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
                        tile.TestCase();
                    }
                }
            }
            Debug.Log("✅ RemoveRow hoàn tất, thực hiện logic tiếp theo...");
        }));
        OnCheckClearBoard();
        AudioManager.Instance.PlaySFX("sfx_row_clear");
    }
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform contTent;
    //[ContextMenu("Clone Remaining Tiles to Bottom")]
    public void CloneRemainingTilesToBottom()
    {
        List<TileNumber> remainingTiles = new();

        foreach (var tile in tileNumbers)
        {
            if (tile.Value != -1 && !tile.IsMatch)
            {
                remainingTiles.Add(tile);
            }
        }
        int indexStart = GetFirstEmptyIndex();
        int indexEnd = GetFirstEmptyIndex() + remainingTiles.Count;
        Debug.Log("IndexStart: " + indexStart +"IndexEnd: "+indexEnd);


        if (indexEnd > tileNumbers.Count)
        {
            CreateGenerate(indexEnd - tileNumbers.Count, indexEnd);
        }

        //Debug.Log("IndexStart: " + indexStart + " IndexEnd: " + indexEnd);
        for (int j = 0; j < remainingTiles.Count; j++)
        {
            int i = indexStart + j;

            if (i >= tileNumbers.Count || tileNumbers[i] == null)
            {
                Debug.LogWarning($"tileNumbers[{i}] is null or out of range.");
                continue;
            }

            var targetTile = tileNumbers[i];
            targetTile.SetTileNumber(i, remainingTiles[j].Value, remainingTiles[j].NumberImage.sprite, false);
        }

        if (GameManager.Instance.CurrentStage >= 3)
        {
            var tileValues = tileNumbers
                .Skip(indexStart)
                .Take(indexEnd - indexStart)
                .Where(t => t != null)
                .ToList();

            GenerateGemsInSpawn(tileValues, availableGemTypes);

        }

        StartCoroutine(CheckLoseGame(indexStart, indexEnd, () => {
            Debug.Log("Done checking lose game!");
            OnLoseGame();
        }));
        tileNumbersShake.Clear();
    }
    private IEnumerator CheckLoseGame(int indexStart, int indexEnd, Action onComplete)
    {
        int startRow = indexStart / BoardConfig.ColumnCount;
        int endRow = indexEnd / BoardConfig.ColumnCount;

        AnimationGenerate(startRow, endRow);

        yield return StartCoroutine(SetNumberTilesSequentially(indexStart, indexEnd));

        // Gọi callback nếu có
        onComplete?.Invoke();
    }

    private void CreateGenerate(int indexStart, int indexEnd)
    {
        for (int i = indexStart; i < indexEnd; i++)
        {
            var obj = Instantiate(TitlePrefab, holderTile);

            int row = i / BoardConfig.ColumnCount;
            int col = i % BoardConfig.ColumnCount;

            TileNumber tileNumber = obj.GetComponent<TileNumber>();
            tileNumber.SetNullImage(i);
            tileNumbers.Add(tileNumber);
        }

    }
    private void HandleOverValue(int index)
    {
        int sumHasValue = SumHasValue();
        if(sumHasValue + index > BoardConfig.COUNTTILE_DEFAULT)
        {

        }
    }
    private void ChangeContent(int index)
    {
        int emptyIndex = GetFirstEmptyIndex();
        

    }
    private int SumHasValue()
    {
        int count = 0;
        for(int row = 0; row < BoardConfig.COUNTTILE_DEFAULT; row++)
        {
            for(int col = 0; col < BoardConfig.ColumnCount; col++)
            {
                TileNumber tile = GetTile(row, col);
                if(tile.Value != -1)
                {
                    count++;
                }
            }
        }
        return count;
    }
//#endif
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
                output += value + " ";

                if (value >= 1 && value <= 9)
                    counts[value]++;
            }
            output += "\n";
        }

        output += "\n📊 Thống kê số lần xuất hiện:\n";
        for (int i = 1; i <= 9; i++)
        {
            output += $"🔢 Số {i}: {counts[i]} lần\n";
        }

        output += "\n🚨 Cảnh báo nếu lệch phân phối:\n";
        for (int i = 1; i <= 9; i++)
        {
            if (counts[i] < 1 || counts[i] > 4)
                output += $"⚠️ Số {i} lệch phân phối: {counts[i]} lần\n";
        }

        Debug.Log(output);
    }
    [SerializeField] private int columnCount = 9; // Cố định số cột, chỉnh trong Inspector nếu cần

    public void FindAndCheckMatches(int[] flatGrid)
    {
        HashSet<int> matched = new HashSet<int>();
        int totalMatches = 0;

        int cols = BoardConfig.ColumnCount;
        int rows = flatGrid.Length / cols;

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
    //[ContextMenu("Next Stage")]
    public void NextStage()
    {
        for(int row = 0; row < tileNumbers.Count / BoardConfig.ColumnCount;  row++)
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
        GameManager.Instance.ResetAllGemCounts();

        this.OnCheckClearBoard();
    }
    [SerializeField] private List<GemComponent> availableGemTypes = new List<GemComponent>();
    public void GenerateGemsInSpawn(List<TileNumber> spawnedTiles, List<GemComponent> availableGemTypes)
    {
        int count = spawnedTiles.Count;
        if (count == 0 || availableGemTypes.Count == 0) return;

        int Z = Mathf.Min(2, availableGemTypes.Sum(gem => gem.Count)); // ✅ Tối đa 2 viên gem mỗi lượt
        int X = UnityEngine.Random.Range(5, 8); // Random từ 5 đến 7%
        int Y = Mathf.CeilToInt((count + 1) / 2);

        int gemCount = 0;
        int tilesSinceLastGem = 0;

        var randomTiles = spawnedTiles.OrderBy(_ => UnityEngine.Random.value).ToList();

        foreach (var tile in randomTiles)
        {
            if (gemCount >= Z) break;

            bool forcePlaceGem = tilesSinceLastGem >= Y;
            bool chancePlaceGem = UnityEngine.Random.Range(0f, 100f) < X;

            if (forcePlaceGem || chancePlaceGem)
            {
                var validGemTypes = availableGemTypes.Where(g => g.Count > 0).ToList();
                if (validGemTypes.Count == 0) break;

                var selectedGemComponent = validGemTypes[UnityEngine.Random.Range(0, validGemTypes.Count)];
                GemType selectedGem = selectedGemComponent.GemType;

                if (IsSafeToPlaceGem(tile, spawnedTiles))
                {
                    tile.SetAsGem(GameManager.Instance.GetSpriteGem(selectedGem));
                    tile.IsGem = true;
                    tile.GemImage.enabled = true;
                    tile.GemImage.color = new Color32(255, 255, 255, 255);
                    tile.SetGemType(selectedGem);
                    selectedGemComponent.DecreaseCount();
                    gemCount++;
                    tilesSinceLastGem = 0;
                    continue;
                }
            }

            tilesSinceLastGem++;
        }
    }



    private bool IsSafeToPlaceGem(TileNumber currentTile, List<TileNumber> allTiles)
    {
        int value = currentTile.Value; // Hàm trả về giá trị số của viên gem (ví dụ: 5)
        int index = currentTile.Index; // Vị trí index trong lưới 1 chiều
        int cols = BoardConfig.ColumnCount;

        // Helper để lấy tile theo index an toàn
        TileNumber GetTile(int i)
        {
            return (i >= 0 && i < allTiles.Count) ? allTiles[i] : null;
        }

        // Kiểm tra ngang (trái - giữa - phải)
        int row = index / cols;
        int col = index % cols;

        if (col >= 1 && col < cols - 1)
        {
            var left = GetTile(index - 1);
            var right = GetTile(index + 1);
            if (left != null && right != null && left.Value == value && right.Value == value)
                return false;
        }

        // Kiểm tra dọc (trên - giữa - dưới)
        if (row >= 1 && row < (allTiles.Count / cols) - 1)
        {
            var up = GetTile(index - cols);
            var down = GetTile(index + cols);
            if (up != null && down != null && up.Value == value && down.Value == value)
                return false;
        }

        return true;
    }

}
[Serializable]
public class GemComponent
{
    [SerializeField] GemType gemType;
    public GemType GemType => gemType;
    [SerializeField] int count;
    public int Count => count;

    public GemComponent(GemType type, int count)
    {
        this.gemType = type;
        this.count = count;
    }
    public void DecreaseCount()
    {
        if (count > 0) count--;
    }
    public void ResetCount()
    {
        count = 0;
    }
}
public enum GemType
{
    Pink,       // Đá hồng
    Orange,      // Đá cam
    Purple      // Đá tím
}
