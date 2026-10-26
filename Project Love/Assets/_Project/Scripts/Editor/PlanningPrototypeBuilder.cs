using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using ProjectLove.Data;
namespace ProjectLove.EditorTools
{
    public static class PlanningPrototypeBuilder
    {
        public const string ScenePath="Assets/_Project/Scenes/Gameplay/StudioPlanningPrototype.unity";
        public const string ConfigPath="Assets/_Project/Data/Planning/PlanningCampaign.asset";
        private const string Root="Assets/_Project/Data/Planning/";
        [Serializable] private sealed class Seed {public string configId,productionId,presentationId,rulesId;public PresentationText[] text;}
        [MenuItem("Tools/Project Love/Open Planning Prototype")]
        public static void BuildOrOpen()
        {
            if(EditorApplication.isPlaying)return;
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            BuildFromCommandLine();EditorSceneManager.OpenScene(ScenePath);
        }
        public static void BuildFromCommandLine()
        {
            StudioPrototypeBuilder.BuildFromCommandLine();
            Directory.CreateDirectory(Root);AssetDatabase.Refresh();
            var game=AssetDatabase.LoadAssetAtPath<GameConfigSO>(ConfigPath);
            if(game==null)
            {
                string json=File.ReadAllText("Assets/_Project/Data/Settings/PlanningSeed.json");var seed=JsonUtility.FromJson<Seed>(json);
                var original=AssetDatabase.LoadAssetAtPath<GameConfigSO>(StudioPrototypeBuilder.ConfigPath);
                var rules=ScriptableObject.CreateInstance<PlanningConfigSO>();JsonUtility.FromJsonOverwrite(json,rules);rules.id=seed.rulesId;
                AssetDatabase.CreateAsset(rules,Root+"PlanningRules.asset");
                var style=UnityEngine.Object.Instantiate(original.production.presentation);style.id=seed.presentationId;
                style.text=style.text.Where(t=>!seed.text.Any(s=>s.key==t.key)).Concat(seed.text).ToArray();
                AssetDatabase.CreateAsset(style,Root+"PlanningPresentation.asset");
                var production=UnityEngine.Object.Instantiate(original.production);production.id=seed.productionId;production.planning=rules;production.presentation=style;
                AssetDatabase.CreateAsset(production,Root+"PlanningProduction.asset");
                game=UnityEngine.Object.Instantiate(original);game.id=seed.configId;game.production=production;
                AssetDatabase.CreateAsset(game,ConfigPath);AssetDatabase.SaveAssets();
            }
            var texts=JsonUtility.FromJson<Seed>(File.ReadAllText("Assets/_Project/Data/Settings/PlanningSeed.json")).text;
            var presentation=game.production.presentation;
            var missing=texts.Where(t=>!presentation.text.Any(p=>p.key==t.key)).ToArray();
            if(missing.Length>0){presentation.text=presentation.text.Concat(missing).ToArray();EditorUtility.SetDirty(presentation);}
            if(!File.Exists(ScenePath))IsometricPrototypeBuilder.BuildScene(game,game.production.presentation,ScenePath);
            AssetDatabase.SaveAssets();
        }
    }
}
