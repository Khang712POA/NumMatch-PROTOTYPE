using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class GamePlayManager : SaiMonoBehaviour
{
    private static GamePlayManager instance;
    public static GamePlayManager Instance => instance;
    [SerializeField] private int numberGenerate = 6;
    [SerializeField] private int currentStage = 1;
    [SerializeField] private int currentStageNext = 0;
    [SerializeField] private GemComponent[] currentGemTypes = new GemComponent[2];
    [SerializeField] Sprite[] imageGems = new Sprite[3];
    public int NumberGenerate => numberGenerate;
    public GemComponent[] CurrentGemTypes => currentGemTypes;
    public int CurrentStage => currentStage;
    public int CurrentStageNext => currentStageNext;

    public void DeductNumberAdd()
    {
        numberGenerate--;
    }
    public void IncreaseNumberAdd()
    {
        currentStage++;
    }
    public void ResetNumberGenerate()
    {
        numberGenerate = 6;
    }
    public void SetCurrentStageNext(int next)
    {
        currentStageNext = next;
    }
    public void ResetCurrentStage()
    {
        currentStage = 1;
    }
    protected override void Awake()
    {
        base.Awake();
        if (instance != null)
        {
            Debug.LogError("Only 1 GameManager instance allowed!");
            return;
        }
        instance = this;
    }
    protected override void LoadComponents()
    {
        base.LoadComponents();
        this.LoadImageGems();
    }
    protected void LoadImageGems()
    {
        if (imageGems.Length > 0) return;

        imageGems = Resources.LoadAll<Sprite>("Sprites/gem");

        if (imageGems.Length == 0)
        {
            Debug.LogWarning("Không tìm thấy ảnh trong Resources/_Sprites/number");
        }
    }
    private GemComponent[] LoadGemComponentsFromText(string fileContent, string selectedLevel)
    {
        List<GemComponent> gemComponents = new List<GemComponent>();
        string[] lines = fileContent.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            if (!line.StartsWith(selectedLevel + ":"))
                continue;

            string gemData = line.Substring(line.IndexOf(":") + 1);
            string[] pairs = gemData.Split(',');

            foreach (string pair in pairs)
            {
                string[] parts = pair.Split('=');
                if (parts.Length != 2) continue;

                if (Enum.TryParse(parts[0], out GemType gemType) &&
                    int.TryParse(parts[1], out int count))
                {
                    gemComponents.Add(new GemComponent(gemType, count));
                }
            }
            break;
        }
        return gemComponents.ToArray();
    }

    public void LoadGemData(int stage)
    {
        string levelName = "Stage" + stage;
        for (int i = 0; i < currentGemTypes.Length; i++) currentGemTypes[i] = null;

        TextAsset textFile = Resources.Load<TextAsset>("Data/stages_gem_data");
        if (textFile) currentGemTypes = LoadGemComponentsFromText(textFile.text, levelName);
    }
    public Sprite GetSpriteGem(GemType typeGem)
    {
        switch (typeGem)
        {
            case GemType.Pink:
                return imageGems[0];
            case GemType.Orange:
                return imageGems[1];
            case GemType.Purple:
                return imageGems[2];
        }
        return null;
    }
    public void DeDuctAvailableGemTypes(GemType typeGem, bool isGem)
    {
        foreach (var gem in currentGemTypes)
        {
            if(gem.GemType == typeGem && isGem)
            {
                gem.DecreaseCount();
                Debug.Log("DecreaseCount: " + gem.Count);
            }
        }
    }
    public void ResetAllGemCounts()
    {
        foreach (var gem in currentGemTypes)
        {
            gem.ResetCount(); 
        }
    }

    public bool AreAllGemsDepleted()
    {
        Debug.Log("=== Checking GemComponent Counts ===");

        for (int i = 0; i < currentGemTypes.Length; i++)
        {
            var gem = currentGemTypes[i];
            if (gem == null)
            {
                Debug.LogWarning($"Gem at index {i} is null");
            }
            else
            {
                Debug.Log($"GemType: {gem.GemType}, Count: {gem.Count}");
            }
        }

        bool result = currentGemTypes.All(gem => gem != null && gem.Count == 0);
        Debug.Log($"All Gems Depleted: {result}");
        return result;
    }
}
