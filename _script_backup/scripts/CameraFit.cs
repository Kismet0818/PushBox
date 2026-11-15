using UnityEngine;

namespace PushBox
{
    /// <summary>把正交相机调到刚好框住整个棋盘，多关不同尺寸也不用改参数。</summary>
    [RequireComponent(typeof(Camera))]
    public class CameraFit : MonoBehaviour
    {
        public float padding = 0.6f;

        void Start() => FitToLevel();

        public void FitToLevel()
        {
            var level = FindFirstObjectByType<LevelBuilder>();
            if (level == null || level.Board == null) return;

            var cam = GetComponent<Camera>();
            cam.orthographic = true;

            Rect rect = level.Board.WorldRect;
            float halfHeight = rect.height * 0.5f + padding;
            float halfWidth = rect.width * 0.5f + padding;

            cam.orthographicSize = Mathf.Max(halfHeight, halfWidth / Mathf.Max(0.0001f, cam.aspect));
            transform.position = new Vector3(rect.center.x, rect.center.y, -10f);
        }
    }
}
