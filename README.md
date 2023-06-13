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
expressions across a very large corpus of code repositories. However,
Eugene is a general purpose library that can find broad applicability
in many domains of work.

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

Admittedly this is a bit of a contrived example, but its useful to illustrate
the point.

First, Zoekt extracts the three known literals from the string, getting:

* quickly
* browning
* foxhound

and then goes about finding all the files in the corpus that contain these three
literals. Once it finds a file with all three of these string literals, it runs
a regex search over the file to see if it matches the full regex.

Okay, but how does it quickly find those matching files? Now we're getting to the
heart of the matter with this problem.

Zoekt looks for literals using trigrams. Trigrams are sequences of three letters.

The trigrams for "quickly" are: qui, uic, ick, ckl, kly
The trigrams for "browning" are: bro, row, own, wni, nin, ing
The trigrams for "foxhound" are: fox, oxh, xho, hou, oun, und

Suppose we're looking for all documents that contain the string literal "quickly".
We can start by finding all documents that contain the trigram "qui".

For any documents that contain the trigram qui, we look to see if that document
also contains the trigram "kly" at position qui + 4. If it does, we look to see
if the full literal "quickly" exists starting at position qui.

Once we find the same document that contains all necessary literals, only then
do we run the slow regex query over the candidate document to determine if there
truly is a match of the regex.

What's needed, then, is a way to index documents by trigrams. But what we really
need are two dictionaries, stored some how, some way (arrays, linked lists, hashtables,
binary trees, b+trees, trie trees, whatever). At the level of the application code, we don't
necessarily care how those dictionaries are implemented. We just want them to be fast
and have a well abstracted API that we can code against.

Dictionary 1: For a given trigram, give me all the files that contain that trigram
Dictionary 2: For a given file, give me all the trigrams in that file

We can use the first dictionary to find all the files that match the leading trigram
of the target literal. Then for each one of those files, we use Dictionary 2 to discover
whether that file contains the trailing trigram of the target literal.

So that's what Zoekt does. It builds up these dictionaries and then does all the other
necessary work to use them to look up literals and run regex searches on candidate
documents.

The problem comes when we start thinking about how big these indexes might be.
Suppose we are trying to index 1,000,000 repositories on GitHub. Disk space is cheap
these days (well, not as cheap in the cloud, but still pretty cheap), the real problem
is how long it takes to look up a trigram in a huge index, and how many disk accesses
will be needed. Zoekt is aiming to provide lightning fast code searching that can return
thousands of results in millisecond timeframes, so disk access times become significant
on those scales.

In order to reduce disk accesses, Zoekt uses a lot of memory to cache those indexes.
It doesn't seem to have any mechanisms for selectively caching portions of the index,
it basically just keeps the whole index in memory. That means, if you have a large
corpus of repositories, your Zoekt instance needs to have a lot of memory. If you don't
give Zoekt enough memory, it will produce out-of-memory errors.

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

We wanted a way to build these trigram indexes and have a way to put an LRU cache in
front of it so that it can still run fast most of the time, but not have the risk of
running out of memory if the indexes or too big or the VM is too small.
