#!/usr/bin/env python2.7
"""
Command-line interface for fingering generator.

Based off of "run.py" from Stanford CS221 Assignment 'ner' (2013).

Run 'python run.py' for options and help.

Authors: Stephen Koo, Mark Peng 2013
"""

from fingerings.inference import *
from fingerings.parse import Score, parse_line, pretty_format
from fingerings import learning, features, util
import os

def run_command_line(args):
    """Run a command line interpreter"""
    import pickle
    from cmd import Cmd

    class CRFCommandLine(Cmd):
        """A shell to interact with this CRF"""

        def __init__(self, crf):
            """Create a shell to interact with this CRF"""
            Cmd.__init__(self)
            self.prompt = '>> '
            self.crf = crf

        def do_viterbi(self, st):
            """Run the viterbi algorithm on input to produce the most
            likely labelling"""
            #print "value=",value
            #xs = parse_line(value)

            xs = list(map(int,st.split(',')))
            print "xs=",xs
            ys = computeViterbi(crf, xs)
            #print "crf=",crf
            #print "ys=",ys
            print "%%%",list(ys)
            f = open("output.txt", "w")
            f.write(str(list(ys)))
            #for i in list(ys):
            #    print(i)
            #    f.write(str(i))
            #    f.write(",")
            f.close()

        def do_gibbs_best(self, value):
            """Run Gibbs sampling to produce the best labelling for
            given input value"""
            xs = parse_line(value)

            ys = computeGibbsBestSequence( 
                    self.crf, 
                    getCRFBlocks,
                    chooseGibbsCRF,
                    xs,
                    10000 )

            print pretty_format(ys)

        def do_gibbs_dist(self, value):
            """Run Gibbs sampling to produce the best labelling for
            given input value"""
            xs = parse_line(value)

            ys = computeGibbsProbabilities( 
                    self.crf, 
                    getCRFBlocks,
                    chooseGibbsCRF,
                    xs,
                    10000 )

            for label, pr in ys.most_common(10):
                print pr, '\t', pretty_format(label)

        def do_quit(self, value):
            """Exit the interpreter"""
            return True 
        def do_exit(self, value):
            """Exit the interpreter"""
            return True 

    states, parameters = pickle.load( args.parameters )

    crf = LinearChainCRF( states, features.extract, parameters ) 
    cmdline = CRFCommandLine(crf)
    cmdline.cmdloop()

def run_trainer(args):
    import pickle
    print "Loading dataset..."
    train = load_all_data( args.trainData )
    try:
        dev = load_all_data( args.devData )
    except IOError:
        print 'Could not load dev data, ignoring.'
        dev = []

    train_notes = sum(len(passage[0]) for passage in train)
    dev_notes = sum(len(passage[0]) for passage in dev)
    print "Training on %d passages (%d notes) and evaluating on dev set of %s passages (%s notes)" \
            % (len(train), train_notes, len(dev), dev_notes)

    crf = learning.trainLinearChainCRF( train, features.extract, args.iters, dev )

    print "Training done."

    if args.output_path:
        pickle.dump( (crf.TAGS, crf.parameters), open(args.output_path, 'w') )
        print "Saved model to ", args.output_path

def isIllegal(pair1, pair2):
    if (pair2[0] - pair1[0]) > 0:
        if pair2[1] < pair1[1] and pair2[1] != 1:
            return 1
    elif (pair2[0] - pair1[0]) < 0:
        if pair2[1] > pair1[1] and pair1[1] != 1:
            return 1
    return 0


def check_baseline(args):
    import pickle
    states, parameters = pickle.load( args.parameters )
    crf = LinearChainCRF( states, features.extract, parameters )
    
    print "Loading dataset..."
    try:
        dev = load_all_data( args.devData )
    except IOError:
        print 'Could not load dev data, ignoring.'
        dev = []
    numIllegal = 0
    numNotes = 0
    for notes, fingerings in dev:
        computedFingerings = computeGibbsBestSequence(
                                crf,
                                getCRFBlocks,
                                chooseGibbsCRF,
                                notes,
                                10000 )
        numNotes += len(notes)
        for i in range(0, len(computedFingerings) - 1):
            numIllegal += isIllegal((notes[i], computedFingerings[i]), (notes[i+1], computedFingerings[i+1]))
    print "Found %d illegal fingerings out of %d fingerings (%.2f%% legal)" \
            % (numIllegal, numNotes, float(100 - (numIllegal / numNotes * 100)))

def check_optimal(args):
    import pickle
    states, parameters = pickle.load( args.parameters )
    crf = LinearChainCRF( states, features.extract, parameters )
    
    print "Loading dataset..."
    try:
        dev = load_all_data( args.devData )
    except IOError:
        print 'Could not load dev data, ignoring.'
        dev = []
    numUnmatched = 0
    numNotes = 0
    for notes, fingerings in dev:
        computedFingerings = computeGibbsBestSequence(
                                                      crf,
                                                      getCRFBlocks,
                                                      chooseGibbsCRF,
                                                      notes,
                                                      10000 )
        numNotes += len(notes)
        for i in range(0, len(computedFingerings)):
            if computedFingerings[i] != fingerings[i]:
                numUnmatched += 1
    print "Found %d unmatched fingerings out of %d fingerings (%.2f%% matching)" \
        % (numUnmatched, numNotes, 100 - (float(numUnmatched) / numNotes * 100))

def load_all_data(directory):
    filenames = [os.path.join(directory, path) for path in os.listdir(directory)]
    return sum((Score(open(filename), relative=True).passages for filename in filenames), [])

if __name__ == '__main__':
    import argparse

    parser = argparse.ArgumentParser( description='Piano Fingering Generator' )
    subparsers = parser.add_subparsers()

    shell_parser = subparsers.add_parser('shell', help='Open up a shell to interact with a model' )
    shell_parser.add_argument('--parameters', required=True, type=file, help='Use the parameters stored in this file for your CRF' )
    shell_parser.set_defaults(func=run_command_line)

    train_parser = subparsers.add_parser('train', help='Train a CRF' )
    train_parser.add_argument('--trainData', type=str, default='train', help='Directory to use for training-set data' )
    train_parser.add_argument('--devData', type=str, default='dev', help='Directory to use for development-set data' )
    train_parser.add_argument('--iters', type=int, default=10, help='Number of iterations to run' )
    train_parser.add_argument('--output-path', default='', type=str, help='Path to store the trained wieghts' )
    train_parser.set_defaults(func=run_trainer)
    
    baseline_parser = subparsers.add_parser('baseline', help='Check fingerings with baseline evaluation metric' )
    baseline_parser.add_argument('--devData', type=str, default='dev', help='Directory to use for development-set data' )
    baseline_parser.add_argument('--parameters', required=True, type=file, help='Use the parameters stored in this file for your CRF' )
    baseline_parser.set_defaults(func=check_baseline)
    
    baseline_parser = subparsers.add_parser('optimal', help='Check fingerings with optimal evaluation metric' )
    baseline_parser.add_argument('--devData', type=str, default='dev', help='Directory to use for development-set data' )
    baseline_parser.add_argument('--parameters', required=True, type=file, help='Use the parameters stored in this file for your CRF' )
    baseline_parser.set_defaults(func=check_optimal)

    args = parser.parse_args()
    args.func(args)
