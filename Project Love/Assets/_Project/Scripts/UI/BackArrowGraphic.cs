using UnityEngine;
using UnityEngine.UI;

namespace ProjectLove.UI
{
    // A resolution-independent UI icon; no extra texture asset is required.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class BackArrowGraphic : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear(); var r = GetPixelAdjustedRect();
            Vector2 tip = new Vector2(r.xMin + r.width * .22f, r.center.y);
            Segment(mesh, tip, new Vector2(r.xMin + r.width * .78f, r.center.y), r.height * .08f);
            Segment(mesh, tip, tip + new Vector2(r.width * .25f, r.height * .25f), r.height * .08f);
            Segment(mesh, tip, tip + new Vector2(r.width * .25f, -r.height * .25f), r.height * .08f);
        }
        private void Segment(VertexHelper mesh, Vector2 a, Vector2 b, float width)
        {
            Vector2 side = new Vector2(-(b - a).y, (b - a).x).normalized * width / 2;
            int start = mesh.currentVertCount;
            foreach (Vector2 p in new[] { a - side, a + side, b + side, b - side }) mesh.AddVert(p, color, Vector2.zero);
            mesh.AddTriangle(start, start + 1, start + 2); mesh.AddTriangle(start, start + 2, start + 3);
        }
    }
}
