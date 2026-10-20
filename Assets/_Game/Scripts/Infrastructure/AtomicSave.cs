using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace LocalGame
{
    public sealed class AtomicSave
    {
        [Serializable] sealed class Envelope {public int Schema=1;public string Payload,Hash;}
        bool recoveredBackup;readonly string path;readonly object gate=new object();
        public AtomicSave(string path){this.path=path;}
        static string Hash(string s){using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(s))).Replace("-","");}
        public void Write<T>(T state)
        {
            lock(gate)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));string payload=JsonUtility.ToJson(state);
                byte[] bytes=Encoding.UTF8.GetBytes(JsonUtility.ToJson(new Envelope{Payload=payload,Hash=Hash(payload)}));
                using(var file=new FileStream(path+".tmp",FileMode.Create,FileAccess.Write,FileShare.None)){file.Write(bytes,0,bytes.Length);file.Flush(true);}
                if(recoveredBackup&&File.Exists(path)){File.Move(path,path+".corrupt-"+Guid.NewGuid().ToString("N"));File.Move(path+".tmp",path);}
                else if(File.Exists(path))File.Replace(path+".tmp",path,path+".bak");else File.Move(path+".tmp",path);
                recoveredBackup=false;
            }
        }
        public T Read<T>(Action<T> validate,out string status) where T:class
        {
            status="New local game";if(!File.Exists(path)&&!File.Exists(path+".bak"))return null;
            foreach(string candidate in new[]{path,path+".bak"})
            {
                try
                {
                    if(!File.Exists(candidate)||new FileInfo(candidate).Length>4000000)continue;
                    var envelope=JsonUtility.FromJson<Envelope>(File.ReadAllText(candidate));
                    if(envelope==null||envelope.Schema!=1||envelope.Payload==null||envelope.Hash!=Hash(envelope.Payload))continue;
                    T value=JsonUtility.FromJson<T>(envelope.Payload);validate(value);recoveredBackup=candidate!=path;status=candidate==path?"Resumed local game":"Recovered local backup";return value;
                }
                catch(Exception e) when(e is IOException||e is ArgumentException||e is NullReferenceException||e is InvalidOperationException||e is OverflowException){}
            }
            status="Save unreadable — Reset starts a new local game";return null;
        }
    }
}
