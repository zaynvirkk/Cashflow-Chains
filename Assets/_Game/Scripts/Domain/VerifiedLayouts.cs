using System;
using System.Collections.Generic;
using System.Linq;
using LocalGame.Data;

namespace CashflowChains.Domain {
 // The player's actual completed plans, separate from the board currently being edited.
 public static class VerifiedLayouts {
  public static void Validate(List<Board> plans,Profile profile){
   if(plans==null||plans.Count>104||plans.Any(b=>b==null)||plans.Select(b=>b.Challenge).Distinct().Count()!=plans.Count)throw new ArgumentException("Invalid completed plans");
   foreach(var board in plans){var run=new CityRun(board.Challenge,board);long score=run.Preview().Credits;if(profile.Stars[board.Challenge]<1||run.Stars(score)<1||profile.Best[board.Challenge]<score)throw new ArgumentException("Plan has not earned this result");}
  }
  public static void Record(List<Board> plans,Profile profile,Board board){
   var candidate=plans.Where(b=>b.Challenge!=board.Challenge).Select(b=>b.Copy()).ToList();candidate.Add(board.Copy());Validate(candidate,profile);plans.Clear();plans.AddRange(candidate);
  }
  public static Board Find(List<Board> plans,string recipe,Func<int,bool> eligible){
   foreach(var board in plans.AsEnumerable().Reverse()){
    if(!eligible(board.Challenge))continue;var result=Resolver.Run(board,Campaign.Get(board.Challenge).Multiplier);var contributors=CityCollections.Contributors(board,result);
    if(board.Nodes.Any(n=>n.Kind==recipe&&contributors.Contains(n.Id)))return board.Copy();
   }
   return null;
  }
 }
}
