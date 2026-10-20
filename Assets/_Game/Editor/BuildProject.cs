using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using CashflowChains.Presentation;

public static class BuildProject
{
    public const string ScenePath="Assets/_Game/Scenes/Main.unity";
    [MenuItem("Game/Rebuild scene")]
    public static void CreateScene()
    {
        // Import TMP resources using the package's noninteractive API, never a modal menu.
        if(!File.Exists("Assets/TextMesh Pro/Resources/TMP Settings.asset"))
        {
            TMP_PackageResourceImporter.ImportResources(true,false,false);
        }
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var camera=new GameObject("Main Camera",typeof(Camera)).GetComponent<Camera>();camera.tag="MainCamera";camera.orthographic=true;camera.orthographicSize=5;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.028f,.106f,.165f);camera.transform.position=new Vector3(0,0,-10);
        camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
        var light=new GameObject("Global 2D Light").AddComponent<Light2D>();light.lightType=Light2D.LightType.Global;light.intensity=1;
        new GameObject("Game").AddComponent<GameApp>();
        PlayerSettings.companyName="VirkGames";PlayerSettings.productName="Cashflow Chains";PlayerSettings.bundleVersion="0.1.0";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.virkgames.cashflowchains");
        PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;PlayerSettings.defaultScreenWidth=540;PlayerSettings.defaultScreenHeight=960;PlayerSettings.runInBackground=true;
        PlayerSettings.Android.minSdkVersion=(AndroidSdkVersions)26;PlayerSettings.Android.targetSdkVersion=(AndroidSdkVersions)36;PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneLinux64,false);PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneLinux64,new[]{GraphicsDeviceType.OpenGLCore});
        var stripping=GraphicsSettings.GetRenderPipelineSettings<URPShaderStrippingSetting>();if(stripping!=null)stripping.stripUnusedPostProcessingVariants=true;
        QualitySettings.vSyncCount=0;QualitySettings.SetQualityLevel(0,true);
        EditorSettings.serializationMode=SerializationMode.ForceText;
        // Unity Connect services are not part of this offline mechanic slice.
        var config=AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/UnityConnectSettings.asset");
        if(config.Length>0){var so=new SerializedObject(config[0]);var it=so.GetIterator();while(it.Next(true))if(it.propertyType==SerializedPropertyType.Boolean&&it.name=="m_Enabled")it.boolValue=false;so.ApplyModifiedPropertiesWithoutUndo();}
        EditorSceneManager.SaveScene(scene,ScenePath);EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};AssetDatabase.SaveAssets();Debug.Log("PROJECT_BOOTSTRAP_OK Unity "+Application.unityVersion+" orthographic="+camera.orthographic+" URP="+GraphicsSettings.currentRenderPipeline);
    }
    [MenuItem("Game/Build Linux")]
    public static void Linux()
    {
        Directory.CreateDirectory("Builds/Linux");var r=BuildPipeline.BuildPlayer(new[]{ScenePath},"Builds/Linux/CashflowChains.x86_64",BuildTarget.StandaloneLinux64,BuildOptions.Development);
        if(r.summary.result!=BuildResult.Succeeded)throw new Exception("Linux build failed: "+r.summary.result);
        Directory.CreateDirectory("Artifacts/Logs");File.WriteAllText("Artifacts/Logs/linux-build.txt",r.summary.result+" Unity "+Application.unityVersion+" bytes="+r.summary.totalSize+" errors="+r.summary.totalErrors);
    }
    public static void Android()
    {
        Directory.CreateDirectory("Builds/Android");EditorUserBuildSettings.buildAppBundle=false;
        var r=BuildPipeline.BuildPlayer(new[]{ScenePath},"Builds/Android/CashflowChains-Mechanic-0.1.apk",BuildTarget.Android,BuildOptions.Development);
        if(r.summary.result!=BuildResult.Succeeded)throw new Exception("Android build failed");
    }
}
