using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeType_", menuName = "Scriptable Objects/Dungeon/Room Node Type")]
public class RoomNodeTypeSO : ScriptableObject
{
    public string roomNodeTypeName;

    #region Header
    [Header("Only flag the RoomNodeTypes that should be visible in the editor")]
    #endregion Header
    public bool DisplayInNodeGraphEditor = true;
    #region
    [Header("One type should be a corridor")]
    #endregion
    public bool IsCorridor;
    #region
    [Header("One type should be a CorridorNS")]
    #endregion
    public bool IsCorridorNS;
    #region
    [Header("One type should be a CorridorEW")]
    #endregion
    public bool IsCorridorEW;
    #region
    [Header("One type should be an Entrance")]
    #endregion
    public bool IsEntrance;
    #region
    [Header("One type should be a Boss Room")]
    #endregion
    public bool IsBossRoom;
    #region
    [Header("One tyoe should be none (unassigned)")]
    #endregion
    public bool IsNone;

    #region validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(roomNodeTypeName), roomNodeTypeName);
    }
#endif
        #endregion
    
}
