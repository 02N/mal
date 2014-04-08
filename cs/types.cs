using System;
using System.Collections.Generic;
using Mal;

namespace Mal {
    public class types {
        //
        // Exceptiosn/Errors
        //
        public class MalThrowable : Exception {
            public MalThrowable() : base() { }
            public MalThrowable(string msg) : base(msg) {  }
        }
        public class MalError : MalThrowable {
            public MalError(string msg) :base(msg) { }
        }
        public class MalContinue : MalThrowable { }

        // Thrown by throw function
        public class MalException : MalThrowable {
            MalVal value;
            public MalException(MalVal value) {
                this.value = value;
            }
            public MalException(string value) {
                this.value = new MalString(value);
            }
            public MalVal getValue() { return value; }
        }

        //
        // General functions
        //

        public static bool _equal_Q(MalVal a, MalVal b) {
            Type ota = a.GetType(), otb = b.GetType();
            if (!((ota == otb) ||
                (a is MalList && b is MalList))) {
                return false;
            } else {
                if (a is MalInteger) {
                    return ((MalInteger)a).getValue() ==
                        ((MalInteger)b).getValue();
                } else if (a is MalSymbol) {
                    return ((MalSymbol)a).getName() ==
                        ((MalSymbol)b).getName();
                } else if (a is MalString) {
                    return ((MalString)a).getValue() ==
                        ((MalString)b).getValue();
                } else if (a is MalList) {
                    if (((MalList)a).size() != ((MalList)b).size()) {
                        return false;
                    }
                    for (int i=0; i<((MalList)a).size(); i++) {
                        if (! _equal_Q(((MalList)a)[i], ((MalList)b)[i])) {
                            return false;
                        }
                    }
                    return true;
                } else {
                    return a == b;
                }
            }
        }


        public abstract class MalVal {
            // Default is just to call regular toString()
            public virtual string ToString(bool print_readably) {
                return this.ToString();
            }
            public virtual bool list_Q() { return false; }
        }

        public class MalConstant : MalVal {
            string value;
            public MalConstant(string name) { value = name; }
            public MalConstant copy() { return this; }

            public override string ToString() {
                return value;
            }
            public override string ToString(bool print_readably) {
                return value;
            }
        }

        static public MalConstant Nil = new MalConstant("nil");
        static public MalConstant True = new MalConstant("true");
        static public MalConstant False = new MalConstant("false");

        public class MalInteger : MalVal {
            int value;
            public MalInteger(int v) { value = v; }
            public MalInteger copy() { return this; }

            public int getValue() { return value; }
            public override string ToString() {
                return value.ToString();
            }
            public override string ToString(bool print_readably) {
                return value.ToString();
            }
            public static MalConstant operator <(MalInteger a, MalInteger b) {
                return a.getValue() < b.getValue() ? True : False;
            }
            public static MalConstant operator <=(MalInteger a, MalInteger b) {
                return a.getValue() <= b.getValue() ? True : False;
            }
            public static MalConstant operator >(MalInteger a, MalInteger b) {
                return a.getValue() > b.getValue() ? True : False;
            }
            public static MalConstant operator >=(MalInteger a, MalInteger b) {
                return a.getValue() >= b.getValue() ? True : False;
            }
            public static MalInteger operator +(MalInteger a, MalInteger b) {
                return new MalInteger(a.getValue() + b.getValue());
            }
            public static MalInteger operator -(MalInteger a, MalInteger b) {
                return new MalInteger(a.getValue() - b.getValue());
            }
            public static MalInteger operator *(MalInteger a, MalInteger b) {
                return new MalInteger(a.getValue() * b.getValue());
            }
            public static MalInteger operator /(MalInteger a, MalInteger b) {
                return new MalInteger(a.getValue() / b.getValue());
            }
        }

        public class MalSymbol : MalVal {
            string value;
            public MalSymbol(string v) { value = v; }
            public MalSymbol copy() { return this; }

            public string getName() { return value; }
            public override string ToString() {
                return value;
            }
            public override string ToString(bool print_readably) {
                return value;
            }
        }

        public class MalString : MalVal {
            string value;
            public MalString(string v) { value = v; }
            public MalString copy() { return this; }

            public string getValue() { return value; }
            public override string ToString() {
                return "\"" + value + "\"";
            }
            public override string ToString(bool print_readably) {
                if (print_readably) {
                    return "\"" + value.Replace("\\", "\\\\")
                        .Replace("\"", "\\\"")
                        .Replace("\n", "\\n") + "\"";
                } else {
                    return value;
                }
            }
        }



        public class MalList : MalVal {
            public string start = "(", end = ")";
            List<MalVal> value;
            public MalList() {
                value = new List<MalVal>();
            }
            public MalList(List<MalVal> val) {
                value = val;
            }
            public MalList(params MalVal[] mvs) {
                value = new List<MalVal>();
                conj_BANG(mvs);
            }

            public MalList copy() {
                return (MalList)this.MemberwiseClone();
            }

            public List<MalVal> getValue() { return value; }
            public override bool list_Q() { return true; }

            public override string ToString() {
                return start + printer.join(value, " ", true) + end;
            }
            public override string ToString(bool print_readably) {
                return start + printer.join(value, " ", print_readably) + end;
            }

            public MalList conj_BANG(params MalVal[] mvs) {
                for (int i = 0; i < mvs.Length; i++) {
                    value.Add(mvs[i]);
                }
                return this;
            }

            public int size() { return value.Count; }
            public MalVal nth(int idx) { return value[idx]; }
            public MalVal this[int idx] {
                get { return value[idx]; }
            }
            public MalList rest() {
                if (size() > 0) {
                    return new MalList(value.GetRange(1, value.Count-1));
                } else {
                    return new MalList();
                }
            }
            public virtual MalList slice(int start) {
                return new MalList(value.GetRange(start, value.Count-start));
            }
            public virtual MalList slice(int start, int end) {
                return new MalList(value.GetRange(start, end-start));
            }

        }

        public class MalVector : MalList {
            // Same implementation except for instantiation methods
            public MalVector() :base() {
                start = "[";
                end = "]";
            }
            public MalVector(List<MalVal> val)
                    :base(val) {
                start = "[";
                end = "]";
            }
            /*
            public MalVector(MalVal[] mvs) : base(mvs.ToArray()) {
                start = "[";
                end = "]";
            }
            */
            public new MalVector copy() {
                return (MalVector)this.MemberwiseClone();
            }

            public override bool list_Q() { return false; }

            public override MalList slice(int start, int end) {
                var val = this.getValue();
                return new MalVector(val.GetRange(start, val.Count-start));
            }
        }

        public class MalHashMap : MalVal {
            Dictionary<string, MalVal> value;
            public MalHashMap(Dictionary<string, MalVal> val) {
                value = val;
            }
            public MalHashMap(MalList lst) {
                value = new Dictionary<String, MalVal>();
                assoc_BANG(lst.getValue().ToArray());
            }
            /*
            public MalHashMap(params MalVal[] mvs) {
                value = new Dictionary<String, MalVal>();
                assoc_BANG(mvs);
            }
            */
            public MalHashMap copy() {
                return (MalHashMap)this.MemberwiseClone();
            }

            public Dictionary<string, MalVal> getValue() { return value; }

            public override string ToString() {
                return "{" + printer.join(value, " ", true) + "}";
            }
            public override string ToString(bool print_readably) {
                return "{" + printer.join(value, " ", print_readably) + "}";
            }

            /*
            public Set _entries() {
                return value.entrySet();
            }
            */
            
            public MalHashMap assoc_BANG(params MalVal[] mvs) {
                for (int i=0; i<mvs.Length; i+=2) {
                    value.Add(((MalString)mvs[i]).getValue(), mvs[i+1]);
                }
                return this;
            }
        }

        public class MalFunction : MalVal {
            Func<MalList, MalVal> fn = null;
            MalVal ast = null;
            Mal.env.Env env = null;
            MalList fparams;
            bool macro = false;
            public MalFunction(Func<MalList, MalVal> fn) {
                this.fn = fn;
            }
            public MalFunction(MalVal ast, Mal.env.Env env, MalList fparams,
                               Func<MalList, MalVal> fn) {
                this.fn = fn;
                this.ast = ast;
                this.env = env;
                this.fparams = fparams;
            }

            public override string ToString() {
                if (ast != null) {
                    return "<fn* " + Mal.printer._pr_str(fparams,true) +
                           " " + Mal.printer._pr_str(ast, true) + ">";
                } else {
                    return "<builtin_function " + fn.ToString() + ">";
                }
            }

            public MalVal apply(MalList args) {
                return fn(args);
            }

            public MalVal getAst() { return ast; }
            public Mal.env.Env getEnv() { return env; }
            public MalList getFParams() { return fparams; }
            public Mal.env.Env genEnv(MalList args) {
                return new Mal.env.Env(env, fparams, args);
            }
            public bool isMacro() { return macro; }
            public void setMacro() { macro = true; }

        }
    }
}
