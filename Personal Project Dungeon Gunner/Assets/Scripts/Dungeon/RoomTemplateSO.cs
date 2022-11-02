using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu(fileName = "Room_", menuName = "Scriptable Objects/Dungeon/Room")]
public class RoomTemplateSO : ScriptableObject
{
    [HideInInspector] public string guid;

    #region Header ROOM PREFAB

    [Space(10)]
    [Header("ROOM PREFAB")]

    #endregion Header ROOM PREFAB

    #region Tooltip

    [Tooltip("The gameobject prefab for the room (This will contain all the tilemaps for the room and environment game objects)")]

    #endregion Tooltip

    public GameObject Prefab;

    [HideInInspector] public GameObject previousPefab; // this is used to regenerate the guid if the SO is copied and the prefab is changed

    #region Header ROOM CONFIGURATION

    [Space(10)]
    [Header("ROOM CONFIGURATION")]

    #endregion Header ROOM CONFIGURATION

    #region Tooltip

    [Tooltip("The room node type SO, the room node types corrospond to the room nodes used in the room node graph. The exeptions being with corridors." +
        " In the room node graph there is just one corridor type 'Corridor'. For the room templates there are 2 corridor node types - CorridorNS and CorridorEW ")]

    #endregion Tooltip

    public RoomNodeTypeSO RoomNodeType;

    #region ToolTip

    [Tooltip("If you remember a triangle around the room tilemap that just completly encloses it, the room lower bounds represent the bottom left corner" +
        "of that triangle. This should be determinded from the tilemap for the room (Using the cordinate brush pointer to get the tilemap grid position" +
        "for that bottom left corner (note: this is the local tilemap position  and not world position)")]

    #endregion

    public Vector2Int lowerBounds;

    #region ToolTip

    [Tooltip("If you remember a triangle around the room tilemap that just completly encloses it, the room upper bounds represent the top right corner" +
        "of that triangle. This should be determinded from the tilemap for the room (Using the cordinate brush pointer to get the tilemap grid position" +
        "for that top right corner (note: this is the local tilemap position  and not world position)")]

    #endregion

    public Vector2Int UpperBounds;

    #region ToolTip

    [Tooltip("There should be a maximun of four doorways per room - one for each compass direction. These should have a consistent 3 tile wide opening, with the middle " +
        "tile being the doorway coordanite position")]

    #endregion

    [SerializeField] public List<Doorway> doorwayList;

    #region ToolTip

    [Tooltip("Each possible spawn position (Used for enemies and chest) for the room in tilemap coordinates should be added to this array")]

    #endregion

    public Vector2Int[] spawnPossitionArray;

    //returns the list of entrances for the room template

    public List<Doorway> GetDoorwayList()
    {
        return doorwayList;
    }

    #region Validation

#if UNITY_EDITOR


    //Validate SO files

    private void OnValidate()
    {
        //set unique GUID if empty or the prefab changes
        if(guid == "" || previousPefab != Prefab)
        {
            guid = GUID.Generate().ToString();
            previousPefab = Prefab;
            EditorUtility.SetDirty(this);
        }

        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(doorwayList), doorwayList);

        //check spawn positions
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(spawnPossitionArray), spawnPossitionArray);
    }

#endif

    #endregion Validation       
}
