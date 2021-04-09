# Dotted-version vector clocks

_This is adaptation from Erlang to C#_  
Original code: <https://github.com/ricardobcl/Dotted-Version-Vectors>

## **STRUCTURE**

**clock()** :: {entries(), values()}.  
**vector()** :: [{counter(), values()}].  
**entries()** :: [{id()}, {counter(), [values()]}].  
**id()** :: string().  
**values()** :: [value()].  
**value()** :: string().  
**counter()** :: non_neg_integer().  

### Structure of clocks

**_{id,{counter,{values}}_**

Each operation with clock causes a counter increment. Id is a unique value, entries in the clock cannot have the same id value.

## Methods

### Create (clock, Id)

Metod Create advances the causal history with the given id.  

Example: we take a empty clock with a value "v1" and create a new clock with id "a":  
**_Create(new Clock("v1"), "a") -> [{a,1,["v1"]}],[]_**

### Join

Return a entries without any values.  

Example: **_Join([{a,1,["v1"]}],[] -> [{a,1,[]}]_**  

### Clock Update(clock1, clock2, Id)  

Advances the causal history of the first clock with the given id, while synchronizing with the second clock, thus the new clock is causally newer than both clocks in the argument. The new value is the *anonymous dot* of the clock.  

Examples:  
***Update([{a,1,[]}],["v2"]**(first clock)*,***[{a,1,["v1"]}],[]**(second clock)*,***"a"(id)) -> [{a,2,[v2]}],[]***  
***Update([{a,1,[]}],["v4"]**(first clock)*,***[{a,2,[]}],["v2"]**(second clock)*,***"b"(id)) -> [{a,2,[v2]}],[{b,1,[v4]}],[]***  
***Update([{a,1,[]}],["v5"]**(first clock)*,***[{a,2,[]}],[v2]**(second clock)*,***"a"(id)) -> [{a,3,[v5][v2]}],[]***  

### SyncClocks(Clock clock1, Clock clock2)
