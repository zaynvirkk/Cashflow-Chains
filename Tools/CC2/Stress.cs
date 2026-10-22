using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using CashflowChains.Domain;
internal static class Stress {
 internal static void Run(int count,string output){var random=new Random(21022026);var kinds=Pieces.All.Select(x=>x.Id).ToArray();int maxEvents=0;for(int sample=0;sample<count;sample++){var board=new Board{Width=6,Height=6,Budget=100000,ImportQuota=sample%3==0?16:0,ImportCost=sample%4};for(int c=0;c<36;c++)if(random.Next(4)==0)board.Nodes.Add(new Node(c+1,kinds[random.Next(kinds.Length)],c%6,c/6,random.Next(4)){InitialGoods=random.Next(5),InitialEnergy=random.Next(3)});var result=Resolver.Run(board);if(!result.Conserved()||result.Credits!=result.Events.Sum(x=>x.Credits)||result.Trace()!=Resolver.Run(board).Trace()||result.Events.Count>10000)throw new Exception("Resolver stress failure at case "+sample);maxEvents=Math.Max(maxEvents,result.Events.Count);}Directory.CreateDirectory(Path.GetDirectoryName(output));File.WriteAllText(output,JsonSerializer.Serialize(new{passed=true,cases=count,seed=21022026,buildingTypes=kinds.Length,maxEvents,scope="Actual unchanged8-beat production resolver; all32 recipes,imports,conservation,exact money trace,determinism,bounded event count"},Program.Json));Console.WriteLine(output);}
}
