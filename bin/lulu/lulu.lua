------------------------------------------------------------------------------
-- LuLu - Lua VM on Lua  version 0.05, June 8th, 2008
--
--  Copyright (C) 2008 hzkr <binhzkr@gmail.com>
-- 
--  This software is provided 'as-is', without any express or implied
--  warranty.  In no event will the authors be held liable for any damages
--  arising from the use of this software.
--
--  Permission is granted to anyone to use this software for any purpose,
--  including commercial applications, and to alter it and redistribute it
--  freely, subject to the following restrictions:
--
--  1. The origin of this software must not be misrepresented; you must not
--     claim that you wrote the original software. If you use this software
--     in a product, an acknowledgment in the product documentation would be
--     appreciated but is not required.
--  2. Altered source versions must be plainly marked as such, and must not be
--     misrepresented as being the original software.
--  3. This notice may not be removed or altered from any source distribution.
------------------------------------------------------------------------------

lulu = {}

-------------------------------------------------------------------------
-- <<Utilities>>
-------------------------------------------------------------------------

local function bits(n, lsb, numbits)
  return math.floor(n / 2^lsb) % 2^numbits
end

local function dup(obj)
  local new = {}
  for k,v in pairs(obj) do new[k]=v end
  return new
end

-------------------------------------------------------------------------
-- <<Representation of Values>>
--   LuLu VM での値は、基本的には、LuLuを実行しているホストLuaでの
--   値でそのまま表現されます。ただし、関数とスレッドは、特別なタグを
--   つけたテーブルで表現します。
--
--   On LuLu VM, each value except a function and a thread in programs is
--   represented by the corresponding value on host Lua (the Lua environ-
--   ment running LuLu itself). Functions and threads are represented by
--   tables with a special tag-field.
--
-- Example:
--   "true"           --> true
--   "1.23"           --> 1.23
--   "{a=100,b=200}"  --> {a=100,b=200}
--   "function() end" --> {type=type_function, ...} (by a table w/tag)
--   "print"          --> function print()...end (native funcs by funcs)
-------------------------------------------------------------------------

local function type_function(x)
  return type(x)=="table" and x.type==type_function
end

local function type_thread(x)
  return type(x)=="table" and x.type==type_thread
end

local function type_table(x)
  return type(x)=="table" and x.type~=type_function and x.type~=type_thread
end

-------------------------------------------------------------------------
-- Prototype
--   関数オブジェクトの"原型"。UpValue(周囲スコープのローカル変数)と
--   環境(グローバル変数の参照先)が決まっていない状態の関数オブジェクト。
--   luac コマンドや string.dump の出力を lulu.loadproto(binstr) で
--   ロードすることでオブジェクトを作成できます。
--
--   Fields:
--      type   = type_function (tag)
--      numup  = Number of UpValues
--      numpr  = Number of Parameters
--      varflg = Flag for variadic parameters
--      maxstk = Maximum stack consumption
--      code   = Instructions
--      kv     = Constant-Table
--      kp     = Prototype-Table
-------------------------------------------------------------------------

local function inst_decode(i) -- Instruction Decoder
  return { rawval = i,
           OP = bits(i, 0, 6),
            A = bits(i, 6, 8),
            C = bits(i,14, 9),
            B = bits(i,23, 9),
            Bx= bits(i,14,18),
           sBx= bits(i,14,18) - 131071 }
end

function lulu.loadproto(binstr)
  if binstr:sub(1,4) ~= "\027Lua" then
    error("Not a valid Lua chunk file!")
  end

  local loader = {
    header = binstr:sub(1,12),
    d = binstr:sub(13), -- ヘッダはスキップ。x86 で標準的な形式を暗黙に仮定
    bytes =
      function(self, n)
        local dd = self.d
        self.d = self.d:sub(n+1)
        return dd:byte(1,n)
      end,
    -----------------------------
    byte = -- 1バイト整数
      function(self)
        return self:bytes(1)
      end,
    int  = -- 4バイト整数（リトルエンディアン）
      function(self)
        local a,b,c,d = self:bytes(4)
        return a+256*(b+256*(c+256*d))
      end,
    inst = -- 命令 (4バイト整数(ここでデコードする))
      function(self)
        return inst_decode(self:int())
      end,
    string = -- 文字列 = 長さ(size_t) ++ char * 長さ ++ \0
      function(self)
        local n = self:int()
        if n == 0 then
          return ""
        else
          local dd = self.d
          self.d = self.d:sub(n+1)
          return dd:sub(1,n-1)
        end
      end,
    number = -- 数値 = 倍精度(1+11+52)浮動小数点数
      function(self)
        local l,h,s = self:int(), self:int(), -1
        if h < 2^31 then s = 1 end
        local e = 1 + math.floor(h/2^20)%2^11 - 2^10
        if e == -1023 then
          return 0
        else
          return s*(2^e + math.ldexp(h%2^20,e-20) + math.ldexp(l,e-52))
        end
      end,
    code = -- コード = 命令の列
      function(self)
        local n = self:int()
        local code = {}
        for i=1,n do
          code[i] = self:inst()
        end
        return code
      end,
    proto = -- 関数プロトタイプ
      function(self)
        local _begin = self.d
        local obj = {type = type_function}
        self:string() -- 定義されたソースファイル名
        self:int()    -- 開始行
        self:int()    -- 終了行
        obj.numup  = self:byte()
        obj.numpr  = self:byte()
        obj.varflg = self:byte()
        obj.maxstk = self:byte()
        obj.code   = self:code()
        obj.kv     = self:constsV()
        obj.kp     = self:constsP()
        self:dbginfo()
        obj.rawdata = self.header .. _begin:sub(1, #_begin - #self.d)
        return obj
      end,
    constsV = -- 定数テーブル
      function(self)
        local n  = self:int()
        local kv = {}
        for i=1, n do
          local t = self:byte()
          if t == 0 then -- TNIL
            kv[i] = nil
          elseif t == 1 then -- TBOOLEAN
            kv[i] = self:byte()~=0
          elseif t == 3 then -- TNUMBER
            kv[i] = self:number()
          elseif t == 4 then -- TSTRING
            kv[i] = self:string()
          else
            error("Bad Tag")
          end
        end
        return kv
      end,
    constsP = -- 関数プロトタイプテーブル
      function(self)
        local n  = self:int()
        local kf = {}
        for i=1, n do
          kf[i] = self:proto()
        end
        return kf
      end,
    dbginfo = -- デバッグ情報。とりあえずスキップ
      function(self)
        local n = self:int()
        for i=1, n do
          self:int()
        end
        n = self:int()
        for i=1, n do
          self:string()
          self:int()
          self:int()
        end
        n = self:int()
        for i=1, n do
          self:string()
        end
      end,
  }
  return loader:proto()
end

-------------------------------------------------------------------------
-- Function
--   関数を表現するオブジェクト。lulu.newfunction(proto, env) で、
--   元となるPrototypeオブジェクトとグローバル変数テーブルを指定して
--   作成する。UpValueは後から適当に埋める。
--
--   Fields (<< Prototype):
--     upval  = Array of Upvalues
--     env    = Table of global values
-------------------------------------------------------------------------

local running_vm = nil
local function_metatable = {__call = function(fn, ...)
  return unpack({running_vm:co_resume(lulu.newthread(fn), ...)}, 2)
end}

function lulu.newfunction(proto, env)
  local fn = dup(proto)
  fn.upval = {}
  fn.env   = env
  setmetatable(fn, function_metatable)
  return fn
end

-------------------------------------------------------------------------
-- VM
--   lulu.newvm() で仮想マシンオブジェクトを作成します。
--   仮想マシンオブジェクトには以下のフィールドがあります。
--      vm.running_thread  (現在実行中のスレッド)
--      vm.main_thread     (メインスレッド)
--      vm:run_as_main(th) (指定されたスレッドをメインスレッドとして実行)
--      vm:run(proto)      (指定されたPrototypeを標準ライブラリ付きで実行)
-------------------------------------------------------------------------

local STARTUP_CODE = {type=type_function, code={inst_decode(29)}} -- TAILCALL

function lulu.newvm()
  return {
    main_thread    = nil,
    running_thread = nil,
    run_as_main = function(vm, th)
      vm.main_thread = th
      vm:co_resume(vm.main_thread)
    end,
    run = function(vm, proto)
      vm:run_as_main(lulu.newthread(lulu.newfunction(proto, lulu.stdlib(vm))))
    end,
    ----------------------------------------------------------
    -- コルーチン関係
    ----------------------------------------------------------
    co_resume = function(vm, co, ...)
      local pre_vm = running_vm
      running_vm = vm
      if co.status == "notstarted" then
        co.dstk = {nil, co.main, ...}
        co.dtop = 1 + #co.dstk
        co.cstk = {{func=STARTUP_CODE, pc=1, base=2, retpos=1, wanted=-1, uvcache={}}}
        co.ctop = 2
      elseif co.status == "suspended" then
        local a = {...}
        local narg = (co.yield_wanted==-1 and #a or co.yield_wanted)
        for i=1, narg do
          co.dstk[co.yield_from+i-1] = a[i]
        end
        co.dtop = co.yield_from + narg
      else
        error( "cannot resume ".. co.status .." coroutine" )
      end

      local rt = vm.running_thread
      if rt then rt.status = "normal" end
      vm.running_thread = co
      co.status = "running"
      local ret = co_run(co)
      vm.running_thread = rt
      if rt then rt.status = "running" end

      running_vm = pre_vm
      return true, unpack(ret)
    end,
    co_yield = function(vm, ...)
      vm.running_thread.yield_from    = YF -- てぬき
      vm.running_thread.yield_wanted  = YW -- てぬき
      vm.running_thread.return_values = {...}
      vm.running_thread.status        = "suspended"
    end,
    co_wrap = function(vm,fn)
      local co = lulu.newthread(fn)
      return function(...)
        local a = {vm:co_resume(co, ...)}
        return unpack(a, 2, #a)
      end
    end,
    co_running = function(vm)
      return vm.running_thread~=vm.main_thread and vm.running_thread or nil
    end,
  }
end

-------------------------------------------------------------------------
-- 標準ライブラリ
--
-- Unsupported:
--   dofile/load/loadfile/loadstring (なんとかなる気もする)
--   pcall/xpcall      (なんとかなる気もする)
--   module/require/package (loadがあれば)
--   debug (めんどう)
-------------------------------------------------------------------------

function lulu.stdlib(vm)
  local lib = dup(_G) -- とりあえずホスト環境のライブラリをコピー
  lib._G       = lib
  lib._VERSION = "LuLu 0.4"
  lib.arg      = {[-1]=arg[-1].." "..arg[0], [0]=arg[1], unpack(arg,2)}
  lib.type     = function(v)
                   local s = type(v)
                   if s == "table" then
                      return v.type==type_function and "function" or
                                (v.type==type_thread and "thread" or s)
                   else
                      return s
                   end
                 end
  lib.coroutine = { create = lulu.newthread,
                    resume = function(...) return vm:co_resume(...) end,
                    yield  = function(...) return vm:co_yield(...) end,
                    wrap   = function(fn) return vm:co_wrap(fn) end,
                    running= function() return vm:co_running() end,
                    status = function(co) return co:status_string() end, }
  local function getfunc(fn)
    if type_function(fn) then
      return fn
    else
      local rt = vm.running_thread
      fn = fn or 1
      if fn == 0 then
        return rt.main
      else
        return rt.cstk[rt.ctop-fn].func
      end
    end
  end
  lib.getfenv = function(fn)
                  return getfunc(fn).env
                end
  lib.setfenv = function(fn, t)
                  local fn = getfunc(fn)
                  fn.env = t
                  return fn
                end
  lib.getmetatable = function(obj)
                        if type_table(obj) then
                           return getmetatable(obj)
                        else
                           return nil
                        end
                     end
  lib.setmetatable = function(obj, mt)
                        if type_table(obj) then
                           setmetatable(obj, mt)
                        else
                           error("bad argument to setmetatable: table expected")
                        end
                        return obj
                     end
  lib.string = dup(_G.string)
  lib.string.dump = function(fn)
                       if type_function(fn) then
                           return fn.rawdata
                       elseif type(fn) then
                           return string.dump(fn)
                       end
                    end
  lib.tostring = function(obj) -- 同じことをprintでやるべき？うーむ
                   local s = tostring(obj)
                   if type_function(obj) then
                      s = s:gsub("table", "function")
                   elseif type_thread(obj) then
                      s = s:gsub("table", "thread")
                   end
                   return s
                 end
  -- 非対応
  lib.debug   = nil
  lib.module  = nil
  lib.require = nil
  lib.package = nil
  return lib
end

-------------------------------------------------------------------------
-- Thread
--   スレッド。コルーチン。
--
--   Fields:
--     type       = type_thread (tag)
--     dstk, dtop = data stack
--     cstk, ctop = call stack
--     status     = running status
--     main       = main function
-------------------------------------------------------------------------

function lulu.newthread(fn)
  return {
    type   = type_thread,
    status = "notstarted",
    main   = fn,
    status_string = function(co)
      return (co.status=="notstarted" and "suspended" or co.status)
    end,
  }
end

-------------------------------------------------------------------------
-- メタテーブルを考慮したテーブルアクセス
--   ToDo: 同様に__callの実装
-------------------------------------------------------------------------

function gettable(table, key)
  if not type_table(table) then
    error("not a table")
  end

  local v = rawget(table, key)
  if v~=nil then return v end
  local mt = getmetatable(table)
  if mt==nil then return v end
  local ix = rawget(mt, "__index")
  if ix==nil then return v end

  if type_function(ix) or type(ix)=="function" then
    return ix(table, key)
  else
    return gettable(ix, key)
  end
end

function settable(table, key, value)
  if not type_table(table) then
    error("not a table")
  end

  local v = rawget(table, key)
  if v~=nil then rawset(table, key, value) return end
  local mt = getmetatable(table)
  if mt==nil then rawset(table, key, value) return end
  local ix = rawget(mt, "__index")
  if ix==nil then rawset(table, key, value) return end

  if type_function(ix) or type(ix)=="function" then
    ix(table, key, value)
  else
    settable(ix, key, value)
  end
end

-------------------------------------------------------------------------
-- メインの実行ループ
-------------------------------------------------------------------------

function co_run(co)
  -- コールスタック
  local ci = co.cstk[co.ctop-1]
  local func,base,retpos,wanted,uvcache = ci.func, ci.base, ci.retpos, ci.wanted, ci.uvcache
  local function cipush(tail, a,b,c,d,e,f)
     ci = {func=a, pc=b, base=c, retpos=d, wanted=e, uvcache=f}
     if not tail then
       co.ctop=co.ctop+1
     end
     co.cstk[co.ctop-1]=ci
     func,base,retpos,wanted,uvcache = ci.func, ci.base, ci.retpos, ci.wanted, ci.uvcache
  end
  local function cipop()
     co.ctop=co.ctop-1
     ci = co.cstk[co.ctop-1]
     func,base,retpos,wanted,uvcache = ci.func, ci.base, ci.retpos, ci.wanted, ci.uvcache
  end

  -- データスタック
  local stk = co.dstk
  local function getreg(n)   return stk[base+n] end
  local function setreg(n,v) stk[base+n] = v    end
  local function getregs(i,n) return unpack(stk, base+i, base+i+n-1) end
  local function setregs(i,n,vals)
    local m = n
    if m > #vals then m = #vals end
    for j=1,m   do stk[base+i+j-1] = vals[j] end
    for j=m+1,n do stk[base+i+j-1] = nil  end
  end

  -- upvalueをスタックから逃がす処理
  local function closeupvals(from) 
    from = from or 0
    for k,u in pairs(uvcache) do
      if from<=k and not rawequal(u.base, u) then
        u[1]   = u.base[u.idx]
        u.base = u
        u.idx  = 1
        uvcache[k] = nil --逃がし終わったらテーブルから消す
      end
    end
  end

  local function do_call(fn, A, narg, nret, tail) -- 関数呼び出しの共通処理
     if type_function(fn) then
        cipush(tail, fn, 1, base+A+1, base+A, nret, {})

        -- 実引数が仮引数より少ない場合、残りを nil で埋める
        if narg < fn.numpr then
           for i=narg, fn.numpr-1 do setreg(i, nil) end
           narg = fn.numpr
        end
        -- 可変長引数な時、必須引数を前に持ってきてbaseをずらす
        if func.varflg>0 then
           for i=0,fn.numpr-1 do
             stk[base+narg+i] = stk[base+i]
             stk[base+i]      = nil
           end
           base = base+narg
           co.cstk[co.ctop-1].base = base
        end
        -- ローカル変数領域をnilクリア
        for i=fn.numpr, fn.maxstk-1 do setreg(i,nil) end
     else
        YF, YW = A, nret -- てぬき：fnがco_yieldだった場合に使う
        local a = {fn(getregs(A+1, narg))} -- TODO: nil がかえると{nil}は長さゼロになるバグ！
        setregs(A, (nret==-1 and #a or nret), a)
        co.dtop = base+A+#a
     end
  end

  -- メインの実行ループ
  co.return_values = nil
  repeat
    local inst = func.code[ci.pc]
    ci.pc = ci.pc+1

    local OP  = inst.OP
    local A   = inst.A
    local C   = inst.C
    local B   = inst.B
    local Bx  = inst.Bx
    local sBx = inst.sBx
    local RKB if B>=2^8 then RKB = func.kv[B-2^8+1] else RKB = getreg(B) end
    local RKC if C>=2^8 then RKC = func.kv[C-2^8+1] else RKC = getreg(C) end

    if OP ==  0 then -- MOVE
       setreg(A, getreg(B))
    elseif OP == 1 then -- LOADK
       setreg(A, func.kv[Bx+1])
    elseif OP == 2 then -- LOADBOOL
       setreg(A, B~=0)
       if C~=0 then ci.pc = ci.pc+1 end
    elseif OP == 3 then -- LOADNIL
       for i=A,B do setreg(i, nil) end
    elseif OP == 4 then -- GETUPVAL
       local u = func.upval[B+1]
       setreg(A, u.base[u.idx])
    elseif OP == 5 then -- GETGLOBAL
       setreg(A, gettable(func.env, func.kv[Bx+1]))
    elseif OP == 6 then -- GETTABLE
       setreg(A, gettable(getreg(B), RKC))
    elseif OP == 7 then -- SETGLOBAL
       settable(func.env, func.kv[Bx+1], getreg(A))
    elseif OP == 8 then -- SETUPVAL
       local u = func.upval[B+1]
       u.base[u.idx] = getreg(A)
    elseif OP == 9 then -- SETTABLE
       settable(getreg(A), RKB, RKC)
    elseif OP == 10 then -- NEWTABLE
       setreg(A, {})
    elseif OP == 11 then -- SELF
       local rb = getreg(B)
       setreg(A,   rb[RKC])
       setreg(A+1, rb)
    elseif OP == 12 then -- ADD
       setreg(A, RKB + RKC)
    elseif OP == 13 then -- SUB
       setreg(A, RKB - RKC)
    elseif OP == 14 then -- MUL
       setreg(A, RKB * RKC)
    elseif OP == 15 then -- DIV
       setreg(A, RKB / RKC)
    elseif OP == 16 then -- MOD
       setreg(A, RKB % RKC)
    elseif OP == 17 then -- POW
       setreg(A, RKB ^ RKC)
    elseif OP == 18 then -- UNM
       setreg(A, - RKB)
    elseif OP == 19 then -- NOT
       setreg(A, not RKB)
    elseif OP == 20 then -- LEN
       setreg(A, # RKB)
    elseif OP == 21 then -- CONCAT
       local s = ""
       for i=B,C do s = s .. getreg(i) end
       setreg(A, s)
    elseif OP == 22 then -- JMP
       ci.pc = ci.pc + sBx
    elseif OP == 23 then -- EQ
       if (RKB==RKC) ~= (A~=0) then ci.pc=ci.pc+1 end
    elseif OP == 24 then -- LT
       if (RKB< RKC) ~= (A~=0) then ci.pc=ci.pc+1 end
    elseif OP == 25 then -- LE
       if (RKB<=RKC) ~= (A~=0) then ci.pc=ci.pc+1 end
    elseif OP == 26 then -- TEST
       if (not getreg(A)) == (C~=0) then ci.pc=ci.pc+1 end
    elseif OP == 27 then -- TESTSET
       if (not getreg(B)) == (C~=0) then ci.pc=ci.pc+1 else setreg(A,getreg(B)) end
    elseif OP == 28 then -- CALL : R[A],...,R[A+C-2], R[A]( R[A+1], ..., R[A+B-1] )
       do_call(getreg(A), A, B==0 and co.dtop-(base+A+1) or B-1, C-1)
    elseif OP == 29 then -- TAILCALL : return R[A]( R[A+1], ..., R[A+B-1] )
       closeupvals()
       local fn = getreg(A)
       if type_function(fn) then
          local narg = B==0 and co.dtop-(base+A+1) or B-1
          setregs(retpos-base, narg+1, {getregs(A, narg+1)})
          do_call(fn, retpos-base, narg, C-1, true)
       else
          local a = {fn(getregs(A+1, B==0 and co.dtop-(base+A+1) or B-1))}
          if co.ctop == 2 then
            co.status = "dead"
            co.return_values = a
          else
            setregs(retpos-base, (wanted==-1 and #a or wanted), a)
            co.dtop = retpos + #a
            cipop()
          end
       end
    elseif OP == 30 then -- RETURN : R[A], ..., R[A+B-1]
       closeupvals()
       local numret = B==0 and co.dtop-(base+A) or B-1
       if co.ctop == 2 then
         co.status = "dead"
         co.return_values = {getregs(A, numret)}
       else
         setregs(retpos-base, (wanted==-1 and numret or wanted), {getregs(A,numret)})
         co.dtop = retpos + numret
         cipop()
       end
    elseif OP == 31 then -- FORLOOP : for i=a,b,c 型ループ
       local idx = getreg(A)+getreg(A+2)
       if (0<getreg(A+2)) == (idx<=getreg(A+1)) then
         ci.pc = ci.pc + sBx
         setreg(A,   idx) -- ループ用の内部カウンタ
         setreg(A+3, idx) -- Luaのコードから見える変数
       end
    elseif OP == 32 then -- FORPREP : ループ開始
       setreg(A, getreg(A)-getreg(A+2))
       ci.pc = ci.pc + sBx
    elseif OP == 33 then -- TFORLOOP : for _ in R[A] 型ループ
       func.code[ci.pc].OP = 999 -- 無理矢理実装／動的命令書き換え JMP(22)-> 999
       setreg(A+3, getreg(A))
       setreg(A+4, getreg(A+1))
       setreg(A+5, getreg(A+2))
       do_call(getreg(A+3), A+3, 2, C)
      elseif OP == 999 then -- TFORLOOPの直後のJMP
       A = func.code[ci.pc-2].A+3
       if getreg(A) ~= nil then
         setreg(A-1, getreg(A))
         ci.pc = ci.pc + sBx
       end
    elseif OP == 34 then -- SETLIST : RA[(C-1)*FPF+i] = R[A+i] for 1<=i<=B
       local FPF = 50 -- 本家Luaのソースではマクロで定義されてる
       if B==0 then B = co.dtop-(base+A)-1 end
       if C==0 then C = func.code[ci.pc].rawval ci.pc=ci.pc+1 end
       local arr = getreg(A)
       for i=1,B do
          arr[(C-1)*FPF+i] = getreg(A+i)
       end
    elseif OP == 35 then -- CLOSE
       closeupvals(A)
    elseif OP == 36 then -- CLOSURE
       local cl = lulu.newfunction(func.kp[Bx+1], func.env)
       for i=1, cl.numup do
         if func.code[ci.pc].OP == 0 then -- MOVE : 現在のスタックの変数をupval化
            local b = func.code[ci.pc].B
            local uv = uvcache[b]
            if not uv then uv={base=stk, idx=base+b} uvcache[b]=uv end
            cl.upval[i] = uv
         else -- GETUPVAL : 親(祖先)のスタックのupvalを持ってくる
            cl.upval[i] = func.upval[func.code[ci.pc].B+1]
         end
         ci.pc = ci.pc+1
       end
       setreg(A, cl)
    elseif OP == 37 then -- VARARG : R[A],...,R[A+B-2] = ...
       local numvar = base-retpos-1-func.numpr
       if B == 0 then -- multivalue
          B = numvar
          co.dtop = base + A + numvar
       else
          B = B-1
       end
       for i=0,B-1 do
         if i < numvar then
           setreg(A+i, stk[base-numvar+i])
         else
           setreg(A+i, nil)
         end
       end
    else
       error("Unknown VM Instruction")
    end
  until co.return_values
  return co.return_values
end

-------------------------------------------------------------------------
-- Main
-------------------------------------------------------------------------

if arg[1] then
   local chunk = string.dump(loadfile(arg[1]))
   local vm = lulu.newvm()
   vm:run( lulu.loadproto(chunk) )
else
   print( "Usage: lua lulu.lua [script.lua] [args]" )
end
