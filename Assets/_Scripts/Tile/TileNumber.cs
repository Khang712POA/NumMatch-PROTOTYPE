using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TileNumber : SaiMonoBehaviour
{
    [SerializeField]
    private int value;
    public int Value => value;
    [SerializeField]
    private int index;
    public int Index => index;

    public int Row => index / BoardConfig.ColumnCount;
    public int Col => index % BoardConfig.ColumnCount;

    public Vector2Int GridPosition { get; private set; }

    [SerializeField] private Color targetColor;
    [SerializeField] private float colorTransitionDuration = 0.2f; // Thời gian đổi màu
    [SerializeField] private float shrinkDuration = 0.6f; // Thời gian thu nhỏ

    [SerializeField] private Image numberImage;
    public Image NumberImage => numberImage;

    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject animationCircle;
    [SerializeField] private Button tileButton;
    public Button TileButton => tileButton;


    [SerializeField] private ObjectShake objectShake;
    public ObjectShake ObjectShake => objectShake;
    [SerializeField] private Image circleRemove;
    public Image CircleRemove => circleRemove;


    public bool IsMatch;
    public TileNumber(int value, int index, int columnCount)
    {
        this.value = value;
        this.index = index;
        SetGridPosition(index, columnCount);
    }
    protected override void Start()
    {
        InitButton();
    }
    public void InitButton()
    {
        if (GridManager.Instance.SelectedTiles.Count >= 2 || value == -1)
        {
            tileButton.onClick.RemoveAllListeners();
            return;
        }

        tileButton.onClick.RemoveAllListeners(); // Xoá tránh trùng
        tileButton.onClick.AddListener(OnTileClicked);
    }
    private void OnTileClicked()
    {
        Debug.Log("AAAAA");

        OnclickAnimationTile();
        GridManager.Instance.OnTileSelected(this);
        AudioManager.Instance.PlaySFX("sfx_choose_number");
    }
    public void SetNullValue()
    {
        this.value = -1;
        this.circleRemove.fillAmount = 1;
    }

    public void SetGridPosition(int index, int columnCount)
    {
        GridPosition = new Vector2Int(index / columnCount, index % columnCount);
    }

    private void OnclickAnimationTile()
    {
        backgroundImage.gameObject.SetActive(true);

        StopAllCoroutines(); // Ngăn hiệu ứng trước đó

        StartCoroutine(AnimateTileEffect(backgroundImage, backgroundImage.color, targetColor, Vector3.one * 0.8f, Vector3.one, colorTransitionDuration));
    }
    public void OnTileRemoveAnimation()
    {
        circleRemove.gameObject.SetActive(true);

        StartCoroutine(AnimateTileEffect(circleRemove, circleRemove.color, targetColor, Vector3.one * 0.6f, Vector3.one, colorTransitionDuration));
    }
    private IEnumerator AnimateTileEffect(Image Image, Color fromColor, Color toColor, Vector3 fromScale, Vector3 toScale, float duration)
    {
        float time = 0f;
        Image.transform.localScale = fromScale;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            backgroundImage.color = Color.Lerp(fromColor, toColor, t);
            Image.transform.localScale = Vector3.Lerp(fromScale, toScale, t);

            yield return null;
        }

        backgroundImage.color = toColor;
        Image.transform.localScale = toScale;
    }
    private IEnumerator AnimateTileFade(Image image, float duration, bool fadeOut)
    {
        float time = 0f;
        Color startColor = image.color;
        Color endColor = startColor;

        if (fadeOut)
            endColor.a = 0f; // Mờ dần biến mất
        else
            endColor.a = 1f; // Hiện dần ra (trường hợp đang bị ẩn)

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            Color currentColor = Color.Lerp(startColor, endColor, t);
            image.color = currentColor;

            yield return null;
        }

        image.color = endColor;
    }
    public void FadeImage(Image image)
    {
        StartCoroutine(FadeImageSequence(image));
    }
    private IEnumerator FadeImageSequence(Image image)
    {
        yield return StartCoroutine(AnimateTileFade(image, 0.25f, true));
    }
    public void ClearImage()
    {
        backgroundImage.color = new Color32(194, 230, 239, 255);
        StopAllCoroutines();
        StartAnimateScaleCoroutine2(() => {
            animationCircle.SetActive(true);
        });

        numberImage.color = new Color32(30, 91, 102, 140);
        IsMatch = true;
        tileButton.interactable = false;
    }  
    public void CopyTile(TileNumber tileCopy)
    {
        this.value = tileCopy.Value;
        this.IsMatch = tileCopy.IsMatch;
        this.numberImage.color = tileCopy.NumberImage.color;
        this.numberImage.sprite = tileCopy.NumberImage.sprite;
        this.tileButton.interactable = tileCopy.TileButton.interactable;
    }
    public void SetImageNumberRemove()
    {
        this.numberImage.gameObject.SetActive(false);
    }
    public void SetNullImage(int index)
    {
        this.value = -1;
        this.index = index;
        this.numberImage.gameObject.SetActive(false);
    }

    public void DeselectBackground()
    {
        backgroundImage.gameObject.SetActive(false);
    }
    public void SelectBackground()
    {
        backgroundImage.gameObject.SetActive(true);
    }

    public void SetTileNumber(int index, int value, Sprite sprite, bool activeAnimation)
    {
        this.value = value;
        this.index = index;

        if (numberImage != null)
        {
            if(!activeAnimation) numberImage.gameObject.SetActive(true);
            numberImage.sprite = sprite;
            numberImage.enabled = sprite != null;
            this.tileButton.interactable = true;
        }
    }
    public void MoveNumberImageUp(float duration, int indexRemove)
    {
        this.numberImage.gameObject.SetActive(true);
        if (numberImage == null) return;
        StartCoroutine(MoveUpCoroutine(duration, indexRemove));
    }

    private IEnumerator MoveUpCoroutine(float duration, int indexRemove)
    {
        RectTransform rect = numberImage.rectTransform;
        Vector2 startPos = rect.anchoredPosition;
        startPos.y -= indexRemove * 115f;
        Vector2 targetPos = new Vector2(startPos.x, startPos.y + indexRemove * 115f);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }
        rect.anchoredPosition = targetPos;
    }

    [ContextMenu("Matched")]
    public void OnContextTile()
    {
        value = -1;

        IsMatch = true; //Fix
        tileButton.interactable = false;
        numberImage.color = new Color32(30, 91, 102, 140);
    }
    public void SetRemoveTile()
    {
        value = -1;

        IsMatch = false; //Fix
        tileButton.interactable = false;
        numberImage.gameObject.SetActive(false);
    }
    public void SetValueNotNull()
    {
        value = 10;
    }
    public void SpinCircle()
    {
        circleRemove.rectTransform.localScale = Vector2.one * -1f;
        circleRemove.gameObject.SetActive(true);
        circleRemove.fillAmount = 1;

        // Lần 1
        SpinFillAmountCoroutine(0.15f,()=>
        {
            circleRemove.fillAmount = 1f;

            // Lần 2
            SpinFillAmountCoroutine(0.25f, () =>
            {
                Debug.Log("Spin complete!");
                circleRemove.gameObject.SetActive(false);
            });
        });
    }

    private void SpinFillAmountCoroutine(float decreaseAmount, Action onComplete)
    {
        StartCoroutine(SpinFillAmount(decreaseAmount, onComplete));
    }

    private IEnumerator SpinFillAmount(float decreaseAmount, Action onComplete)
    {
        float currentFill = 1f;
        circleRemove.fillAmount = currentFill;

        while (currentFill > 0f)
        {
            yield return new WaitForSeconds(0.1f);

            currentFill -= decreaseAmount;

            if (currentFill < 0f)
                currentFill = 0f;

            circleRemove.fillAmount = currentFill;
        }

        // Gọi callback sau khi hoàn thành
        onComplete?.Invoke();
    }
    public void StartAnimateScaleCoroutine(Action onComplete)
    {
        backgroundImage.gameObject.SetActive(true);
        StartCoroutine(AnimateScale(Vector2.one*0.9f, 0.15f, backgroundImage, onComplete));
    }
    public void StartAnimateScaleCoroutine2(Action onComplete)
    {
        backgroundImage.gameObject.SetActive(true);
        StartCoroutine(AnimateScale(Vector2.one*0.92f , 0.23f, backgroundImage, onComplete));
    }
    private IEnumerator AnimateScale(Vector2 fromScale, float decreaseAmount, Image image, Action onComplete)
    {
        image.rectTransform.localScale = fromScale;

        while (image.transform.localScale.x > 0f)
        {
            yield return new WaitForSeconds(0.08f);

            Vector2 currentScale = image.rectTransform.localScale;
            Vector2 newScale = currentScale - new Vector2(decreaseAmount, decreaseAmount);

            image.rectTransform.localScale = newScale;
        }
        // Gọi callback sau khi hoàn thành
        onComplete?.Invoke();
    }
}
