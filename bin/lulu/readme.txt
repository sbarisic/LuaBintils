
------------------------------------------------------------------
 LuLu 0.05
 http://lulu.luaforge.net/
                                                      2008/06/08
                                        hzkr <binhzkr@gmail.com>
                                     http://d.hatena.ne.jp/hzkr/
------------------------------------------------------------------


-- Usage

 > lua lulu.lua your_lua_program.lua



-- What's This?

 LuLu is a Lua 5.1 VM implementation in Lua language itself.
 It aims to be a concise, easily readable, and customizable LuaVM implemantation.

 Currently, LuLu consists of the interpreter of virtual machine instructions +
 the coroutine library. The implmentation of datatypes (strings, tables, etc) and
 most of the standard library functions are simply reusing the implementation
 of the host Lua environment.

 Supported Features:
   - All VM Instructions
   - Most of the stand library functions, including
      - io.*         (forwarded to the host Lua)
      - file:*       (forwarded to the host Lua)
      - string.*     (forwarded to the host Lua, except string.dump)
      - math.*       (forwarded to the host Lua)
      - coroutine.*  (pure Lua implementation)

 Currently Unsupported Features:
   - Metatables
        (actually, all metamethods except __call should work under the metatable
         mechanism of the host Lua.)
   - The following standard library functions:
      - dofile/load/loadfile/loadstring/string.dump
      - module/require/package.*
      - pcall/xpcall
      - debug.*

--
hzkr <binhzkr@gmail.com>
