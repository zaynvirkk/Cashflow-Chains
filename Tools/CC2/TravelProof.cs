using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using CashflowChains.Domain;
internal static class TravelProof {
 internal static void Run(string directory){Directory.CreateDirectory(directory);var results=new List<object>();using(var writer=new StreamWriter(Path.Combine(directory,"witnesses.jsonl")))for(int i=0;i<104;i++){var m=Campaign.Get(i,1);var run=new CityRun(i,m.Solution.Copy());var r=run.Preview();if(!r.Conserved()||r.Credits<=0||run.Stars(r.Credits)!=3)throw new Exception("Invalid travel reference "+i);writer.WriteLine(JsonSerializer.Serialize(m.Solution,Program.Json).Replace("\n","").Replace("\r",""));results.Add(new{level=i,reference=r.Credits,start=Resolver.Run(m.Start,m.Multiplier).Credits,target=m.Target});}
  var random=new Random(231023);for(int test=0;test<10000;test++){var b=new Board{Width=6,Height=6,Budget=100000,RulesVersion=1};foreach(int cell in Enumerable.Range(0,36).OrderBy(_=>random.Next()).Take(3+random.Next(12))){var p=Pieces.All[random.Next(Pieces.All.Length)];b.Nodes.Add(new Node(b.Nodes.Count+1,p.Id,cell%6,cell/6,random.Next(4)){InitialGoods=random.Next(5),InitialEnergy=random.Next(5)});}var before=Resolver.Run(b);b.Nodes.Reverse();if(!before.Conserved()||before.Trace()!=Resolver.Run(b).Trace())throw new Exception("Travel stress failed "+test);}
  for(int size=4;size<=7;size++){var t=new TrafficSimulation(644,644,size,size);for(int step=0;step<6000;step++){t.Advance(1d/30);if(t.HasOverlap())throw new Exception("Traffic overlap "+size+":"+step);}}
  File.WriteAllText(Path.Combine(directory,"report.json"),JsonSerializer.Serialize(new{passed=true,travelReferences=104,stressCases=10000,trafficCheckedSteps=24000,scope="Pure domain simulation only; native/human acceptance separate",results},Program.Json));Console.WriteLine("104 travel references,10000 conserved deterministic layouts,24000 non-overlap traffic steps pass");
 }
}
