﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Interpreter.Scripts;

namespace Interpreter.ASTNodes
{
    public abstract class AstNode
    {
        public int Line;
    }
    public class ProgramNode
    {
        public List<AstNode> Statements { get; set; } = new List<AstNode>();
    }

    public class SpawnCommand : AstNode
    {
        public AstNode X { get; }
        public AstNode Y { get; }
        public SpawnCommand(AstNode x, AstNode y, int line) { X = x; Y = y; Line = line; }
    }
    public class ReSpawnCommand : AstNode
    {
        public AstNode X { get; }
        public AstNode Y { get; }
        public ReSpawnCommand(AstNode x, AstNode y, int line) { X = x; Y = y; Line = line; }
    }
    public class ColorCommand : AstNode
    {
        public AstNode Color { get; }
        public ColorCommand(AstNode color, int line) { Color = color; Line = line; }
    }

    public class SizeCommand : AstNode
    {
        public AstNode Size { get; }
        public SizeCommand(AstNode size, int line) { Size = size; Line = line; }
    }

    public class DrawLineCommand : AstNode
    {
        public AstNode DirX { get; }
        public AstNode DirY { get; }
        public AstNode Distance { get; }
        public DrawLineCommand(AstNode dirX, AstNode dirY, AstNode distance, int line)
        {
            DirX = dirX;
            DirY = dirY;
            Distance = distance;
            Line = line;
        }
    }

    public class DrawCircleCommand : AstNode
    {
        public AstNode DirX { get; }
        public AstNode DirY { get; }
        public AstNode Radius { get; }
        public DrawCircleCommand(AstNode dirX, AstNode dirY, AstNode radius, int line)
        {
            DirX = dirX;
            DirY = dirY;
            Radius = radius;
            Line = line;
        }
    }

    public class DrawRectangleCommand : AstNode
    {
        public AstNode DirX { get; }
        public AstNode DirY { get; }
        public AstNode Distance { get; }
        public AstNode Width { get; }
        public AstNode Height { get; }
        public DrawRectangleCommand(AstNode dirX, AstNode dirY, AstNode distance, AstNode width, AstNode height, int line)
        {
            DirX = dirX;
            DirY = dirY;
            Distance = distance;
            Width = width;
            Height = height;
            Line = line;
        }
    }

    public class FillCommand : AstNode
    {
        public FillCommand(int line) { Line = line; }
    }

    public class AssignmentStatement : AstNode
    {
        public string VariableName { get; }
        public AstNode Expression { get; }
        public AssignmentStatement(string varName, AstNode expr, int line) { VariableName = varName; Expression = expr; Line = line; }
    }

    public class LabelStatement : AstNode
    {
        public string LabelName { get; }
        public LabelStatement(string labelName, int line) { LabelName = labelName; Line = line; }
    }

    public class GoToStatement : AstNode
    {
        public string LabelName { get; }
        public AstNode Condition { get; }
        public GoToStatement(string labelName, AstNode condition, int line) { LabelName = labelName; Condition = condition; Line = line; }
    }

    // Expressions
    public abstract class Expression : AstNode
    {
        public ExpressionType Type;
        public abstract bool CheckSemantic();
    }

    public class LiteralExpression<T> : Expression
    {
        public T Value { get; }
        public override bool CheckSemantic() => true;
        public LiteralExpression(T value, int line)
        {
            Value = value;
            Line = line;
            if (typeof(T) == typeof(int) || typeof(T) == typeof(double)) { Type = ExpressionType.Numeric; }
            else
            if (typeof(T) == typeof(bool)) { Type = ExpressionType.Boolean; }
            else
            if (typeof(T) == typeof(string)) { Type = ExpressionType.String; } else { Type = ExpressionType.Error; }
        }
    }

    public class VariableExpression : Expression
    {
        public string VariableName { get; }
        public override bool CheckSemantic()
        {
            bool check = LocalMemory._variables.TryGetValue(VariableName, out object value);

            if (check)
            {
                bool BoolParse = true;
                bool IntParse = true;
                bool StrParse = true;

                try
                {
                    bool val = (bool)value;
                }
                catch
                {
                    BoolParse = false;
                }

                try
                {
                    int val = (int)value;
                }
                catch
                {
                    IntParse = false;
                }

                try
                {
                    string val = (string)value;
                }
                catch
                {
                    StrParse = false;
                }

                if (BoolParse) { Type = ExpressionType.Boolean; }
                else
                if (IntParse) { Type = ExpressionType.Numeric; }
                else
                if (StrParse) { Type = ExpressionType.String; } else { Type = ExpressionType.Error; }
            }
            else
            {
                Type = ExpressionType.Error;
            }
            return check;
        }
        public VariableExpression(string varName, int line) { VariableName = varName; Line = line; }
    }

    public abstract class Operation : Expression { }

    public abstract class BinaryOp : Operation {
        public Expression Left { get; }
        public string Operator { get; }
        public Expression Right { get; }
        public override bool CheckSemantic()
        {
            bool checkLeftExpr = Left.CheckSemantic();
            bool checkRightExpr = Right.CheckSemantic();
            bool BinAOp = (Left.Type == ExpressionType.Numeric && Right.Type == ExpressionType.Numeric);

            if (!(checkLeftExpr && checkRightExpr && BinAOp)) { Type = ExpressionType.Error; }

            return checkLeftExpr && checkRightExpr && BinAOp;
        }
        public BinaryOp(Expression left, string op, Expression right, int line)
        {
            Left = left;
            Right = right;
            Operator = op;
            Line = line;
        }
    }
    public class BinAritExpression : BinaryOp
    {
        public BinAritExpression(Expression left, string op, Expression right, int line) : base(left, op, right, line)
        {
            Type = ExpressionType.Numeric;
        }
    }
    public class BinCompExpression : BinaryOp
    {
        public BinCompExpression(Expression left, string op, Expression right, int line) : base(left, op, right, line)
        {
            Type = ExpressionType.Numeric;
        }
    }
    public class BinEqualOpExpression : BinaryOp
    {
        public override bool CheckSemantic()
        {
            bool checkLeftExpr = Left.CheckSemantic();
            bool checkRightExpr = Right.CheckSemantic();
            bool BinAOp = Left.Type == Right.Type && Left.Type != ExpressionType.Error;

            if (checkLeftExpr && checkRightExpr && BinAOp) { Type = ExpressionType.Boolean; } else { Type = ExpressionType.Error; }

            return checkLeftExpr && checkRightExpr && BinAOp;
        }
        public BinEqualOpExpression(Expression left, string op, Expression right, int line) : base(left, op, right, line) { }
    }

    public abstract class UnaryOp : Operation
    {
        public Expression Right { get; }
        public string Operator { get; }
        public abstract override bool CheckSemantic();
        public UnaryOp(Expression right, string op, int line)
        {
            Right = right;
            Operator = op;
            Line = line;
        }
    }

    public class UnaryAritmeticOpExpression : UnaryOp
    { 

        public override bool CheckSemantic()
        {
            bool checkExpr = Right.CheckSemantic();
            bool UnaryAOp = Right.Type == ExpressionType.Numeric;
            Type = ExpressionType.Numeric;
            return checkExpr && UnaryAOp;
        }
        public UnaryAritmeticOpExpression(Expression right, string op, int line) : base(right, op, line) { }
    }

    public class UnaryBooleanOpExpression : UnaryOp
    {
        public override bool CheckSemantic()
        {
            bool checkExpr = Right.CheckSemantic();
            bool UnaryAOp = Right.Type == ExpressionType.Boolean;
            Type = ExpressionType.Boolean;
            return checkExpr && UnaryAOp;
        }
        public UnaryBooleanOpExpression(Expression right, string op, int line) : base(right, op, line) { }
    }

    public class FunctionCallExpression : Expression
    {
        public string FunctionName { get; }
        public List<AstNode> Arguments { get; }
        public override bool CheckSemantic() => true;
        public FunctionCallExpression(string functionName, List<AstNode> args, int line) { FunctionName = functionName; Arguments = args; Type = ExpressionType.Numeric; Line = line; }
    }

    public enum ExpressionType
    {
        Numeric,
        Boolean,
        String,
        Error
    }

}
