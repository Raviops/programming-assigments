# -*- coding: utf-8 -*-
"""
Created on Sat May 14 19:38:42 2022

@author: ravim
"""
import re

def prepare(text):
    '''

    Parameters
    ----------
    text : string
        This will be the string that is going to be prepared.

    Returns
    -------
    wordlist : List of strings
        After the text has been cleaned, all of the words that can be found
        in the string are separated and then separately added to this list.

    '''
    CleanText = re.sub(r'[!?,."<>()]', " ", text)
    wordlist = CleanText.split()

    return wordlist

def ngrams(seq, n=3):
    '''

    Parameters
    ----------
    seq : string
        This is the string from which this function will determine the ngrams.
    n : Integer, optional
        With this integer you can determine what type of ngram you would like
        to find with this function, e.g. 2  for bigrams or 3 for trigrams.
        The default is 3, so the default is set to find the trigrams of a string.

    Returns
    -------
    ngrams : List of strings
        All the ngrams that this function acquires will be added to this list.
        So the ngrams of the string will all be listed here.

    '''
    ngrams = []
    
    for x in range(len(seq)):
        if len(seq[x: x + n]) >= n:
            ngram = seq[x: x + n]
            ngrams.append(ngram)
        
    return ngrams

from collections import Counter


def ngram_table(text, n=3, limit=0):
    '''

    Parameters
    ----------
    text : string
        The function finds the ngrams in the text 
        and calculates how frequently these ngrams occur in this text.
    n : integer, optional
        This integer indicates what type of ngram the function is going to search for.
        For bigrams fill in 2, for trigrams 3 etc.
        The default is set to 3, so it finds trigrams by default.
    limit : integer, optional
        This integer will determine the size of the dictionary that will be returned.
        EXAMPLE: Set to 5 to find the 5 most frequent ngrams in the text.
        The default is 0, which will return the full dictionary.

    Returns
    -------
    Dictionary
        The dictionary contains pairs of ngrams and their frequencies, 
        ngrams as keys and frequencies as values. 
        It starts with the most frequent ngrams and goes down to the least frequent.
        Its size can be determined by the parameter 'limit'
    '''
    
    tokenized_text = prepare(text)
    new_wordlist = []
    
    for word in tokenized_text:
        new_word = "<{}>".format(word)
        new_wordlist.append(new_word)
    
    ngramlist = []
    for angle_ngram in new_wordlist:
        ngramlist += ngrams(angle_ngram, n)
        
    ngramcounts = Counter(ngramlist)
    
    if limit == 0:
        return ngramcounts
    else:
        tuple_list = []
        
        for word, frequency in sorted(ngramcounts.items(), key=lambda x: x[1], reverse=True):
            tuple_list.append((word, frequency))
            
        ngram_dict = {word: frequency for (word, frequency) in tuple_list[0:limit]}
        return ngram_dict

def write_ngrams(table, filename):
    
    with open(filename, "w", encoding="utf8") as outfile:
        for words in table:
            outfile.write("{} {}\n".format(table[words], words))
    
def read_ngrams(filename):
    ngram_dict = dict()
    with open(filename, encoding="utf8") as file:
        lines_ = file.read()
        lines = lines_.splitlines()
        for line in lines:
            items = line.split()
            frequency = int(items[0])
            ngram = items[1]
            ngram_dict[ngram] = frequency
            
    return ngram_dict

import math

def cosine_similarity(known, unknown):
   '''
    Parameters
    ----------
    known : dictionary
        Contains ngram frequencies of a known reference text.
    unknown : dictionary
        Contains ngram frequencies of the text we want to identify.

    Returns
    -------
    take_cos : integer
        Should be 1 if the dictionaries are the same and close to 0
        if they are not similar.

    '''
    
   dot_product = 0
   mag_known = 0
   mag_unknown = 0
    
   for frequency_known in known.values():
       mag_known += frequency_known**2
   for frequency_unknown in unknown.values():
        mag_unknown += frequency_unknown**2
    
   magnitude_known = math.sqrt(mag_known)
   magnitude_unknown = math.sqrt(mag_unknown)
       
   for key in known.keys():
        if key in unknown:
            dot_product += known[key] * unknown[key]
            
   take_cos = dot_product / (magnitude_known * magnitude_unknown)
    
   return take_cos

    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
        