using UnityEngine;

namespace PushBox
{
    /// <summary>一次移动的完整记录，用于撤销。</summary>
    public class MoveRecord
    {
        public Vector2Int playerFrom;
        public Vector2Int playerTo;
        public Box box;
        public Vector2Int boxFrom;
        public Vector2Int boxTo;

        public bool PushedBox => box != null;
    }

    /// <summary>
    /// 棋盘数据：墙壁、目标点、箱子占位。
    /// 只认整数格子坐标，不碰 Transform——渲染由 Box / PlayerController 负责。
    /// </summary>
    public class Board
    {
        public int MinX { get; }
        public int MinY { get; }
        public int Width { get; }
        public int Height { get; }

        readonly bool[,] walls;
        readonly bool[,] targets;
        readonly Box[,] boxes;

        public Board(int minX, int minY, int width, int height)
        {
            MinX = minX;
            MinY = minY;
            Width = Mathf.Max(1, width);
            Height = Mathf.Max(1, height);
            walls = new bool[Width, Height];
            targets = new bool[Width, Height];
            boxes = new Box[Width, Height];
        }

        public bool InBounds(int x, int y)
            => x >= MinX && x < MinX + Width && y >= MinY && y < MinY + Height;

        /// <summary>越界一律当墙，这样推箱子到边界会被直接挡住，调用方不用额外判断。</summary>
        public bool IsWall(int x, int y) => !InBounds(x, y) || walls[x - MinX, y - MinY];

        public bool IsTarget(int x, int y) => InBounds(x, y) && targets[x - MinX, y - MinY];

        public void SetWall(int x, int y)
        {
            if (InBounds(x, y)) walls[x - MinX, y - MinY] = true;
        }

        public void SetTarget(int x, int y)
        {
            if (InBounds(x, y)) targets[x - MinX, y - MinY] = true;
        }

        public Box BoxAt(int x, int y) => InBounds(x, y) ? boxes[x - MinX, y - MinY] : null;

        public void SetBox(Box box, int x, int y)
        {
            if (InBounds(x, y)) boxes[x - MinX, y - MinY] = box;
        }

        public void ClearBox(int x, int y)
        {
            if (InBounds(x, y)) boxes[x - MinX, y - MinY] = null;
        }

        /// <summary>棋盘的网格矩形（单位=格），供相机取景使用。</summary>
        public Rect WorldRect => new Rect(MinX, MinY, Width, Height);
    }

    /// <summary>格子坐标 → 世界坐标。Grid 缺失时按 1 单位一格兜底。</summary>
    public static class GridUtil
    {
        public static Vector3 CellCenter(Grid grid, Vector2Int cell)
            => grid != null
                ? grid.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0))
                : new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f);
    }
}
