using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace PushBox
{
    /// <summary>
    /// 关卡装配：从 Tilemap 里读出"哪里有墙、哪里是目标、箱子/玩家从哪开始"，
    /// 然后生成箱子与玩家。整个关卡的地图数据就是那几个 Tilemap，没有额外配置文件。
    ///
    /// 外观优先级：预制体自带的 Sprite → 下面拖的 Sprite → 代码生成的色块。
    /// </summary>
    public class LevelBuilder : MonoBehaviour
    {
        [Header("Tilemap（留空会按名字自动找）")]
        [SerializeField] Tilemap wallTilemap;
        [SerializeField] Tilemap targetTilemap;
        [SerializeField] Tilemap spawnBoxTilemap;
        [SerializeField] Tilemap spawnPlayerTilemap;

        [Header("预制体（留空则用下面的贴图或色块）")]
        [SerializeField] Box boxPrefab;
        [SerializeField] PlayerController playerPrefab;

        [Header("贴图（没有预制体时使用）")]
        [SerializeField] Sprite boxSprite;
        [SerializeField] Sprite playerSprite;

        public Board Board { get; private set; }
        public PlayerController Player { get; private set; }
        public IReadOnlyList<Box> Boxes => boxes;

        readonly List<Box> boxes = new List<Box>();
        Grid grid;
        Transform spawnedRoot;
        Sprite runtimeBoxSprite;
        Sprite runtimePlayerSprite;

        void Awake()
        {
            grid = GetComponentInChildren<Grid>();
            if (grid == null) grid = FindFirstObjectByType<Grid>();

            if (wallTilemap == null) wallTilemap = FindTilemap("Wall");
            if (targetTilemap == null) targetTilemap = FindTilemap("Target");
            if (spawnBoxTilemap == null) spawnBoxTilemap = FindTilemap("SpawnBox");
            if (spawnPlayerTilemap == null) spawnPlayerTilemap = FindTilemap("SpawnPlayer");

            Build();
        }

        Tilemap FindTilemap(string objectName)
        {
            foreach (var tm in FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
                if (tm.name == objectName) return tm;

            Debug.LogWarning($"[PushBox] 没找到名为 {objectName} 的 Tilemap，这一层会被当成空。");
            return null;
        }

        /// <summary>读取 Tilemap 并生成所有动态物体。可以重复调用。</summary>
        public void Build()
        {
            if (wallTilemap == null)
            {
                Debug.LogError("[PushBox] 缺少 Wall Tilemap，棋盘无法生成。");
                return;
            }

            BoundsInt bounds = wallTilemap.cellBounds;
            if (bounds.size.x <= 0 || bounds.size.y <= 0)
            {
                Debug.LogError("[PushBox] Wall 层是空的——先用 Tile Palette 刷出墙再运行。");
                return;
            }

            Board = new Board(bounds.xMin, bounds.yMin, bounds.size.x, bounds.size.y);

            foreach (var pos in wallTilemap.cellBounds.allPositionsWithin)
                if (wallTilemap.HasTile(pos)) Board.SetWall(pos.x, pos.y);

            if (targetTilemap != null)
                foreach (var pos in targetTilemap.cellBounds.allPositionsWithin)
                    if (targetTilemap.HasTile(pos)) Board.SetTarget(pos.x, pos.y);

            EnsureSpawnedRoot();
            SpawnBoxes();
            SpawnPlayer();
            HideSpawnLayers();
        }

        void EnsureSpawnedRoot()
        {
            if (spawnedRoot != null) return;
            var go = new GameObject("Spawned");
            go.transform.SetParent(transform, false);
            spawnedRoot = go.transform;
        }

        void SpawnBoxes()
        {
            if (spawnBoxTilemap == null) return;

            foreach (var pos in spawnBoxTilemap.cellBounds.allPositionsWithin)
            {
                if (!spawnBoxTilemap.HasTile(pos)) continue;

                Vector2Int cell = new Vector2Int(pos.x, pos.y);
                if (Board.IsWall(cell.x, cell.y))
                {
                    Debug.LogWarning($"[PushBox] 箱子出生点 {cell} 落在墙上，已跳过。");
                    continue;
                }

                Box box = boxPrefab != null ? Instantiate(boxPrefab, spawnedRoot) : CreateRuntimeBox();
                EnsureSprite(box.gameObject, boxSprite, ref runtimeBoxSprite,
                             new Color(0.72f, 0.5f, 0.28f), new Color(0.35f, 0.22f, 0.1f));
                box.Init(Board, grid, cell);
                boxes.Add(box);
            }
        }

        void SpawnPlayer()
        {
            Vector2Int cell = new Vector2Int(Board.MinX + Board.Width / 2, Board.MinY + 1);
            bool found = false;

            if (spawnPlayerTilemap != null)
            {
                foreach (var pos in spawnPlayerTilemap.cellBounds.allPositionsWithin)
                {
                    if (!spawnPlayerTilemap.HasTile(pos)) continue;
                    cell = new Vector2Int(pos.x, pos.y);
                    found = true;
                    break;
                }
            }

            if (!found) Debug.LogWarning("[PushBox] 没找到玩家出生点，先放在棋盘底部中央。");

            Player = playerPrefab != null ? Instantiate(playerPrefab, spawnedRoot) : CreateRuntimePlayer();
            EnsureSprite(Player.gameObject, playerSprite, ref runtimePlayerSprite,
                         new Color(0.35f, 0.7f, 1f), new Color(0.1f, 0.3f, 0.6f));
            Player.Init(Board, grid, cell);
        }

        /// <summary>只关渲染器，保留 tile 数据——这样重开时可以重新读一次出生点。</summary>
        void HideSpawnLayers()
        {
            foreach (var tm in new[] { spawnBoxTilemap, spawnPlayerTilemap })
            {
                if (tm == null) continue;
                var r = tm.GetComponent<TilemapRenderer>();
                if (r != null) r.enabled = false;
            }
        }

        /// <summary>重开：清掉动态物体后重新读 Tilemap。</summary>
        public void Rebuild()
        {
            if (spawnedRoot != null)
                for (int i = spawnedRoot.childCount - 1; i >= 0; i--)
                    Destroy(spawnedRoot.GetChild(i).gameObject);

            boxes.Clear();
            Player = null;
            Build();
        }

        Box CreateRuntimeBox()
        {
            var go = new GameObject("Box");
            go.transform.SetParent(spawnedRoot, false);
            go.AddComponent<SpriteRenderer>();
            return go.AddComponent<Box>();
        }

        PlayerController CreateRuntimePlayer()
        {
            var go = new GameObject("Player");
            go.transform.SetParent(spawnedRoot, false);
            go.AddComponent<SpriteRenderer>();
            return go.AddComponent<PlayerController>();
        }

        /// <summary>按优先级补图：预制体已有图 > 拖进来的 Sprite > 代码生成色块。</summary>
        static void EnsureSprite(GameObject go, Sprite preferred, ref Sprite cache, Color fill, Color border)
        {
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr == null) sr = go.AddComponent<SpriteRenderer>();

            if (sr.sprite != null) return;

            if (preferred != null)
            {
                sr.sprite = preferred;
                return;
            }

            if (cache == null) cache = MakeSolidSprite(fill, border);
            sr.sprite = cache;
        }

        static Sprite MakeSolidSprite(Color fill, Color border)
        {
            const int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool edge = x < 3 || y < 3 || x >= size - 3 || y >= size - 3;
                    px[y * size + x] = edge ? border : fill;
                }
            }

            tex.SetPixels(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}
