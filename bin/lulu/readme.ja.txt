
------------------------------------------------------------------
 LuLu 0.05
 http://lulu.luaforge.net/
                                                      2008/06/08
                                        hzkr <binhzkr@gmail.com>
                                     http://d.hatena.ne.jp/hzkr/
------------------------------------------------------------------


-- 使い方

 > lua lulu.lua your_lua_program.lua



-- これはなに？

 LuLu は Lua 自身で実装された Lua 5.1 の VM です。

 いまのところ、LuLu が実装しているのは、VMの命令列の解釈とコルーチンの処理部分です。
 データ型（文字列やテーブル）や標準ライブラリ関数の実装は、基本的にホストのLuaに
 丸投げしています。

 Supported Features:
   - VM の全命令
   - ライブラリ関数の大部分
      - io.*         (forwarded to the host Lua)
      - file:*       (forwarded to the host Lua)
      - string.*     (forwarded to the host Lua, except string.dump)
      - math.*       (forwarded to the host Lua)
      - coroutine.*  (pure Lua implementation)

 Currently Unsupported Features:
   - メタテーブル
        (actually, all metamethods except __call should work under the metatable
         mechanism of the host Lua.)
   - 以下のライブラリ関数
      - dofile/load/loadfile/loadstring/string.dump
      - module/require/package.*
      - pcall/xpcall
      - debug.*

