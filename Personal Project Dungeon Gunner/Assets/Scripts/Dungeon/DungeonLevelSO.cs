using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenuAttribute(fileName = "DungeonLevel_", menuName = "Scriptable Objects/Dungeon/Dungeon level")]

public class DungeonLevelSO : ScriptableObject
{

    #region Header BASIC LEVEL DETAILS
    [Space(10)]
    [Header("BASIC LEVEL DETAILS")]
    #endregion header BASIC LEVEL DETAILS
    #region Tooltip
    [Tooltip("The name for the level")]
    #endregion Tooltip

    public string levelName;

    #region Header ROOM TEMPLATES FOR LEVEL
    [Space(10)]
    [Header("ROOM TEMPLATES FOR LEVEL")]
    #endregion Header ROOM TEMPLATES FOR LEVEL
    #region Tooltip
    [Tooltip("Populare the list with the room templates that you want to be part of the level. You need to ensure that room templates are included for " +
        "all room node types that are specified in the room node graphs for the level")]
    #endregion Tooltip

    public List<RoomTemplateSO> roomTemplateList;

    #region Header ROOM NODE GRAPHS FOR THE LEVEL
    [Space(10)]
    [Header("ROOM NODE GRAPHS FOR LEVEL")]
    #endregion Header ROOM NODE GRAPHS FOR THE LEVEL
    #region Tooltip
    [Tooltip("Populate this with the room node graphs which should be randomly selected from the level.")]
    #endregion Tooltip

    public List<RoomNodeGraphSO> roomNodeGraphList;

    #region Validation
#if UNITY_EDITOR

    //validate scriptable object details entered
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(levelName), levelName);
        if (HelperUtilities.ValidateCheckEnumerableValues(this, nameof(roomTemplateList), roomTemplateList))
        return;
        if (HelperUtilities.ValidateCheckEnumerableValues(this, nameof(roomNodeGraphList), roomNodeGraphList))
        return;


        //check to make sure that room templates are specified for all the node types in the specified node graphs

        //first check that north/south coridor, east/west corridor and entrance types have been specified.
        bool isEWCorridor = false;
        bool isNSCorridor = false;
        bool isEntrance = false;

        foreach(RoomTemplateSO roomTemplateSO in roomTemplateList)
        {
            if(roomTemplateSO == null)
            {
                return;
            }

            if (roomTemplateSO.roomNodeType.IsCorridorEW)
            {
                isEWCorridor = true;
            }


            if (roomTemplateSO.roomNodeType.IsCorridorNS)
            {
                isNSCorridor = true;
            }


            if (roomTemplateSO.roomNodeType.IsEntrance)
            {
                isEntrance = true;
            }
        }

        if(isEWCorridor == false)
        {
            Debug.Log("In" + this.name.ToString() + "No EW Corridor Room Type Specified");
        }

        if (isNSCorridor == false)
        {
            Debug.Log("In" + this.name.ToString() + "No NS Corridor Room Type Specified");
        }

        if (isEntrance == false)
        {
            Debug.Log("In" + this.name.ToString() + "No Entrance Room Type Specified");
        }

        //loop trough all node graphs
        foreach(RoomNodeGraphSO roomNodeGraph in roomNodeGraphList)
        {
            if(roomNodeGraph == null)
            {
                return;
            }
            
            //loop trough all nodes in node graph
            foreach(RoomNodeSO roomNodeSO in roomNodeGraph.roomNodeList)
            {
                if(roomNodeSO == null)
                {
                    continue;
                }

                //check that a room template has been specified for each roomnode type

                //corridors and entrances already checked
                if (roomNodeSO.roomNodeType.IsEntrance || roomNodeSO.roomNodeType.IsCorridorNS || roomNodeSO.roomNodeType.IsCorridorEW ||
                    roomNodeSO.roomNodeType.IsCorridor || roomNodeSO.roomNodeType.IsNone)
                {
                    continue;
                }

                bool isRoomNodeTypeFound = false;

                //loop trough all room templates to check that this node type has been specified
                foreach (RoomTemplateSO roomTemplateSO in roomTemplateList)
                {
                    if(roomTemplateSO == null)
                    {
                        continue;
                    }

                    if(roomTemplateSO.roomNodeType == roomNodeSO.roomNodeType)
                    {
                        isRoomNodeTypeFound = true;
                        break;
                    }
                }

                if (!isRoomNodeTypeFound)
                {
                    Debug.Log("In" + this.name.ToString() + ": No room remplate" + roomNodeSO.roomNodeType.name.ToString() + "found for node graph "
                        + roomNodeGraph.name.ToString());
                }
            }
        }


    }


#endif
    #endregion Validation
}
