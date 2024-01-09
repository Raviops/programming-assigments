# -*- coding: utf-8 -*-
"""
Created on Tue May 17 11:29:01 2022

@author: ravim
"""

import os
import langdetect


def make_profiles(datafolder, n=3, limit=200):
    '''
    

    Parameters
    ----------
    datafolder : folderpath
        This function will create folders containing the models in this datafolder.
    n : integer, optional
        This integer determines what type of ngram the folder will contain.
        This n can be found in the name of the folder that contains the files (leftmost number).
        The default is 3.
    limit : integer, optional
        In the list of most frequent ngrams, the limit number of ngrams will be shown. The default is 200.

    Returns
    -------
    None.
    Creates folders.

    '''
    path = "./models/" + str(n) + "-" + str(limit) + "/"
    
    for filename in os.listdir(datafolder):
        Language_Encoding = filename.split("-")
        Encoding = Language_Encoding[1]
        Language = Language_Encoding[0]
        
        if Encoding == "UTF8":
            with open(datafolder + "/" + filename, "r", encoding="utf8") as UTF8_file:
                data = UTF8_file.read()
        
        else:
            with open(datafolder + "/" + filename, "r", encoding="latin1") as Latin1_file:
                data = Latin1_file.read()
        
        table = langdetect.ngram_table(data, n, limit)
        
        if os.path.exists(path):
            langdetect.write_ngrams(table, path + Language)
        else:
            os.makedirs(path)
            langdetect.write_ngrams(table, path + Language)

make_profiles("./datafiles/training/", 3, 200)
make_profiles("./datafiles/training/", 2, 200)
