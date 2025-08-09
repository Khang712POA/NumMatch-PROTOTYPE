using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GemSpawner : MonoBehaviour
{
    [SerializeField]
    private List<GemComponent> availableGemTypes = new List<GemComponent>();
    public void SetAvailableGemTypes(List<GemComponent> AvailableGemTypes)
    {
        availableGemTypes.Clear();
        availableGemTypes = AvailableGemTypes;
    }
    public void GenerateGemsInSpawn(List<TileNumber> spawnedTiles)
    {
        int count = spawnedTiles.Count;
        if (count == 0 || availableGemTypes.Count == 0) return;

        int maxGemsPerTurn = Mathf.Min(2, availableGemTypes.Sum(gem => gem.Count)); // Tối đa 2 viên gem mỗi lượt
        int chancePercent = Random.Range(5, 8); // Random 5% đến 7%
        int maxTilesSinceLastGem = Mathf.CeilToInt((count + 1) / 2f);

        int gemCount = 0;
        int tilesSinceLastGem = 0;

        var randomTiles = spawnedTiles.OrderBy(_ => Random.value).ToList();

        foreach (var tile in randomTiles)
        {
            if (gemCount >= maxGemsPerTurn) break;

            bool forcePlaceGem = tilesSinceLastGem >= maxTilesSinceLastGem;
            bool chancePlaceGem = Random.Range(0f, 100f) < chancePercent;

            if (forcePlaceGem || chancePlaceGem)
            {
                var validGemTypes = availableGemTypes.Where(g => g.Count > 0).ToList();
                if (validGemTypes.Count == 0) break;

                var selectedGemComponent = validGemTypes[Random.Range(0, validGemTypes.Count)];
                GemType selectedGem = selectedGemComponent.GemType;

                if (IsSafeToPlaceGem(tile, spawnedTiles))
                {
                    tile.SetAsGem(GamePlayManager.Instance.GetSpriteGem(selectedGem), selectedGem);
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
        int value = currentTile.Value;
        int index = currentTile.Index;
        int cols = BoardConfig.ColumnCount;

        TileNumber GetTile(int i)
        {
            return (i >= 0 && i < allTiles.Count) ? allTiles[i] : null;
        }

        int row = index / cols;
        int col = index % cols;

        // Check horizontal triple
        if (col >= 1 && col < cols - 1)
        {
            var left = GetTile(index - 1);
            var right = GetTile(index + 1);
            if (left != null && right != null && left.Value == value && right.Value == value)
                return false;
        }

        // Check vertical triple
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
