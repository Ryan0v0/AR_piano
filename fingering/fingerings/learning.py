"""
Linear Chain CRF parameter learning tools.

Created by the Stanford CS221 Autumn 2013 course staff.
"""
import util
from inference import *
import itertools as it

###################################
# Utility functions for computing probabilities

def computeLogLikelihood( crf, xs, ys ):
    r"""
    Compute the unnormalized log-likelihood of a sequence.
    \log p(y | x  ; \theta) = \sum_{t=1}^T \theta \cdot \phi_{local}(t, y_{t-1}, y_t, x).
    @params crf LinearChainCRF - the CRF
    @params xs list string - observation sequence
    @params ys list string - tag sequence
    @return double - the log likelihood
    """
    return sum(Counters.dot( crf.parameters, 
                crf.featureFunction(t, 
                    ys[t-1] if t > 0 else BEGIN_TAG, 
                    ys[t], 
                    xs)) 
                for t in xrange(len(xs)))

def computeLikelihood( crf, xs, ys ):
    r"""
    Compute the unnormalized likelihood of a sequence.
    Effectively \prod_t=1^T G_t(y_{t-1}, y_t ; x, \theta)
    @params crf LinearChainCRF - the CRF
    @params xs list string - observation sequence
    @params ys list string - tag sequence
    @return double - the likelihood
    """
    return math.exp( computeLogLikelihood( crf, xs, ys ) )

#########################
# Utility Functions for probabilities

def computeLogPartitionFunction(crf, xs):
    r"""
    Computes \log Z(x) = \log \sum_{y} \prod_{t} G_t(y_{t-1}, y_t; x, \theta)
    @param xs list string - the sequence of observed words
    @return double - returns the partition function for the sequence.
    """
    z, _ = computeForward(crf, xs)
    return z

def computeLogProbability(crf, xs, ys):
    r"""
    Compute log probability for a sequence 
    p(y | x ; \theta ) = \frac{1}{Z(x)}\exp\left( \sum_{t=1}^T \theta
                    \cdot \phi_{local}(y_{t-1}, y_t, x) \right).

    @param xs list string - observation sequence
    @param ys list string - tag sequence
    @return double - \log p(y | x ; \theta).
    """
    z = computeLogPartitionFunction(crf, xs)
    return computeLogLikelihood(crf, xs, ys) - z

def computeProbability(crf, xs, ys):
    r"""
    Compute probability for a sequence 
    @param xs list string - observation sequence
    @param ys list string - tag sequence
    @return double - \log p(y | x ; \theta).
    """
    return math.exp( computeLogProbability(crf, xs, ys) )

#########################
# Utility Functions for training

def computeGradient(crf, xs, ys):
    r"""
    Gradient of the log-likelihood \log p(y|x ; \theta) with repsect to parameters (used in G) 
    \nabla_{\theta} \ell(x,y,\theta) = \sum_{t=1}^T \phi_{local}(t, y_{t-1}, y_t, x) 
        - \sum_{t=1}^T \sum_{y_{t-1}, y_{t}} p(y_{t-1}, y_{t} | x ;
          \theta) \phi_{local}(t, y_{t-1}, y_t, x).
    @params xs list string - the sequence of observed words
    @params ys list string - the sequence of hidden tags (e.g. named entity tags)
    @return Counter - amount to increment each feature in parameters.
g    Possibly useful:
    - crf.featureFunction
            + Feature function takes in t, y_{t-1}, y_t, x as input and
            returns a sparse feature vector as a Counter
    - computeEdgeMarginals
    """

    gradient = Counter()
    T = computeEdgeMarginals(crf,  xs )

    # Base case
    gradient.update(crf.featureFunction( 0, BEGIN_TAG, ys[0], xs ))
    for i in xrange(1, len(xs)):
        gradient.update(crf.featureFunction( i, ys[i-1], ys[i], xs ))

    for tag in crf.TAGS:
        for key, val in crf.featureFunction( 0, BEGIN_TAG, tag, xs).iteritems():
            gradient[key] -= T[0][(BEGIN_TAG, tag)] * val
            assert gradient[key] != float('nan')

    for i in xrange(1, len(xs)):
        for tag in crf.TAGS:
            for tag_ in crf.TAGS:
                for key, val in crf.featureFunction( i, tag_, tag, xs).iteritems():
                    gradient[key] -= T[i][(tag_, tag)] * val
                    assert gradient[key] != float('nan')

    return gradient

def reportF1( dataset, crf, printConfusionMatrix = True ):
    """
    Compute the F1 score and print a confusion matrix
    """
    confusionMatrix = Counter({ (tag, tag_) : 0. for tag in crf.TAGS for tag_ in crf.TAGS })
    for xs, ys in dataset:
        ys_ = computeViterbi(crf, xs)
        confusionMatrix.update( Counter(zip(ys,ys_)) )

    if printConfusionMatrix:
        print "Rows are true counts, columns are predicted counts"
        print 'T\P\t' + '\t'.join( map(str, crf.TAGS) )
        for tag in crf.TAGS:
            print str(tag) + '\t' + '\t'.join( str(confusionMatrix[(tag,tag_)]) for tag_ in crf.TAGS )

    # Only compute F1 scores for the special classes
    tags = list(crf.TAGS)
    # tags.remove('-O-')

    precisions, recalls = {}, {}
    for tag in tags:
        correct = (confusionMatrix[(tag, tag)])
        predictions = sum( confusionMatrix[(tag, tag_)] for tag_ in crf.TAGS )
        precisions[tag] = float(correct)/predictions if correct > 0. else 0.  
        corrects = sum( confusionMatrix[(tag_, tag)] for tag_ in crf.TAGS )
        recalls[tag] = float(correct)/corrects if correct > 0. else 0.  

    f1s = {tag : 
            2. * precisions[tag] * recalls[tag]/( precisions[tag] + recalls[tag] ) 
                 if precisions[tag] + recalls[tag] != 0. else 0.
           for tag in tags }

    return sum(f1s.values()) / len(tags)

def gradientCheck( crf, xs, ys ):
    r"""
    This function checks to see if the analytic computation of the gradient, i.e.
    \frac{f(x + \epsilon) - f(x - \epsilon)}{2 \epsilon} agrees with computeGradient.
    """
    eps = 1e-4

    # Compute gradient
    gradient = computeGradient(crf, xs, ys)
    for key, value in gradient.iteritems():
        # Modify key by eps
        crf.parameters[key] += eps
        lhood_upper = computeLogProbability(crf, xs, ys)
        crf.parameters[key] -= 2. * eps
        lhood_lower = computeLogProbability(crf, xs, ys)
        # Revert change
        crf.parameters[key] += eps

        value_ = (lhood_upper - lhood_lower)/(2.*eps)
        if abs( value_ - value ) > 10*eps:
            print 'Gradients are not correct.'
            assert abs( value_ - value ) < 10*eps
            return False
    return True

def trainLinearChainCRF(dataset, featureFunction, iters = 10, dev_set = []):
    """
    Given |dataset|, do stochastic gradient descent to obtain a parameter vector.
    @param dataset list (list string, list string) - A collection of labeled sequences. 
    """
    stepSize = 0.9

    # Get all viable tags
    TAGS = list(set( it.chain.from_iterable( ys for _, ys in dataset ) ))

    # Initialize with a simple CRF with 0 parameters.
    crf = LinearChainCRF(TAGS, featureFunction)
    timer = util.Timer()

    for i in xrange(iters):
        gradientCheck(crf, dataset[0][0], dataset[0][1])
        # Print status
        lhood = sum( computeLogProbability(crf, xs, ys) for xs, ys in dataset ) / len(dataset)
        print "Training set confusion matrix:"
        f1  = reportF1( dataset, crf ) 
        print "Development set confusion matrix:"
        dev_f1  = reportF1( dev_set, crf ) if dev_set else 0.
        print 'Iter %d, Likelihood %0.3f, Train F1: %0.3f, Dev F1 %0.3f' % (i, lhood, f1, dev_f1)

        timer.start()
        for (j, (xs, ys)) in enumerate(dataset):
            gradient = computeGradient(crf, xs, ys)
            for key, value in gradient.iteritems():
                crf.parameters[key] += stepSize * value
            util.update_progress( float(j) / len(dataset) )
        util.update_progress( 1.0 )
        stepSize *= (1. + i) / (2. + i)
        print 'Iter %d took %0.2f seconds' % (i, timer.ticks())

    lhood = sum( computeLogProbability(crf, xs, ys) for xs, ys in dataset ) / len(dataset)
    f1  = reportF1( dataset, crf ) 
    dev_f1  = reportF1( dev_set, crf ) if dev_set else 0.
    print 'Iter %d, Likelihood %0.3f, Train F1: %0.3f, Dev F1 %0.3f' % (i, lhood, f1, dev_f1)

    return crf

