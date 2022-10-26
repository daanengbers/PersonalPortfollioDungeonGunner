using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RoomNodeSO : ScriptableObject
{
    [HideInInspector] public string id;
     public List<string> parentRoomNodeIDList = new List<string>();
     public List<string> childRoomNodeIDList = new List<string>();
    [HideInInspector] public RoomNodeGraphSO roomNodeGraph;
    public RoomNodeTypeSO roomNodeType;
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;

    #region Editor Code
#if UNITY_EDITOR
   
    [HideInInspector] public Rect rect;
    [HideInInspector] public bool isLeftClickDragging = false;
    [HideInInspector] public bool isSelected = false;

    public void Initialise(Rect rect, RoomNodeGraphSO nodeGraph, RoomNodeTypeSO roomNodeType)
    {
        this.rect = rect;
        this.id = Guid.NewGuid().ToString();
        this.name = "RoomNode";
        this.roomNodeGraph = nodeGraph;
        this.roomNodeType = roomNodeType;

        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    public void Draw(GUIStyle nodeStyle)
    {
        // draw node box using begin area
        GUILayout.BeginArea(rect, nodeStyle);

        // start region to detect popup selection changes
        EditorGUI.BeginChangeCheck();

        //if the room node has a parent or is of type entrance, display a lable, else, display a popup
        if (parentRoomNodeIDList.Count > 0 || roomNodeType.IsEntrance)
        {
            //display a label that cant be changed
            EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);
        }
        else
        {

            // display a popup using the RoomNodeType name values that can be selected from (default to currently set roomNodeType)
            int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);

            int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());

            roomNodeType = roomNodeTypeList.list[selection];

            //if the room type selection has changed making child connections potentially invalid
            if (roomNodeTypeList.list[selected].IsCorridor && !roomNodeTypeList.list[selection].IsCorridor ||
                !roomNodeTypeList.list[selected].IsCorridor && roomNodeTypeList.list[selection].IsCorridor ||
                !roomNodeTypeList.list[selected].IsBossRoom && roomNodeTypeList.list[selection].IsBossRoom)
            {

                //if a room node type has been changed and it already has children then delete the parent child links since we need to revalidate
                    if (childRoomNodeIDList.Count > 0)
                    {
                        for (int i = childRoomNodeIDList.Count - 1; i >= 0; i--)
                        {
                            // get child room node
                            RoomNodeSO childRoomNode = roomNodeGraph.GetRoomNode(childRoomNodeIDList[i]);

                            //if the child room node is selected
                            if (childRoomNode != null)
                            {
                                //remove childID from parent room node
                               RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);

                                //remove parentID from child room node
                               childRoomNode.RemoveParentRoomNodeIDFromRoomNode(id);
                            }
                        }
                    }
                
            }

        }
        if (EditorGUI.EndChangeCheck())
        
            EditorUtility.SetDirty(this);

            GUILayout.EndArea();
        
    }

    public string[] GetRoomNodeTypesToDisplay()
    {
        string[] roomArray = new string[roomNodeTypeList.list.Count];

        for (int i = 0; i < roomNodeTypeList.list.Count; i ++)
        {
            if(roomNodeTypeList.list[i].DisplayInNodeGraphEditor)
            {
                roomArray[i] = roomNodeTypeList.list[i].roomNodeTypeName;
            }
        }
        return roomArray;
    }

    public void ProcessEvents(Event currentEvent)
    {
        // switch cases by different eventTypes and call their functions
        switch (currentEvent.type)
        {

            case EventType.MouseDown:
                processMouseDownEvent(currentEvent);
                break;
            case EventType.MouseUp:
                processMouseUpEvent(currentEvent);
                break;
            case EventType.MouseDrag:
                processMouseDragEvent(currentEvent);
                break;

            default:
                break;
        }
    }

    private void processMouseDownEvent(Event currentEvent)
    {
        // if left mouse button is down run process
        if(currentEvent.button == 0)
        {
            processLeftClickDownEvent();
        }

        // if right mouse button is down run process
        if (currentEvent.button == 1)
        {
            processRightClickDownEvent(currentEvent);
        }
    }

    private void processLeftClickDownEvent()
    {
        //selects the object in the unity editor when clicked
        Selection.activeObject = this;

        // toggle node selection
        if (isSelected == true)
        {
            isSelected = false;
        }
        else
        {
            isSelected = true;
        }

    }

    private void processRightClickDownEvent(Event currentEvent)
    {
        roomNodeGraph.SetNodeToDrawLConnectionLineFrom(this, currentEvent.mousePosition);
    }

    private void processMouseUpEvent(Event currentEvent)
    {
        // if left mouse button goes up call function
        if (currentEvent.button == 0)
        {
            processLeftClickUpEvent();
        }
    }

    private void processLeftClickUpEvent()
    {
        // if left mouse button goes up during dragging, set dragging to false
        if (isLeftClickDragging)
        {
            isLeftClickDragging = false;
        }
    }

    private void processMouseDragEvent(Event currentEvent)
    {
        // if left mouse button is dragging call function
        if(currentEvent.button == 0)
        {
            processLeftMouseDragEvent(currentEvent);
        }
    }

    private void processLeftMouseDragEvent(Event currentEvent)
    {
        isLeftClickDragging = true;

        // currentEvent.delta measures the relative distance between the last event end the mouse position
        DragNode(currentEvent.delta);
        GUI.changed = true;
    }

     public void DragNode(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }

    public bool AddChildRoomNodeIDToRoomNode(string childID)
    {
        // check if a child node is valid to add to a parent
        if (IsChildRoomValid(childID))
        {
            childRoomNodeIDList.Add(childID);
            return true;
        }

        return false;
    }

    // validation check for child parent connections
    public bool IsChildRoomValid(string childID)
    {
        bool isConnectedBossNodeAlready = false;

        //check if there is already a connected boss node in the node graph
        foreach (RoomNodeSO roomNode in roomNodeGraph.roomNodeList)
        {
            if (roomNode.roomNodeType.IsBossRoom && roomNode.parentRoomNodeIDList.Count > 0)
            {
                isConnectedBossNodeAlready = true;
            }
        }
        
        // if the child node has a type of boss room and there is already a connected boss room node then return false
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.IsBossRoom && isConnectedBossNodeAlready)
        {
            return false;
        }

        // if the child node has a type of none then return false
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.IsNone)
        {
            return false;
        }

        // if the node already has a child with this child id return false
        if (childRoomNodeIDList.Contains(childID))
        {
            return false;
        }

        // if child id and this id are the same return false
        if(id == childID)
        {
            return false;
        }

        // if child id is already in parent list return false
        if (parentRoomNodeIDList.Contains(childID))
        {
            return false;
        }

        // if child already has a parent return false
        if(roomNodeGraph.GetRoomNode(childID).parentRoomNodeIDList.Count > 0)
        {
            return false;
        }

        // if child is corridor and this node is coridor return false
        if(roomNodeGraph.GetRoomNode(childID).roomNodeType.IsCorridor && roomNodeType.IsCorridor)
        {
            return false;
        }

        // if this child room is not a coridor and this room is not a corridor return false
        if(!roomNodeGraph.GetRoomNode(childID).roomNodeType.IsCorridor && !roomNodeType.IsCorridor)
        {
            return false;
        }

        // if adding a corridor, check that this node has the maximun permitted child corridors else returne false
        if(roomNodeGraph.GetRoomNode(childID).roomNodeType.IsCorridor && childRoomNodeIDList.Count >= Settings.maxChildCorridors)
        {
            return false;
        }

        // if the child room is an entrance return false
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.IsEntrance)
        {
            return false;
        }

        // if adding a room to a corridor, check that this corridor node doesnt already have a room added
        if(!roomNodeGraph.GetRoomNode(childID).roomNodeType.IsCorridor && childRoomNodeIDList.Count > 0)
        {
            return false;
        }

        else
        {
            return true;
        }
    }

    public bool AddParentRoomNodeIDToRoomNode(string parentID)
    {
        parentRoomNodeIDList.Add(parentID);
        return true;
    }

    // remove childID from the node
    public bool RemoveChildRoomNodeIDFromRoomNode(string childID)
    {
        //if the node contains the child id then remove it
        if (childRoomNodeIDList.Contains(childID))
        {
            childRoomNodeIDList.Remove(childID);
            return true;
        }
        return false;
    }

    // remove parentID from the node
    public bool RemoveParentRoomNodeIDFromRoomNode(string parentID)
    {
        //if the node contains the parent id then remove it
        if (parentRoomNodeIDList.Contains(parentID))
        {
            parentRoomNodeIDList.Remove(parentID);
            return true;
        }
        return false;
    }

#endif
    #endregion Editor Code
}
