"""
Tools for parsing Lilypond data.

Author: Stephen Koo 2013
"""
import re
import util
import math
import StringIO

NOTE = re.compile(r"([a-g](?:is|es){0,2})(['|,]*)")
FINGERING = re.compile(r'-([1-5])|\^"([1-5])"')
BASE_OCTAVE = 3
OCTAVE_SIZE = 12
REL_SEARCH_RADIUS = OCTAVE_SIZE / 2
NUM_LETTERS = 7
PITCH_CLASS = {
	"c" 	: 0,	"deses" : 0,	"bis" 	: 0,
	"cis"	: 1,	"des"	: 1,	"bisis"	: 1,
	"d"		: 2,	"cisis"	: 2,	"eeses"	: 2,
	"dis"	: 3,	"ees"	: 3,	"feses"	: 3,
	"e"		: 4,	"disis"	: 4,	"fes"	: 4,
	"f"		: 5,	"eis"	: 5,	"geses"	: 5,
	"fis"	: 6,	"ges"	: 6,	"eisis"	: 6,
	"g"		: 7,	"fisis"	: 7,	"aeses"	: 7,
	"gis"	: 8,	"aes"	: 8,
	"a"		: 9,	"gisis"	: 9,	"beses"	: 9,
	"ais"	: 10,	"bes"	: 10,	"ceses"	: 10,
	"b"		: 11,	"aisis"	: 11,	"ces"	: 11
}
LETTER = {
	"c" 	: 0,	"d"		: 1,	"e"		: 2,
	"f"		: 3,	"g"		: 4,	"a"		: 5,
	"b"		: 6
}

class Score(object):
	def __init__(self, fp, relative=False, sep=None):
		"""
		Initialize Score from a file containing notes (and fingerings) in LilyPond format.

		Each line in the file should represent a passage.
		If relative octave entry is used, then the first note must be written with absolute
		octave entry.
		"""
		self.passages = []
		self.relative = relative
		self.prev_natural = None

		if sep is None:
			for line in fp:
				self.parse(line)
		else:
			for line in util.itersplit(fp.read(), sep):
				self.parse(line)

	def read_note(self, token):
		"""
		Returns integer representation of pitch in note token.
		"""
		note_match = NOTE.match(token)
		if not note_match:
			return None

		# Pull note (and naturalized note) and shift out of regex results
		letter = note_match.group(1)
		#print note_match.group(1),"+++",note_match.group(2)
		#print "letter=",letter
		pitch = PITCH_CLASS[letter]
		natural = LETTER[letter[0]]
		natural_pitch = PITCH_CLASS[letter[0]]
		if natural_pitch - pitch > 2:
			natural -= NUM_LETTERS
			print "POOP"
		elif pitch - natural_pitch > 2:
			natural += NUM_LETTERS
			print "POOP"

		if note_match.group(2):
			shift = {"'":1, ",":-1}[note_match.group(2)[0]] * len(note_match.group(2))
		else:
			shift = 0

		print "shift=",shift
		if self.relative and self.prev_natural is not None:
			# relative scheme
			octave = 0
			while abs((natural + octave * NUM_LETTERS) - self.prev_natural) > 3:
				octave += 1
			octave += shift
		else:
			# absolute scheme
			octave = shift + BASE_OCTAVE

		note = pitch + (octave * OCTAVE_SIZE)

		self.prev_natural = natural + (octave * NUM_LETTERS)

		return note

	def read_fingering(self, token):
		"""
		Returns integer representation of fingering in note token.
		"""
		fingering_match = FINGERING.search(token)
		fingering = max(fingering_match.groups()) if fingering_match else 0
		return int(fingering)

	def parse(self, line):
		"""
		Parse a string into a new passage, added to self.passages.
		"""
		passage = []
		fingerings = []
		for token in util.itersplit(line):
			note = self.read_note(token)
			fingering = self.read_fingering(token)

			if note is not None:
				passage.append(note)
				fingerings.append(fingering)
				# passage.append((note, fingering))
		print "note=",passage
		print "fingering=",fingerings
		self.passages.append((passage, fingerings))
		print "passage=",self.passages

	def __iter__(self):
		"""
		Simply returns an iterator over the parsed passages, so you can do
		this with a Score object:

		for passage in myscore:
			# do stuff
		"""
		return iter(self.passages)

def parse_line(line, relative=True):
	"""Parses a single line of Lilypond notes into the integer note values."""
	ss = StringIO.StringIO(line)
	score = Score(ss, relative)
	print "score=",score
	return score.passages[0][0]

def pretty_format(fingerings):
	"""Returns an array of fingerings (ints) as a tab-separated string."""
	return '\t'.join( map(str, fingerings) )

