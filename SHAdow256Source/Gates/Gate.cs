using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gates
{
    /// <summary>
    /// Represents a logic gate
    /// </summary>
    public class Gate
    {
        /// <summary>
        /// Operation type enum - describes gate behavior
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// Variable. Doesn't have inputs
            /// </summary>
            Var,
            /// <summary>
            /// Constant. Doesn't have inputs, but has constant value assigned
            /// </summary>
            Const,
            /// <summary>
            /// Conjunction
            /// </summary>
            And,
            /// <summary>
            /// Disjunction
            /// </summary>
            Or,
            /// <summary>
            /// Exclusive or
            /// </summary>
            Xor,
            /// <summary>
            /// Negation
            /// </summary>
            Not,
        }

        /// <summary>
        /// Name for easy recognition. It can be set by end-user
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gate type, on which depends its behavior
        /// </summary>
        public Type GateType { get; set; }

        /// <summary>
        /// Gate input "children" gates.
        /// </summary>
        public Gate[] Inputs = new Gate[] { };

        /// <summary>
        /// Unique gate id number
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Unique variable id number (only for Var type of gates)
        /// </summary>
        public int VarId { get; private set; }

        /// <summary>
        /// Current value assigned to gate. Obligatory for Const gates
        /// </summary>
        public bool? Value { get; set; }

        private static int nextId = 0;
        private static int nextVarId = 1;

        private static readonly Dictionary<Type, Func<bool, bool, bool>> gateOperations = new Dictionary<Type, Func<bool, bool, bool>>
        {
            {Type.And, (x,y)=>x&y },
            {Type.Or, (x,y)=>x|y },
            {Type.Xor, (x,y)=>x^y },
            {Type.Not, (x,y)=>!x },
        };

        private Gate()
        {
            Id = nextId;
            nextId++;

            VarId = -1;
            Name = "Unnamed";
        }

        /// <summary>
        /// Creates constant
        /// </summary>
        /// <param name="value">Constant value</param>
        /// <returns></returns>
        public static Gate CreateConst(bool value)
        {
            var gate = new Gate();
            gate.GateType = Type.Const;
            gate.Value = value;
            gate.Name = "Const";

            return gate;
        }

        /// <summary>
        /// Creates variable
        /// </summary>
        /// <param name="name">Optional name of variable</param>
        /// <returns></returns>
        public static Gate CreateVar(string name = null)
        {
            var gate = new Gate();
            gate.GateType = Type.Var;
            gate.VarId = nextVarId;
            nextVarId++;

            if (name != null)
            {
                gate.Name = name;
            }
            else
            {
                gate.Name = "Var" + gate.VarId;
            }

            return gate;
        }

        /// <summary>
        /// Creates AND gate (conjunction)
        /// </summary>
        /// <param name="a">First input</param>
        /// <param name="b">Second input</param>
        /// <returns></returns>
        public static Gate CreateAnd(Gate a, Gate b)
        {
            var gate = new Gate();
            gate.GateType = Type.And;
            gate.Name = "And";
            gate.Inputs = new[] { a, b };

            return gate;
        }

        /// <summary>
        /// Creates OR gate (disjunction)
        /// </summary>
        /// <param name="a">First input</param>
        /// <param name="b">Second input</param>
        /// <returns></returns>
        public static Gate CreateOr(Gate a, Gate b)
        {
            var gate = new Gate();
            gate.GateType = Type.Or;
            gate.Name = "Or";
            gate.Inputs = new[] { a, b };

            return gate;
        }

        /// <summary>
        /// Creates XOR gate (exclusive or)
        /// </summary>
        /// <param name="a">First input</param>
        /// <param name="b">Second input</param>
        /// <returns></returns>
        public static Gate CreateXor(Gate a, Gate b)
        {
            var gate = new Gate();
            gate.GateType = Type.Xor;
            gate.Name = "Xor";
            gate.Inputs = new[] { a, b };

            return gate;
        }

        /// <summary>
        /// Creates NOT gate (negation)
        /// </summary>
        /// <param name="a">Input</param>
        /// <returns></returns>
        public static Gate CreateNot(Gate a)
        {
            var gate = new Gate();
            gate.GateType = Type.Not;
            gate.Name = "Not";
            gate.Inputs = new[] { a };

            return gate;
        }

        public static implicit operator Gate(bool x)
        {
            return CreateConst(x);
        }

        public static Gate operator &(Gate x, Gate y)
        {
            return CreateAnd(x, y);
        }

        public static Gate operator |(Gate x, Gate y)
        {
            return CreateOr(x, y);
        }

        public static Gate operator ^(Gate x, Gate y)
        {
            return CreateXor(x, y);
        }

        public static Gate operator !(Gate x)
        {
            return CreateNot(x);
        }

        /// <summary>
        /// Evaluates value for this gate, based on values on depending gates (children gates).
        /// If returns null it means evaluation cannot be performed, because input values are incomplete.
        /// </summary>
        /// <returns></returns>
        public bool? Evaluate()
        {
            var expanded = Expand();
            var values = new Dictionary<Gate, bool>();
            expanded.Where(x => x.Value != null).ToList().ForEach(x => values.Add(x, (bool)x.Value));

            while(!values.ContainsKey(this))
            {
                var computable = expanded.FirstOrDefault(x => x.Inputs.Length > 0 && !values.ContainsKey(x) && x.Inputs.All(o => values.ContainsKey(o)));
                if(computable == null)
                {
                    break;
                }
                var operation = gateOperations[computable.GateType];
                var inputValues = computable.Inputs.Select(x => values[x]).ToList();
                var result = operation(inputValues[0], inputValues.Count >= 2 ? inputValues[1] : false);
                values.Add(computable, result);
            }

            if(values.ContainsKey(this))
            {
                return values[this];
            }
            return null;
        }

        /// <summary>
        /// Expands this gate into set containing itself, and all of its subgates
        /// </summary>
        /// <returns></returns>
        public HashSet<Gate> Expand()
        {
            var set = new HashSet<Gate>();
            var stack = new Stack<Gate>(new[] { this });
            while(stack.Count > 0)
            {
                var p = stack.Pop();
                if(!set.Contains(p))
                {
                    set.Add(p);
                    foreach(var input in p.Inputs)
                    {
                        stack.Push(input);
                    }
                }
            }

            return set;
        }

        /// <summary>
        /// Checks if values on input gates gives right value on this gate.
        /// If any value on input is null, or this gate value is null it returns null.
        /// If values assigned gives assigned output value according to gate type it returns true, false otherwise.
        /// </summary>
        /// <returns></returns>
        public bool? Check()
        {
            if(Value == null)
            {
                return null;
            }
            if(Inputs.Any(x=>x.Value == null))
            {
                return null;
            }
            if(gateOperations.ContainsKey(GateType))
            {
                var inputValues = Inputs.Select(x => (bool)x.Value).ToList();
                var operation = gateOperations[GateType];
                var shouldBe = operation(inputValues[0], inputValues.Count >= 2 ? inputValues[1] : false);
                if(shouldBe != Value)
                {
                    return false;
                }
            }
            return true;
        }

        public override string ToString()
        {
            var inputs = ", Inputs IDs: " + string.Join(",", Inputs.Select(x => x.Id));            
            if(Inputs.Length == 0)
            {
                inputs = "";
            }
            if(GateType == Type.Var)
            {
                return string.Format("{0} GateID:{1} VarID:{4} Value:{2} Name: {3}{5}", GateType, Id, Value ?? (object)"null", Name ?? "", VarId, inputs);
            }
            return string.Format("{0} GateID:{1} Value:{2} Name: {3}{4}", GateType, Id, Value ?? (object)"null", Name ?? "", inputs);
        }
    }
}
