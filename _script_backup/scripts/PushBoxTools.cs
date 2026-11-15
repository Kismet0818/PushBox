using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace PushBox.EditorTools
{
    /// <summary>
    /// 推箱子项目的编辑器工具：打开关卡、静态校验场景装配是否完整。
    /// 校验只读不写，可以把问题在按 Play 之前就暴露出来。
    /// </summary>
    public static class PushBoxTools
    {
        const string ScenePath = "Assets/Scenes/Level01.unity";

        [MenuItem("PushBox/打开 Level01", priority = 1)]
        public static void OpenLevel()
        {
            if (!System.IO.File.Exists(ScenePath))
            {
                Debug.LogError($"[PushBox] 找不到场景 {ScenePath}");
                return;
            }
            EditorSceneManager.OpenScene(ScenePath);
        }

        [MenuItem("PushBox/校验场景装配", priority = 20)]
        public static void Validate()
        {
            int problems = RunChecks(out string report);
            if (problems == 0) Debug.Log("[PushBox] 校验通过：\n" + report);
            else Debug.LogWarning($"[PushBox] 发现 {problems} 个问题：\n" + report);
        }

        /// <summary>给命令行批处理用：跑完按结果退出编辑器。</summary>
        public static void ValidateBatch()
        {
            int problems;
            string report;
            try
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                problems = RunChecks(out report);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[PushBox] 场景打开失败: " + e);
                EditorApplication.Exit(2);
                return;
            }

            if (problems == 0) Debug.Log("PUSHBOX_OK\n" + report);
            else Debug.LogError("PUSHBOX_FAIL\n" + report);

            EditorApplication.Exit(problems == 0 ? 0 : 1);
        }

        static int RunChecks(out string report)
        {
            int problems = 0;
            var sb = new System.Text.StringBuilder();

            var level = Object.FindFirstObjectByType<LevelBuilder>();
            if (level == null) { sb.AppendLine("- 场景里没有 LevelBuilder 组件"); problems++; }
            else sb.AppendLine("- LevelBuilder ✓");

            if (Object.FindFirstObjectByType<GameManager>() == null)
            { sb.AppendLine("- 场景里没有 GameManager 组件"); problems++; }
            else sb.AppendLine("- GameManager ✓");

            var cam = Camera.main;
            if (cam == null) { sb.AppendLine("- 没有 Main Camera"); problems++; }
            else if (cam.GetComponent<CameraFit>() == null)
            { sb.AppendLine("- Main Camera 上没有 CameraFit"); problems++; }
            else sb.AppendLine("- CameraFit ✓");

            if (Object.FindFirstObjectByType<UnityEngine.Rendering.Universal.Light2D>() == null)
            { sb.AppendLine("- 场景里没有 2D 光源（URP 下精灵会渲染成黑色）"); problems++; }
            else sb.AppendLine("- 2D 光源 ✓");

            foreach (string name in new[] { "Wall", "Target", "SpawnBox", "SpawnPlayer", "Floor" })
            {
                Tilemap tm = null;
                foreach (var t in Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
                    if (t.name == name) { tm = t; break; }

                if (tm == null) { sb.AppendLine($"- 缺少 Tilemap：{name}"); problems++; continue; }

                int cells = 0;
                foreach (var pos in tm.cellBounds.allPositionsWithin)
                    if (tm.HasTile(pos)) cells++;

                sb.AppendLine($"- {name}: {cells} 格 {(cells == 0 ? "（空！）" : "")}");
                if (cells == 0) problems++;
            }

            report = sb.ToString();
            return problems;
        }
    }
}
