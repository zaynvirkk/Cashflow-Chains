using System;using System.Linq;
namespace CashflowChains.Domain {
 public static class ExpansionCampaign {
  public const int Total=104;
  public static Mission Get(int index){if(index<72||index>=Total)throw new ArgumentException("Invalid expansion city");bool harbor=index<84;int local=index-(harbor?72:84),template=local%(harbor?6:10),id=0;var b=new Board{Width=6,Height=6,Challenge=index,Budget=8000,ImportQuota=harbor?16+local%3*8:0,ImportCost=harbor?3+local%2:0};Action<string,int,int,int> add=(kind,x,y,r)=>b.Nodes.Add(new Node(++id,kind,x,y,r){InitialEnergy=kind=="farm"?1:0});
   if(harbor){
    if(template==2){var original=Campaign.Get(53).Solution;b.Nodes=original.Nodes.Select(n=>n.Copy()).ToList();id=b.Nodes.Max(n=>n.Id);}
    else if(template==3){add("apartment",1,2,0);add("market",2,2,0);add("port",2,3,3);add("bank",3,2,0);add("park",1,1,0);}
    else if(template==5){add("port",1,1,1);add("warehouse",1,2,0);add("boutique",2,2,0);add("home",2,3,3);add("diner",1,3,1);add("home",0,3,0);add("port",4,3,3);add("hotel",4,2,0);add("apartment",3,2,0);}
    else {string router=template==1?"logistics":template==4?"freight_yard":"warehouse";int sx=template==1?4:3;add("port",2,1,1);add(router,2,2,0);add(template==4?"shopping_mall":"cafe",sx,2,0);add(template==4?"apartment":"home",sx,3,3);add("diner",2,3,1);add("home",1,3,0);add("bank",sx+1,2,0);}
   }else{
    if(template==0||template==1){add("home",1,2,0);add(template==0?"bakery":"cinema",2,2,0);add(template==0?"farm":"solar_array",2,3,3);add("bank",3,2,0);}
    if(template==2){add("home",2,1,1);add("power",1,2,0);add("university",2,2,0);add("cafe",3,2,0);add("farm",3,3,3);add("diner",2,3,1);add("farm",1,3,0);}
    if(template==3){add("power",2,1,1);add("battery",2,2,0);add("cinema",3,2,0);add("home",3,3,3);add("greenhouse",2,3,0);add("diner",2,4,1);add("home",1,4,0);}
    if(template==4||template==5){add("power",2,1,1);add(template==4?"greenhouse":"data_center",2,2,0);add("cafe",3,2,0);add("home",3,3,3);add("diner",2,3,1);add("home",1,3,0);add("bank",4,2,0);}
    if(template==6){add("apartment",2,1,1);add("tram",2,2,0);add("cinema",3,2,0);add("solar_array",3,3,3);add("cafe",2,3,1);add("farm",1,3,0);}
    if(template==7){add("apartment",1,2,0);add("exchange",2,2,0);add("farm",2,3,3);add("power",2,1,1);add("bank",3,2,0);}
    if(template==8){add("home",2,0,1);add("power",1,1,0);add("university",2,1,0);add("cinema",3,1,0);add("solar_array",3,0,1);add("tram",2,2,0);add("bakery",3,2,0);add("farm",3,3,3);add("diner",2,3,1);add("farm",1,3,0);}
    if(template==9){add("power",2,1,1);add("greenhouse",2,2,0);add("bakery",3,2,0);add("home",3,3,3);add("diner",2,3,1);add("home",1,3,0);add("home",0,4,0);add("tram",1,4,0);add("cinema",2,4,0);add("solar_array",2,5,3);add("landmark",4,3,0);add("bank",4,2,0);}
   }
   // Rotate the complete graph, keeping every directed port relationship intact.
   for(int r=0;r<local/6%4;r++)foreach(var n in b.Nodes){int x=n.X;n.X=5-n.Y;n.Y=x;n.Rotation=(n.Rotation+1)%4;}
   var random=new Random(index*479+23);for(int i=0;i<2+local%4;i++){int c=random.Next(36);if(!b.Nodes.Any(n=>n.X==c%6&&n.Y==c/6))b.Blocked|=1UL<<c;}
   b.Budget=b.Nodes.Sum(n=>Pieces.Get(n.Kind).Price)+80+(local%3)*40;int multiplier=10000+(local%4)*500;var result=Resolver.Run(b,multiplier);if(result.Credits<=0)throw new InvalidOperationException("Unsolvable expansion reference "+index);var start=b.Copy();var free=Enumerable.Range(0,36).Where(c=>(b.Blocked&(1UL<<c))==0).OrderBy(_=>random.Next()).ToArray();foreach(var n in start.Nodes){int c=free[n.Id-1];n.X=c%6;n.Y=c/6;n.Rotation=random.Next(4);}
   string[] harborNames={"First cargo","Across the canal","Three delivery windows","Market at the wharf","The freight fork","Two shores"},financeNames={"Bread and butter","Opening night","Campus connections","Stored potential","Glasshouse growth","Digital supply","The culture line","Market makers","Night school","A district in harmony"};
   return new Mission{Index=index,Title=(harbor?harborNames:financeNames)[template]+(local>=(harbor?6:10)?" · expert":""),District=Campaign.Districts[harbor?6:7],Brief=harbor?"Limited imports make every delivery matter. Read the quota and cargo cost.":"Build a culture and energy network. Every advanced recipe needs real inputs.",Modifier=harbor?"IMPORT LIMIT "+b.ImportQuota+" · "+b.ImportCost+" CR / GOODS":"ADVANCED RECIPES · "+(local<10?"OPEN CITY":"TIGHT BUDGET"),Multiplier=multiplier,Solution=b,Start=start,Par=result.Credits,Target=Math.Max(80,result.Credits*(local<10?70:80)/100)};
  }
 }
}
