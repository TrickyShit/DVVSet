%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%% STRUCTURE
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

Erlang

-type dvv() :: {vector(), dot()} | {}.
-type vector() :: [dot()].
-type dot() :: {id(), {counter(), timestamp()}} | null.

-type id() :: term().
-type counter() :: integer().
-type timestamp() :: integer().

C#

dvv - кортеж (vector, dot)
vector - массив [dot]
dotwithid - кортеж (id, dot)
dot - кортеж (counter, timestamp)

counter - тип int
timestamp - тип int
id - тип string? (тип term в Erlang - типа любой)

