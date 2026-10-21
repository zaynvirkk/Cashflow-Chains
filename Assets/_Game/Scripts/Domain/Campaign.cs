using System;
using System.Collections.Generic;
using System.Linq;
namespace CashflowChains.Domain {
 public sealed class Mission {
  public int Index;public string Title,District,Brief,Modifier;public int Multiplier;public Board Solution,Start;public long Par,Target;
 }
 public static class Campaign {
  public static readonly string[] Districts={"Little Beginnings","Garden Quarter","The Working City","Glass & Gold","Harbour Lights","The Living Metropolis","Harbor Stories","Finance After Dark"};
  static readonly string[] Titles={"First light","A morning ritual","Farm to table","Corner economy","Second shift","Through the depot","The long connection","House of plenty","After hours","A finer taste","Three little businesses","An electric city"};
  static readonly string[] Modifiers={"Opening day","Morning trade","Local suppliers","Busy streets","Night shift","Long deliveries","Freight connection","City festival","Dinner crowd","Luxury season","Diverse economy","Clean power"};
  public static Mission Get(int index) {
   if(index>=72)return ExpansionCampaign.Get(index);
   if(index<0||index>=72)throw new ArgumentException("Invalid mission");int d=index/12,t=index%12;var b=new Board{Width=d==0&&t!=6&&t!=10?4:6,Height=d==0&&t!=6&&t!=10?4:6,Challenge=index,Budget=4000};
   int id=0;Action<string,int,int,int> add=(kind,x,y,r)=>b.Nodes.Add(new Node(++id,kind,x,y,r){InitialEnergy=kind=="farm"?1:0});
   string shop=t==3?(d>=3?"market":"grocer"):t==7?(d==4?"stadium":d>=3?"shopping_mall":"grocer"):t==8?"diner":t==9?(d>=3?"boutique":"market"):t==11?(d>=4?"stadium":d>=3?"hotel":"grocer"):d==4?"hotel":"cafe";
   if(t==10&&d>=3) {
    for(int j=0;j<3;j++){add(j==2?"office":"home",0,j*2,0);add(new[]{"cafe","grocer","diner"}[j],1,j*2,0);add("farm",1,j*2+1,3);}
    add("landmark",4,2,0);add("bank",2,2,0);
   } else if(t==4&&d<2){add("home",0,1,0);add("cafe",1,1,0);add("farm",1,2,3);add("home",2,0,0);add("diner",3,0,0);add("farm",3,1,3);
   } else if(t==4&&d>=2||t==5||t==6||t==11&&d>=2) {
    string router=t==4?"factory":t==5?(d>=4?"freight_yard":"warehouse"):t==6?(d>=2?"logistics":"warehouse"):d>=5?"recycler":"factory";
    add(router,2,2,0);add("farm",2,1,1);
    if(router=="factory"||router=="recycler")add(d%2==0?"power":"solar_array",1,2,0);
    int sx=t==6&&d>=2?4:3;add("cafe",sx,2,0);add("home",sx,3,3);
    add("diner",2,3,1);add("home",1,3,0);
    if(d>=2)add("bank",sx+1,2,0);
   } else {
    add((t==7||t==11)&&d>=1?"apartment":d>=5?"tech_office":"home",1,2,0);add(shop,2,2,0);
    if(t==0&&d==0)b.Nodes[1].InitialGoods=14;
    else add(t==7&&d>=4?"port":"farm",2,3,3);
    if(d>=1&&(t==2||t==9||d>1))add("park",1,1,0);
    if(d>=1)add("bank",3,2,0);
    if(t==1&&d>=2)add("office",2,1,1);
   }
   if(t==5&&d>=4){b.Nodes.Clear();id=0;add("port",1,0,1);add("warehouse",1,1,0);add("freight_yard",2,1,0);add("warehouse",2,2,0);add("cafe",1,2,1);add("home",0,2,0);add("diner",3,1,0);add("home",3,2,3);add("grocer",2,3,1);add("apartment",1,3,0);}
   // Rotate the whole directed network, preserving every port and resource relationship.
   for(int r=0;r<d%4;r++)foreach(var n in b.Nodes){int x=n.X;n.X=b.Width-1-n.Y;n.Y=x;n.Rotation=(n.Rotation+1)%4;}
   b.Budget=b.Nodes.Sum(n=>Pieces.Get(n.Kind).Price)+160;
   if(index>=12){var random=new Random(700+index);for(int i=0;i<d+1;i++){int cell=random.Next(b.Width*b.Height);if(!b.Nodes.Any(n=>n.Y*b.Width+n.X==cell))b.Blocked|=1UL<<cell;}}
   int multiplier=10000+d*500+(t%3)*250;long par=Resolver.Run(b,multiplier).Credits;
   if(par<=0)throw new InvalidOperationException("Invalid campaign network "+index);
   var start=b.Copy();var rng=new Random(20260+index);var free=Enumerable.Range(0,b.Width*b.Height).Where(c=>(b.Blocked&(1UL<<c))==0).OrderBy(_=>rng.Next()).ToArray();
   foreach(var n in start.Nodes){int c=free[n.Id-1];n.X=c%b.Width;n.Y=c/b.Width;n.Rotation=rng.Next(4);}
   if(index==0){start=b.Copy();start.Nodes[1].X=3;} // connect customers in one move
   if(index==1){start=b.Copy();start.Nodes.First(n=>n.Kind=="farm").Rotation=2;} // turn the supplier toward the cafe
   if(index==2){start=b.Copy();start.Nodes.First(n=>n.Kind=="home").X=0;start.Nodes.First(n=>n.Kind=="farm").Rotation=2;} // combine both learned actions
   return new Mission{Index=index,Title=index==0?"Your first connection":index==1?"Turn the delivery":index==2?"Put it together":t==11?new[]{"A little neighbourhood","The evening rush","Power after dark","A place to stay","Friday floodlights","A living metropolis"}[d]:Titles[t],District=Districts[d],Brief=index==0?"Move the café beside the home. Its arrow delivers customers.":"Connect people and supplies to a business. Check its recipe, then run eight beats.",Modifier=Modifiers[t],Multiplier=multiplier,Solution=b,Start=start,Par=par,Target=Math.Max(80,par*65/100)};
  }
  public static string[] Combos(Board b,RoundResult r)=>ComboBook.Evaluate(b,r);
 }
 public sealed class CityRun {
  public Board Board{get;private set;}public Mission Mission{get;private set;}public bool Finished{get;private set;}public int Moves{get;private set;}
  readonly Stack<Board> history=new Stack<Board>();
  public CityRun(int level,Board restored=null){Mission=Campaign.Get(level);Board=restored??Mission.Start.Copy();Board.Validate();if(Board.Width!=Mission.Solution.Width||Board.Height!=Mission.Solution.Height||Board.Budget!=Mission.Solution.Budget||Board.Blocked!=Mission.Solution.Blocked||Board.Challenge!=level||Board.ImportQuota!=Mission.Solution.ImportQuota||Board.ImportCost!=Mission.Solution.ImportCost)throw new ArgumentException("Altered mission constraints");foreach(var n in Board.Nodes){var original=Mission.Solution.Nodes.Find(a=>a.Id==n.Id&&a.Kind==n.Kind);if(n.Id>1000000||!Allowed.Contains(n.Kind)||Board.Nodes.Count(a=>a.Kind==n.Kind)>4||n.InitialEnergy!=(n.Kind=="farm"?1:0)||n.InitialGoods!=0&&(original==null||n.InitialGoods!=original.InitialGoods))throw new ArgumentException("Invalid building or endowment");}}
  string[] BaseAllowed=>Pieces.All.Where(p=>(UnlockLevel(p.Id)<=Mission.Index||Mission.Index==0&&p.Id=="farm")||Mission.Solution.Nodes.Any(n=>n.Kind==p.Id)).Select(p=>p.Id).ToArray();
  public string[] Allowed {get {var allowed=BaseAllowed;if(!Board.DraftMode)return allowed;var required=Mission.Solution.Nodes.Select(n=>n.Kind).Concat(Board.Nodes.Select(n=>n.Kind)).Where(allowed.Contains).Distinct();var random=new Random(Mission.Index*409+Board.DraftRoll*1777+701);return required.Concat(allowed.Except(required).OrderBy(_=>random.Next()).Take(3)).Distinct().ToArray();}}
  public void BeginDraft(){if(Finished)return;Board.DraftMode=true;Board.DraftRoll=0;}
  public bool RerollDraft(LocalGame.Data.Profile profile,string operation){if(!Board.DraftMode||Finished||Board.DraftRoll>=100)return false;if(Board.DraftRoll>0&&!profile.SpendDraftReroll(operation))return false;Board.DraftRoll++;return true;}

  static int UnlockLevel(string id){switch(id){case "home":case "cafe":return 0;case "farm":return 1;case "grocer":return 3;case "warehouse":return 5;case "diner":return 8;case "market":return 9;case "park":return 12;case "office":return 15;case "bank":return 18;case "apartment":return 19;case "power":return 24;case "factory":return 26;case "logistics":return 29;case "boutique":return 36;case "hotel":return 39;case "shopping_mall":return 42;case "landmark":return 45;case "port":return 48;case "freight_yard":return 51;case "stadium":return 54;case "tech_office":return 60;case "recycler":return 63;case "solar_array":return 64;default:return 84;}}
  public int Spent=>Board.Nodes.Sum(n=>Pieces.Get(n.Kind).Price);
  public bool Place(string kind,int x,int y,int rotation=0){if(Finished||!Allowed.Contains(kind)||rotation<0||rotation>3||x<0||y<0||x>=Board.Width||y>=Board.Height||(Board.Blocked&(1UL<<(y*Board.Width+x)))!=0||Board.Nodes.Any(n=>n.X==x&&n.Y==y)||Board.Nodes.Count(n=>n.Kind==kind)>=4||Spent+Pieces.Get(kind).Price>Board.Budget)return false;var b=Board.Copy();int id=b.Nodes.Count==0?1:b.Nodes.Max(n=>n.Id)+1;if(id>1000000)return false;b.Nodes.Add(new Node(id,kind,x,y,rotation){InitialEnergy=kind=="farm"?1:0});Change(b);return true;}
  public bool Remove(int id){if(Finished)return false;var b=Board.Copy();if(b.Nodes.RemoveAll(n=>n.Id==id)==0)return false;Change(b);return true;}

  void Change(Board b){if(history.Count>=150)history.Clear();history.Push(Board);Board=b;Moves++;}
  public bool Move(int id,int x,int y){if(Finished)return false;var b=Board.Copy();if(!b.Move(id,x,y))return false;Change(b);return true;}
  public bool Rotate(int id){if(Finished)return false;var b=Board.Copy();var n=b.Nodes.Find(x=>x.Id==id);if(n==null)return false;n.Rotation=(n.Rotation+1)%4;Change(b);return true;}
  public bool Undo(){if(Finished||history.Count==0)return false;var previous=history.Pop();previous.DraftMode=Board.DraftMode;previous.DraftRoll=Board.DraftRoll;Board=previous;Moves++;return true;}
  public void Reset(){Finished=false;Board=Mission.Start.Copy();history.Clear();Moves=0;}
  public RoundResult Preview()=>Resolver.Run(Board,Mission.Multiplier);
  public int Stars(long score)=>score<Mission.Target?0:score<Mission.Par*85/100?1:score<Mission.Par?2:3;
  public RoundResult Activate(LocalGame.Data.Profile profile,string receipt){if(Finished)return Preview();var result=Preview();profile.Award(Mission.Index,Stars(result.Credits),Math.Max(0,result.Credits),receipt);if(Stars(result.Credits)>0)CityCollections.Record(profile,Board,result);Finished=true;return result;}
  public Node Hint(){foreach(var target in Mission.Solution.Nodes.OrderBy(n=>n.Id)){var current=Board.Nodes.Find(n=>n.Id==target.Id&&n.Kind==target.Kind)??Board.Nodes.FirstOrDefault(n=>n.Kind==target.Kind);if(current==null||current.X!=target.X||current.Y!=target.Y||current.Rotation!=target.Rotation)return new Node(target.Id,target.Kind,current?.X??-1,current?.Y??-1,current?.Rotation??0);}return null;}
 }
}
