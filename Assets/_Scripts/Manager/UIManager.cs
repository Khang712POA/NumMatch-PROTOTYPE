using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : SaiMonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance => instance;
    [Space(2)]
    [Header("Prefab")]
    [SerializeField] private GameObject uIGemTopPrefab;
    [SerializeField] private GameObject uIGemPrefab;


    [Space(2)]
    [Header("Button UI")]
    [SerializeField] private ObjButton generateButton;
    [SerializeField] private ObjButton nextStageButton;
    [SerializeField] private ObjButton homeButton;
    [SerializeField] private ObjButton settingButton;
    [SerializeField] private ObjButton loseButton;


    [Space(2)]
    [Header("GameObject UI")]
    [SerializeField] private GameObject stageCompeteAnimation;
    public GameObject StageCompeteAnimation => stageCompeteAnimation;

    [Space(2)]
    [Header("RectTransForm UI")]
    [SerializeField] private RectTransform uIFade;
    public RectTransform UIFade => uIFade;
    [SerializeField] private RectTransform uITop;
    public RectTransform UITop => uITop;
    [SerializeField] private RectTransform uIBottom;
    public RectTransform UIBottom => uIBottom;
    [SerializeField] private RectTransform uILose;
    public RectTransform UILose => uILose;
    [SerializeField] private RectTransform uIWin;
    public RectTransform UIWin => uIWin;
    [SerializeField] private RectTransform uISetting;
    public RectTransform UISetting => uISetting;

    [Space(2)]
    [Header("Holder UI")]
    [SerializeField] private RectTransform holderUIGemTop;
    [SerializeField] private RectTransform holderUIGemWins;
    [SerializeField] private List<UIGemTop> uIGemTopList;   

    protected override void Awake()
    {
        base.Awake();
        if (instance != null)
        {
            Debug.LogError("Only 1 UIManager instance allowed!");
            return;
        }
        instance = this; ;
    }
    protected override void Start()
    {
        generateButton.AddOnClickListener(OnGenerateValid);
        homeButton.AddOnClickListener(BackHome);
        settingButton.AddOnClickListener(ActiveSettingUI);
    }
    public void LoadUIGemTop()
    {
        uIGemTopList.Clear();
        foreach (Transform child in holderUIGemTop)
        {
            Destroy(child.gameObject);
        }
        var gemTypes = GameManager.Instance.CurrentGemTypes;
        foreach (var gem in gemTypes)
        {
            var obj = Instantiate(uIGemTopPrefab, holderUIGemTop);
            //obj.name = $"Gem:{gem.GemType}_Count:{gem.Count}";
            UIGemTop uIGemTop = obj.GetComponent<UIGemTop>();
            uIGemTop.textUI.text = gem.Count.ToString();
            uIGemTop.image.sprite = GameManager.Instance.GetSpriteGem(gem.GemType);
            uIGemTop.GemType = gem.GemType;
            obj.SetActive(true);
            uIGemTopList.Add(uIGemTop);
        }
    }
    public void UpdateGemUI(GemComponent[] gemComponents)
    {
        foreach (GemComponent gemComponent in gemComponents)
        {
            foreach (var uiGem in uIGemTopList)
            {
                if (uiGem.GemType == gemComponent.GemType)
                {
                    uiGem.textUI.text = gemComponent.Count.ToString();
                    break; 
                }
            }
        }
    }
    private void OnResetStage()
    {
        uIFade.gameObject.SetActive(false);
        UILose.gameObject.SetActive(false);

        GameManager.Instance.ResetCurrentStage();
        GridManager.Instance.StartNewStage(1);
    }
    private void OnNextStage()
    {
        Debug.Log("OnNextStage");

        uIFade.gameObject.SetActive(false);
        UIWin.gameObject.SetActive(false);

        GameManager.Instance.IncreaseNumberAdd();
        GridManager.Instance.StartNewStage(GameManager.Instance.CurrentStage);
        UpdateTextStage(GameManager.Instance.CurrentStage);
    }
    private void OnGenerateValid()
    {
        if (GameManager.Instance.NumberGenerate < 1) return;

        generateButton.gameObject.SetActive(false);
        generateButton.gameObject.SetActive(true);
        GameManager.Instance.DeductNumberAdd();
        generateButton.GetComponentInChildren<Text>().text = GameManager.Instance.NumberGenerate.ToString();
        GridManager.Instance.AnimationGenerate();
        GridManager.Instance.CloneRemainingTilesToBottom();
        Debug.Log("OnGenerateValid");
    }
    public void ResetTextGenerate()
    {
        generateButton.GetComponentInChildren<Text>().text = 6.ToString();
    }
    public void UpdateTextStage(int currentStage)
    {
        Transform textStageTransform = uITop.Find("TextStage");
        if (textStageTransform == null) Debug.LogError("Not Find Text Stage");
        textStageTransform.GetComponent<Text>().text = currentStage.ToString();
    }
    public void ActiveUILose()
    {
        uIFade.gameObject.SetActive(true);
        uILose.gameObject.SetActive(true);

        loseButton.AddOnClickListener(OnResetStage);
        
    }
    public void ActiveUIWin()
    {
        uIFade.gameObject.SetActive(true);
        UIWin.gameObject.SetActive(true);

        LoadUIGemWin();
        UIWin.Find("TextLevel").GetComponent<Text>().text = GameManager.Instance.CurrentStage + " COMPLETE";

        nextStageButton.AddOnClickListener(OnNextStage);
    }
    private void LoadUIGemWin()
    {
        foreach (Transform child in holderUIGemWins)
        {
            Destroy(child.gameObject);
        }
        var gemTypes = GameManager.Instance.CurrentGemTypes;
        foreach (var gem in gemTypes)
        {
            var obj = Instantiate(uIGemPrefab, holderUIGemWins);
            //obj.name = $"Gem:{gem.GemType}_Count:{gem.Count}";
            obj.GetComponent<Image>().sprite = GameManager.Instance.GetSpriteGem(gem.GemType);

            obj.SetActive(true);
        }
    }
    private void BackHome()
    {
        ScenesManager.instance.LoadScene("GameHome");
    }
    private void ActiveSettingUI()
    {
        uIFade.gameObject.SetActive(true);
        uISetting.gameObject.SetActive(true);
    }
}
