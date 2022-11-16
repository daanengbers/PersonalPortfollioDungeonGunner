using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Settings
{
    #region ROOM SETTINGS
    //max number of child corridors leading from a room, max should be 3 although 3 is already not recommended as it can break the dungeon creator
    public const int maxChildCorridors = 3;
    #endregion

    #region DUNGEON BUILD SETTINGS
    public const int maxDungeonRebuildAttemptsForRoomGraph = 1000;
    public const int maxDungeonBuildAttempts = 10;
    #endregion

}
