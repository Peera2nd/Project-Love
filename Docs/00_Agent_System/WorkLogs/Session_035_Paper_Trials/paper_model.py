"""Offline design arithmetic, not Unity/game implementation. Run from workspace root."""
from pathlib import Path
import hashlib,json,math,csv,statistics
OUT=Path('Docs/00_Agent_System/WorkLogs/Session_035_Paper_Trials');OUT.mkdir(parents=True,exist_ok=True)
PEOPLE=('design','art','code')
TASKS={
 'brief':dict(owner='design',effort=12,deps=(),earlier=None),
 'rules':dict(owner='design',effort=12,deps=(),earlier='brief'),
 'scene':dict(owner='art',effort=12,deps=(),earlier=None),
 'character':dict(owner='art',effort=20,deps=('brief',),earlier='scene'),
 'setup':dict(owner='code',effort=12,deps=(),earlier=None),
 'integration':dict(owner='code',effort=20,deps=('character',),earlier='setup')}
QUEUES={who:[t for t,d in TASKS.items() if d['owner']==who] for who in PEOPLE}
COMFORT={'brief':6,'rules':14,'scene':6,'character':14,'setup':6,'integration':22}
TIGHT={**COMFORT,'brief':1,'character':6,'integration':12}
SCENARIOS=('comfortable','compressed','disruption_hold','disruption_replan','disruption_replan_ot')
MODELS=('fixed','phase_random','task_day_random')
def band(s):
 return (1.,.05) if s<40 else (.90,.10) if s<70 else (.75,.15) if s<90 else (.55,.20)
def u(seed,*key):
 raw=('paper-v1|'+str(seed)+'|'+'|'.join(map(str,key))).encode()
 return int.from_bytes(hashlib.sha256(raw).digest()[:8],'big')/2**64

def is_work(t):return t%4<3

def finish_after(start,count):
 t=start
 while count:
  if is_work(t):count-=1
  if count:t+=1
 return t

def quick_forecast(now,remaining,stress,finished):
 cache={};starts={};ticks={}
 def project(task):
  if task in finished:return finished[task]
  if task in cache:return cache[task]
  d=TASKS[task];prior=list(d['deps'])+([d['earlier']] if d['earlier'] else [])
  start=max([now]+[project(x)+1 for x in prior])
  while not is_work(start):start+=1
  n=max(1,math.ceil((remaining[task]-1e-9)/(4*band(stress[d['owner']])[0])))
  starts[task]=start;ticks[task]=n;cache[task]=finish_after(start,n)
  return cache[task]
 for task in TASKS:project(task)
 return cache,starts,ticks

def simulate(seed,model,scenario,trace=False):
 remaining={t:float(d['effort']) for t,d in TASKS.items()};stress={w:30. for w in PEOPLE}
 finished={};due=dict(TIGHT if scenario=='compressed' else COMFORT);peak=30.;ot={w:0 for w in PEOPLE}
 fees=0;records=[];day_start=dict(stress);completion=None;week_end=None;event=False
 for t in range(56):
  day=t//4+1;phase=t%4
  if phase==0:day_start=dict(stress)
  if t==8 and scenario.startswith('disruption'):
   event=True
   for task in ('character','integration'):
    assert task not in finished
    remaining[task]+=12
   for w in ('art','code'):stress[w]=min(100,stress[w]+4)
   if scenario in ('disruption_replan','disruption_replan_ot'):
    due['character']=18;due['integration']=26
  forecasts,starts,ticks=quick_forecast(t,remaining,stress,finished)
  pressures={w:0 for w in PEOPLE}
  for task in starts:
   available=sum(is_work(k) for k in range(starts[task],due[task]+1))
   ratio=ticks[task]/max(1,available)
   p=2 if available==0 or ratio>1 else 1 if ratio>.8 else 0
   pressures[TASKS[task]['owner']]=max(pressures[TASKS[task]['owner']],p)
  # Choose from the start-of-phase completion snapshot; no same-phase dependency spill.
  targets={}
  for w in PEOPLE:
   targets[w]=next((x for x in QUEUES[w] if x not in finished and all(d in finished for d in TASKS[x]['deps'])),None)
  for w in PEOPLE:
   task=targets[w];before=stress[w];output=0.;is_ot=False
   if phase==3:
    if scenario=='disruption_replan_ot' and event and day<=7 and task and stress[w]<70 and ot[w]<2:
     # A bounded paper policy requests OT only if normal-hours projection threatens the fixed demo.
     if any(forecasts.get(x,-1)>26 for x in QUEUES[w] if x not in finished):
      is_ot=True;stress[w]=min(100,stress[w]+10);ot[w]+=1;fees+=5
    if not is_ot:stress[w]=max(0,stress[w]-5)
   if task and (phase<3 or is_ot):
    mean,width=band(stress[w]);factor=1.
    if model=='phase_random':factor=1+width*(2*u(seed,'phase',w,task,t)-1)
    elif model=='task_day_random':
     daily_width=band(day_start[w])[1]
     factor=(.9+.2*u(seed,'task',w,task))*(1+daily_width*(2*u(seed,'day',w,day)-1))
    output=min(remaining[task],4*mean*factor);remaining[task]=max(0,remaining[task]-output)
    if remaining[task]<1e-8:remaining[task]=0;finished[task]=t
    if not is_ot:stress[w]=min(100,stress[w]+1+pressures[w])
   elif phase<3:
    waiting=any(x not in finished for x in QUEUES[w])
    stress[w]=min(100,stress[w]+1) if waiting and pressures[w]>0 else max(0,stress[w]-1)
   if day<=7:peak=max(peak,stress[w])
   if trace and day<=7:records.append(dict(day=day,phase=('Morning','Daytime','Evening','Night')[phase],worker=w,task=task or '',effort=round(output,4),stress_before=round(before,2),stress_after=round(stress[w],2),pressure=pressures[w],ot=is_ot))
  if len(finished)==len(TASKS) and completion is None:completion=t
  if t==27:week_end=dict(stress)
  if t>=27 and completion is not None:break
 assert completion is not None,(seed,model,scenario,remaining)
 return dict(finish=completion,internal_late=sum(finished[x]>due[x] for x in TASKS),peak=peak,week_end=week_end,fees=fees,ot=sum(ot.values()),finished=finished,due=due,trace=records)

def quantile(xs,p):
 ys=sorted(xs);return ys[math.ceil(p*len(ys))-1]

def stamp(t):return 'Day '+str(t//4+1)+' '+('Morning','Daytime','Evening','Night')[t%4]
results=[];runs=1000
for model in MODELS:
 for scenario in SCENARIOS:
  rows=[simulate(seed,model,scenario) for seed in range(runs)]
  q=[quantile([r['finish'] for r in rows],p) for p in (.1,.5,.9)]
  row=dict(model=model,scenario=scenario,runs=runs,on_time=sum(r['finish']<=27 for r in rows),deadline_rate=round(sum(r['finish']<=27 for r in rows)/runs,4),finish_p10=q[0],finish_p50=q[1],finish_p90=q[2],finish_window=[stamp(x) for x in q],mean_peak_stress=round(statistics.mean(r['peak'] for r in rows),2),p90_peak_stress=quantile([r['peak'] for r in rows],.9),mean_late_internal_tasks=round(statistics.mean(r['internal_late'] for r in rows),2),mean_ot=round(statistics.mean(r['ot'] for r in rows),3),mean_ot_fees=round(statistics.mean(r['fees'] for r in rows),3))
  results.append(row)
  print(model,scenario,row['on_time'],row['finish_window'],'peak',row['mean_peak_stress'],'late',row['mean_late_internal_tasks'],'OT',row['mean_ot'],flush=True)
for scenario in SCENARIOS:
 r=simulate(17,'task_day_random',scenario,True)
 with (OUT/('trace_seed17_'+scenario+'.csv')).open('w',encoding='utf-8',newline='') as f:
  writer=csv.DictWriter(f,fieldnames=r['trace'][0].keys());writer.writeheader();writer.writerows(r['trace'])
summary=dict(kind='offline_design_paper_simulation_not_Unity',version=1,seeds=list(range(runs)),models=MODELS,scenarios=SCENARIOS,task_definitions=TASKS,comfortable_due_cursors=COMFORT,compressed_due_cursors=TIGHT,results=results,limits=['Illustrative six-task week; not actual Studio SO workload.','No player energy/economy/romance, full 30-day/route or UI simulation.','Forecast pressure uses current-stress mean projections, not hidden future random factors.','Optional automatic handoff and queue policy are paper assumptions, not approved runtime changes.','Same keyed random inputs across schedules; model differs intentionally.'])
(OUT/'summary.json').write_text(json.dumps(summary,indent=2)+'\n',encoding='utf-8')
with (OUT/'summary.csv').open('w',encoding='utf-8',newline='') as f:
 writer=csv.DictWriter(f,fieldnames=[k for k in results[0] if k!='finish_window']);writer.writeheader();writer.writerows({k:v for k,v in r.items() if k!='finish_window'} for r in results)
