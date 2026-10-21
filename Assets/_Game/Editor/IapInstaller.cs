using System;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
public static class IapInstaller {
 static SearchRequest search;static AddRequest add;static double deadline;
 public static void Install(){search=Client.Search("com.unity.purchasing",false);deadline=EditorApplication.timeSinceStartup+500;EditorApplication.update+=Poll;}
 static void Poll(){if(EditorApplication.timeSinceStartup>deadline){Done("FAILED timeout");return;}if(search!=null){if(!search.IsCompleted)return;if(search.Status!=StatusCode.Success){Done("FAILED "+search.Error.message);return;}string version=search.Result[0].versions.latestCompatible;search=null;add=Client.Add("com.unity.purchasing@"+version);return;}if(add==null||!add.IsCompleted)return;Done(add.Status==StatusCode.Success?"OK "+add.Result.packageId:"FAILED "+add.Error.message);}
 static void Done(string result){EditorApplication.update-=Poll;Directory.CreateDirectory("Artifacts/Logs");File.WriteAllText("Artifacts/Logs/iap-install.txt",result);}
}
