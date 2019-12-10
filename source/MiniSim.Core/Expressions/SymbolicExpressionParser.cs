using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniSim.Core.Expressions;
using Sprache;

namespace MiniSim.Core.Expressions
{
    public class SymbolicExpressionParser
    {
        private Dictionary<string, Variable> _knownVariables = new Dictionary<string, Variable>();

        public Expression ParseExpression(string text)
        {
            return Lambda.Parse(text);
        }
        public Equation ParseEquation(string text)
        {
            return Equation.Parse(text);
        }

        public void ClearVariables()
        {
            _knownVariables.Clear();
        }
        public void RegisterVariable(Variable variable)
        {
            RegisterVariable(variable.Name, variable);
        }


        public void RegisterVariable(string id, Variable variable)
        {
            if (!_knownVariables.ContainsKey(id))
                _knownVariables.Add(id, variable);
            else
            {
                _knownVariables[id] = variable;
            }
        }

        private Variable FindVariable(string id)
        {
            if (_knownVariables.ContainsKey(id))
                return _knownVariables[id];
            throw new ParseException("Variable " + id + " not defined.");
        }


        static Parser<Expression> Operator(string op, Func<Expression> opType)
        {
            return Parse.String(op).Token().Return(opType());
        }


        readonly Parser<Expression> Add = Operator("+", () => new Expression("+"));
        readonly Parser<Expression> Subtract = Operator("-", () => new Expression("-"));
        private readonly Parser<Expression> Multiply = Operator("*", () => new Expression("*"));
        readonly Parser<Expression> Divide = Operator("/", () => new Expression("/"));
        readonly Parser<Expression> Power = Operator("^", () => new Expression("^"));

        readonly Parser<Expression> Constant;

        private readonly Parser<string> Identifier;

        private readonly Parser<Expression> Variable;

        private readonly Parser<Expression> Factor;
        //.XOr(Function);

        readonly Parser<Expression> Operand;

        readonly Parser<Expression> InnerTerm;

        readonly Parser<Expression> Term;

        readonly Parser<Expression> Expr;

        private readonly Parser<Expression> Lambda;

        private readonly Parser<Equation> Equation;

        private readonly Parser<Expression> Function;
        private readonly Parser<Expression> Atomic;


        Expression MakeBinary(Expression parent, Expression left, Expression right)
        {
            switch (parent.Name)
            {
                case "+":
                    return left + right;
                case "/":
                    return left / right;
                case "*":
                    return left * right;
                case "-":
                    return left - right;
                case "^":
                    return Sym.Pow(left, right);
            }
            throw new ArgumentException("Unsupported expression");
        }

        Expression MakeUnaryFunction(string name, Expression parameter)
        {
            switch (name)
            {
                case "exp":
                    return Sym.Exp(parameter);
            }

            throw new ArgumentException("Unsupported function");
        }

        public SymbolicExpressionParser()
        {
            Add = Operator("+", () => new Expression("+"));
            Subtract = Operator("-", () => new Expression("-"));
            Multiply = Operator("*", () => new Expression("*"));
            Divide = Operator("/", () => new Expression("/"));
            Power = Operator("^", () => new Expression("^"));

            Constant =
                  Parse.DecimalInvariant
                  .Select(x => new Variable("C", double.Parse(x, NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture)))
                  .Named("number");

            Identifier =
                   from startLetter in Parse.Letter.AtLeastOnce().Text()
                   from rest in Parse.LetterOrDigit.Many().Text()
                   select startLetter + rest;

            Variable =
                   from id in Identifier
                   from n in Parse.Not(Parse.Char('('))
                   select FindVariable(id);

            Function =
            from name in Parse.Letter.AtLeastOnce().Text()
            from lparen in Parse.Char('(')
            from expr in Expr
            from rparen in Parse.Char(')')
            select MakeUnaryFunction(name, expr);



            Factor =

                 (from lparen in Parse.Char('(')
                  from expr in Parse.Ref(() => Expr)
                  from rparen in Parse.Char(')')
                  select Sym.Par(expr)).Named("expression")
                     .XOr(Constant)
                     .XOr(Variable);

            Atomic = Factor.Or(Function);

            Operand =
                ((from sign in Parse.Char('-')
                  from factor in Atomic
                  select -(factor)
                 ).XOr(Atomic)).Token();

            InnerTerm = Parse.ChainOperator(Power, Operand, MakeBinary);

            Term = Parse.ChainOperator(Multiply.Or(Divide), InnerTerm, MakeBinary);

            Expr = Parse.ChainOperator(Add.Or(Subtract), Term, MakeBinary);

            Lambda =
               Expr.End().Select(body => body);

            Equation =
                from left in Parse.Ref(() => Expr)
                from _ in Parse.Char('=')
                from right in Parse.Ref(() => Expr)
                select new Equation(left - right);
        }
    }
}
