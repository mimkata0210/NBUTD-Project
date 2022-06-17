using Mirror;
public class Tile
{
    public Tile(int x, int z, int[] colorIndexs)
    {
        this.x = x;
        this.z = z;
        this.colorIndexs = colorIndexs;
    }

    #region Private.
    private int x;
    private int z;
    private int[] colorIndexs;
    private bool hasTower;
    private TowerBase towerBase;

    #endregion

    #region Pubblic.
    public int X { get { return x; } }
    public int Z { get { return z; } }
    public int[] ColorIndexs { get { return colorIndexs; } }
    public bool HasTower { get { return hasTower; } set { hasTower = value; } }
    public TowerBase TowerBase { get { return towerBase; } set { towerBase = value; } }
    #endregion
}


#region Private Serializer.
public static class TileSerializer
{
    public static void WriteTile(this NetworkWriter writer, Tile tile)
    {
        writer.WriteInt(tile.X);
        writer.WriteInt(tile.Z);
        writer.WriteArray<int>(tile.ColorIndexs);
        writer.WriteBool(tile.HasTower);
        writer.WriteWorldObjectData(tile.TowerBase.Data);
    }

    public static Tile ReadTile(this NetworkReader reader)
    {
        
        int x = reader.ReadInt();
        int z = reader.ReadInt();
        int[] colorIndexs = reader.ReadArray<int>();
        bool hasTower = reader.ReadBool();

        Tile tile = new Tile(x, z, colorIndexs);
        tile.HasTower = hasTower;
        tile.TowerBase = (TowerBase)WorldObjectManager.Instance.ReturnWorldObjectPublic(reader.ReadWorldObjectData());
        return tile;
    }
}
#endregion
