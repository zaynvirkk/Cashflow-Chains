using System;
using System.Collections;
using System.IO;
using System.Linq;
using CashflowChains.Domain;
using LocalGame;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CashflowChains.Presentation
{
    public sealed class GameApp:MonoBehaviour
    {
        public Session Game{get;private set;}public int DisplayBeat{get;private set;}
        RectTransform root,grid;AtomicSave save;TextMeshProUGUI hint,score,detail,phase;RectTransform overlay;
        int selected=2,rotation;string draft="";bool running,saveEnabled=true,reduced;string message="";
        Color bg=FlatUI.Hex("#071B2A"),panel=FlatUI.Hex("#102E40"),mint=FlatUI.Hex("#27EDBD"),cream=FlatUI.Hex("#F6F0DA"),muted=FlatUI.Hex("#A8C5CA"),gold=FlatUI.Hex("#FFBE54");
        void Start()
        {
            Application.targetFrameRate=60;Application.runInBackground=true;
            string dir=Debug.isDebugBuild||Application.isEditor?FlatUI.Arg("-save-dir"):null;
            save=new AtomicSave(Path.Combine(dir??Application.persistentDataPath,"cashflow-v1.json"));
            var state=save.Read<SessionState>(s=>Session.Restore(s),out message);saveEnabled=!message.StartsWith("Save unreadable");Game=state==null?new Session():Session.Restore(state);
            root=FlatUI.Root(bg);Draw();
        }
        void Update(){if(Keyboard.current?.f12Key.wasPressedThisFrame==true)FlatUI.Capture("cashflow");if(Keyboard.current?.escapeKey.wasPressedThisFrame==true&&!running)ShowHelp();}
        void Save()
        {
            if(!saveEnabled)return;
            try{save.Write(Game.Export());Probe();}catch(Exception e){message="Could not save locally: "+e.GetType().Name;saveEnabled=false;}
        }
        void OnApplicationPause(bool pause){if(pause&&Game!=null)Save();}
        void OnApplicationQuit(){if(Game!=null)Save();}
        void Clear(Transform p){foreach(Transform child in p)Destroy(child.gameObject);}
        void Draw()
        {
            Clear(root);overlay=null;
            FlatUI.Text(root,"Kicker","E L E C T R I C   L E D G E R",40,24,520,30,15,mint);
            FlatUI.Text(root,"Title","CASHFLOW\nCHAINS",40,62,530,112,48,cream);
            FlatUI.Button(root,"Help","?",594,60,86,90,panel,cream,ShowHelp);
            string[] titles={"01   FIRST CONNECTION","02   FEED THE NEIGHBORHOOD","03   BUILD A BETTER CHAIN"};
            FlatUI.Text(root,"Mission",titles[Game.Snapshot.Board.Challenge],40,184,640,30,18,muted);
            FlatUI.Panel(root,"MetricLine",40,229,640,2,FlatUI.Hex("#285064"));
            FlatUI.Text(root,"Target","TARGET  "+Game.Target.ToString("N0")+" cr",40,245,280,40,24,gold);
            score=FlatUI.Text(root,"Score","PREVIEW  —",340,245,340,40,24,mint,TextAlignmentOptions.Right);
            hint=FlatUI.Text(root,"Hint",HelpLine(),40,290,640,50,20,cream);
            DrawBoard();
            string[] options={"home","farm","cafe"};
            for(int i=0;i<3;i++)
            {
                string kind=options[i];FlatUI.Button(root,"Draft"+kind,(draft==kind?"+ ":"")+Pieces.Get(kind).Label,40+i*216,968,208,82,draft==kind?mint:panel,draft==kind?bg:cream,()=>{if(running||Game.Snapshot.Resolved)return;draft=kind;selected=-1;Draw();});
            }
            FlatUI.Button(root,"Rotate","ROTATE",40,1066,208,82,panel,cream,()=>{if(running)return;if(selected>0&&Game.Rotate(selected)){Save();Draw();}else{rotation=(rotation+1)%4;message="Place a piece before rotating.";Draw();}});
            FlatUI.Button(root,"Undo","UNDO",256,1066,208,82,panel,cream,()=>{if(running)return;Game.Undo();Save();Draw();});
            FlatUI.Button(root,"Preview","PREVIEW",472,1066,208,82,panel,cream,()=>{if(running)return;var r=Game.Preview();score.text="PREVIEW  "+r.Credits.ToString("N0")+" cr";hint.text=r.Credits==0?"No sales yet. Check ports and the supply path.":"Exact 8-beat preview · no rewards granted.";});
            FlatUI.Button(root,"Activate",Game.Snapshot.Resolved?"VIEW RESULT":"ACTIVATE CHAIN  →",40,1164,640,88,mint,bg,()=>{if(running)return;if(Game.Snapshot.Resolved)ShowResult();else StartCoroutine(Activate());});
            if(Game.Snapshot.Resolved)score.text="LAST RUN  "+Game.Preview().Credits.ToString("N0")+" cr";
            Probe();
        }
        string HelpLine()
        {
            if(!saveEnabled)return message;
            if(Game.Snapshot.Resolved)return "Saved result. Edit this layout or try the next board.";
            if(draft!="")return "Tap an empty cell to place "+Pieces.Get(draft).Label+".";
            if(Game.Snapshot.Board.Challenge==0)return "Tap the CAFE, then the cell next to HOME. Goods: 14.";
            return "Connect HOME → CAFE. Move FARM below CAFE (↑).";
        }
        void DrawBoard()
        {
            if(grid)Destroy(grid.gameObject);
            grid=FlatUI.Panel(root,"Board",60,352,600,600,panel);int w=Game.Snapshot.Board.Width,h=Game.Snapshot.Board.Height;
            for(int y=0;y<h;y++)for(int x=0;x<w;x++)
            {
                int cx=x,cy=y;var node=Game.Snapshot.Board.Nodes.Find(n=>n.X==x&&n.Y==y);bool sel=node!=null&&node.Id==selected;
                var cell=FlatUI.Panel(grid,"Cell"+x+"_"+y,x*720f/w+3,y*1280f/h+5,720f/w-6,1280f/h-10,sel?FlatUI.Hex("#1B5A60"):FlatUI.Hex("#0C2433"));
                FlatUI.Hit(cell,()=>TapCell(cx,cy),()=>{if(node!=null&&!running&&!Game.Snapshot.Resolved){selected=node.Id;draft="";}},p=>{if(FlatUI.CellAt(grid,p,w,h,out int tx,out int ty))Move(tx,ty);});
                if(node==null){FlatUI.Text(cell,"Empty","·",0,0,720,1280,25,FlatUI.Hex("#305260"),TextAlignmentOptions.Center);continue;}
                var glyphRect=FlatUI.Rect(cell,"Glyph",36,38,648,1000);var glyph=glyphRect.gameObject.AddComponent<BuildingGlyph>();glyph.Kind=node.Kind;glyph.Rotation=node.Rotation;glyph.color=node.Kind=="cafe"||node.Kind=="grocer"?gold:mint;glyph.raycastTarget=false;
                FlatUI.Text(cell,"Name",Pieces.Get(node.Kind).Label,24,1050,672,160,16,cream,TextAlignmentOptions.Center);
                if(Game.Snapshot.Board.Challenge==0&&node.Kind=="cafe")FlatUI.Text(cell,"Stock","14 goods",30,855,660,150,13,muted,TextAlignmentOptions.Center);
            }
        }
        void TapCell(int x,int y)
        {
            if(running||Game.Snapshot.Resolved)return;var n=Game.Snapshot.Board.Nodes.Find(z=>z.X==x&&z.Y==y);
            if(n!=null){selected=n.Id;draft="";Draw();hint.text=Pieces.Get(n.Kind).Hint+" · tap an empty cell to move";return;}
            if(draft!="")
            {if(Game.Place(draft,x,y)){draft="";Save();Draw();}else hint.text="Budget exceeded or this piece is unavailable.";}
            else Move(x,y);
        }
        void Move(int x,int y){if(running||Game.Snapshot.Resolved)return;if(Game.Move(selected,x,y)){Save();Draw();}else if(hint)hint.text="Choose a piece, then a free cell.";}
        IEnumerator Activate()
        {
            var result=Game.Preview();if(!Game.Activate(Guid.NewGuid().ToString("N")))yield break;
            running=true;Save();long shown=0;
            for(int beat=1;beat<=8;beat++)
            {
                DisplayBeat=beat;shown+=result.Events.Where(e=>e.Beat==beat).Sum(e=>e.Credits);score.text=shown.ToString("N0")+" cr";hint.text="BEAT "+beat+" / 8  ·  "+result.Events.Count(e=>e.Beat==beat&&e.Count>0&&e.Credits==0)+" shipments";
                foreach(var e in result.Events.Where(e=>e.Beat==beat&&e.Credits==0))StartCoroutine(Pulse(e));
                yield return new WaitForSecondsRealtime(reduced?.06f:.48f);
            }
            running=false;Draw();ShowResult();
        }
        IEnumerator Pulse(FlowEvent e)
        {
            if(reduced)yield break;var b=Game.Snapshot.Board;var a=b.Nodes.Find(n=>n.Id==e.Source);var z=b.Nodes.Find(n=>n.Id==e.Target);if(a==null||z==null)yield break;
            var dot=FlatUI.Panel(grid,"ResourcePulse",0,0,32,32,e.Kind==0?mint:e.Kind==1?gold:cream);dot.anchorMin=dot.anchorMax=Vector2.zero;dot.sizeDelta=new Vector2(12,12);var rect=grid.rect;
            Vector2 P(Node n)=>new Vector2((n.X+.5f)/b.Width*rect.width,(1-(n.Y+.5f)/b.Height)*rect.height);
            for(float t=0;t<1;t+=Time.unscaledDeltaTime*3){if(!dot)yield break;dot.anchoredPosition=Vector2.Lerp(P(a),P(z),t);yield return null;}if(dot)Destroy(dot.gameObject);
        }
        RectTransform Modal(string title)
        {
            if(overlay)Destroy(overlay.gameObject);overlay=FlatUI.Panel(root,"Modal",18,240,684,850,FlatUI.Hex("#153E50"));overlay.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;
            FlatUI.Text(overlay,"Heading",title,44,50,632,180,38,cream);return overlay;
        }
        void ShowResult()
        {
            var r=Game.Preview();var m=Modal(r.Credits>=Game.Target?"CHAIN COMPLETE":"KEEP EXPERIMENTING");
            FlatUI.Text(m,"Total",r.Credits.ToString("N0")+" credits",44,260,632,150,58,mint);
            string breakdown=string.Join("\n",Game.Snapshot.Board.Nodes.Where(n=>r.Events.Any(e=>e.Source==n.Id&&e.Credits>0)).Select(n=>Pieces.Get(n.Kind).Label+"   "+r.Events.Where(e=>e.Source==n.Id).Sum(e=>e.Credits).ToString("N0")));
            FlatUI.Text(m,"Breakdown",(breakdown==""?"No outlet had both customers and goods.":breakdown)+"\n\n"+r.Consumed[0]+" customers · "+r.Consumed[1]+" goods used\nFirst-clear ledger: "+Game.Snapshot.Wallet+" credits",44,440,632,360,25,cream);
            FlatUI.Button(m,"Edit","EDIT LAYOUT",44,860,632,135,mint,bg,()=>{Game.Edit();Save();Draw();});
            FlatUI.Button(m,"Next","NEXT BOARD  →",44,1020,632,135,panel,cream,()=>{Game.Next();selected=2;draft="";Save();Draw();});
        }
        void ShowHelp()
        {
            if(running)return;var m=Modal("MAKE THE CONNECTION");
            FlatUI.Text(m,"Rules","Tap a piece, then an empty cell to move. Or drag it.\n\nLime ports send. Amber ports receive. ROTATE changes the real connection.\n\nCAFE needs customers + goods. Board 1 starts with 14 goods. FARM needs its one supplied energy charge.\n\nPREVIEW is exact and free. ACTIVATE runs 8 beats. No timer, no purchases.",44,240,632,620,25,cream);
            FlatUI.Button(m,"Motion",reduced?"MOTION: REDUCED":"MOTION: ON",44,880,308,120,panel,cream,()=>{reduced=!reduced;ShowHelp();});
            FlatUI.Button(m,"Reset","RESET BOARD",368,880,308,120,panel,cream,()=>{saveEnabled=true;Game.Reset();Save();Draw();});
            FlatUI.Button(m,"Close","BACK TO BOARD",44,1030,632,130,mint,bg,Draw);
        }
        void Probe()
        {
            if(!Debug.isDebugBuild&&!Application.isEditor)return;string dir=FlatUI.Arg("-capture-dir");if(dir==null)return;Directory.CreateDirectory(dir);File.WriteAllText(Path.Combine(dir,"state.json"),JsonUtility.ToJson(Game.Export(),true));
        }
    }
}
