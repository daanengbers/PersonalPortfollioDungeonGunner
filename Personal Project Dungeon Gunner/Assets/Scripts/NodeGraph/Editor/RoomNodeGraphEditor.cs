using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyle RoomNodeStyle;

    private const float nodeWidth = 160f;
    private const float nodeHeight = 100f;
    private const int nodePadding = 25;
    private const int nodeBorder = 12;


   [MenuItem("Room Node Graph Editor", menuItem ="Window/Dungeon Editor/Room Node Graph Editor" )]

   private static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Room Node Editor Graph");
    }

    private void OnEnable()
    {
        RoomNodeStyle = new GUIStyle();
        RoomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        RoomNodeStyle.normal.textColor = Color.white;
        RoomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        RoomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);
    }

    private void OnGUI()
    {

        GUILayout.BeginArea(new Rect(new Vector2(100f, 100f), new Vector2(nodeWidth, nodeHeight)), RoomNodeStyle);
        EditorGUILayout.LabelField("node 1");
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(new Vector2(300f, 300f), new Vector2(nodeWidth, nodeHeight)), RoomNodeStyle);
        EditorGUILayout.LabelField("node 1");
        GUILayout.EndArea();
    }
}
