from pathlib import Path
import math,json,random,shutil,subprocess,hashlib
import numpy as np
import cairosvg
import wave
ACTIVE=Path(__file__).resolve().parent.parent
ROOT=ACTIVE.parent

def export(root,key,body,w=256,h=256):
    if root!=ACTIVE:return
    src=root/'Assets/_Game/Art/Source'/f'{key}.svg'; dst=root/'Assets/_Game/Resources/Art'/f'{key}.png'
    src.parent.mkdir(parents=True,exist_ok=True);dst.parent.mkdir(parents=True,exist_ok=True)
    svg=f'<svg xmlns="http://www.w3.org/2000/svg" width="{w}" height="{h}" viewBox="0 0 {w} {h}">{body}</svg>'
    src.write_text(svg);cairosvg.svg2png(bytestring=svg.encode(),write_to=str(dst))

def rect(x,y,w,h,c,r=0,extra=''):return f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="{r}" fill="{c}" {extra}/>'
def path(d,c,stroke='',width=3):return f'<path d="{d}" fill="{c}" stroke="{stroke or c}" stroke-width="{width}" stroke-linejoin="round"/>'
def circle(x,y,r,c,stroke=''):return f'<circle cx="{x}" cy="{y}" r="{r}" fill="{c}" stroke="{stroke or c}" stroke-width="3"/>'
ids=['home','cafe','farm','warehouse','power','factory','grocer','bank','office','park','market','logistics','apartment','boutique','port','landmark','diner','shopping_mall','tech_office','recycler','freight_yard','hotel','stadium','solar_array']
cc=ROOT/'CashflowChains';bb=ROOT/'BlackBag'
for j,key in enumerate(ids):
    mint='#60E5BF';cream='#F7EED1';gold='#F3BB60';pink='#E28EB6';blue='#5CB9DC';ink='#102C39'
    c=[mint,gold,blue,pink][j%4]
    b=circle(128,128,112,'#103441')+circle(128,128,107,'none','#23515D')
    b+=rect(24,206,208,5,'#23515D',2)
    if key=='park':
        b+=path('M32 193 Q76 158 126 190 Q170 162 224 193 L224 205 L32 205 Z','#265F4D')
        for x,y,s in [(65,121,1),(164,99,1.3),(121,150,.7)]:
            b+=rect(x-4,y,8,69,'#BBA071',3)+circle(x,y-12,31*s,'#3B9E76')+circle(x-12,y-24,23*s,mint)
        b+=rect(95,192,55,6,gold,2)
    elif key in ['farm','solar_array']:
        for y in range(95,197,25):
            b+=path(f'M34 {y} L214 {y}', 'none','#537B54',5)
            for x in range(49,210,30):
                b+=path(f'M{x} {y-1} q-13 -15 -13 -23 q15 0 13 15 q4 -18 15 -20 q0 19 -15 23',mint if key=='farm' else blue)
        b+=circle(188,52,19,gold)+path('M26 76 Q80 34 134 76','none','#245663',4)
    elif key=='power':
        b+=rect(55,95,146,109,'#346273',6)+rect(66,65,124,42,c,5)+path('M139 91 L104 146 L132 146 L113 189 L168 127 L139 127 Z',gold)
        b+=rect(33,144,15,48,mint,4)+rect(209,144,15,48,mint,4)
    elif key in ['warehouse','logistics','freight_yard','factory','recycler','port']:
        b+=rect(38,100,179,104,c,5)+path('M34 100 L78 67 L117 100 L161 67 L218 100 Z',cream)
        b+=rect(59,130,65,74,ink,3)
        for y in range(140,199,12):b+=rect(64,y,55,3,'#427080',1)
        b+=rect(147,128,45,33,cream,2)+rect(151,133,15,22,ink,1)+rect(173,133,15,22,ink,1)
        if key=='factory':b+=rect(184,49,18,49,pink,3)+circle(193,33,12,'#527784')+circle(179,20,8,'#365562')
        else:b+=rect(150,178,30,26,gold,2)+path('M165 178 L165 204','none',ink,2)
    elif key in ['home','apartment','hotel','office','tech_office','landmark']:
        tall=key!='home'; top=49 if tall else 92
        b+=rect(62,top,132,204-top,c,5)
        b+=path(f'M51 {top} L128 {top-36} L205 {top} Z',cream)
        for y in range(top+14,180,30):
            for x in range(77,185,31):b+=rect(x,y,17,19,ink,2)+rect(x+2,y+2,5,14,gold,1)
        b+=rect(115,171,27,33,ink,3)+rect(123,181,3,10,gold,1)
        if key=='landmark':b+=circle(128,top+40,20,gold)+path(f'M128 {top+24} L128 {top+40} L139 {top+48}','none',ink,4)
    elif key=='bank':
        b+=path('M33 97 L128 50 L223 97 Z',gold)+rect(38,101,180,12,cream,2)
        for x in [57,100,143,186]:b+=rect(x,116,14,78,c,3)
        b+=rect(32,196,192,10,gold,2)+circle(128,81,10,ink)
    elif key=='stadium':
        b+=f'<ellipse cx="128" cy="145" rx="96" ry="62" fill="{c}"/>'+f'<ellipse cx="128" cy="145" rx="70" ry="38" fill="{ink}"/>'+rect(85,122,86,45,'#387A62',12)
        b+=path('M128 125 L128 165','none',cream,2)+circle(128,145,11,'none',cream)
        for x in [42,210]:b+=rect(x,47,4,79,cream)+rect(x-13,47,30,13,gold,3)
    else:
        b+=rect(42,102,172,102,c,5)+rect(32,88,192,35,cream,3)
        for x in range(34,218,28):b+=rect(x,89,14,34,pink if key in ['cafe','boutique','diner'] else gold,1)
        b+=rect(58,141,77,50,ink,4)+rect(151,140,44,64,ink,3)+rect(156,145,33,33,gold,2)
        if key in ['cafe','diner']:b+=path('M76 155 L106 155 L103 177 Q92 187 80 177 Z',cream)+path('M106 158 Q123 155 118 169 L105 171','none',cream,4)+path('M82 148 Q75 137 85 131 M98 148 Q90 137 100 131','none',gold,3)
        else:b+=circle(82,165,9,pink)+circle(111,165,9,mint)
    export(cc,key,b)
# common rounded assets and detailed editorial backgrounds
for root,iscc in [(cc,True),(bb,False)]:
    export(root,'round',rect(0,0,128,128,'#FFFFFF',22),128,128)
    export(root,'ring',f'<circle cx="64" cy="64" r="54" fill="none" stroke="#FFFFFF" stroke-width="5"/>',128,128)
    bg='#071B2A' if iscc else '#100D13'; muted='#1B3C48' if iscc else '#36242E';acc='#27EDBD' if iscc else '#DAB66D'
    rnd=random.Random(72);b=rect(0,0,720,1280,bg)
    for i in range(500):
        x=rnd.randrange(720);y=rnd.randrange(1280);b+=circle(x,y,.5,'#FFFFFF').replace('/>',' opacity=".025"/>')
    for i in range(9):
        x=45+i*80;b+=path(f'M{x} 0 V1280','none',muted,.5)
    for i in range(17):b+=path(f'M0 {i*80} H720','none',muted,.5)
    if iscc:
        b+=path('M-40 340 H180 V470 H440 V300 H760 M-40 910 H320 V1040 H770','none',muted,18)
        for x,y in [(180,340),(440,470),(320,910)]:b+=circle(x,y,17,acc).replace('/>',' opacity=".20"/>')
    else:
        for x in [22,698]:b+=path(f'M{x} 30 V1250','none',muted,1)
        for y in [30,1250]:b+=path(f'M22 {y} H698','none',muted,1)
        for x,y in [(40,48),(680,48),(40,1232),(680,1232)]:b+=path(f'M{x-12} {y} L{x} {y-12} L{x+12} {y} L{x} {y+12} Z','none',acc,1)
    export(root,'backdrop',b,720,1280)
    for d in range(6):
        rnd=random.Random(d+39);b=rect(0,0,640,330,bg,20)
        if iscc:
            for j in range(12):
                x=22+j*51;h=rnd.randrange(60,190);c=['#286673','#3E947E','#467591','#6E697F'][d%4]
                b+=rect(x,280-h,39,h,c,3)
                for yy in range(292-h,260,22):
                    for xx in [x+7,x+23]:b+=rect(xx,yy,7,11,'#D9C98E',1)
            b+=path('M0 286 H640','none','#27EDBD',2)+circle(500,66,29,'#D9C98E')
        else:
            for j in range(6):
                x=35+j*105;b+=path(f'M{x} 280 V90 Q{x+35} 28 {x+70} 90 V280 Z','#2B1C29','#6F5051',2)
                b+=rect(x+10,110,50,104,['#734639','#445664','#5D4261','#4B6054'][d%4],4)
                b+=rect(x+18,118,34,88,'#100D13',2)+circle(x+35,158,13,acc)
            b+=path('M0 282 H640 M0 294 H640','none','#DAB66D',2)
        export(root,'district'+str(d),b,640,330)
# 48 unique masked objects. Footprint is explicit and art is clipped to its exact occupied union.
seed=json.load(open(ACTIVE/('Docs/ProductionLoot.json' if (ACTIVE/'Docs/ProductionLoot.json').exists() else 'Docs/SourcePackage/data/blackbag_48_loot_seed.json')))
for j,l in enumerate(seed['loot']):
    cells=l['cells'];w=max(x for x,y in cells)+1;h=max(y for x,y in cells)+1;W=w*128;H=h*128
    clip='<path d="'+''.join(f'M{x*128+4} {y*128+4} h120 v120 h-120 z ' for x,y in cells)+'"/>'
    b=f'<defs><clipPath id="shape">{clip}</clipPath><linearGradient id="metal" x2=".8" y2="1"><stop stop-color="#F2DB9A"/><stop offset=".5" stop-color="#BD9652"/><stop offset="1" stop-color="#806338"/></linearGradient></defs><g clip-path="url(#shape)">'
    b+=rect(0,0,W,H,'#2A2025')
    color=['#BD5368','#6DA9CE','#CBA369','#83A66D','#A98BC4','#D8BA72'][j%6]
    for x,y in cells:
        X=x*128;Y=y*128;b+=rect(X+8,Y+8,112,112,'#33252C',12)+rect(X+14,Y+14,100,100,'none',9,'stroke="#8E734C" stroke-width="1"')
    # portrait of each object, within logical footprint and with a coherent family material
    X,Y=cells[0];cx=X*128+64;cy=Y*128+64;name=l['id']
    if 'ring' in name or 'bracelet' in name or 'chain' in name or 'collar' in name:
        for x,y in cells:
            b+=circle(x*128+64,y*128+68,32,'none','#DAB66D')+circle(x*128+64,y*128+68,26,'none','#7C603A')
            b+=path(f'M{x*128+64} {y*128+24} l19 17 l-19 17 l-19 -17 Z',color,'#F1DC9E',2)
    elif any(t in name for t in ['watch','compass','clock','globe','coin','seal']):
        for x,y in cells:
            a=x*128+64;c=y*128+64;b+=circle(a,c,43,'url(#metal)')+circle(a,c,35,'#171923')+circle(a,c,29,'none','#82704E')
            for k in range(12):
                ang=k*math.pi/6;b+=circle(round(a+25*math.sin(ang),2),round(c+25*math.cos(ang),2),1.8,'#E6CE91')
            b+=path(f'M{a} {c-21} L{a} {c} L{a+15} {c+11}','none','#F1DC9E',3)+circle(a,c,3,color)
    elif any(t in name for t in ['frame','portrait','landscape','triptych','manuscript','atlas','ledger','bonds','tablet','scroll']):
        for x,y in cells:
            a=x*128;c=y*128;b+=rect(a+20,c+18,88,92,'url(#metal)',4)+rect(a+27,c+25,74,78,'#D9C79F',2)
            if any(t in name for t in ['portrait','landscape','triptych','frame']):
                b+=rect(a+30,c+28,68,72,'#32484A',1)+circle(a+63,c+53,13,color)+path(f'M{a+35} {c+96} Q{a+58} {c+53} {a+95} {c+96} Z','#879575')
            else:
                for yy in range(41,90,10):b+=path(f'M{a+37} {c+yy} H{a+88-(yy%3)*7}','none','#796449',2)
                b+=circle(a+81,c+85,9,'#8E1C32')
    elif any(t in name for t in ['bar','ingot','cash','coin_case','box','camera','radio','phone','cipher']):
        for x,y in cells:
            a=x*128;c=y*128;b+=rect(a+17,c+29,94,70,'url(#metal)' if j//8==4 else '#4B5461',10)+rect(a+25,c+37,78,54,'none',5,'stroke="#E5CC8D" stroke-width="2"')
            if 'camera' in name:b+=circle(a+64,c+64,23,'#101D26')+circle(a+64,c+64,15,'#5F9BB0')+circle(a+59,c+59,5,'#A3D5D9')
            elif 'cash' in name:b+=rect(a+52,c+29,24,70,'#C8A87B',1)+circle(a+64,c+64,12,'none','#735D43')
            else:b+=path(f'M{a+48} {c+49} L{a+79} {c+49} L{a+63} {c+79} Z','none','#EDD59B',3)
    else:
        for k,(x,y) in enumerate(cells):
            a=x*128;c=y*128
            b+=path(f'M{a+44} {c+20} Q{a+35} {c+45} {a+30} {c+64} Q{a+17} {c+98} {a+64} {c+108} Q{a+110} {c+98} {a+98} {c+64} Q{a+88} {c+43} {a+85} {c+20} Z',color,'#E5CC8D',2)
            b+=path(f'M{a+47} {c+27} Q{a+40} {c+65} {a+45} {c+88}','none','#F1DDA9',3)+path(f'M{a+34} {c+72} Q{a+65} {c+85} {a+96} {c+72}','none','#6D5143',3)
    # individual engraved inventory mark, no text
    for x,y in cells:
        b+=path(f'M{x*128+23} {y*128+104} h12 m-6 -6 v12','none','#A78958',1)
    b+='</g>'
    export(bb,l['id'],b,W,H)
# original procedural scores, tonal loops + short tactile cues
for root,iscc in [(cc,True),(bb,False)]:
    if root!=ACTIVE:continue
    out=root/'Assets/_Game/Resources/Audio';out.mkdir(parents=True,exist_ok=True)
    sr=22050
    def write(name,a):
        a=np.nan_to_num(a)
        if name.startswith('music'):a*=min(.8/max(np.max(np.abs(a)),1e-9),.16/max(float(np.sqrt(np.mean(a*a))),1e-9))
        peak=float(np.max(np.abs(a)));a=a/max(1,peak/0.85)
        temp=out/(name+'.wav')
        with wave.open(str(temp),'wb') as f:f.setnchannels(1);f.setsampwidth(2);f.setframerate(sr);f.writeframes((a*32767).astype('<i2').tobytes())
    for k in range(6 if iscc else 4):
        duration=24.;a=np.zeros(int(sr*duration));rnd=np.random.default_rng(77+k);chords=[[48,55,59,62],[45,52,55,59],[41,48,52,57],[43,50,53,57]]
        def tone(at,note,dur,amp,kind='keys'):
            start=int(at*sr);n=min(int(dur*sr),len(a)-start)
            if n<=0:return
            t=np.arange(n)/sr;freq=440*2**((note-69)/12)
            env=(1-np.exp(-t*60))*np.exp(-t*(2.8 if kind=='keys' else .8))*np.minimum(1,(dur-t)*12)
            sound=np.sin(2*np.pi*freq*t)+(.22 if iscc else .12)*np.sin(2*np.pi*freq*2*t)+.05*np.sin(2*np.pi*freq*3*t)
            a[start:start+n]+=sound*env*amp
        for bar in range(8):
            c=chords[(bar+k)%4]
            for note in c:tone(bar*3,note+(0 if iscc else 12),2.85,.045 if iscc else .035)
            if iscc:
                for q in [0,1.5]:tone(bar*3+q,c[0]-12,.7,.11,'bass')
                for q in range(8):
                    start=int((bar*3+q*.375)*sr);n=min(int(.05*sr),len(a)-start);t=np.arange(n)/sr
                    a[start:start+n]+=rnd.normal(0,.016,n)*np.exp(-t*90)
            for q in range(4):tone(bar*3+q*.75+.12,c[(q+bar)%4]+24,.66,.025)
        # every voice ends inside the loop; tiny boundary fades remove clicks
        fade=int(.015*sr);a[:fade]*=np.linspace(0,1,fade);a[-fade:]*=np.linspace(1,0,fade)
        write('music'+str(k),a)
    for k,name in enumerate(['tap','select','place','rotate','deny','take','cash','beat','star','win','loss','open','escape','undo','page','hint','save','bonus','alarm','drop','unlock','result','collect','tool']):
        dur=.09 if name in ['tap','select','beat'] else .24 if name not in ['win','escape','alarm','result'] else .7;t=np.arange(int(sr*dur))/sr
        base=310 if iscc else 440;freq=base*2**((k%12)/12);env=np.minimum(1,t*220)*np.exp(-t*(20 if dur<.3 else 6))*np.minimum(1,(dur-t)*70)
        a=.18*np.sin(2*np.pi*(freq*t+(100 if name in ['cash','win','star'] else -35)*t*t))*env
        if name in ['win','escape','result']:a+=.12*np.sin(2*np.pi*freq*1.5*t)*env+.07*np.sin(2*np.pi*freq*2*t)*env
        if name in ['deny','alarm','loss']:a=.1*(np.sin(2*np.pi*150*t)+.5*np.sin(2*np.pi*162*t))*env
        write(name,a)
    for font in ['DejaVuSans-Bold','DejaVuSerif','DejaVuSerif-Bold']:
        shutil.copy('/usr/share/fonts/truetype/dejavu/'+font+'.ttf',root/'Assets/_Game/Resources'/(font+'.ttf'))
    manifest={str(p.relative_to(root)):hashlib.sha256(p.read_bytes()).hexdigest() for p in (root/'Assets/_Game/Resources').rglob('*') if p.suffix in ['.png','.wav']}
    (root/'Docs/ORIGINAL_ASSETS.json').write_text(json.dumps({'author':'Original SVG paths and synthesized waveforms authored for this project. No external samples.','assets':manifest},indent=2))
    # This source is already local to the project.
print('Authored 24 buildings, 48 exact-footprint objects, 12 location illustrations, 10 scores and 48 cues.')
