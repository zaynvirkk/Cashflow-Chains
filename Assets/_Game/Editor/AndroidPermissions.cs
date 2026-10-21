using System.IO;
using System.Xml;
using UnityEditor.Android;
public sealed class AndroidPermissions:IPostGenerateGradleAndroidProject {
 public int callbackOrder=>100;
 public void OnPostGenerateGradleAndroidProject(string path){
  string manifest=Path.Combine(path,"src/main/AndroidManifest.xml");var xml=new XmlDocument();xml.Load(manifest);const string ns="http://schemas.android.com/apk/res/android";
  bool vibrate=false;foreach(XmlNode node in xml.DocumentElement.SelectNodes("uses-permission"))if(node.Attributes?["name",ns]?.Value=="android.permission.VIBRATE")vibrate=true;
  if(!vibrate){var permission=xml.CreateElement("uses-permission");var name=xml.CreateAttribute("android","name",ns);name.Value="android.permission.VIBRATE";permission.Attributes.Append(name);xml.DocumentElement.AppendChild(permission);}
  foreach(XmlElement activity in xml.DocumentElement.SelectNodes("application/activity"))if(activity.GetAttribute("name",ns)=="com.unity3d.player.UnityPlayerGameActivity")activity.SetAttribute("name",ns,"com.virkgames.platform.GameEntryActivity");
  xml.Save(manifest);
 }
}
