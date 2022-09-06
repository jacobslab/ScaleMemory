import os
import random
import shutil

class SplitData:
    def __init__(self, fileLocation, Images300, Images235, Images300file, Images235file, fLRemoved, fLRemovedPath):
        self.filelocation = fileLocation
        self.Images300 = Images300
        self.Images235 = Images235
        self.fLRemovedPath = fLRemovedPath
        self.NL = 216
        self.NegNL = 283
        self.Total = 0
        self.Removed = []
        
        file1 = open(fLRemoved, 'r')
        Lines = file1.readlines() 

        for line in Lines:
            (self.Removed).append(int(line))

        if not (os.path.exists(Images300)):
            os.makedirs(self.Images300) 
        else:
            os.system("rm -rf "+Images300)
            os.makedirs(self.Images300) 
        
        if not (os.path.exists(Images235)):
            os.makedirs(self.Images235) 
        else:
            os.system("rm -rf "+Images235)
            os.makedirs(self.Images235) 

        if not (os.path.exists(fLRemovedPath)):
            os.makedirs(self.fLRemovedPath) 
        else:
            os.system("rm -rf "+fLRemovedPath)
            os.makedirs(self.fLRemovedPath) 

        if not (os.path.exists(Images300file)):
            self.f300 = open(Images300file, 'w')
        else:
            os.remove(Images300file)
            self.f300 = open(Images300file, 'w')
        
        if not (os.path.exists(Images235file)):
            self.f235 = open(Images235file, 'w')
        else:
            os.remove(Images235file)
            self.f235 = open(Images235file, 'w')        

        self.D = {}
        self.RealList = []
        self.RShuffle = []
    
    def getList(self):
        count = 0
        for filename in sorted(os.listdir(self.filelocation)):
            if not filename == ".DS_Store":
                print(filename)
                WholePath = os.path.join(self.filelocation, filename)
                self.D[count] = WholePath
                count = count + 1
    
    def RealListToBeShuffled(self):
        for idx,loc in enumerate(self.D):
            if idx not in self.Removed:
               (self.RealList).append(idx)
        self.Total = len(self.RealList)
        self.NegNL = self.Total - self.NL

    def RSShuffle(self):
        print(len(self.RealList))
        #print(self.D[534])
        S = self.RealList
        random.shuffle(S)
        self.RShuffle = S
        
    def copyfirst300(self):
        for i in range(0,self.NegNL):
            shutil.copy2(self.D[self.RShuffle[i]],self.Images300)

        A = self.RShuffle[:self.NegNL]
        A.sort()

        for i in range(0,self.NegNL):
            (self.f300).write(str(A[i]))
            if not i == self.NegNL-1:
                (self.f300).write("\n")

    def copylast235(self):
        for i in range(self.NegNL,self.Total):
            shutil.copy2(self.D[self.RShuffle[i]],self.Images235)

        A = self.RShuffle[self.NegNL:self.Total]
        A.sort()

        for i in range(0,self.NL):
            (self.f235).write(str(A[i]))
            if not i == self.NL-1:
                (self.f235).write("\n")

    def writeremoved(self):
        for fKey in self.Removed:
            shutil.copy2(self.D[fKey],self.fLRemovedPath)


fL = "/Users/sai.vaddi/Documents/Unity/CityBlock/ScaleMemory/Assets/Resources_IGNORE/Images"
fLRemoved = "/Users/sai.vaddi/Documents/Unity/CityBlock/ScaleMemory/Assets/Resources_IGNORE/RemovedStimuli.txt"
fL300 = "/Users/sai.vaddi/Documents/Unity/CityBlock/ScaleMemory/Assets/Resources_IGNORE/Images300"
fL235 = "/Users/sai.vaddi/Documents/Unity/CityBlock/ScaleMemory/Assets/Resources_IGNORE/Images235"
fLRemovedPath = "/Users/sai.vaddi/Documents/Unity/CityBlock/ScaleMemory/Assets/Resources_IGNORE/RemovedStimuli"
fL300file = "/Users/sai.vaddi/Documents/Unity/CityBlock/ScaleMemory/Assets/Resources_IGNORE/Images300.txt"
fL235file = "/Users/sai.vaddi/Documents/Unity/CityBlock/ScaleMemory/Assets/Resources_IGNORE/Images235.txt"

sD = SplitData(fL,fL300,fL235,fL300file,fL235file,fLRemoved,fLRemovedPath)
sD.getList()
sD.RealListToBeShuffled()
sD.RSShuffle()
sD.copyfirst300()
sD.copylast235()
sD.writeremoved()



