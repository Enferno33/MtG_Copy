using UnityEngine;

[System.Serializable]
public struct TileState
{
    public Vector2 Position;    // The grid position of the tile
    public bool CanMoveUp;      // Can move up from this tile
    public bool CanMoveDown;    // Can move down from this tile
    public bool CanMoveLeft;    // Can move left from this tile
    public bool CanMoveRight;   // Can move right from this tile
}
