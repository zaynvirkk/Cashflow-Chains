using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using CashflowChains.Economy;
internal static class Program {
 internal static readonly JsonSerializerOptions Json=new JsonSerializerOptions{IncludeFields=true,WriteIndented=true};
 static int Main(string[] args){try{var config=JsonSerializer.Deserialize<BalanceConfig>(File.ReadAllText(args[1]),Json);config.Validate();if(args[0]=="validate"){Console.WriteLine(JsonSerializer.Serialize(new{passed=true,revision=config.Revision,items=config.Items.Length,businesses=config.Businesses.Length,upgrades=config.Upgrades.Length,activeFeatures=config.EnabledFeatures},Json));return 0;}if(args[0]=="apply"){ServerBridge.Run(config,args[2]);return 0;}if(args[0]=="stress"){Stress.Run(int.Parse(args[2]),args[3]);return 0;}if(args[0]=="simulate"){Simulation.Run(config,args.Skip(2).ToArray());return 0;}throw new ArgumentException("Unknown command");}catch(Exception e){Console.Error.WriteLine(e.ToString());return 1;}}
}
