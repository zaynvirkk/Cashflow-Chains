using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
namespace LocalGame {
 public sealed class RuntimeReview:MonoBehaviour {
  readonly List<OverflowIssue> overflow=new List<OverflowIssue>();readonly Dictionary<string,int> suspects=new Dictionary<string,int>();int auditFrames;
  string dir;readonly List<float> frame=new List<float>();float elapsed;int frames;void Start(){dir=FlatUI.Arg("-capture-dir");if(dir==null){enabled=false;return;}var listener=FindFirstObjectByType<AudioListener>();if(listener){var audio=listener.gameObject.AddComponent<MixCapture>();audio.DirectoryPath=dir;} }
  void Update(){if(++auditFrames%6==0&&dir!=null)AuditText();elapsed+=Time.unscaledDeltaTime;if(elapsed<3)return;frames++;if(frame.Count<30000)frame.Add(Time.unscaledDeltaTime*1000);if(frames%600==0)Write();}void OnApplicationQuit(){if(dir!=null)Write();}
  void AuditText(){foreach(var text in FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None)){if(!text.isActiveAndEnabled||text.textInfo.characterCount==0||!text.isTextOverflowing)continue;string key=text.text+"|"+Screen.width+"x"+Screen.height;int count=suspects.ContainsKey(key)?suspects[key]+1:1;suspects[key]=count;if(count==3)overflow.Add(new OverflowIssue{Text=text.text,Width=text.rectTransform.rect.width,Height=text.rectTransform.rect.height,Font=text.fontSize,Screen=Screen.width+"x"+Screen.height});}}
  [Serializable] sealed class OverflowIssue{public string Text,Screen;public float Width,Height,Font;}
  [Serializable] sealed class OverflowReport{public List<OverflowIssue> Issues;}
  void Write(){if(frame.Count==0)return;Directory.CreateDirectory(dir);File.WriteAllText(Path.Combine(dir,"text-overflows.json"),JsonUtility.ToJson(new OverflowReport{Issues=overflow},true));frame.Sort();Directory.CreateDirectory(dir);File.WriteAllText(Path.Combine(dir,"performance.json"),JsonUtility.ToJson(new Stats{Width=Screen.width,Height=Screen.height,Frames=frames,MedianMs=frame[frame.Count/2],P95Ms=frame[(int)(frame.Count*.95f)],MaxMs=frame[frame.Count-1],AllocatedBytes=UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong(),AudioSampleRate=AudioSettings.outputSampleRate},true));}
  [Serializable] sealed class Stats{public int Width,Height,Frames,AudioSampleRate;public float MedianMs,P95Ms,MaxMs;public long AllocatedBytes;}
 }
 public sealed class MixCapture:MonoBehaviour {
  public string DirectoryPath;readonly object gate=new object();readonly List<short> pcm=new List<short>();int channels,sampleRate;bool written;float elapsed;
  void Awake(){sampleRate=AudioSettings.outputSampleRate;}
  void OnAudioFilterRead(float[] data,int count){lock(gate){if(written||pcm.Count>=sampleRate*2*25)return;channels=count;foreach(float f in data)pcm.Add((short)(Mathf.Clamp(f,-1,1)*32767));}}
  void Update(){elapsed+=Time.unscaledDeltaTime;if(elapsed>26&&!written)Write();}void OnApplicationQuit(){Write();}
  void Write(){lock(gate){if(written||DirectoryPath==null||channels==0)return;written=true;System.IO.Directory.CreateDirectory(DirectoryPath);using(var writer=new BinaryWriter(File.Create(Path.Combine(DirectoryPath,"actual-mix.wav")))){writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+pcm.Count*2);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));writer.Write(16);writer.Write((short)1);writer.Write((short)channels);writer.Write(sampleRate);writer.Write(sampleRate*channels*2);writer.Write((short)(channels*2));writer.Write((short)16);writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(pcm.Count*2);foreach(short value in pcm)writer.Write(value);}}}
 }
}
