using UnityEditor;using UnityEditor.PackageManager;using UnityEditor.PackageManager.Requests;using UnityEngine;
public static class FreeReleasePackages {
 static AddAndRemoveRequest request;static double deadline;
 public static void RemoveBilling(){request=Client.AddAndRemove(new string[0],new[]{"com.unity.purchasing"});deadline=EditorApplication.timeSinceStartup+300;EditorApplication.update+=Poll;}
 static void Poll(){if(!request.IsCompleted){if(EditorApplication.timeSinceStartup<deadline)return;Debug.LogError("Free release package cleanup timed out");EditorApplication.Exit(2);return;}EditorApplication.update-=Poll;if(request.Status==StatusCode.Success){Debug.Log("FREE_RELEASE_BILLING_REMOVED");EditorApplication.Exit(0);}else{Debug.LogError(request.Error.message);EditorApplication.Exit(1);}}
}
