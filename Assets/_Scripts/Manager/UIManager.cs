using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : SaiMonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private GameObject uIGemTopPrefab;
    [SerializeField] private GameObject uIGemPrefab;

    [Header("Buttons")]
    [SerializeField] private ObjButton generateButton;
    [SerializeField] private ObjButton nextStageButton;
    [SerializeField] private ObjButton homeButton;
    [SerializeField] private ObjButton settingButton;
    [SerializeField] private ObjButton loseButton;

    [Header("UI GameObjects")]
    [SerializeField] private GameObject stageCompleteAnimation;

    [Header("RectTransforms")]
    [SerializeField] private RectTransform uiFade;
    [SerializeField] private RectTransform uiTop;
    [SerializeField] private RectTransform uiBottom;
    [SerializeField] private RectTransform uiLose;
    [SerializeField] private RectTransform uiWin;
    [SerializeField] private RectTransform uiSetting;
    [SerializeField] RectTransform animationRemoveColumn1;
    public RectTransform AnimationRemoveColumn1 => animationRemoveColumn1;
    [SerializeField] RectTransform animationRemoveColumn2;
    public RectTransform AnimationRemoveColumn2 => animationRemoveColumn2;

    [Header("UI Holders")]
    [SerializeField] private RectTransform holderUIGemTop;
    [SerializeField] private RectTransform holderUIGemWins;

    private readonly List<UIGemTop> uiGemTopList = new();

    #region Unity Callbacks

    protected override void Awake()
    {
        base.Awake();

        if (Instance != null)
        {
            Debug.LogError("Only one UIManager instance allowed!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    protected override void Start()
    {
        SetupButtonListeners();
    }

    #endregion

    #region Setup

    private void SetupButtonListeners()
    {
        generateButton.AddOnClickListener(OnGenerateClicked);
        homeButton.AddOnClickListener(OnBackHomeClicked);
        settingButton.AddOnClickListener(OnSettingClicked);
    }

    #endregion

    #region UIGemTop Management

    public void LoadUIGemTop()
    {
        ClearChildren(holderUIGemTop);
        uiGemTopList.Clear();

        var gemTypes = GamePlayManager.Instance.CurrentGemTypes;
        foreach (var gem in gemTypes)
        {
            var obj = Instantiate(uIGemTopPrefab, holderUIGemTop);
            var uiGemTop = obj.GetComponent<UIGemTop>();
            uiGemTop.textUI.text = gem.Count.ToString();
            uiGemTop.image.sprite = GamePlayManager.Instance.GetSpriteGem(gem.GemType);
            uiGemTop.GemType = gem.GemType;
            obj.SetActive(true);
            uiGemTopList.Add(uiGemTop);
        }
    }

    public void UpdateGemUI(GemComponent[] gemComponents)
    {
        foreach (var gemComponent in gemComponents)
        {
            var uiGem = uiGemTopList.Find(ui => ui.GemType == gemComponent.GemType);
            if (uiGem != null)
            {
                uiGem.textUI.text = gemComponent.Count.ToString();
            }
        }
    }

    #endregion

    #region UI Control Methods
    public void ActiveStageCompleteAnimation()
    {
        stageCompleteAnimation.gameObject.SetActive(true);
    }
    public void ActiveUILose()
    {
        uiFade.gameObject.SetActive(true);
        uiLose.gameObject.SetActive(true);

        loseButton.AddOnClickListener(OnResetStageClicked);
    }

    public void ActiveUIWin()
    {
        uiFade.gameObject.SetActive(true);
        uiWin.gameObject.SetActive(true);

        LoadUIGemWin();
        var textLevel = uiWin.Find("TextLevel")?.GetComponent<Text>();
        if (textLevel != null)
        {
            textLevel.text = $"{GamePlayManager.Instance.CurrentStage} COMPLETE";
        }

        nextStageButton.AddOnClickListener(OnNextStageClicked);
    }

    private void LoadUIGemWin()
    {
        ClearChildren(holderUIGemWins);

        var gemTypes = GamePlayManager.Instance.CurrentGemTypes;
        foreach (var gem in gemTypes)
        {
            var obj = Instantiate(uIGemPrefab, holderUIGemWins);
            var image = obj.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = GamePlayManager.Instance.GetSpriteGem(gem.GemType);
            }
            obj.SetActive(true);
        }
    }

    public void UpdateTextStage(int currentStage)
    {
        var textStageTransform = uiTop.Find("TextStage");
        if (textStageTransform == null)
        {
            Debug.LogError("TextStage not found under uiTop");
            return;
        }

        var textComponent = textStageTransform.GetComponent<Text>();
        if (textComponent != null)
        {
            textComponent.text = currentStage.ToString();
        }
    }

    public void ResetTextGenerate()
    {
        SetGenerateButtonText(6);
    }

    private void SetGenerateButtonText(int number)
    {
        var text = generateButton.GetComponentInChildren<Text>();
        if (text != null)
        {
            text.text = number.ToString();
        }
    }

    #endregion

    #region Button Callbacks

    private void OnGenerateClicked()
    {
        if (GamePlayManager.Instance.NumberGenerate < 1) return;

        generateButton.gameObject.SetActive(false);
        generateButton.gameObject.SetActive(true);

        GamePlayManager.Instance.DeductNumberAdd();
        SetGenerateButtonText(GamePlayManager.Instance.NumberGenerate);

        GridManager.Instance.AnimationGenerate();
        GridManager.Instance.CloneRemainingTilesToBottom();

        Debug.Log("OnGenerateClicked");
    }

    private void OnResetStageClicked()
    {
        uiFade.gameObject.SetActive(false);
        uiLose.gameObject.SetActive(false);

        GamePlayManager.Instance.ResetCurrentStage();
        GridManager.Instance.StartNewStage(1);
    }

    private void OnNextStageClicked()
    {
        Debug.Log("OnNextStageClicked");

        uiFade.gameObject.SetActive(false);
        uiWin.gameObject.SetActive(false);

        GamePlayManager.Instance.IncreaseNumberAdd();
        GridManager.Instance.StartNewStage(GamePlayManager.Instance.CurrentStage);
        UpdateTextStage(GamePlayManager.Instance.CurrentStage);
    }

    private void OnBackHomeClicked()
    {
        ScenesManager.instance.LoadScene("GameHome");
    }

    private void OnSettingClicked()
    {
        uiFade.gameObject.SetActive(true);
        uiSetting.gameObject.SetActive(true);
    }

    #endregion

    #region Helpers

    private void ClearChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }

    #endregion
}
