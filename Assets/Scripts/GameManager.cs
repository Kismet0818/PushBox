using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;



namespace PushBox
{
    public class GameManager:MonoBehaviour
    {
        [SerializeField] Font uiFont;

        public static GameManager Instance{get;private set;}

        public LevelBuilder Level{get;private set;}
        public int Moves{get;private set;}
        public bool Won{get;private set;}

        readonly Stack<MoveRecord> History=new Stack<MoveRecord>();
        
        GUIStyle LabelStyle;
        GUIStyle WinStyle;

        public void Awake()
        {
            if(Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Start()
        {
            if(Level == null)
            {
                Level = FindFirstObjectByType<LevelBuilder>();
            }
        }

        public void OnPlayerMoved(MoveRecord record)
        {
            Moves++;
            History.Push(record);

            if(AllBoxOnTarget())
            {
                Won = true;
                Debug.Log($"[PushBox] 过关！步数 {Moves}");
            }
        }

        bool AllBoxOnTarget()
        {
            if(Level == null || Level.Boxes.Count == 0)
                return false;
            foreach(var box in Level.Boxes)
                if(!box.OnTarget)
                    return false;
            return true;
        }

        void Update()
        {
            if(Level == null)
                Level = FindFirstObjectByType<LevelBuilder>();

            if(RestartPressed())
                Restart();
            else if(UndoPressed())
                Undo();
        }

        static bool RestartPressed()
        {
            var kb = Keyboard.current;
            return kb != null && kb.rKey.wasPressedThisFrame;
        }

        static bool UndoPressed()
        {
            var kb = Keyboard.current;
            return kb != null && kb.zKey.wasPressedThisFrame;
        }

        public void Restart()
        {
            if(Level == null)
                return;
            
            History.Clear();
            Moves=0;
            Won=false;
            Level.Rebuild();
        }

        public void Undo()
        {
            if(Level == null || Level.Player == null)
                return;
            if(Level.Player.IsMoving || History.Count == 0)
                return;

            MoveRecord record=History.Pop();
            Level.Player.ApplyUndo(record);

            Moves=Mathf.Max(0,Moves-1);
            Won=false;
        }

        void OnGUI()
        {
            if(LabelStyle == null)
                LabelStyle=new GUIStyle(GUI.skin.label) {fontSize = 20,font = uiFont};

            GUI.Label(new Rect(16,10,900,30),
                $"PushBox    Steps: {Moves}    WASD Move    R Restart  Z Undo",LabelStyle);

            if(!Won)
                return;
            
            if(WinStyle == null)
            {
                WinStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 44,
                    font = uiFont,
                    alignment = TextAnchor.MiddleCenter
                };
                WinStyle.normal.textColor = Color.yellow;
            }

            GUI.Label(new Rect(0,Screen.height*0.4f,Screen.width,70), $"PASS!!! Steps: {Moves} (R Restart)", WinStyle);

        }

    }

}