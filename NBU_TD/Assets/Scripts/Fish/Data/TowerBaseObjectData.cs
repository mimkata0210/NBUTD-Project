using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class TowerBaseObjectData : WorldObjectData
{
    public enum TowerTypes
    {
        Catcher = 0,
        Moth = 1,
        Scorpion = 2
    }
    public enum TowerLevel
    {
        Default = 0,
        Lvl1 = 1,
        Lvl2 = 2
    }
    public TowerTypes towerType { get; private set; }
    public TowerLevel towerLevel { get; private set; }

    public List<Tile> myTiles;
    public int playerOwner;

    public void SetTowerBaseType(TowerTypes type) { towerType = type; }
    public void SetTowerBaseLevel(TowerLevel level) { towerLevel = level; }
}