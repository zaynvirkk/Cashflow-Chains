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
        var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();camera.tag="MainCamera";camera.orthographic=true;camera.orthographicSize=5;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.028f,.106f,.165f);camera.transform.position=new Vector3(0,0,-10);
        camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
        var light=new GameObject("Global 2D Light").AddComponent<Light2D>();light.lightType=Light2D.LightType.Global;light.intensity=1;
        new GameObject("Game").AddComponent<MobileGameApp>();
        PlayerSettings.SplashScreen.show=false;var icon=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Art/AppIcon.png");if(icon)PlayerSettings.SetIcons(NamedBuildTarget.Unknown,new[]{icon},IconKind.Any);
        PlayerSettings.companyName="VirkGames";PlayerSettings.productName="Cashflow Chains";PlayerSettings.bundleVersion="1.0.0";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.virkgames.cashflowchains");
        PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;PlayerSettings.resizableWindow=true;PlayerSettings.fullScreenMode=FullScreenMode.FullScreenWindow;PlayerSettings.runInBackground=true;
        PlayerSettings.Android.forceInternetPermission=false;PlayerSettings.Android.bundleVersionCode=1;PlayerSettings.stripEngineCode=true;
        PlayerSettings.Android.minSdkVersion=(AndroidSdkVersions)26;PlayerSettings.Android.targetSdkVersion=(AndroidSdkVersions)36;PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneLinux64,false);PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneLinux64,new[]{GraphicsDeviceType.OpenGLCore});
        var stripping=GraphicsSettings.GetRenderPipelineSettings<URPShaderStrippingSetting>();if(stripping!=null)stripping.stripUnusedPostProcessingVariants=true;
        QualitySettings.vSyncCount=0;QualitySettings.SetQualityLevel(0,true);
        EditorSettings.serializationMode=SerializationMode.ForceText;
        // Unity Connect services are not part of this offline mechanic slice.
        var config=AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/UnityConnectSettings.asset");
        if(config.Length>0){var so=new SerializedObject(config[0]);var it=so.GetIterator();while(it.Next(true))if(it.propertyType==SerializedPropertyType.Boolean&&(it.name=="m_Enabled"||it.name=="m_EngineDiagnosticsEnabled"))it.boolValue=false;so.ApplyModifiedPropertiesWithoutUndo();}
        EditorSceneManager.SaveScene(scene,ScenePath);EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};AssetDatabase.SaveAssets();Debug.Log("PROJECT_BOOTSTRAP_OK Unity "+Application.unityVersion+" orthographic="+camera.orthographic+" URP="+GraphicsSettings.currentRenderPipeline);
    }
    [MenuItem("Game/Build Linux")]
    public static void Linux()
    {
        CreateScene();StoreIcon();CampaignProof.Export();
        Directory.CreateDirectory("Builds/Linux");var r=BuildPipeline.BuildPlayer(new[]{ScenePath},"Builds/Linux/CashflowChains.x86_64",BuildTarget.StandaloneLinux64,BuildOptions.None);
        if(r.summary.result!=BuildResult.Succeeded)throw new Exception("Linux build failed: "+r.summary.result);
        Directory.CreateDirectory("Artifacts/Logs");File.WriteAllText("Artifacts/Logs/linux-build.txt",r.summary.result+" Unity "+Application.unityVersion+" bytes="+r.summary.totalSize+" errors="+r.summary.totalErrors);
    }
    public static void Android()
    {
        CreateScene();CampaignProof.Export();
        Directory.CreateDirectory("Builds/Android");EditorUserBuildSettings.buildAppBundle=false;
        var r=BuildPipeline.BuildPlayer(new[]{ScenePath},"Builds/Android/CashflowChains-Playtest-1.0.apk",BuildTarget.Android,BuildOptions.None);
        if(r.summary.result!=BuildResult.Succeeded)throw new Exception("Android build failed");
    }
    public static void AndroidBundle()
    {
        CreateScene();CampaignProof.Export();EditorUserBuildSettings.buildAppBundle=true;Directory.CreateDirectory("Builds/Android");
        var r=BuildPipeline.BuildPlayer(new[]{ScenePath},"Builds/Android/CashflowChains-1.0.0-build.aab",BuildTarget.Android,BuildOptions.None);
        if(r.summary.result!=BuildResult.Succeeded)throw new Exception("Android bundle build failed");
    }

    public static void StoreIcon()
    {
        // CPU texture export also works with the CLI's Null graphics device.
        // Preserve the generated source PNG and let Unity sample the standard store size.
        var original=new Texture2D(2,2,TextureFormat.RGBA32,false);
        original.LoadImage(File.ReadAllBytes("Assets/_Game/Art/AppIcon.png"));
        var export=new Texture2D(512,512,TextureFormat.RGB24,false);
        for(int y=0;y<512;y++)for(int x=0;x<512;x++)export.SetPixel(x,y,original.GetPixelBilinear((x+.5f)/512,(y+.5f)/512));
        export.Apply();Directory.CreateDirectory("Release/Store");File.WriteAllBytes("Release/Store/icon-512.png",export.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(export);UnityEngine.Object.DestroyImmediate(original);
    }

}
