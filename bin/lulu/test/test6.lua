function foo(a,b,...)
  print(a,b,a/b)
  bar(...)
  print(a,b,a/b)
end

function bar(c,d,...)
  print(c,d,c/d)
  print(...)
end

foo(1,2,3,4,5,6)
