Dotted-version vector clocks

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%  
%%%%%%%%%% STRUCTURE %%%%%%%%%%%%%%  
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%  
  
clock()         :: {entries(), values()}.  
vector()        :: [{id(), counter(), values()}].  
entries()       :: [{id()}, {counter(), [values()]}].  
id()            :: string().  
values()        :: [value()].  
value()         :: string().  
counter()       :: non_neg_integer().  
