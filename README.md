# Project Status
THIS PROJECT IS NEW AND NOT YET READY TO BE USED.

# About Eugene
Eugene is a data structure peristence library for .NET projects.
It gives you access to traditional in-memory data structures such
as arrays, linked lists, array lists, hash tables, binary trees,
trie trees, etc., in a format that is continuously persisted to
disk as you modify the data structure.

Of course there are many options for persisting data to disk. The
world has SQL databases, NoSQL databases, text files such as CSV,
XML, JSON, and YAML, and all manner of other ways of persisting data to
disk. Why do we need yet another way of persisting data?

While each of the solutions above have their place, there are times
where we need a hand-crafted data structure consisting of multiple
interconnected collections to achieve high performance retrieval of
the data.

One such use case, and the use case that was at the forefront of our
minds when we started Eugene, was to build a high performance trigram
text indexing library that can be used to quickly search for regular
expressions across a very large corpus of code repositories. To do that,
we need a way to build up a customized, specialized index that can store
trigram text indexes for fast retrieval.

That's just one sample use case, however. Eugene is a general purpose 
library that can find broad applicability in many domains of work. Any
time you need fine grained control over a specialized data structure that
doesn't fit neatly into other persisting solutions, Eugene may be a good
solution for your project.

# Sister Projects
Eugene is one part of a series of projects designed to deliver high
speed code searching capabilities.

* Eugene  - This project, is a Nuget package that provides general purpose 
            persistent data structures
* Spinach - Spinach is a Nuget packages that builds on top of Eugene and 
            provides an API and indexing file format for performing regular 
            expression text searching
* Popeye  - Popeye is an installable code searching engine that can be
            installed in various ways, including Docker, Kubernetes, etc.
            Essentially, Popeye aims to be a C# version of Zoekt that uses
            similar approaches to indexing the text repositories as does
            Zoekt. Popeye is not a direct, line-for-line port of Zoekt,
            but is based on similar ideas.

Popeye uses the Spinach package, which in turn uses Eugene.

# Motivation
To understand the motivation for Eugene, one should start by understanding
what Zoekt is.

Zoekt is an open source code searching engine written in Go. Being written in Go,
it wasn't overly useful for our projects here at OpenSquiggly, being that
our codebase is written in C#. We wanted a C# version of Zoekt, and so we
went about reading the Zoekt codebase to consider how we might access it
from C# or perhaps port it to C# line-by-line.

Unfortunately, due to the many philosophical differences between Go and C#,
directly porting a Go project to C# seemed quite difficult.

We thought about using Zoekt in a separate container and accessing it using
its REST API, but that just seemed like a gigantic hassle. We really wanted
a pure C# solution that could be tightly and properly integrated with OpenSquiggly.

Next, we thought about whether we should port our entire product to Go, but
we decided not to do that.

Finally, we wondered if we could write a new C# project from scratch that would
follow the same ideas as Zoekt.

Let's take the time to understand at a very high level what Zoekt does. Suppose
we are running a regular expression search for:

```
quickly.*browning.*foxhound
```

Admittedly this is a bit of a contrived example, but it's useful to illustrate
the point.

First, Zoekt extracts the three known literals from the string, getting:

* quickly
* browning
* foxhound

and then goes about finding all the files in the corpus that contain these three
literals. Once it finds a file with all three of these string literals, it runs
a regex search over the file to see if it matches the full regex.

Okay, but how does it quickly find those matching files? Aha! Now we're getting to the
heart of the matter with this problem.

In a nutshell, Zoekt builds up an index of trigrams that it can quickly read off
of disk to search for string literals using their trigrams. Zoekt doesn't document
their file format in very much detail - their design document describes it in
broad brush strokes, but the details are sketchy.

What we needed was a way to do the same thing in C#, except we thought it would be
nice if in the process we abstracted out the reusable, general purpose data structures
that we needed into a separate library. Doing so would make the Spinach and Popeye
code much cleaner and easier to improve it over time. Thus, Eugene was born.

# More About How Zoekt Works
Zoekt looks for literals using trigrams. Trigrams are sequences of three letters.

* The trigrams for "quickly" are: qui, uic, ick, ckl, kly
* The trigrams for "browning" are: bro, row, own, wni, nin, ing
* The trigrams for "foxhound" are: fox, oxh, xho, hou, oun, und

Suppose we're looking for all documents that contain the string literal "quickly".

First we get a list of all the documents that contain the leading trigram, "qui".
Then we get another list of all the documents containing the trailing trigram, "kly" 
and see if the trigram "kly" exists at position qui + 4. Then we perform the interesection
of these two lists. Finally, we check the document to see if it truly contains
the full literal "quickly" at position qui.

In actuality we don't have to search for specifically leading and trailing trigrams, we 
can search for any two trigrams of our choosing within the literal. As long as we
have two lists of trigrams, we can intersect the list and hopefully wind up with a 
relatively small list of matching documents. This observation allows for some 
optimizations - we can search for the least frequently occurring trigrams to minimize 
the number of documents we need to search.

If we carefully arrange the documents so that they always come back ordered by
sequential file ids, then intersecting the two lists can be very fast because we can skip
over large numbers of unmatched documents as we perform the intersection.

This is why we need a custom file format that we have fine grained control over.
We want to iterate over the indexes in very specific ways, with some strategically
applied optimizations, to return results back to the user very quickly.

So that's what Zoekt does. It builds up these indexes and then does all the other
necessary work to use them to look up literals and run regex searches on candidate
documents.

The problem comes when we start thinking about how big these indexes might be.
Suppose we are trying to index 1,000,000 repositories on GitHub. Disk space is cheap
these days (well, not as cheap in the cloud, but still pretty cheap), the real problem
is how long it takes to look up a trigram in a huge index, and how many disk accesses
will be needed. Zoekt is aiming to provide lightning fast code searching that can return
thousands of results in millisecond timeframes, so disk access times become significant
on those scales.

In order to reduce disk accesses, Zoekt performs some vaguely documented magic. The
documentation is a little fuzzy on whether it does or does not cache the indexes in
memory, and whether it does or does not use a lot of memory. According to some documents
released by Zoekt's current maintainer, memory usage in Zoekt is potentially problematic.
If the Zoekt instance is not given enough memory, it may produce out-of-memory errors.

Zoekt also imposes some annoying limitations to limit its memory usage. It breaks up the
index into shards, with one repo per shard, and the sets the maximum size of a shard to 1GB.

In the world of cloud providers, as of this writing in the year 2023, memory is still
quite expensive and adds up quickly. If you need a VM with 4GB of memory, Azure or AWS
will give that to you for around $70/month or so. But if you want a 32GB VM, that might
run you $350/month.

So to run Zoekt, you have to make some calculations and some tradeoffs. Do you want to
overpay by spinning up a VM with more memory than you'll probably need, or do you want
to save money and run the risk of running out of memory. Running out of memory means
that someone is probably waking up a 3am to go fix the problem, and no one wants to do
that. We all want to sleep easy at night.

That's the problem we're trying to address with Eugene, Spinach, and Popeye.

We wanted a way to build these trigram indexes with clean, flexible, easy to modify C#
code, and also have a way to put an LRU cache in front of the data structures so that
the amount of memory used can be controlled more carefully. The goal is to make the
indexing run very fast even on small memory VM instances, and to never produce out of
memory errors.

# Where Did the Names Come From?
Zoekt is a Dutch word that means "seek".

The creator of Zoekt used the following tag line in his documentation:

```
"Zoekt, en gij zult spinazie eten" - Jan Eertink

("seek, and ye shall eat spinach" - My primary school teacher)
```

Here in America, everyone knows that Popeye the Sailor Man gets strong by eating spinach,
and so our names are based on this theme. Eugene is named after the character Eugene the Jeep
in the comic series.
