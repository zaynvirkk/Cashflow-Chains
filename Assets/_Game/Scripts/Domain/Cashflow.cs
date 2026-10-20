using System;
using System.Collections.Generic;
using System.Linq;

namespace CashflowChains.Domain
{
    public sealed class Piece
    {
        public readonly string Id,Label,Hint; public readonly int Price,Inputs,Outputs;
        public readonly int[] Need,Make; public readonly int Rate,SourceKind,SourceRate; public readonly long Credits;
        public Piece(string id,string label,string hint,int price,int input,int output,int[] need=null,int[] make=null,int rate=0,long credits=0,int source=-1,int sourceRate=0)
        {Id=id;Label=label;Hint=hint;Price=price;Inputs=input;Outputs=output;Need=need??new int[3];Make=make??new int[3];Rate=rate;Credits=credits;SourceKind=source;SourceRate=sourceRate;}
    }
    public static class Pieces
    {
        // N=1,E=2,S=4,W=8. Default sources face E; outlet accepts W and S.
        public static readonly Piece[] All={
            new Piece("home","HOME","3 customers / beat →",80,0,2,source:0,sourceRate:3),
            new Piece("cafe","CAFE","1 customer + 1 goods = 80 cr",100,12,0,new[]{1,1,0},rate:2,credits:80),
            new Piece("farm","FARM","2 goods / beat → · finite supply",60,0,2,source:1,sourceRate:2),
            new Piece("warehouse","DEPOT","Routes up to 6 goods / beat",70,9,6,new[]{0,1,0},new[]{0,1,0},6),
            new Piece("power","POWER","4 energy / beat →",80,0,2,source:2,sourceRate:4),
            new Piece("factory","FACTORY","1 energy + 1 goods → 3 goods",120,9,6,new[]{0,1,1},new[]{0,3,0},2),
            new Piece("grocer","GROCER","2 customers + 1 goods = 110 cr",110,12,0,new[]{2,1,0},rate:2,credits:110),
            new Piece("bank","BANK","+25% adjacent outlet · once",150,15,0),
            new Piece("office","OFFICE","1 customer / beat · +10% adjacent sales",100,0,2,source:0,sourceRate:1),
            new Piece("park","PARK","+5% adjacent cafe sales",50,0,0)
        };
        public static Piece Get(string id) {var p=All.FirstOrDefault(x=>x.Id==id);if(p==null)throw new ArgumentException("Unknown piece");return p;}
        public static int Ports(int mask,int rotation) {for(int k=0;k<rotation;k++) mask=((mask<<1)&15)|((mask>>3)&1);return mask;}
    }
    [Serializable] public sealed class Node
    {
        public int Id,X,Y,Rotation,InitialGoods,InitialEnergy; public string Kind;
        public Node(){} public Node(int id,string kind,int x,int y,int rotation=0){Id=id;Kind=kind;X=x;Y=y;Rotation=rotation;}
        public Node Copy()=>new Node(Id,Kind,X,Y,Rotation){InitialGoods=InitialGoods,InitialEnergy=InitialEnergy};
    }
    [Serializable] public sealed class Board
    {
        public int Width=4,Height=4,Budget=300,Challenge; public List<Node> Nodes=new List<Node>();
        public Board Copy()=>new Board{Width=Width,Height=Height,Budget=Budget,Challenge=Challenge,Nodes=Nodes.Select(x=>x.Copy()).ToList()};
        public static Board Tutorial(int index)
        {
            var b=new Board{Challenge=index,Width=index==2?5:4,Height=index==2?5:4,Budget=index==2?500:300};
            b.Nodes.Add(new Node(1,"home",0,1)); b.Nodes.Add(new Node(2,"cafe",2,1){InitialGoods=index==0?14:0});
            if(index>0)b.Nodes.Add(new Node(3,"farm",1,3,3){InitialEnergy=1});
            return b;
        }
        public bool Move(int id,int x,int y)
        {var n=Nodes.FirstOrDefault(z=>z.Id==id);if(n==null||x<0||y<0||x>=Width||y>=Height||Nodes.Any(z=>z.Id!=id&&z.X==x&&z.Y==y))return false;n.X=x;n.Y=y;return true;}
        public string Fingerprint()=>Width+"x"+Height+":"+Challenge+":"+Budget+":"+string.Join(";",Nodes.OrderBy(x=>x.Id).Select(n=>$"{n.Id},{n.Kind},{n.X},{n.Y},{n.Rotation},{n.InitialGoods},{n.InitialEnergy}"));
        public void Validate()
        {
            if(Width<1||Height<1||Width>7||Height>7||Nodes==null||Nodes.Count>Width*Height||Budget<0||Budget>1000000||Challenge<0||Challenge>2)throw new ArgumentException("Invalid board");
            var ids=new HashSet<int>();var cells=new HashSet<int>();
            foreach(var n in Nodes)
            {
                if(n==null||n.Id<1||!ids.Add(n.Id)||n.X<0||n.Y<0||n.X>=Width||n.Y>=Height||!cells.Add(n.Y*Width+n.X)||n.Rotation<0||n.Rotation>3||n.InitialGoods<0||n.InitialGoods>64||n.InitialEnergy<0||n.InitialEnergy>64)throw new ArgumentException("Invalid node");
                Pieces.Get(n.Kind);
            }
            if(Nodes.Sum(n=>Pieces.Get(n.Kind).Price)>Budget)throw new ArgumentException("Board exceeds budget");
        }
    }
    public sealed class FlowEvent
    {
        public int Beat,Source,Target,Kind,Count;public long Credits;public string Reason;
        public override string ToString()=>$"{Beat}:{Source}>{Target}:{Kind}:{Count}:{Credits}:{Reason}";
    }
    public sealed class RoundResult
    {
        public int Beats=8; public long Credits;
        public readonly long[] Initial=new long[3],Generated=new long[3],Consumed=new long[3],Spilled=new long[3],Remaining=new long[3];
        public readonly List<FlowEvent> Events=new List<FlowEvent>();
        public bool Conserved()=>Enumerable.Range(0,3).All(k=>Initial[k]+Generated[k]==Consumed[k]+Spilled[k]+Remaining[k]);
        public string Trace()=>Credits+"|"+string.Join(";",Events.Select(e=>e.ToString()));
    }
    public static class Resolver
    {
        public static readonly int[] Dx={0,1,0,-1},Dy={-1,0,1,0};
        public static RoundResult Run(Board board,int roundBasisPoints=10000)
        {
            board.Validate();if(roundBasisPoints<0||roundBasisPoints>80000)throw new ArgumentOutOfRangeException(nameof(roundBasisPoints));
            var nodes=board.Nodes.OrderBy(n=>n.Id).ToArray();int len=nodes.Length;
            var r=new RoundResult();var stock=new int[len,3];var outgoing=new int[len,3];var inbox=new int[len,3];var farmStarted=new bool[len];
            var pieces=nodes.Select(n=>Pieces.Get(n.Kind)).ToArray();
            for(int i=0;i<len;i++){stock[i,1]=nodes[i].InitialGoods;stock[i,2]=nodes[i].InitialEnergy;r.Initial[1]+=stock[i,1];r.Initial[2]+=stock[i,2];}
            for(int beat=1;beat<=8;beat++)
            {
                // Deliver last beat's shipments; overflow is waste, never profit.
                for(int i=0;i<len;i++)for(int k=0;k<3;k++){Add(stock,i,k,inbox[i,k],r);inbox[i,k]=0;}
                for(int i=0;i<len;i++)
                {
                    var p=pieces[i];if(p.SourceKind<0)continue;
                    if(p.Id=="farm"&&!farmStarted[i])
                    {if(stock[i,2]<1)continue;stock[i,2]--;r.Consumed[2]++;farmStarted[i]=true;}
                    int amount=p.SourceRate;r.Generated[p.SourceKind]+=amount;Add(outgoing,i,p.SourceKind,amount,r);
                }
                // Transport a bounded snapshot before processing recipes. Converter output waits a beat.
                for(int i=0;i<len;i++)for(int direction=0;direction<4;direction++)
                {
                    if((Pieces.Ports(pieces[i].Outputs,nodes[i].Rotation)&(1<<direction))==0)continue;
                    int tx=nodes[i].X+Dx[direction],ty=nodes[i].Y+Dy[direction];int target=Array.FindIndex(nodes,n=>n.X==tx&&n.Y==ty);
                    if(target<0||(Pieces.Ports(pieces[target].Inputs,nodes[target].Rotation)&(1<<((direction+2)%4)))==0)continue;
                    int capacity=6;
                    for(int k=0;k<3;k++)
                    {
                        if(pieces[target].Need[k]<=0)continue;
                        int count=Math.Min(outgoing[i,k],capacity);if(count==0)continue;
                        outgoing[i,k]-=count;inbox[target,k]+=count;capacity-=count;
                        r.Events.Add(new FlowEvent{Beat=beat,Source=nodes[i].Id,Target=nodes[target].Id,Kind=k,Count=count,Reason="shipment"});
                    }
                }
                for(int i=0;i<len;i++)
                {
                    var p=pieces[i];int ops=p.Rate;if(ops==0)continue;
                    for(int k=0;k<3;k++)if(p.Need[k]>0)ops=Math.Min(ops,stock[i,k]/p.Need[k]);
                    if(ops==0)continue;
                    for(int k=0;k<3;k++)
                    {
                        int consumed=ops*p.Need[k],made=ops*p.Make[k];stock[i,k]-=consumed;r.Consumed[k]+=consumed;r.Generated[k]+=made;Add(outgoing,i,k,made,r);
                    }
                    if(p.Credits>0)
                    {
                        var adjacent=nodes.Where(n=>Math.Abs(n.X-nodes[i].X)+Math.Abs(n.Y-nodes[i].Y)==1).ToArray();
                        // Auxiliary bank links select one outlet (lowest stable ID). Several banks never stack.
                        bool bank=adjacent.Any(n=>n.Kind=="bank"&&nodes.Where(z=>Pieces.Get(z.Kind).Credits>0&&Math.Abs(z.X-n.X)+Math.Abs(z.Y-n.Y)==1).OrderBy(z=>z.Id).First().Id==nodes[i].Id);
                        int bp=10000+(bank?2500:0)+adjacent.Count(n=>n.Kind=="office")*1000+(p.Id=="cafe"?adjacent.Count(n=>n.Kind=="park")*500:0);
                        bp=Math.Min(30000,bp);long credit=checked(checked(ops*p.Credits)*bp/10000);credit=checked(credit*roundBasisPoints/10000);
                        r.Credits=checked(r.Credits+credit);r.Events.Add(new FlowEvent{Beat=beat,Source=nodes[i].Id,Target=nodes[i].Id,Credits=credit,Count=ops,Reason="sale"});
                    }
                }
            }
            for(int i=0;i<len;i++)for(int k=0;k<3;k++)r.Remaining[k]+=stock[i,k]+outgoing[i,k]+inbox[i,k];
            if(!r.Conserved())throw new InvalidOperationException("Conservation failed");return r;
        }
        static void Add(int[,] inventory,int i,int k,int value,RoundResult r)
        {int amount=inventory[i,k]+value;inventory[i,k]=Math.Min(64,amount);r.Spilled[k]+=Math.Max(0,amount-64);}
    }
    [Serializable] public sealed class SessionState
    {
        public int Schema=1;public Board Board=Board.Tutorial(0);public long Wallet;public bool Resolved;
        public string ResultId="";public List<int> Cleared=new List<int>();public List<string> Operations=new List<string>();
    }
    public sealed class Session
    {
        public SessionState Snapshot{get;private set;}=new SessionState();
        readonly Stack<Board> undo=new Stack<Board>();
        public RoundResult Preview()=>Resolver.Run(Snapshot.Board);
        public bool Move(int id,int x,int y)
        {if(Snapshot.Resolved)return false;var b=Snapshot.Board.Copy();if(!b.Move(id,x,y))return false;Push(b);return true;}
        public bool Rotate(int id)
        {if(Snapshot.Resolved)return false;var b=Snapshot.Board.Copy();var n=b.Nodes.Find(x=>x.Id==id);if(n==null)return false;n.Rotation=(n.Rotation+1)%4;Push(b);return true;}
        public bool Place(string kind,int x,int y)
        {
            if(Snapshot.Resolved)return false;var b=Snapshot.Board.Copy();var p=Pieces.Get(kind);
            string[] allowed=b.Challenge==2?new[]{"home","cafe","farm","warehouse","bank","grocer"}:new[]{"home","cafe","farm"};
            if(!allowed.Contains(kind)||b.Nodes.Sum(n=>Pieces.Get(n.Kind).Price)+p.Price>b.Budget||x<0||y<0||x>=b.Width||y>=b.Height||b.Nodes.Any(n=>n.X==x&&n.Y==y))return false;
            b.Nodes.Add(new Node(b.Nodes.Count==0?1:b.Nodes.Max(n=>n.Id)+1,kind,x,y){InitialEnergy=kind=="farm"?1:0});Push(b);return true;
        }
        void Push(Board b){if(undo.Count>=100)undo.Clear();undo.Push(Snapshot.Board.Copy());Snapshot.Board=b;}
        public bool Undo(){if(Snapshot.Resolved||undo.Count==0)return false;Snapshot.Board=undo.Pop();return true;}
        public bool Activate(string operation)
        {
            if(Snapshot.Resolved||string.IsNullOrEmpty(operation)||Snapshot.Operations.Contains(operation))return false;
            var r=Preview();Snapshot.Resolved=true;Snapshot.ResultId=operation;Snapshot.Operations.Add(operation);
            if(r.Credits>=Target&&!Snapshot.Cleared.Contains(Snapshot.Board.Challenge)){Snapshot.Cleared.Add(Snapshot.Board.Challenge);Snapshot.Wallet=checked(Snapshot.Wallet+500);}
            return true;
        }
        public int Target=>Snapshot.Board.Challenge==0?480:Snapshot.Board.Challenge==1?800:1200;
        public void Edit(){Snapshot.Resolved=false;Snapshot.ResultId="";}
        public void Reset(){Snapshot.Board=Board.Tutorial(Snapshot.Board.Challenge);Snapshot.Resolved=false;Snapshot.ResultId="";undo.Clear();}
        public void Next(){Snapshot.Board=Board.Tutorial((Snapshot.Board.Challenge+1)%3);Snapshot.Resolved=false;Snapshot.ResultId="";undo.Clear();}
        public SessionState Export()=>new SessionState{Board=Snapshot.Board.Copy(),Wallet=Snapshot.Wallet,Resolved=Snapshot.Resolved,ResultId=Snapshot.ResultId,Cleared=new List<int>(Snapshot.Cleared),Operations=new List<string>(Snapshot.Operations)};
        public static Session Restore(SessionState state)
        {
            if(state==null||state.Schema!=1||state.Wallet<0||state.Wallet>1500||state.Cleared==null||state.Cleared.Count>3||state.Cleared.Any(x=>x<0||x>2)||state.Cleared.Distinct().Count()!=state.Cleared.Count||state.Wallet!=state.Cleared.Count*500L||state.Operations==null||state.Operations.Count>100000||state.Operations.Distinct().Count()!=state.Operations.Count)throw new ArgumentException("Invalid save");
            state.Board.Validate();if(state.Resolved&&!state.Operations.Contains(state.ResultId))throw new ArgumentException("Invalid result");
            return new Session{Snapshot=state};
        }
    }
}
