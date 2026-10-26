using System;
using System.IO;
using System.Linq;
using ProjectLove.Core;
using ProjectLove.Data;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace ProjectLove.EditorTools
{
    public static class StudioUiUpdate
    {
        private const string Art = "Assets/_Project/Art/UI/Mockup/";
        [Serializable] private sealed class DependencySeed { public string taskId, displayName; public string[] prerequisites; }
        [Serializable] private sealed class Seed { public int version; public DependencySeed[] dependencies; public PresentationText[] text; }
        [MenuItem("Tools/Project Love/Apply Studio UI and Dependencies")]
        public static void Apply()
        {
            var game = AssetDatabase.LoadAssetAtPath<GameConfigSO>(StudioPrototypeBuilder.ConfigPath);
            if (game == null) return;
            var seed = JsonUtility.FromJson<Seed>(File.ReadAllText("Assets/_Project/Data/Settings/StudioUiSeed.json"));
            var style = game.production.presentation;
            if (style.uiContentVersion < seed.version)
            {
                foreach (string path in Directory.GetFiles(Art, "*.png", SearchOption.AllDirectories))
                {
                    string normalized = path.Replace('\\', '/');
                    var importer = (TextureImporter)AssetImporter.GetAtPath(normalized);
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = normalized.EndsWith("ui_progress_bar_sheet.png", StringComparison.Ordinal) ? SpriteImportMode.Multiple : SpriteImportMode.Single;
                    importer.alphaIsTransparency = true; importer.mipmapEnabled = false;
                    importer.filterMode = FilterMode.Bilinear; importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.spritePixelsPerUnit = 100; importer.isReadable = false;
                    if (normalized.Contains("/Panels/") && !normalized.Contains("frame") && !normalized.Contains("ring")) importer.spriteBorder = Vector4.one * 24;
                    if (importer.spriteImportMode == SpriteImportMode.Multiple)
                    {
                        var factory = new SpriteDataProviderFactories(); factory.Init();
                        var provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
                        var previous = provider.GetSpriteRects(); string[] names = { "track", "fill", "frame", "forecast" };
                        var rects = names.Select((n, i) => new SpriteRect { name = "ui_bar_" + n, rect = new Rect(i * 256, 0, 256, 64),
                            pivot = new Vector2(.5f, .5f), alignment = SpriteAlignment.Center, border = Vector4.one * 16,
                            spriteID = previous.FirstOrDefault(r => r.name == "ui_bar_" + n)?.spriteID ?? GUID.Generate() }).ToArray();
                        provider.SetSpriteRects(rects);
                        provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
                        provider.Apply();
                    }
                    importer.SaveAndReimport();
                }
                Sprite Single(string folder, string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Art + folder + "/ui_" + name + ".png");
                Undo.RecordObject(style, "Bind supplied studio UI");
                var icons = Directory.GetFiles(Art + "Icons", "*.png").Select(p => new PresentationIcon {
                    key = Path.GetFileNameWithoutExtension(p).Substring(3), sprite = AssetDatabase.LoadAssetAtPath<Sprite>(p.Replace('\\', '/')) }).ToArray();
                style.icons = style.icons.Where(i => !icons.Any(n => n.key == i.key)).Concat(icons).ToArray();
                style.panelSprite = Single("Panels", "panel"); style.taskCardSprite = Single("Panels", "task_card");
                style.avatarFrameSprite = Single("Panels", "avatar_frame"); style.selectionRingSprite = Single("Panels", "selection_ring");
                var bars = AssetDatabase.LoadAllAssetsAtPath(Art + "Bars/ui_progress_bar_sheet.png").OfType<Sprite>().ToArray();
                style.barTrack = bars.Single(s => s.name == "ui_bar_track"); style.barFill = bars.Single(s => s.name == "ui_bar_fill");
                style.barFrame = bars.Single(s => s.name == "ui_bar_frame"); style.barForecast = bars.Single(s => s.name == "ui_bar_forecast");
                foreach (var person in game.characters)
                {
                    string key = person.id.StartsWith("studio_", StringComparison.Ordinal) ? person.id.Substring(7) : person.id;
                    Undo.RecordObject(person, "Bind owner portrait"); person.taskAvatar = Single("Portraits", "avatar_" + key); EditorUtility.SetDirty(person);
                }
                foreach (var stream in game.workStreams)
                {
                    Undo.RecordObject(stream, "Bind discipline icon"); stream.icon = style.Icon(stream.id.Replace("work_", "")); EditorUtility.SetDirty(stream);
                }
                style.text = style.text.Where(t => !seed.text.Any(s => s.key == t.key)).Concat(seed.text).ToArray();
                style.useStudioSkin = true; style.uiContentVersion = seed.version; EditorUtility.SetDirty(style);
            }
            if (game.production.dependencyContentVersion < seed.version)
            {
                foreach (var binding in seed.dependencies)
                {
                    var task = game.production.tasks.Single(t => t.id == binding.taskId);
                    Undo.RecordObject(task, "Author studio dependency chain"); task.displayName = binding.displayName;
                    task.prerequisites = binding.prerequisites.Select(id => game.production.tasks.Single(t => t.id == id)).ToArray(); EditorUtility.SetDirty(task);
                }
                _ = new GameSession(game);
                Undo.RecordObject(game.production, "Stamp dependency content"); game.production.dependencyContentVersion = seed.version; EditorUtility.SetDirty(game.production);
            }
            AssetDatabase.SaveAssets();
        }
    }
}
