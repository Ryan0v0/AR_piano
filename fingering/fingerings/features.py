"""
Feature extractors for fingering linear chain CRFs.

Authors: Stephen Koo, Mark Peng 2013
"""
from collections import Counter

# def calculateFingeringGap(f1, f2):
#     """
#     @param f1 string - fingering of a note
#     @param f2 string - fingering of a note
#     """
#     return f2 - f1

# def calculateNoteGap(n1, n2):
#     """
#     @param n1, n2 - string notes
#     """
#     return n2 - n1

# def getPitchClass(note):
#     return (note % 12)

def extract(t, y_, y, xs):
    """
    @param t int - index in the observation sequence, 0-based.
    @param y_ int - value of fingering at time t-1 (y_{t-1}),
    @param y int - value of of fingering at time t (y_{t}),
    @param xs list int - list of full observation sequence
    @return Counter - feature vector
    """
    
    features = Counter()

    x_, x = xs[t-1], xs[t]
    # extract pitch classes
    xc_, xc = (x_ % 12), (x % 12)
    
    ## MYOPIC FEATURES
    # Direct feature on the consecutive pair of fingerings
    features[(y_, y)] = 1.0

    ## LOOKBACKS
    if t == 0:
        # indicator feature on y and that it's the beginning
        features[('PREV', y, '-BEGIN-')] = 1.0
    else:
        # indicator feature on y and previous note
        features[('PREV', y, xc_)] = 1.0
        # indicator feature on y or y_ and the jump from previous note
        features[('GAP', y, x - x_)] = 1.0
        features[('GAP_', y_, x - x_)] = 1.0
        # stretch factor, conditioned on whether or not the previous finger was thumb
        features[('STRETCH', (y_ == 1))] = (abs(x - x_) + 1) / (abs(y - y_) + 1)
        # indicator for direct relationship between notes and fingerings
        features[('MAPPING', xc_, x - x_, y_, y)] = 1.0
        # crossover problem indicator (downwards)
        features['CROSSDOWN'] = float(y_ != 1 and y < y_ and x < x_)
        # crossover problem indicator (upwards)
        features['CROSSUP'] = float(y != 1 and y > y_ and x > x_)
    
    ## LOOKAHEADS
    if t == (len(xs) - 1):
        # indicator feature on y and that it's the end
        features[('NEXT', y, '-END-')] = 1.0
    else:
        xn = xs[t+1]
        xcn = xn % 12
        # indicator on y and the next note
        features[('NEXT', y, xcn)] = 1.0
        # indicator feature on y and the gap between current note and next note
        features[('GAP+', y, xn - x)] = 1.0
        
    return features

