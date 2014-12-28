# Writing Exercise Programs

Workout Wotch allows you to author exercise programs with a flexible and powerful syntax. This syntax is described below. Firstly, however, it is perhaps most instructive to see a realistic example:

```
# Facet Joint Health

## Squats

* 3 sets x 15 reps
* Before:
  * Prepare for 10s
* After sets ^last:
  * Break for 25s
* During Rep:
  * Metronome at 0s*, 2s, 1.5s, 1.5s-

## Forward Lunges

* 2 sets x 20 reps
* Before:
  * Break for 30s
* Before set 1:
  * Say 'right leg forward'
* Before set 2:
  * Say 'left leg forward'
* After sets ^last:
  * Break for 15s
* During Rep:
  * Metronome at 0s*, 1.25s, 2s, 2s-
```

The above document contains a single exercise program (*Facet Joint Health*) with two exercises in it (*Squats* and *Forward Lunges*).

## Exercise Programs

Exercise programs are started with a level one title. That is, a single '#' character followed by some whitespace, then the exercise program name.

Here are some examples:

```
# Exercise Program 1

# Exercise Program 2
```

In these examples neither exercise program has any exercises in it, so it's basically empty.

## Exercises

Exercises are started with a level two title. That is, two '#' characters followed by some whitespace, then the exercise name.

Here are some examples:

```
# Exercise Program

## Exercise 1

## Exercise 2
```

In these examples, the exercises aren't tecnically valid. Exercises require information about how many sets and repetitions are within before they're valid, and they also require actions to be associated with events before they're actually useful.

## Sets and Repetitions

The first thing an exercise requires is a specification for how many sets and repetitions it contains. This is provided as a level one bullet list entry.

Here are some examples:

```
## Exercise 1

* 3 sets x 10 reps

## Exercise 2

* 1 set x 20 reps

```

In the first example, the exercise will contain 3 sets, each with 10 repetitions. In the second example, the exercise only contains a single set, but that set has 20 repetitions.

## Events and Actions

Once the set and repetition count is provided, any number of events can be associated with any number of actions. Each event appears as a level one bullet list entry and the associated actions appear as level two bullet list entries underneath it.

Here are some examples:

```
## Exercise

* 2 sets x 5 reps
* Before:
  * Prepare for 30s
* After:
  * Say 'well done!'
  * Wait for 1s
  * Say 'now go and watch tv'
* After set ^last:
  * Break for 10s
* During rep:
  * Metronome at 0s*, 1s, 1.5s, 1s-
```

In the above example, several events are specified (*Before*, *After*, *After set*, and *During rep*) and actions are associated with those events. In the case of the *After* event, several actions are specified. Each of those actions will be executed one after the other, in sequence.

The table below lists all available events listed in life-cycle order:

Name | Allows Numerical Constraints | Description
:--: | :--------------------------: | -----------
Before | No | Occurs before the exercise.
Before set | Yes | Occurs before a set executes.
Before rep | Yes | Occurs before a repetition executes.
During rep | Yes | Occurs when a repetition executes.
After rep | Yes | Occurs after a repetition executes.
After set | Yes | Occurs after a set executes.
After  | No | Occurs after the exercise.

The events that allow numerical constraints can contain further qualification such that only certain instances of that event will trigger the associated actions. The example above contains an *After set* event with a numerical constraint that excludes the last set, so it will execute for every set apart from the last. The full details of numerical constraints are provided below.

The table below lists all available actions:

Name | Example Usage | Description
---- | ------------- | -----------
Break | Break for 30s | Tells the user they're having a break and then waits the specified amount of time. Warns the user when their break is about to end.
Prepare | Prepare for 10s | Tells the user they should prepare and then waits the specified amount of time. Warns the user when their preparation time is nearly up.
Metronome | Metronome at 0s*, 1s, 1.5, 1s- | Plays a metronome to the user so they have audial timing information whilst performing their exercise. Any number of "ticks" can be specified. Each specifies the amount of time between the previous tick and this tick. An asterisk (*) signifies a tick that will play a "bell"" sound. A hypen (-) signifies a tick that will have no audial feedback. If the tick has neither asterisk nor hyphen, it will play a normal "click" sound.
Say | Say 'nice job' | Says something to the user.
Wait | Wait for 3s | Waits for the specified amount of time.
Don't Wait | Don't wait: | Provides any number of additional actions to execute without waiting for their execution to complete. The additional actions are provided as a list at an additional level of indentation below.
Parallel | Parallel: | Provides any number of additional actions to execute in parallel. That is, all actions are executed at the same time. The additional actions are provided as a list at an additional level of indentation below.
Sequence | Sequence: | Provides any number of additional actions to execute in sequence. That is, all actions are executed one after the other. The additional actions are provided as a list at an additional level of indentation below.

The last three actions (*Don't wait*, *Parallel*, and *Sequence*) are perhaps the trickiest to understand. Typically you would not even need to use them, but here is an example:

```
* Before:
  * Parallel:
    * Wait for 10s
    * Sequence:
      * Wait for 8s
      * Don't wait:
        * Say 'only two seconds left'
```

In this highly contrived example, we simultaneously kick off a 10 second wait with an 8 second wait. When the 8 second wait completes, we request that 'only two seconds left' be said to the user. We ensure that we don't wait for the speech to complete by specifying it as a child of a *Don't wait* action.

Note that, by default, multiple actions for an event are executed in sequence.

## Numerical Constraints

Events that include an identifying number (e.g. set number, or repetition number) permit numerical constraints to be specified. This allows the associated actions to be executed only when the number matches the constraint.

There are several different types of numerical constraints, each of which is discussed below.

### Literals

Literal numerical constraints specify a literal number to constrain against:

```
* Before set 1:
  * Say 'this executes before the first set'
* After rep 3:
  * Say 'this executes after the third rep'
```

### Symbols

One can use the symbols `first` and `last` to represent the first and last numbers:

```
* Before set first:
  * Say 'this executes before the first set'
* After rep last:
  * Say 'this executes after the last rep'
```

Using symbols instead of "hard coding" numbers can make the exercise program more resilient to changes.

### Symbol Expressions

A symbol can optionally include a simple expression after it to adjust for some offset:

```
* Before set first+1:
  * Say 'this executes before the second set'
* After rep last-2:
  * Say 'this executes after the third-to-last set'
```

The `first` symbol can only include an addition expression, and the `last` symbol can only include a subtraction expression.

### Ranges

A range of numbers can be specified:

```
* Before set 2..4:
  * Say 'this will execute before sets 2, 3, and 4'
```

An additional skip value can be provided between the start and end numbers for the range:

```
* Before set 2..2..8:
  * Say 'this will execute before sets 2, 4, 6, and 8'
```

Symbols and symbol expressions can also be leveraged within ranges:

```
* Before set first+1..last-1:
  * Say 'this will execute before all sets apart from the first and last'
* Before set first+1..2..last-1:
  * Say 'this will execute before all odd numbered sets apart from the first and last'
```

### Sets

Numerical constraints can also be provided as a set (a mathematical set - not an exercise set!) of values:

```
* Before set 1, 3, 8:
  * Say 'this will execute before sets 1, 3, and 8'
```

All prior concepts can be incorporated into the set:

```
* Before set first, last - 2, 3..5:
  * Say 'this will execute before the first, second-to-last, third, fourth, and fifth sets'
```

### Logical Not

Finally, a numerical constraint can be prefixed with a caret (^) to invert it:

```
* Before set ^2:
  * Say 'this will execute before every set apart from number 2'
* Before set ^last:
  * Say 'this will execute before every set apart from the last'
```

Again, all prior concepts can be incorporated into the expression:

```
* Before reps ^3..4, 8..last:
  * Say 'this will execute before any repetition that isn't number 3, 4, 8 or higher'
```

Notice that the logical not applies to the entire set, not individual members of the set. In other words, the example above could be re-written as `^[3..4, 8..last]` where the braces demarcate the mathematical set.