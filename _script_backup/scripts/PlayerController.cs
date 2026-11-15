using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace PushBox
{
    /// <summary>
    /// 玩家：回合制网格移动。按一次方向键只走一格，动画没结束时忽略输入。
    /// 推箱判定全在 TryMove 里：撞墙就停；撞箱子再看箱子后面那格能不能进。
    /// 输入同时兼容旧 Input Manager 与新 Input System（靠 Unity 的编译宏切换）。
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        public Vector2Int Cell { get; private set; }
        public bool IsMoving { get; private set; }
        public float moveDuration = 0.12f;

        [SerializeField] int sortingOrder = 10;

        Board board;
        Grid grid;
        GameManager game;

        public void Init(Board owner, Grid ownerGrid, Vector2Int cell)
        {
            board = owner;
            grid = ownerGrid;
            Cell = cell;
            transform.position = GridUtil.CellCenter(grid, cell);

            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = sortingOrder;
        }

        void Update()
        {
            if (board == null || IsMoving) return;
            if (game == null) game = GameManager.Instance;
            if (game != null && game.Won) return;

            Vector2Int dir = ReadDirection();
            if (dir != Vector2Int.zero) TryMove(dir);
        }

        /// <summary>读取本帧按下的方向；没有按下返回 zero。</summary>
        static Vector2Int ReadDirection()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb == null) return Vector2Int.zero;

            if (kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame) return Vector2Int.up;
            if (kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame) return Vector2Int.down;
            if (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame) return Vector2Int.left;
            if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame) return Vector2Int.right;
            return Vector2Int.zero;
#else
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) return Vector2Int.up;
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) return Vector2Int.down;
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) return Vector2Int.left;
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) return Vector2Int.right;
            return Vector2Int.zero;
#endif
        }

        /// <summary>返回是否真的移动了；失败（撞墙或箱子堵死）时不产生任何记录。</summary>
        public bool TryMove(Vector2Int dir)
        {
            if (board == null || IsMoving) return false;

            Vector2Int next = Cell + dir;
            if (board.IsWall(next.x, next.y)) return false;

            Box box = board.BoxAt(next.x, next.y);
            MoveRecord record;

            if (box != null)
            {
                Vector2Int beyond = next + dir;
                if (board.IsWall(beyond.x, beyond.y)) return false;
                if (board.BoxAt(beyond.x, beyond.y) != null) return false;

                record = new MoveRecord
                {
                    playerFrom = Cell,
                    playerTo = next,
                    box = box,
                    boxFrom = next,
                    boxTo = beyond
                };
            }
            else
            {
                record = new MoveRecord { playerFrom = Cell, playerTo = next };
            }

            StartCoroutine(MoveRoutine(record));
            return true;
        }

        IEnumerator MoveRoutine(MoveRecord record)
        {
            IsMoving = true;

            if (record.PushedBox) StartCoroutine(record.box.MoveRoutine(record.boxTo, moveDuration));

            Vector3 start = transform.position;
            Vector3 end = GridUtil.CellCenter(grid, record.playerTo);
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.0001f, moveDuration);
                transform.position = Vector3.Lerp(start, end, Mathf.Clamp01(t));
                yield return null;
            }
            transform.position = end;
            Cell = record.playerTo;
            IsMoving = false;

            if (game == null) game = GameManager.Instance;
            if (game != null) game.OnPlayerMoved(record);
        }

        /// <summary>撤销：不播动画，直接把玩家和箱子放回原格。</summary>
        public void ApplyUndo(MoveRecord record)
        {
            if (record.PushedBox) record.box.PlaceAt(record.boxFrom);
            Cell = record.playerFrom;
            transform.position = GridUtil.CellCenter(grid, record.playerFrom);
        }
    }
}
