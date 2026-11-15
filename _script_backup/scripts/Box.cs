using System.Collections;
using UnityEngine;

namespace PushBox
{
    /// <summary>
    /// 箱子：只保存"我在第几格"这一个逻辑状态，位置由格坐标换算而来。
    /// 站到目标点上会染成暖色，作为最省事的"到位"反馈。
    /// </summary>
    public class Box : MonoBehaviour
    {
        public Vector2Int Cell { get; private set; }
        public bool OnTarget { get; private set; }

        [SerializeField] int sortingOrder = 5;

        Board board;
        Grid grid;
        SpriteRenderer sr;

        static readonly Color NormalColor = Color.white;
        static readonly Color TargetColor = new Color(1f, 0.83f, 0.4f);

        public void Init(Board owner, Grid ownerGrid, Vector2Int cell)
        {
            board = owner;
            grid = ownerGrid;
            sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sortingOrder = sortingOrder;
            PlaceAt(cell);
        }

        /// <summary>瞬移到某一格（开局摆放与撤销都用它），同时维护棋盘上的占位。</summary>
        public void PlaceAt(Vector2Int cell)
        {
            if (board != null && board.BoxAt(Cell.x, Cell.y) == this) board.ClearBox(Cell.x, Cell.y);
            Cell = cell;
            if (board != null) board.SetBox(this, cell.x, cell.y);
            transform.position = GridUtil.CellCenter(grid, cell);
            Refresh();
        }

        /// <summary>平滑移动一格；逻辑占位在动画开始时就更新，避免被重复推。</summary>
        public IEnumerator MoveRoutine(Vector2Int to, float duration)
        {
            if (board != null) board.ClearBox(Cell.x, Cell.y);
            Cell = to;
            if (board != null) board.SetBox(this, to.x, to.y);
            Refresh();

            Vector3 start = transform.position;
            Vector3 end = GridUtil.CellCenter(grid, to);
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.0001f, duration);
                transform.position = Vector3.Lerp(start, end, Mathf.Clamp01(t));
                yield return null;
            }
            transform.position = end;
        }

        public void Refresh()
        {
            OnTarget = board != null && board.IsTarget(Cell.x, Cell.y);
            if (sr != null) sr.color = OnTarget ? TargetColor : NormalColor;
        }
    }
}
