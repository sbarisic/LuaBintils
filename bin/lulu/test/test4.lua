
function foo()
  local x = 0
  function bar()
    x = x+1
    function buz() function bbb() x=x end end
    buz()
  end
  bar()
  x = x+1
  bar()
  x = x+1
  print("#",x)
end

foo()
