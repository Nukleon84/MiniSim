import clr
clr.AddReference(r"..\..\bin\MiniSim.Core")
clr.AddReference(r"..\..\bin\MiniSim.FlowsheetDrawing")

import MiniSim.Core.Expressions as expr
from  MiniSim.Core.Flowsheeting import MaterialStream, Flowsheet,IconTypes,PortNormal
from  MiniSim.Core.Flowsheeting.Documentation import SpreadsheetElement
import MiniSim.Core.Numerics as num
from MiniSim.Core.UnitsOfMeasure import Unit, SI, METRIC, PhysicalDimension
from MiniSim.Core.ModelLibrary import Flash, Heater, Mixer, Splitter, EquilibriumStageSection,Source,Sink
import MiniSim.Core.PropertyDatabase as chemsep
from MiniSim.Core.Reporting import Generator, StringBuilderLogger
from MiniSim.Core.Thermodynamics import ThermodynamicSystem
from MiniSim.FlowsheetDrawing import FlowsheetDrawer, DrawingOptions

Database = chemsep.ChemSepAdapter()
logger = StringBuilderLogger();
reporter = Generator(logger)

import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import networkx as nx  
from IPython.display import Image as render



def calc(flowsheet, relax, maxIter):
    presolver=  num.BasicNewtonSolver(logger)
    presolver.MaximumIterations=maxIter
    presolver.BrakeFactor=relax
    presolver.Solve(flowsheet)
    print (logger.Flush())
    
def solve(flowsheet):
    solver= num.DecompositionSolver(logger)
    solver.Solve(flowsheet)
    print (logger.Flush())
    
def convertFlowsheet(flowsheet, options=None):
    drawer= FlowsheetDrawer()
    if(options!=None):
        drawer.Options=options
    return bytes(drawer.DrawAsBase64(flowsheet)) 
def report(item):
    reporter.Report(item)
    print(logger.Flush())
    return

def Genotype2Phenotype(genotype,sys):
    flowsheet= Flowsheet(f"Individual_Gen{genotype['Generation']}:{genotype['ID']}")
    for u in genotype['UnitGenes']:
        unit=None
        if u['Class']=="Flash":
            unit= Flash(u['ID'],sys)           
            for p in u['Parameters']:         
                unit.Specify(p['Name'],p['Value'], p['UoM'])            
            flowsheet.AddUnit(unit)
        if u['Class']=="Heater":
            unit= Heater(u['ID'],sys)           
            for p in u['Parameters']:         
                unit.Specify(p['Name'],p['Value'], p['UoM'])            
            flowsheet.AddUnit(unit)            
        if u['Class']=="Feed":
            unit= Source(u['ID'],sys)           
            for p in u['Parameters']:         
                unit.Specify(p['Name'],p['Value'], p['UoM'])    
            flowsheet.AddUnit(unit)
        if u['Class']=="Product":
            unit= Sink(u['ID'],sys)                          
            flowsheet.AddUnit(unit)
        if u['Class']=="Column":
            unit= Sink(u['ID'],sys)  
            base_name=u['ID']+"_"
            
            OV=MaterialStream(base_name+"OV",sys)
            LC=MaterialStream(base_name+"LC",sys)
            LR=MaterialStream(base_name+"LR",sys)            
            BO=MaterialStream(base_name+"BO",sys)
            VR=MaterialStream(base_name+"VR",sys)
            COL = (EquilibriumStageSection(u['ID'],sys,u['Stages'])
            .Connect("VIn", VR)
            .Connect("LIn", LR)
            .Connect("VOut", OV)
            .Connect("LOut", BO)            
            .MakeAdiabatic()
            .MakeIsobaric()
            .FixStageEfficiency(1.0)
            )

            REB =(Flash(base_name+"REB",sys)
            .Connect("In", BO)
            .Connect("Vap", VR)            
            .Specify("P", 1, METRIC.bar)
            .Specify("VF",u['Parameters'][1]['Value'])
            )    

            COND = (Heater(base_name+"COND",sys)
            .Connect("In", OV)
            .Connect("Out", LC)
            .Specify("P",1, METRIC.bar)
            .Specify("VF",0)
            )
    
            RS = (Splitter(base_name+"RS",sys)
            .Connect("In", LC)            
            .Connect("Out2", LR)
            .Specify("DP",0, METRIC.bar)
            .Specify("K",u['Parameters'][0]['Value'])
            )        

            flowsheet.AddUnits(COL,REB,COND,RS)
            flowsheet.AddMaterialStreams(OV,LC,LR,BO,VR)
        
    for s in genotype['StreamGenes']:                      
        stream=MaterialStream(s['ID'],sys)
        flowsheet.AddMaterialStream(stream)        
        flowsheet.GetUnit(s['From'][0]).Connect(s['From'][1],stream)
        
        unit_to=flowsheet.GetUnit(s['To'][0])
        if(unit_to.Class=="TraySection"):               
            unit_gene = next(filter(lambda x: x['ID'] == s['To'][0], genotype['UnitGenes']))            
            stage=unit_gene['FeedLoc']            
            unit_to.ConnectFeed(stream, stage)
        else:
            unit_to.Connect(s['To'][1],stream)
    return flowsheet


layerwidth=140
layerheight=300


def updatePosition(unit, depth,y,visitedNodes):
    
    if(unit in visitedNodes):
        return 0
    else:
        visitedNodes.append(unit)
        
    x=depth*layerwidth    
    h=0             
    for i,port in enumerate(unit.GetMaterialOutPorts()):
        if(not port.IsConnected):
            continue
        suc= port.Streams[0].Sink        
        if(suc):      
            h+=updatePosition(suc, depth+1, y+h,visitedNodes)
            if(port.Normal==PortNormal.Up or port.Normal==PortNormal.Down):
                h+=60

    y=y+unit.Icon.Height+h/2
    if(unit.Class=="Source" or unit.Class=="Sink"):
        unit.SetIcon(IconTypes.Stream, x,y)
    if(unit.Class=="Flash"):
        unit.SetIcon(IconTypes.TwoPhaseFlash,x ,y)
    if(unit.Class=="Heater"):
        unit.SetIcon(IconTypes.Heater, x,y)
    if(unit.Class=="Mixer"):
        unit.SetIcon(IconTypes.Mixer, x,y)        
    if(unit.Class=="Splitter"):
        unit.SetIcon(IconTypes.Splitter, x,y)
    if(unit.Class=="TraySection"):
        unit.SetIcon(IconTypes.ColumnSection, x,y)           
        
    return h


def AutomaticLayout(phenotype):    
    y=0
    visitedNodes=[]    
    
    sources= phenotype.GetUnitsByModelClass("Source")
    for s in sources:
        h=updatePosition(s,1,y,visitedNodes)
        y+=h
    
    for u in phenotype.Units:
        if(u.Icon.Y<y):
            y=u.Icon.Y
    
    if(y<0):
        y=abs(y)
    else:
        y=0
        
    for u in phenotype.Units:        
            u.Icon.Y+=y+50
    
    return
def intersection(lst1, lst2): 
    return list(set(lst1) & set(lst2)) 

def calculateFractionOfKnownPredecessors(graph, sequence, node):
    all_pre= list(graph.predecessors(node))
    known_pre= intersection(sequence,all_pre)
    return len(known_pre)/len(all_pre)

def generateGraph(flowsheet):
    G=nx.MultiDiGraph()
    #G.add_node(0)
    #G.add_edge(0, 1)
    for u in flowsheet.Units:
        G.add_node(u.Name)
    for s in flowsheet.MaterialStreams:
        #print(f"{s.Name} From: {s.Source} To: {s.Sink}")
        if(s.Source and s.Sink):
            #if(not G.has_edge(s.Sink.Name, s.Source.Name)):
                G.add_edge(s.Source.Name, s.Sink.Name)
    return G

def sequenceFlowsheet(flowsheet):
    G= generateGraph(flowsheet)
    
    sccs=nx.strongly_connected_components(G)
    sccs = list(reversed(list(sccs)))    
    
    seq=[]
    
    for scc in sccs:
        if(len(scc)==1):
            seq.append(list(scc)[0])
        else:
            strongComponent=scc.copy()            
            while(len(strongComponent) !=0):               
                ranking= sorted(strongComponent, 
                                key=lambda n: calculateFractionOfKnownPredecessors(G,seq,n),
                                reverse=True)
                seq.append(ranking[0])
                strongComponent.remove(ranking[0])        
    return G, seq, sccs

    
def initializeFlowsheet(flowsheet,seq,maxIter, solveMode):
    for i in range(maxIter):
        print(f"## iteration {i}")
        errorCounter=0
        for u in seq:        
            try:
                if(not solveMode):
                    flowsheet.GetUnit(u).Initialize()
                else:
                    flowsheet.GetUnit(u).Solve()
                print(f"Unit {u} initialized")
            except: 
                print(f"** Unit {u} not initialized")
                errorCounter+=1
    return
        
            