# -*- coding: utf-8 -*-
"""
Created on Tue May 17 15:43:36 2022

@author: ravim
"""
import langdetect
import os

class LangMatcher():
    def __init__(self, path):
        '''

        Parameters
        ----------
        path : string
            Path to the directory of which we will creat a LangMatcher object.

        Returns
        -------
        None. 
        
        Creates a dictionary containing the languages as keys and, as values, 
        another dictionary containing ngrams as keys and frequencies as values

        '''
        self.path = path
        
        splitsen = path.split("/")[1].split("-")
        self.n = int(splitsen[0])
        self.limit = int(splitsen[1])
        
        talen = {}
        
        for f in os.listdir(path):
            talen[f] = langdetect.read_ngrams(path + "/" + f)
            
        self.talen = talen
        
    def score(self, text, k_best=1):
        '''

        Parameters
        ----------
        text : string
            All the ngrams that the file contained.
        k_best : integer, optional
            An integer that determines how many of the best matching languages
            will be appended into a list(match). 
            The default is 1.

        Returns
        -------
        list
            A list containing the k_best scoring languages.

        '''
        matchtable = langdetect.ngram_table(text, self.n, self.limit)
        match = []
        for i in self.talen.keys():
            match.append((i, langdetect.cosine_similarity(matchtable, self.talen[i])))
        match.sort(key=lambda x: x[1], reverse=True)
        
        return match[0: k_best]
    
    def recognize(self, filename, encoding="utf-8"):
        '''
        Parameters
        ----------
        filename : string
            The name of the file that we want to run in the recognizer.
        encoding : string 
            This is the specific code the file written in. The default is "utf-8".
        Returns the score def


        '''
        with open(filename, encoding=encoding) as file:
            x = file.read().replace("\n", ' ')
        return self.score(x)
    
import sys

args = sys.argv

del args[0]

if len(args) >= 2:
    matcher = LangMatcher(args[0])
    
    for i in range(1, len(args)):
        filename = args[i].split("/")[-1]
        print("file:", filename, "language:", matcher.recognize(args[i])[0][0], "score:", matcher.recognize(args[i])[0][1])