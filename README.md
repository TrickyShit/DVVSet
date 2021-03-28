
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%  
%% STRUCTURE  
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%  
  
clock()         :: {entries(), values()}.  
vector()        :: [{id(), counter(), logical_time()}].  
entries()       :: [{id(), counter(), values(), logical_time()}].  
id()            :: any().  
values()        :: [value()].  
value()         :: any().  
counter()       :: non_neg_integer().  
logical_time()  :: pos_integer().  
