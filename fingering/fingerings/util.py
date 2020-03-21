"""
Utility functions and classes.

Created by the Stanford CS221 Autumn 2013 course staff.
Originally "util.py" from the "ner" assignment.
"""

import sys
import math, time, random, re
from collections import Sequence

###########################
# START ORIGINAL NER CODE #
###########################

def argmax(f, xs):
    """Our reference argmax implementation"""
    return max((f(x), x) for x in xs)[1]

def multinomial( pdf ):
    """
    Draw from a multinomial distribution
    @param pdf list double - probability of choosing value i
    OR
    @param pdf Counter - probability of choosing value i
    @return int - a sample from a multinomial distribution with above pdf

    Example:
      multinomial([0.4, 0.3, 0.2, 0.1]) will return 0 with 40%
      probability and 3 with 10% probability.
      multinomial({'a':0.4, 'b':0.3, 'c':0.2, 'd':0.1}) will return 'a' with 40%

    """

    if isinstance(pdf, Sequence):
        assert( abs( sum(pdf) - 1. ) < 1e-4 )

        cdf = [0.] * len(pdf)
        for i in xrange(len(pdf)):
            cdf[i] = cdf[i-1] + pdf[i] # Being clever in using cdf[-1] = 0.
        rnd = random.random()
        for i in xrange(len(cdf)):
            if rnd < cdf[i]: 
                return i
        else:
            return len(cdf) - 1
    elif isinstance(pdf, dict):
        names, pdf = zip(*pdf.iteritems())
        return names[ multinomial( pdf ) ]
    else:
        raise TypeError

class Counters:
    """
    Utility functions for counters
    """

    @staticmethod
    def dot( vec1, vec2 ):
        """
        Compute the dot product of two _Counters_. 
        @param vec1 Counter - a sparse vector represented as a counter
        @param vec2 Counter - a sparse vector represented as a counter
        @return double - Their dot product
        """
        if len( vec1 ) < len( vec2 ):
            vec2, vec1 = vec1, vec2
        # vec 2 is always smallest

        v = 0.
        for key, val in vec2.iteritems():
            v += vec1[key] * val 
        return v

    @staticmethod
    def scale( vec1, value ):
        """
        Scale everything in vec1 by value
        """
        for key in vec1:
            vec1[key] *= value
        return vec1

    @staticmethod
    def shift( vec1, value ):
        """
        Shift everything in vec1 by value
        """
        for key in vec1:
            vec1[key] += value
        return vec1

    @staticmethod
    def norm( vec ):
        """
        Compute the 2-norm of a sparse vector (represented as a Counter)
        """
        return math.sqrt( sum( math.pow(v, 2) for v in vec.values() ) )

    @staticmethod
    def approximateEquals( vec1, vec2, eps = 1e-2 ):
        """Approximately check that the contents of two counters match"""
        for key, val in vec1.iteritems():
            if abs( vec2[key] - val ) > eps:
                return False
        for key, val in vec2.iteritems():
            if abs( vec1[key] - val ) > eps:
                return False
        return True

    @staticmethod
    def avgDifference( vec1, vec2 ):
        """Approximately check that the contents of two counters match"""
        err = 0.
        count = 0
        for key, val in vec1.iteritems():
            err += abs( vec2[key] - val ) / (count+1)
            count += 1
        for key, val in vec2.iteritems():
            if key not in vec1:
                err += abs( val ) / (count+1)
                count += 1
        return err


def update_progress(progress):
    """
    Prints a pretty progress bar
    Accepts a float between 0 and 1. Any int will be converted to a float.
    A value under 0 represents a 'halt'.
    A value at 1 or bigger represents 100%
    @param progress double - % progress.
    """
    barLength = 20 # Modify this to change the length of the progress bar
    status = ""
    if isinstance(progress, int):
        progress = float(progress)
    if not isinstance(progress, float):
        progress = 0
        status = "error: progress var must be float\r\n"
    if progress < 0:
        progress = 0
        status = "Halt...\r\n"
    if progress >= 1:
        progress = 1
        status = "Done...\r\n"
    block = int(round(barLength*progress))
    text = "\rPercent: [{0}] {1}% {2}".format( "="*block + " "*(barLength-block), progress*100, status)
    sys.stdout.write(text)
    sys.stdout.flush()

class Timer:
    """Utility class to time steps"""
    def __init__(self):
        """Initialize state to off"""
        self.state = "off"
        self.__ticks = 0

    def start(self):
        "Turns state on and records start time"""
        self.state = "on"
        self.__ticks = time.time()

    def stop(self):
        "Turns state off"""
        self.state = "off"

    def ticks(self):
        "Prints out difference from start-time"""
        assert self.state == "on"
        return time.time() - self.__ticks

    def reset(self):
        "Resets time without changing state"""
        self.__ticks = time.time()

#########################
# END ORIGINAL NER CODE #
#########################

def itersplit(s, sep=None):
    """
    Generator equivalent to string.split().
    Found on StackOverflow:
    http://stackoverflow.com/questions/3862010/is-there-a-generator-version-of-string-split-in-python
    """
    exp = re.compile(r'\s+' if sep is None else re.escape(sep))
    pos = 0
    while True:
        m = exp.search(s, pos)
        if not m:
            if pos < len(s) or sep is not None:
                yield s[pos:]
            break
        if pos < m.start() or sep is not None:
            yield s[pos:m.start()]
        pos = m.end()
