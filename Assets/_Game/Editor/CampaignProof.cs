using System.IO;
using UnityEngine;
using CashflowChains.Domain;
public static class CampaignProof {
 public static void Export(){Directory.CreateDirectory("Artifacts/Campaign");using(var w=new StreamWriter("Artifacts/Campaign/witnesses.jsonl"))for(int i=0;i<104;i++)w.WriteLine(JsonUtility.ToJson(Campaign.Get(i).Solution));}
}
