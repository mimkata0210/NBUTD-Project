
[System.Serializable]
public class GroundObjectData : WorldObjectData
{
    #region Types.
    /// <summary>
    /// States a ground may have.
    /// </summary>
    public enum GroundStates
    {
        Default = 0
    }
    #endregion

    public GroundStates GroundState { get; private set; }

    #region Public.
    public int TerrainWidth { get; set; }
    public int TerrainHight { get; set; }
    public int PlayerNumber { get; set; }
    #endregion

    public void SetGroundState(GroundStates state) { GroundState = state; }
        
}
