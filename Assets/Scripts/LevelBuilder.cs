using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace PushBox
{
    public class LevelBuilder : MonoBehaviour
    {
        [Header("Tilemap 配置")]
        [SerializeField] Tilemap wallTilemap;
        [SerializeField] Tilemap targetTilemap;
        [SerializeField] Tilemap spawnBoxTilemap;
        [SerializeField] Tilemap spawnPlayerTilemap;

        [Header("Prefab 配置")]
        [SerializeField] Box boxPrefab;
        [SerializeField] PlayerController playerPrefab;

        public Board Board { get; private set; }
        public PlayerController Player { get; private set; }
        public List<Box> Boxes { get; private set; } = new List<Box>();

        private Grid grid;
        private Transform spawnedRoot;

        void Awake()
        {
            // 强制要求场景里有个 Grid (通常 Tilemap 会自动附带)
            grid = GetComponentInChildren<Grid>(); 
            
            // 创建用于存放生成物体的根节点
            spawnedRoot = new GameObject("Spawned").transform;
            spawnedRoot.SetParent(transform, false);

            Build();
        }

        public void Build()
        {
            // 1. 根据墙壁图层的边界生成棋盘
            BoundsInt bounds = wallTilemap.cellBounds;
            Board = new Board(bounds.xMin, bounds.yMin, bounds.size.x, bounds.size.y);

            // 2. 刷入墙
            foreach (var pos in wallTilemap.cellBounds.allPositionsWithin)
                if (wallTilemap.HasTile(pos)) Board.SetWall(pos.x, pos.y);

            // 3. 刷入目标点
            foreach (var pos in targetTilemap.cellBounds.allPositionsWithin)
                if (targetTilemap.HasTile(pos)) Board.SetTarget(pos.x, pos.y);

            // 4. 生成箱子
            foreach (var pos in spawnBoxTilemap.cellBounds.allPositionsWithin)
            {
                if (!spawnBoxTilemap.HasTile(pos)) continue;
                
                Vector2Int cell = new Vector2Int(pos.x, pos.y);
                Box box = Instantiate(boxPrefab, spawnedRoot);
                box.Init(Board, grid, cell);
                Boxes.Add(box);
            }

            // 5. 生成玩家 (只取玩家图层里的第一个瓦片位置)
            Vector2Int playerCell = new Vector2Int(Board.MinX + Board.Width / 2, Board.MinY + 1);
            foreach (var pos in spawnPlayerTilemap.cellBounds.allPositionsWithin)
            {
                if (spawnPlayerTilemap.HasTile(pos))
                {
                    playerCell = new Vector2Int(pos.x, pos.y);
                    break;
                }
            }
            
            Player = Instantiate(playerPrefab, spawnedRoot);
            Player.Init(Board, grid, playerCell);

            // 6. 隐藏出生点图层
            spawnBoxTilemap.GetComponent<TilemapRenderer>().enabled = false;
            spawnPlayerTilemap.GetComponent<TilemapRenderer>().enabled = false;
        }

        public void Rebuild()
        {
            foreach (Transform child in spawnedRoot)
                Destroy(child.gameObject);
            
            Boxes.Clear();
            Player = null;
            Build();
        }
    }
}