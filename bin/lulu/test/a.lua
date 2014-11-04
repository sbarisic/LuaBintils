
function foo(a,b,...)
  print(a)
  print(...)
  print(b)
end

function fac(n, a, b, d, f)
  if n<2 then return math.log(a)
         else return fac(n-1, n*a) end
end

print(2.727^fac(10,1))
foo(1,2,3,4,5)
