# -*- coding: utf-8 -*-
"""
Created on Thu May 19 13:56:54 2022

@author: ravim
"""
import os
import match_language
import langdetect

def eval(model_path, test_path):
    '''
    Parameters
    ----------
    model_path : path to model directory
        This directory is used to make a LangMatcher object, so that it can be used
        to compare the models to the test files.
    test_path : path to directory containing test files
        This directory is used to access the test files. These files will be 
        used by the recognizer. The recognizer will 'guess' what language the file is in.
        This will then be compared to the language the file is said to be in, so that we can check
        whether the file is assessed correctly by the recognizer.
        

    Returns
    -------
    None.
    
    This function gives an ERROR statement if the assessment is incorrect and counts how many
    files are assessed correctly and how many ar assessed incorrectly. It then prints a sentence
    containing information on the result of the specified collection.

    '''
    aantalfout = 0
    aantalgoed = 0
    suffixcodes = {'da': 'Danish', 'de': 'German', 'el': 'Greek', 'en': 'English',
                   'es': 'Spanish', 'fi': 'Finnish', 'fr': 'French', 'it': 'Italian',
                   'nl': 'Dutch', 'pt': 'Portuguese', 'sv': 'Swedish'}
    
    
    model_obj = match_language.LangMatcher(model_path)
    
    files = os.listdir(test_path)
    for file in files:
        recognize_lang = model_obj.recognize(test_path + "/" + file)[0][0]
        test_lang = file.split(".")[1]
        if suffixcodes[test_lang] != recognize_lang:
            print("{} {} ERROR {}".format(file, recognize_lang, suffixcodes[test_lang]))
            aantalfout += 1
        else:
            print("{} {}".format(file, recognize_lang))
            aantalgoed += 1
    
    filename = test_path.split("/")[2]
    num_sentences = filename.split("-")[1]
    
    slashsplit = model_path.split("/")
    mapnaam = slashsplit[1]
    n = mapnaam.split("-")[0]
    if n == "2":
        print("Bigram models for {}-word sentences: {} correct, {} incorrect".format(num_sentences, aantalgoed, aantalfout))
    else:
        print("Trigram models for {}-word sentences: {} correct, {} incorrect".format(num_sentences, aantalgoed, aantalfout))

if __name__ == "__main__":
    eval("models/2-200", "datafiles/test/europarl-10")
    print()
    eval("models/3-200", "datafiles/test/europarl-10")
    print()
    eval("models/2-200", "datafiles/test/europarl-30")
    print()
    eval("models/3-200", "datafiles/test/europarl-30")
    print()
    eval("models/2-200", "datafiles/test/europarl-90")
    print()
    eval("models/3-200", "datafiles/test/europarl-90")