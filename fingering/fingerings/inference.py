"""
Linear Chain CRF Inference Tools
Functions for inference of linear chain CRFs using Viterbi algorithm
and Gibbs sampling.

Created by the Stanford CS221 Autumn 2013 course staff.
Originally "submission.py" from the "ner" assignment.
"""

import itertools as it
import math, random

from collections import Counter, namedtuple, defaultdict
import operator

import util
from util import Counters

BEGIN_TAG = '-BEGIN-'

###############################################
# Problem 1. Linear Chain CRFs
###############################################

class LinearChainCRF(object):
    r"""
    This is a 'struct' that contains the specification of the CRF, namely
    the tags, featureFunction and parameters.
    """

    def __init__(self, tags, featureFunction, parameters = None ):
        r"""
        @param tags list string - The domain of y_t. For NER, these
               will be tags like PERSON, ORGANIZATION, etc.
        @param featureFunction function - Function that takes the time step
               t, previous tag y_{t-1}, current tag y_t, and observation
               sequence x, and returns a Counter representing the feature vector
               \phi_{local}(t, y_{t-1}, y_t, x).
               - E.g. unaryFeatureFunction, binaryFeatureFunction
        @param parameters Counter - parameters for the model (map from feature name to feature weight).
        """
        self.TAGS = tags
        self.featureFunction = featureFunction
        if parameters is None:
            parameters = Counter()
        self.parameters = parameters

    def G(self, t, y_, y, xs):
        r"""
        Computes one of the potentials in the CRF.
        @param t int - index in the observation sequence, 0-based.
        @param y_ string - value of of tag at time t-1 (y_{t-1}),
        @param y string - value of of tag at time t (y_{t}),
        @param xs list string - The full observation seqeunce.
        @return double - G_t(y_{t-1}, y_t ; x, \theta)
        """
        return math.exp( Counters.dot( self.parameters, self.featureFunction(t, y_, y, xs) ) )

####################################################3
# Problem 1a
def computeViterbi(crf, xs):
    """
    Compute the maximum weight assignment using the Viterbi algorithm.
    @params crf LinearChainCRF - the CRF model.
    @param xs list string - the sequence of observed words.
    @return list string - the most likely sequence of hidden TAGS.

    Tips:
    + Normalize Viterbi[i] at the end of every iteration (including 0!) to prevent numerical overflow/underflow.

    Possibly useful:
    - BEGIN_TAG
    - crf.TAGS
    - crf.G
    - Counter
    """

    # BEGIN_YOUR_CODE (around 27 lines of code expected)
    # simple type that stores previous tag that maximized previous variable along with potential value
    # (max forward message)
    MaxFM = namedtuple('MaxFM', 'ptag value')
    # size of input
    T = len(xs)

    # compute viterbis forward
    viterbi = [None]*(T+1) # one-indexed array wrt t
    viterbi[0] = Counter({BEGIN_TAG: MaxFM(None, 1.)})

    for t in range(1, T+1):
        # build dict mapping each tag to the max forward message
        viterbi[t] = Counter({
            tag: max(
                (MaxFM(tag_, viterbi[t-1][tag_].value * crf.G(t-1, tag_, tag, xs))
                for tag_ in viterbi[t-1]),
                key=lambda entry: entry.value)
            for tag in crf.TAGS})
        # normalize so all sum to 1
        norm = sum(mfm.value for mfm in viterbi[t].itervalues())
        for tag in viterbi[t]:
            mfm = viterbi[t][tag]
            viterbi[t][tag] = MaxFM(mfm.ptag, mfm.value / norm)

    # rewind and build optimal assignment
    tags = [None]*(T+1) # one-indexed array wrt t
    tags[T] = max(viterbi[T], key=lambda tag: viterbi[T][tag].value)
    for t in range(T, 1, -1):
        tags[t-1] = viterbi[t][tags[t]].ptag

    return tags[1:]

    # END_YOUR_CODE

####################################################3
# Problem 1b
def computeForward(crf, xs):
    r"""
    Computes the normalized version of 
        Forward_t(y_{t}) = \sum_{y_{t-1}} G_t(y_{t-1}, y_t; x, \theta) Forward{t-1}(y_{t-1}).

    @params crf LinearChainCRF - the CRF
    @param xs list string - the sequence of observed words
    @return (double, list Counter) - A tuple of the computed
    log-normalization constant (A), and the sequence Forward_t; each member
    of the list is a counter that represents Forward_t

    Example output: (5.881, [
                Counter({'-FEAT-': 0.622, '-SIZE-': 0.377}), 
                Counter({'-SIZE-': 0.761, '-FEAT-': 0.238}), 
                Counter({'-SIZE-': 0.741, '-FEAT-': 0.258})])

    Tips:
    * In this version, you will need to normalize the values so that at
    each t, \sum_y Forward_t(y_t) = 1.0. 
    * You will also need to collect the normalization constants z_t
      = \sum_{y_{t-1}} \sum_{y_{t-1}} G_t(y_{t-1}, y_{t}; x, \theta) * Forward_{t-1}(y_{t-1}) 
      to return the log partition function A = \sum_t \log(z_t). We need
      to take the log because this value can be extremely small or
      large.
    * Note that Forward_1(y_1) = G_1(-BEGIN-, y_1 ; x, \theta) before normalization.
    
    Possibly useful:
    - BEGIN_TAG
    - crf.G
    - crf.TAGS
    - Counter
    """
    A = 0.
    forward = [ None for _ in xrange(len(xs)) ]

    # BEGIN_YOUR_CODE (around 15 lines of code expected)
    T = len(xs)
    forward[-1] = Counter({BEGIN_TAG: 1.}) # temporarily at end of list for convenience
    for t in range(0, T):
        # compute forward message at t
        forward[t] = Counter({
            tag: sum(
                forward_ * crf.G(t, tag_, tag, xs)
                for tag_, forward_ in forward[t-1].iteritems())
            for tag in crf.TAGS})
        # normalize
        norm = sum(forward[t].itervalues())
        for tag in forward[t]:
            forward[t][tag] /= norm
        A += math.log(norm)

    # END_YOUR_CODE

    return A, forward

####################################################3
# More utility functions

def computeBackward(crf, xs):
    r"""
    Computes a normalized version of Backward. 

    @params crf LinearChainCRF - the CRF
    @param xs list string - the sequence of observed words
    @return list Counter - The sequence Backward_t; each member is a counter that represents Backward_t

    Example output: [
            Counter({'-SIZE-': 0.564, '-FEAT-': 0.435}),
            Counter({'-SIZE-': 0.567, '-FEAT-': 0.432}),
            Counter({'-FEAT-': 0.5, '-SIZE-': 0.5})]

    Tips:
    * In this version, you will need to normalize the values so that at
    each t, \sum_{y_t} Backward_t(y_t) = 1.0. 
    
    Possibly useful:
    - BEGIN_TAG
    - crf.G
    - crf.TAGS
    - Counter
    """

    backward = [ None for _ in xrange(len(xs)) ]

    backward[-1] = Counter( { tag : 1. for tag in crf.TAGS } ) 
    z = sum(backward[-1].values())
    for tag in backward[-1]:
        backward[-1][tag] /= z

    for t in xrange( len(xs)-1, 0, -1 ):
        backward[t-1] = Counter({ tag : 
                    sum( crf.G( t, tag, tag_, xs ) 
                        * backward[t][tag_] for tag_ in crf.TAGS )
                    for tag in crf.TAGS })
        z = sum(backward[t-1].values())
        for tag in backward[t-1]:
            backward[t-1][tag] /= z

    return backward

####################################################3
# Problem 1c
def computeEdgeMarginals(crf, xs):
    r"""
    Computes the marginal probability of tags, 
    p(y_{t-1}, y_{t} | x; \theta) \propto Forward_{t-1}(y_{t-1}) 
            * G_t(y_{t-1}, y_{t}; x, \theta) * Backward_{t}(y_{t}).

    @param xs list string - the sequence of observed words
    @return list Counter - returns a sequence with the probability of observing (y_{t-1}, y_{t}) at each time step

    Example output:
    T = [ Counter({('-BEGIN-', '-FEAT-'): 0.561, ('-BEGIN-', '-SIZE-'): 0.439}),
          Counter({('-FEAT-', '-SIZE-'): 0.463, ('-SIZE-', '-SIZE-'): 0.343, 
                   ('-SIZE-', '-FEAT-'): 0.096, ('-FEAT-', '-FEAT-'): 0.096}),
          Counter({('-SIZE-', '-SIZE-'): 0.590, ('-SIZE-', '-FEAT-'): 0.217,
                   ('-FEAT-', '-SIZE-'): 0.151, ('-FEAT-', '-FEAT-'): 0.041})
        ]

    Tips:
    * At the end of calculating f(y_{t-1}, y_{t}) = Forward_{t-1}(y_{t-1}) 
            * G_t(y_{t-1}, y_{t}; x, \theta) * Backward_{t}(y_{t}), you will
      need to normalize because p(y_{t-1},y_{t} | x ; \theta) is
      a probability distribution. 
    * Remember that y_0 will always be -BEGIN-; at this edge case,
        Forward_{0}(y_0) is simply 1. So, T[0] = p(-BEGIN-, y_1 | x ; \theta)
        = G_1(-BEGIN-, y_1; x, \theta) Backward_1(y_1).

    * Possibly useful:
    - computeForward
    - computeBackward
    """

    _, forward = computeForward(crf, xs)
    backward = computeBackward(crf, xs)

    T = [ None for _ in xrange( len(xs) ) ]

    # BEGIN_YOUR_CODE (around 17 lines of code expected)

    # quick hack for edge case: set forward[-1] to be Forward_{0}
    forward.append(Counter({BEGIN_TAG: 1.}))

    for t in range(len(xs)):
        # compute
        T[t] = Counter({
            (tag_, tag) : forward[t-1][tag_] * crf.G(t, tag_, tag, xs) * backward[t][tag]
            for tag_ in forward[t-1]
            for tag in crf.TAGS
            })
        # normalize
        norm = sum(T[t].itervalues())
        for tag in T[t]:
            T[t][tag] /= norm

    # END_YOUR_CODE

    return T


###############################################
# Problem 3. Gibbs sampling
###############################################

#################################
# Utility Functions

def gibbsRun(crf, blocksFunction, choiceFunction, xs, samples = 500 ):
    r"""
    Produce samples from the distribution using Gibbs sampling.
    @params crf LinearChainCRF - the CRF model.
    @params blocksFunction function - Takes the input sequence xs and
                returns blocks of variables that should be updated
                together.
    @params choiceFunction function - Takes 
                a) the crf model,
                b) the current block to be updated
                c) the input sequence xs and 
                d) the current tag sequence ys
                and chooses a new value for variables in the block based
                on the conditional distribution 
                p(y_{block} | y_{-block}, x ; \theta).
    @param xs list string - Observation sequence
    @param samples int - Number of samples to generate
    @return generator list string - Generates a list of tag sequences
    """

    # Burn in is the number iterations to run from the initial tag
    # you've chosen before you generate the samples. It basically
    # prevents you from being biased based on your starting tag.
    BURN_IN = 100

    # Intitial value
    ys = [random.choice(crf.TAGS) for _ in xrange(len(xs))]

    # Get blocks
    blocks = blocksFunction(xs)

    # While burning-in, don't actually return any of your samples.
    for _ in xrange(BURN_IN):
        # Pick a 'random' block
        block = random.choice(blocks)
        # Update its values
        choiceFunction( crf, block, xs, ys )

    # Return a sample every epoch here.
    for _ in xrange(samples):
        # Pick a 'random' block
        block = random.choice(blocks)
        # Update its values
        choiceFunction( crf, block, xs, ys )
        # Return a sample
        yield tuple(ys)

def getCRFBlocks(xs):
    """
    Groups variables into blocks that are updated simultaneously.
    In this case, each variable belongs in its own block.
    @params xs - observation sequence
    """
    return range(len(xs))

#################################
# Problem 3c
def chooseGibbsCRF(crf, t, xs, ys ):
    r"""
    Choose a new assignment for y_t from the conditional distribution
    p( y_t | y_{-t} , xs ; \theta).

    @param t int - The index of the variable you want to update, y_t.
    @param xs list string - Observation seqeunce
    @param ys list string - Tag seqeunce

    Tips:
    * You should only use the potentials between y_t and its Markov
      blanket.
    * You don't return anything from this function, just update `ys`
      in place.

    Possibly useful:
    - crf.G 
    - util.multinomial: Given a PDF as a list OR counter, util.multinomial draws
      a sample from this distribution; for example,
      util.multinomial([0.4, 0.3, 0.2, 0.1]) will return 0 with 40%
      probability and 3 with 10% probability.
      Alternatively you could use,
      util.multinomial({'a':0.4, 'b':0.3, 'c':0.2, 'd':0.1}) will return 'a' with 40%
      probability and 'd' with 10% probability.
    """
    # BEGIN_YOUR_CODE (around 17 lines of code expected)
    
    # compute partial weights
    if t == 0:
        weights = [crf.G(t, BEGIN_TAG, y, xs) * crf.G(t+1, y, ys[t+1], xs) for y in crf.TAGS]
    elif t == len(xs)-1:
        weights = [crf.G(t, ys[t-1], y, xs) for y in crf.TAGS]
    else:
        weights = [crf.G(t, ys[t-1], y, xs) * crf.G(t+1, y, ys[t+1], xs) for y in crf.TAGS]

    norm = sum(weights)

    ys[t] = util.multinomial({ y : weight/norm for y, weight in zip(crf.TAGS, weights) })
    # END_YOUR_CODE

#################################
# Problem 3c
def computeGibbsProbabilities(crf, blocksFunction, choiceFunction, xs, samples = 2000):
    """
    Empirically estimate the probabilities of various tag sequences. You
    should count the number of labelings over many samples from the
    Gibbs sampler.
    @param xs list string - Observation sequence
    @param samples int - Number of epochs to produce samples
    @return Counter - A counter of tag sequences with an empirical
                      estimate of their probabilities.
    Example output:
        Counter({
        ('-FEAT-', '-SIZE-', '-SIZE-'): 0.379, 
        ('-SIZE-', '-SIZE-', '-SIZE-'): 0.189, 
        ('-FEAT-', '-SIZE-', '-FEAT-'): 0.166, 
        ('-SIZE-', '-SIZE-', '-FEAT-'): 0.135, 
        ('-FEAT-', '-FEAT-', '-SIZE-'): 0.053, 
        ('-SIZE-', '-FEAT-', '-SIZE-'): 0.052, 
        ('-FEAT-', '-FEAT-', '-FEAT-'): 0.018, 
        ('-SIZE-', '-FEAT-', '-FEAT-'): 0.008})

    Possibly useful:
    * Counter
    * gibbsRun
    """
    # BEGIN_YOUR_CODE (around 2 lines of code expected)
    c = Counter(gibbsRun(crf, blocksFunction, choiceFunction, xs, samples))
    norm = float(sum(c.itervalues()))
    for seq in c:
        c[seq] /= norm
    return c
    # END_YOUR_CODE

#################################
# Problem 3c
def computeGibbsBestSequence(crf, blocksFunction, choiceFunction, xs, samples = 2000):
    """
    Find the best sequence, y^*, the most likely sequence using samples
    from a Gibbs sampler. This gives the same output as crf.computeViterbi.
    @param xs list string - Observation sequence
    @param samples int - Number of epochs to produce samples
    @return list string - The most probable tag sequence estimated using Gibbs.
    Example output:
        ('-FEAT-', '-SIZE-', '-SIZE-')

    Possibly useful:
    * Counter.most_common
    * gibbsRun
    """
    # BEGIN_YOUR_CODE (around 1 line of code expected)
    return Counter(gibbsRun(crf, blocksFunction, choiceFunction, xs, samples)).most_common(1)[0][0]
    # END_YOUR_CODE
            
#################################
# Problem 3e
def getLongRangeCRFBlocks(xs):
    """
    Constructs a list of blocks, where each block corresponds
    to the positions t with the same observed word x_t.
    @param xs list string - observation sequence
    @return list list int - A list of blocks; each block is a list
            of indices 't' which have the same x_t.
            Example: "A A B" would return: [[0,1],[2]].
    """
    # BEGIN_YOUR_CODE (around 7 lines of code expected)
    indices = defaultdict(lambda: [])
    for i, x in enumerate(xs):
        indices[x].append(i)
    return indices.values()
    # END_YOUR_CODE

#################################
# Problem 3e
def chooseGibbsLongRangeCRF(crf, block, xs, ys ):
    r"""
    Choose a new assignment for every variable in block from the
    conditional distribution p( y_{block} | y_{-block}, xs; \theta).

    @param block list int - List of variable indices that should be jointly updated.
    @param xs list string - Observation sequence
    @param ys list string - Tag sequence

    Tips:
    * In our model, we have a hard potential between all the variables in the
      block constraining them to be equal. You should only need to
      iterate through crf.TAGS once in order to choose a value for y_{block}
      (as opposed to |block| times).
    * You should only use the potentials between y_t and its Markov
      blanket.
    """
    # BEGIN_YOUR_CODE (around 23 lines of code expected)
    saved_ys = ys
    ys = ys + [BEGIN_TAG]
    def computeBlockWeight(y):
        weight = 1.
        t_ = block[0]
        for t in block:
            # non-contiguous jump: account for potential ending previous section
            if t - t_ > 1:
                weight *= crf.G(t_+1, y, ys[t_+1], xs)
            # potential between current and previous adjacent variable
            weight *= crf.G(t, ys[t-1], y, xs)
            t_ = t
        if t < len(xs) - 1:
            weight *= crf.G(t+1, y, ys[t+1], xs)
        return weight
    
    # compute probabilities/weights
    weights = [computeBlockWeight(y) for y in crf.TAGS]
    norm = sum(weights)

    # randomly choose a tag
    y = util.multinomial({ y : weight/norm for y, weight in zip(crf.TAGS, weights) })

    # update all the tags in the block
    for t in block:
        saved_ys[t] = y
    # END_YOUR_CODE

