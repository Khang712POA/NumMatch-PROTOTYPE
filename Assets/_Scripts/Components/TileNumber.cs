using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TileNumber : SaiMonoBehaviour
{
    [SerializeField] private int value = -1;
    public int Value => value;

    [SerializeField] private int index;
    public int Index => index;

    public int Row => index / BoardConfig.ColumnCount;
    public int Col => index % BoardConfig.ColumnCount;

    public bool IsInPlay { get; private set; }

    [Header("Color & Animation Settings")]
    [SerializeField] private Color targetColor;
    [SerializeField] private float colorTransitionDuration = 0.2f;
    [SerializeField] private float shrinkDuration = 0.6f;

    [Header("UI References")]
    [SerializeField] private Image numberImage;
    public Image NumberImage => numberImage;

    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image gemImage;
    public Image GemImage => gemImage;

    [SerializeField] private GameObject animationCircle;
    [SerializeField] private Button tileButton;
    public Button TileButton => tileButton;

    [SerializeField] private ObjectShake objectShake;
    public ObjectShake ObjectShake => objectShake;

    [SerializeField] private Image circleRemove;
    public Image CircleRemove => circleRemove;

    [SerializeField] private Sprite hintColored;

    private GemType gemType;
    public GemType GemType => gemType;

    public bool IsMatch { get; private set; }
    public bool IsGem { get; private set; }

    [SerializeField] private Animator animator;

    #region Unity Callbacks
    protected override void Start()
    {
        InitButton();
    }
    #endregion

    #region Initialization & Setup
    public TileNumber(int value, int index, int columnCount)
    {
        this.value = value;
        this.index = index;
    }
    public void SetIsInPlayFalse()
    {
        IsInPlay = false;
    }
    public void InitButton()
    {
        tileButton.onClick.RemoveAllListeners();

        if (GridManager.Instance.SelectedTiles.Count < 2 && value != -1)
        {
            tileButton.onClick.AddListener(OnTileClicked);
        }
    }
    #endregion

    #region Interaction
    private void OnTileClicked()
    {
        PlayClickAnimation();
        GridManager.Instance.OnTileSelected(this);
        AudioManager.Instance.PlaySFX("sfx_choose_number");
    }

    private void PlayClickAnimation()
    {
        backgroundImage.gameObject.SetActive(true);
        backgroundImage.color = targetColor;

        StopAllCoroutines(); // Dừng các hiệu ứng đang chạy

        StartCoroutine(AnimateScaleAndColor(backgroundImage, Vector3.one * 0.8f, Vector3.one, colorTransitionDuration));
    }
    #endregion

    #region Animations & Effects

    private IEnumerator AnimateScaleAndColor(Image image, Vector3 fromScale, Vector3 toScale, float duration)
    {
        float elapsed = 0f;
        image.transform.localScale = fromScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            image.transform.localScale = Vector3.Lerp(fromScale, toScale, t);
            yield return null;
        }

        image.transform.localScale = toScale;
    }

    private IEnumerator AnimateFade(Image image, float duration, bool fadeOut)
    {
        float elapsed = 0f;
        Color startColor = image.color;
        Color endColor = fadeOut ? new Color(startColor.r, startColor.g, startColor.b, 0f) : new Color(startColor.r, startColor.g, startColor.b, 1f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            image.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
        image.color = endColor;
    }

    public void SetAnimation()
    {
        StartCoroutine(GemAnimationCoroutine(gemType));
    }

    private IEnumerator GemAnimationCoroutine(GemType gemType)
    {
        gemImage.maskable = false;
        animator.enabled = true;

        yield return new WaitForSeconds(0.6f);

        yield return AnimateFade(gemImage, 0.6f, true);

        yield return new WaitForSeconds(0.8f);

        gemImage.enabled = false;
        animator.enabled = false;

        UIManager.Instance.UpdateGemUI(GamePlayManager.Instance.CurrentGemTypes);
    }

    public void OnTileRemoveAnimation()
    {
        circleRemove.gameObject.SetActive(true);
        circleRemove.color = new Color32(101, 172, 241, 255);

        StartCoroutine(AnimateScaleAndColor(circleRemove, Vector3.one * 0.6f, Vector3.one, colorTransitionDuration));
    }

    public void FadeImage(Image image)
    {
        StartCoroutine(AnimateFade(image, 0.25f, true));
    }

    public void SpinCircle()
    {
        circleRemove.rectTransform.localScale = Vector3.one * -1f;
        circleRemove.color = new Color32(101, 172, 241, 255);
        circleRemove.gameObject.SetActive(true);

        StartCoroutine(SpinFillAmountSequence());
    }

    private IEnumerator SpinFillAmountSequence()
    {
        yield return SpinFillAmount(0.15f);
        circleRemove.fillAmount = 1f;
        yield return SpinFillAmount(0.25f);

        circleRemove.gameObject.SetActive(false);
        Debug.Log("Spin complete!");
    }

    private IEnumerator SpinFillAmount(float decreaseAmount)
    {
        float fill = 1f;

        while (fill > 0f)
        {
            yield return new WaitForSeconds(0.1f);
            fill -= decreaseAmount;
            fill = Mathf.Max(fill, 0f);
            circleRemove.fillAmount = fill;
        }
    }

    #endregion

    #region Public API

    public void ClearImage()
    {
        StopAllCoroutines();

        StartAnimateScaleCoroutine(() => animationCircle.SetActive(true));

        tileButton.interactable = false;

        if (IsGem)
        {
            SetAnimation();
            IsGem = false;
        }

        backgroundImage.color = new Color32(194, 230, 239, 255);
        numberImage.color = new Color32(30, 91, 102, 140);
        circleRemove.gameObject.SetActive(false);

        IsMatch = true;
        IsInPlay = false;
    }

    public void CopyUpperTile(TileNumber tileUpper)
    {
        value = tileUpper.Value;
        IsMatch = tileUpper.IsMatch;
        IsInPlay = tileUpper.IsInPlay;

        numberImage.color = tileUpper.NumberImage.color;
        numberImage.sprite = tileUpper.NumberImage.sprite;
        tileButton.interactable = tileUpper.TileButton.interactable;

        if (tileUpper.IsGem)
        {
            IsGem = true;
            gemImage.sprite = tileUpper.GemImage.sprite;
            backgroundImage.sprite = hintColored;
            SetAsGem(tileUpper.GemImage.sprite, tileUpper.GemType);

            // Reset upper tile
            tileUpper.IsGem = false;
            tileUpper.gemImage.gameObject.SetActive(false);
        }

        animationCircle.SetActive(false);
    }

    public void CopyTileBoard(TileNumber tileCopy)
    {
        numberImage.color = tileCopy.NumberImage.color;
        numberImage.sprite = tileCopy.NumberImage.sprite;
    }

    public void SetImageNumberRemove()
    {
        numberImage.gameObject.SetActive(false);
    }

    public void SetNullImage(int newIndex)
    {
        value = -1;
        index = newIndex;
        IsInPlay = false;
        IsMatch = false;
        numberImage.gameObject.SetActive(false);
    }

    public void DeselectBackground()
    {
        backgroundImage.gameObject.SetActive(false);
    }

    public void SelectBackground()
    {
        backgroundImage.gameObject.SetActive(true);
    }

    public void SetTileNumber(int newIndex, int newValue, Sprite sprite, bool activeAnimation)
    {
        value = newValue;
        index = newIndex;
        IsInPlay = true;

        if (numberImage != null)
        {
            numberImage.gameObject.SetActive(activeAnimation);
            numberImage.sprite = sprite;
            numberImage.color = new Color32(30, 91, 102, 255);
            numberImage.enabled = sprite != null;
            tileButton.interactable = true;
            backgroundImage.sprite = null;
            gemImage.maskable = true;
        }
    }

    public void MoveNumberImageUp(float duration, int indexRemove)
    {
        numberImage.gameObject.SetActive(true);

        if (IsGem)
        {
            gemImage.gameObject.SetActive(true);
            StartCoroutine(MoveUpCoroutine(duration, indexRemove, gemImage));
        }

        if (numberImage != null)
            StartCoroutine(MoveUpCoroutine(duration, indexRemove, numberImage));
    }

    private IEnumerator MoveUpCoroutine(float duration, int indexRemove, Image image)
    {
        RectTransform rect = image.rectTransform;
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
    public void MarkAsMatched()
    {
        value = -1;
        IsMatch = true;
        IsInPlay = false;
        tileButton.interactable = false;
        numberImage.color = new Color32(30, 91, 102, 140);
    }

    public void SetRemoveTile()
    {
        value = -1;
        IsGem = false;
        IsInPlay = false;
        IsMatch = false;
        tileButton.interactable = false;
        numberImage.gameObject.SetActive(false);
        gemImage.sprite = null;
        gemImage.gameObject.SetActive(false);
    }

    public void SetValueNull()
    {
        value = 10;
    }
    public void StartAnimateScaleCoroutine(Action onComplete)
    {
        backgroundImage.gameObject.SetActive(true);
        StartCoroutine(AnimateScale(Vector2.one * 0.9f, 0.15f, backgroundImage, onComplete));
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

        onComplete?.Invoke();
    }

    public void SetAsGem(Sprite spriteGem, GemType gemType)
    {
        gemImage.sprite = spriteGem;
        gemImage.gameObject.SetActive(true);
        backgroundImage.sprite = hintColored;
        IsGem = true;
        gemImage.enabled = true;
        gemImage.color = new Color32(255, 255, 255, 255);
        this.gemType = gemType;
    }

    #endregion
}
