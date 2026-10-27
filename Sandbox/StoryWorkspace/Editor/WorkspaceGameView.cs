using System;
using ProjectLove.Core;
using ProjectLove.EditorTools;
using ProjectLove.UI;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace ProjectLove.StorySandbox
{
    // The saved game view renders the very same detached session inspected by State and Trace.
    public sealed class WorkspaceGameView:IDisposable
    {
        readonly PreviewRenderUtility renderer;
        readonly IsometricRoomView view;
        readonly GameSession session;
        public WorkspaceGameView(GameSession session)
        {
            this.session=session;renderer=new PreviewRenderUtility();
            try
            {
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(ArtistWorkspaceBuilder.PrefabPath);
                if(prefab==null)throw new InvalidOperationException("Game view prefab not found.");
                var root=UnityEngine.Object.Instantiate(prefab);root.hideFlags=HideFlags.HideAndDontSave;renderer.AddSingleGO(root);
                view=root.GetComponent<IsometricRoomView>();view.config=session.Config;view.presentation=session.Config.romance.presentation;view.InitializeEditorPreview(session);
                var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=renderer.camera;
                var rect=root.GetComponent<RectTransform>();rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,1920);rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,1080);
                rect.localScale=Vector3.one*.01f;rect.position=new Vector3(0,0,5);
                renderer.camera.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);renderer.camera.cullingMask=~0;renderer.camera.cameraType=CameraType.Game;
                renderer.camera.orthographic=true;renderer.camera.orthographicSize=5.4f;renderer.camera.nearClipPlane=.01f;renderer.camera.farClipPlane=100;
                renderer.camera.clearFlags=CameraClearFlags.SolidColor;renderer.camera.backgroundColor=Color.black;
            }
            catch{renderer.Cleanup();throw;}
        }
        public Texture Render()
        {
            view.Refresh();view.Authored.PreviewScreen(session.Dialogue.IsActive?AuthoringScreen.Conversation:AuthoringScreen.Room,null);view.RevealAll();
            renderer.BeginPreview(new Rect(0,0,960,540),GUIStyle.none);renderer.camera.aspect=16f/9f;
            Canvas.ForceUpdateCanvases();foreach(var t in view.GetComponentsInChildren<TMP_Text>())t.ForceMeshUpdate();renderer.Render(true);return renderer.EndPreview();
        }
        public void Dispose()=>renderer.Cleanup();
    }
}
