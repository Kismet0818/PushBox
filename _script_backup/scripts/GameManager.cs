using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace PushBox
{
    /// <summary>
    /// 关卡规则：步数统计、胜负判定、撤销与重开。
    /// 用 OnGUI 画一个最小 HUD，省掉第一个版本还要搭 Canvas 的麻烦。
    /// 输入同时兼容旧 Input Manager 与新 Input System（靠 Unity 的编译宏切换）。
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public LevelBuilder Level { get; private set; }
        public int Moves { get; private set; }
        public bool Won { get; private set; }

        readonly Stack<MoveRecord> history = new Stack<MoveRecord>();
        GUIStyle labelStyle;
        GUIStyle winStyle;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            if (Level == null) Level = FindFirstObjectByType<LevelBuilder>();
        }

        /// <summary>玩家成功移动一格后由 PlayerController 调用。</summary>
        public void OnPlayerMoved(MoveRecord record)
        {
            Moves++;
            history.Push(record);

            if (AllBoxesOnTarget())
            {
                Won = true;
                Debug.Log($"[PushBox] 过关！步数 {Moves}");
            }
        }

        bool AllBoxesOnTarget()
        {
            if (Level == null || Level.Boxes.Count == 0) return false;
            foreach (var box in Level.Boxes)
                if (!box.OnTarget) return false;
            return true;
        }

        void Update()
        {
            if (Level == null) Level = FindFirstObjectByType<LevelBuilder>();

            if (RestartPressed()) Restart();
            else if (UndoPressed()) Undo();
        }

        static bool RestartPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            return kb != null && kb.rKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.R);
#endif
        }

        static bool UndoPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            return kb != null && kb.zKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Z);
#endif
        }

        public void Restart()
        {
            if (Level == null) return;

            history.Clear();
            Moves = 0;
            Won = false;
            Level.Rebuild();
        }

        public void Undo()
        {
            if (Level == null || Level.Player == null) return;
            if (Level.Player.IsMoving || history.Count == 0) return;

            MoveRecord record = history.Pop();
            Level.Player.ApplyUndo(record);

            Moves = Mathf.Max(0, Moves - 1);
            Won = false;
        }

        void OnGUI()
        {
            if (labelStyle == null)
                labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 20 };

            GUI.Label(new Rect(16, 10, 900, 30),
                $"推箱子    步数 {Moves}    方向键/WASD 移动    R 重开    Z 撤销", labelStyle);

            if (!Won) return;

            if (winStyle == null)
            {
                winStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 44,
                    alignment = TextAnchor.MiddleCenter
                };
                winStyle.normal.textColor = Color.yellow;
            }

            GUI.Label(new Rect(0, Screen.height * 0.4f, Screen.width, 70), $"过关！用了 {Moves} 步（R 重来）", winStyle);
        }
    }
}
