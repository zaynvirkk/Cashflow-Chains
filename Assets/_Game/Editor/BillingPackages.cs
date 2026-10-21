using UnityEditor;using UnityEditor.PackageManager;using UnityEditor.PackageManager.Requests;using UnityEngine;
public static class BillingPackages {
 static AddRequest request;static double deadline;
 public static void Install(){request=Client.Add("com.unity.purchasing@5.4.4");deadline=EditorApplication.timeSinceStartup+300;EditorApplication.update+=Poll;}
 static void Poll(){if(!request.IsCompleted){if(EditorApplication.timeSinceStartup<deadline)return;Debug.LogError("Billing package install timed out");EditorApplication.Exit(2);return;}EditorApplication.update-=Poll;if(request.Status==StatusCode.Success){Debug.Log("BILLING_PACKAGE_INSTALLED "+request.Result.version);EditorApplication.Exit(0);}else{Debug.LogError(request.Error.message);EditorApplication.Exit(1);}}
}
