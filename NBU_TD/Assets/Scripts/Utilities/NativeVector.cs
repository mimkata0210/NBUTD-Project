using Unity.Collections;
using Unity.Mathematics;

public class NativeVisualVector{
    #region Private.
    private NativeArray<float3> array;
    private int lastPosition;
    #endregion

    #region Public.
    public NativeArray<float3> Array { get { return array; } }
    #endregion


}
