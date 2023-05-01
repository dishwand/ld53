using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    NORTH,
    EAST,
    SOUTH,
    WEST,
    UP,
    DOWN
}

static class DirectionExtensions 
{
  public static Direction Opposite(this Direction direction) 
  {
    switch (direction) 
    {
      case Direction.NORTH:   return Direction.SOUTH;
      case Direction.SOUTH:   return Direction.NORTH;
      case Direction.EAST:    return Direction.WEST;
      case Direction.WEST:    return Direction.EAST;
      case Direction.UP:      return Direction.DOWN;
      case Direction.DOWN:    return Direction.UP;
      default:    return Direction.DOWN;
    }
  }
}

static class Vector3Extensions
{
    public static Vector3Int Translate(this Vector3Int pos, Direction dir) {
        if (dir == Direction.NORTH) pos.x++;
        else if (dir == Direction.SOUTH) pos.x--;
        else if (dir == Direction.EAST) pos.z--;
        else if (dir == Direction.WEST) pos.z++;
        return pos;
    }
}

static class FloatExtenions
{
    public static float Crunch(this float val, float crunch) {
        return (Mathf.Floor(val * crunch)) / crunch;
    }
}

public class DirectionVectors
{
    //Corresponds to Direction enum
    public static Vector3Int[] rollVectors = {
        Vector3Int.back,
        Vector3Int.left,
        Vector3Int.forward,
        Vector3Int.right
    };

    public static Vector3Int[] worldSpace = {
        new Vector3Int( 1, 0, 0),
        new Vector3Int( 0, 0, -1),
        new Vector3Int( -1, 0, 0),
        new Vector3Int( 0, 0, 1),
        new Vector3Int( 0, 1, 0),
        new Vector3Int( 0, -1, 0)
    };

    public static Vector3[] spawnRots = {
        new Vector3(0, 0, 0),
        new Vector3(0, 90, 0),
        new Vector3(0, 180, 0),
        new Vector3(0, -90, 0),
        new Vector3(0, 0, 90),
        new Vector3(0, 0, -90)
    };
}
public class GridWorldLibrary
{
    // Rows correspond to facing direction
    // Column is the rotator direction
    // Value is the output of rotating face by rotator
    // NOTE: rotator directions actually mean the clockwise rotation that
    //       produces that movement. North rotator is actually 90 deg CW
    //       along the West dir. Therefore, "up" means clockwise rotation,
    //       "down" CCW.
    //NESWUD
    //012345
    public static int[,] directionRotators = {
        { 5, 0, 4, 0, 1, 3 },
        { 1, 5, 1, 4, 2, 0 },
        { 4, 2, 5, 2, 3, 1 },
        { 3, 4, 3, 5, 0, 2 },
        { 0, 1, 2, 3, 4, 4 },
        { 2, 3, 0, 1, 5, 5 }
    };

    public static Direction[] planarDirections = {
        Direction.NORTH,
        Direction.EAST,
        Direction.SOUTH,
        Direction.WEST
    };


    //Returns the top edge between two grids that are on the same Z level.
    public static Vector3 GetTopEdgeBetweenSpots(Vector3Int gp1, Vector3Int gp2)
    {
        Vector3 toReturn = new Vector3((gp1.x + gp2.x) / 2f, gp1.y - 0.5f, (gp1.z + gp2.z) / 2f);
        return toReturn;
    }

    public static Direction RotateFaceByDirection(Direction face, Direction rotator) {
        return (Direction)directionRotators[(int)face, (int)rotator];
    }

    public static Vector3Int TranslatePosByDir(Vector3Int pos, Direction dir) {
        return (pos + DirectionVectors.worldSpace[(int)dir]);
    }
    
    public static Vector3Int TranslatePosByDirX(Vector3Int pos, Direction dir, int x) {
        return (pos + x * DirectionVectors.worldSpace[(int)dir]);
    }

    public static float FlatDist(Vector3 p1, Vector3 p2) {
        return Vector2.Distance(new Vector2(p1.x, p1.z), new Vector2(p2.x, p2.z));
    }

    public static GameObject RaycastInDirection(Direction dir, Vector3Int startPosition) {
        RaycastHit hit;
        Vector3 worldDir = DirectionVectors.worldSpace[(int)dir];
        if (Physics.Raycast(startPosition, worldDir, out hit, 1f)) {
            return hit.collider.gameObject;
        }
        return null;
    }

    public static List<GridObject> RaycastAllInDirection(Direction dir, Vector3Int startPosition) {
        Vector3 worldDir = DirectionVectors.worldSpace[(int)dir];
        RaycastHit[] hits = Physics.RaycastAll(startPosition, worldDir, 1f);
        List<GridObject> objs = new List<GridObject>();
        for (int i = 0; i < hits.Length; i++) {
            GridObject go = hits[i].collider.gameObject.GetComponent<GridObject>();
            if (go != null) {
                objs.Add(go);
            }
        }
        return objs;
    }

    public static bool IsGroundBelowPosition(Vector3Int position, Direction facing = Direction.NORTH, bool isPiston = false) {
        GameObject go = RaycastInDirection(Direction.DOWN, position);
        // oops...
        if (go) {
            if (go.tag == "Immovable") return true;
            if (go.tag == "Grate" && !isPiston) return true;
            if (go.tag == "Ramp" && !isPiston) return false;
            if (go.tag == "Ramp" && isPiston) {
                GridObject grid = go.GetComponent<GridObject>();
                if (grid.dir == facing) return true;
            }
        }
        return false;
    }

    // Translates pos by dir first
    // public static bool IsGroundBelowPosition(Vector3Int position, Direction facing, bool isPiston = false) {
    //     return IsGroundBelowPosition(TranslatePosByDir(position, facing), isPiston);
    // }

}
