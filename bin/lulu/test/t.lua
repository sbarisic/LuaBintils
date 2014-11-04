-- read environment variables as if they were global variables

x = {}
--setmetatable(x, {__add = function(x,y) x.foo = y return 9 end})
--print( x + 100 )
--print( x.foo )


local f=function (t,i) return os.getenv(i) end
setmetatable(getfenv(),{__index=f})

-- an example
print(a,USER,PATH)
