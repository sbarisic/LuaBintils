using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaBin {
	public enum OpCode {
		MOVE,
		LOADK,
		LOADBOOL,
		LOADNIL,
		GETUPVAL,
		GETGLOBAL,
		GETTABLE,
		SETGLOBAL,
		SETUPVAL,
		SETTABLE,
		NEWTABLE,
		SELF,
		ADD,
		SUB,
		MUL,
		DIV,
		MOD,
		POW,
		UNM,
		NOT,
		LEN,
		CONCAT,
		JMP,
		EQ,
		LT,
		LE,
		TEST,
		TESTSET,
		CALL,
		TAILCALL,
		RETURN,
		FORLOOP,
		FORPREP,
		TFORLOOP,
		SETLIST,
		CLOSE,
		CLOSURE,
		VARARG,

		COUNT = VARARG
	}
}