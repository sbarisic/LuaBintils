function foo(a,b,c,d) print(a,b,c,d) end
function bar(a,b)     return b,a     end
foo(1,2,bar(3,4))
