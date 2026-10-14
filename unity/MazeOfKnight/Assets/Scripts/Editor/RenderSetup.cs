using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MazeOfKnight.EditorTools
{
    // URP con el renderer 2D (luces 2D y posproceso) como pipeline del proyecto: Assets/Settings/Render/.
    // Los assets se versionan (son configuración, no datos de la ROM).
    public static class RenderSetup
    {
        const string Folder = "Assets/Settings/Render";
        const string RendererPath = Folder + "/MazeRenderer2D.asset";
        const string PipelinePath = Folder + "/MazeURP2D.asset";

        [MenuItem("Maze of Knight/Configurar render 2D (URP)", false, 15)]
        public static void Setup()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Settings")) AssetDatabase.CreateFolder("Assets", "Settings");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Settings", "Render");

            var renderer = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(RendererPath);
            if (renderer == null)
            {
                // Renderer2DMenus.CreateRendererAsset es interno: rellena los materiales y el PostProcessData por defecto.
                var menus = AppDomain.CurrentDomain.GetAssemblies()
                    .Select(a => a.GetType("UnityEditor.Rendering.Universal.Renderer2DMenus"))
                    .First(t => t != null);
                var create = menus.GetMethod("CreateRendererAsset", BindingFlags.Static | BindingFlags.NonPublic);
                renderer = (ScriptableRendererData)create.Invoke(null, new object[] { RendererPath, RendererType._2DRenderer, false, "Renderer" });
            }

            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(pipeline, PipelinePath);
            }
            pipeline.supportsHDR = true;      // el bloom necesita HDR
            pipeline.msaaSampleCount = 1;
            EditorUtility.SetDirty(pipeline);

            GraphicsSettings.defaultRenderPipeline = pipeline;
            int current = QualitySettings.GetQualityLevel();
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = pipeline;
            }
            QualitySettings.SetQualityLevel(current, false);
            AssetDatabase.SaveAssets();
            Debug.Log($"[RenderSetup] {PipelinePath} con {renderer.GetType().Name}");
        }
    }
}
