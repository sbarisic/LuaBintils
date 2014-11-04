co = coroutine.wrap(function()
     while true do
       print(ca())
     end
  end)

ca = coroutine.wrap(function()
     while true do
       print(co())
     end
  end)

co()
