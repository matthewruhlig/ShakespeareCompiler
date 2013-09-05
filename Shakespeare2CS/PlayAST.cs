﻿using Irony.Ast;
using Irony.Interpreter;
using Irony.Parsing;
using Shakespeare.Text;
using Shakespeare.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shakespeare.Ast
{
    internal static class Constant
    {
        public static readonly int COMMENT_COLUMN = 40;
    }

    public class PlayNode : ShakespeareBaseAstNode
    {

        protected override object ReallyDoEvaluate(Irony.Interpreter.ScriptThread thread)
        {
            var sw = thread.tc().Writer;
            sw.WriteLine("/*********************************************************************");
            sw.WriteLine(" *   This C# program was generated by Shakespeare2CS by James Curran *");
            sw.WriteLine(" *   based on spl2c, the Shakespeare to C                            *");
            sw.WriteLine(" *          converter by Jon Åslund and Karl Hasselström.            *");
            sw.WriteLine(" *********************************************************************/");
            sw.WriteLine();
            sw.WriteLine("using System;");
            sw.WriteLine("using Shakespeare.Support;");
            sw.WriteLine();
            sw.WriteLine("namespace Shakespeare.Program");
            sw.WriteLine("{");
            sw.WriteLine("\tclass Program");
            sw.WriteLine("\t{");
            sw.WriteLine("\t\tstatic void Main(string[] args)");
            sw.WriteLine("\t\t{");
            sw.WriteLine("\t\t\tvar script = new Script();");
            sw.WriteLine("\t\t\tscript.Action();");
            sw.WriteLine("\t\t}");
            sw.WriteLine("\t}");
            sw.WriteLine();
            sw.WriteLine("\t\tclass Script : Dramaturge");
            sw.WriteLine("\t\t{");
            sw.WriteLine();
            sw.WriteLine("\t\tpublic Script()");
            sw.WriteLine("\t\t : base(Console.In, Console.Out)");
            sw.WriteLine("\t\t{ }");
            sw.WriteLine();
            sw.WriteLine("\t\tpublic void Action()");
            sw.WriteLine("\t\t{");

            AstNode1.Evaluate(thread);  // Title
            sw.WriteLine();
            var cdl = AstNode2 as CharacterDeclarationListNode;
            foreach (var ch in cdl.Characters)
                ch.Evaluate(thread);

            sw.WriteLine();
            AstNode3.Evaluate(thread);

            sw.WriteLine("\t\t}");
            sw.WriteLine("\t}");
            sw.WriteLine("}");
            sw.Flush();
            sw.Close();
            return sw;
        }

        public override string ToString()
        {
            return "Play";
        }
    }

    public class TitleNode : ShakespeareBaseAstNode
    {
        protected override object ReallyDoEvaluate(ScriptThread thread)
        {
            var tw = thread.tc().Writer;
            tw.WriteLine("/********************************************************************");
            tw.WriteLine("*");
            tw.WriteLine("{0,64}", Child1.Token.Text.ToUpper());
            tw.WriteLine("*");
            tw.WriteLine("*********************************************************************/");
            return this;
        }
    }

    public class CharacterDeclarationListNode : ListNode
    {
        public List<CharacterDeclarationNode> Characters { get; set; }
        public override void Init(AstContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            Characters = treeNode.ChildNodes.Select(cn => cn.AstNode as CharacterDeclarationNode).ToList();
        }
    }

    public class ActHeaderNode : ShakespeareBaseAstNode
    {
        string actnumber;

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            actnumber = String1.str2varname();
        }

        protected override object ReallyDoEvaluate(ScriptThread thread)
        {
            Context.CurrentAct = actnumber;

            var tw = thread.tc().Writer;
            tw.WriteLine();
            tw.Write((Context.CurrentAct+":").PadRight(Constant.COMMENT_COLUMN));
            tw.WriteLine(AstNode2);

            return this;
        }
    }

    public class EnterNode : ShakespeareBaseAstNode
    {
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            if (!Exist1)
            {
                context.AddMessage(Irony.ErrorLevel.Error, Location, @"""Enter"" missing character list");
                return;
            }
        }
        protected override object ReallyDoEvaluate(ScriptThread thread)
        {
            var tw = thread.tc().Writer;
            var cn = TreeNode.ChildNodes;
            if (Child1.AstNode is CharacterListNode)
                cn = Child1.ChildNodes;

            foreach (var ch in cn)
            {
                tw.WriteLine("\t\tEnterScene({0}, {1});", Location.Line, ch.AstNode.ToString().str2varname());
                Context.ActiveCharacters.Add(ch.AstNode as CharacterNode);
            }
            return base.ReallyDoEvaluate(thread);
        }
    }

    public class ExitNode : ShakespeareBaseAstNode
    {
        protected override object ReallyDoEvaluate(ScriptThread thread)
        {
            var charList = new List<CharacterNode>();
            var tw = thread.tc().Writer;
            if (Exist1)
            {
                if (AstNode1 is CharacterNode)
                    charList.Add(AstNode1 as CharacterNode);
                else
                    (AstNode1 as CharacterListNode).Fill(charList);

                foreach (var chr in charList)
                {
                    tw.WriteLine("\t\tExitScene({0}, {1});", Location.Line, chr);
                    Context.ActiveCharacters.Remove(chr);
                }
            }
            else
            {
                tw.WriteLine("\t\tExitSceneAll({0});", Location.Line);
                Context.ActiveCharacters.Clear();
            }

            return this;
        }
    }

    public class LineNode : ShakespeareBaseAstNode
    {
        protected override object ReallyDoEvaluate(ScriptThread thread)
        {
            var tw = thread.tc().Writer;
            tw.WriteLine();
            tw.WriteLine("\t\tActivate({0}, {1});", Location.Line, AstNode1);
            AstNode2.Evaluate(thread);
            return this;
        }
    }

    public class SentenceNode : ShakespeareBaseAstNode
    {
        protected override object ReallyDoEvaluate(ScriptThread thread)
        {
            if (AstNode1 is UnconditionalSentenceNode)
                AstNode1.Evaluate(thread);
            else
            {
                var tw = thread.tc().Writer;
                tw.WriteLine("\t\tif({0}) {{", AstNode1);
                AstNode2.Evaluate(thread);
                tw.WriteLine("\t\t}");
            }

            return base.ReallyDoEvaluate(thread);
        }
    }

    public class UnconditionalSentenceNode : SelfNode   {}

    public class InOutNode : ShakespeareBaseAstNode
    {
        protected override object ReallyDoEvaluate(ScriptThread thread)
        {
            var tw = thread.tc().Writer;
            if (AstNode1 is OpenYourNode)
            {
                if (String2 == "heart (Keyword)")
                {
                    tw.WriteLine("\t\tIntOutput({0});", Location.Line);
                }
                else  // Open Your Mind
                {
                    tw.WriteLine("\t\tCharInput({0});", Location.Line);
                }
            }
            else if (String1 == "speak")
            {
                tw.WriteLine("\t\tCharOutput({0});", Location.Line);
            }
            else if (String1 == "listen to")
            {
                    tw.WriteLine("\t\tIntInput({0});", Location.Line);
            }
            return base.ReallyDoEvaluate(thread);
        }
    }

    public class JumpNode : ShakespeareBaseAstNode
    {
        protected override object ReallyDoEvaluate(ScriptThread thread)
        {
            var tw = thread.tc().Writer;
            if (AstNode2 is SceneRomanNode)
                tw.WriteLine("\t\tgoto {0}_{1};", Context.CurrentAct, AstNode2.ToString().str2varname());
            else
                tw.WriteLine("\t\tgoto {0};", AstNode2.ToString().str2varname());

            return this;
        }

    }

    public class QuestionNode : ShakespeareBaseAstNode
    {
        protected override object ReallyDoEvaluate(ScriptThread thread)
        {
            var tw = thread.tc().Writer;
            tw.WriteLine("\t\tComp1 = {0};", AstNode2);
            tw.WriteLine("\t\tComp2 = {0};", AstNode4);
            tw.WriteLine("\t\tTruthFlag = {0};", AstNode3);

            return base.ReallyDoEvaluate(thread);
        }
    }

    public class RecallNode : ShakespeareBaseAstNode
    {
        protected override object ReallyDoEvaluate(ScriptThread thread)
        {
            var tw = thread.tc().Writer;
            tw.WriteLine("\t\tPop({0});", Location.Line);
            return this;
        }
    }

    public class RememberNode : ShakespeareBaseAstNode
    {
        protected override object ReallyDoEvaluate(ScriptThread thread)
        {
            var tw = thread.tc().Writer;
            tw.WriteLine("\t\tPush({0}, {1});", Location.Line, AstNode2);

            return this;
        }
    }

    public class StatementNode : ShakespeareBaseAstNode
    {
        protected override object ReallyDoEvaluate(ScriptThread thread)
        {
            if (AstNode1 is SecondPersonNode)
            {
                var tw = thread.tc().Writer;
                if (AstNode2 is BeNode)
                {
                    if (AstNode3 is ConstantNode)
                        tw.WriteLine("\t\tAssign({0}, {1});", Location.Line, AstNode3.Evaluate(thread) as string);
                    else    // SECOND_PERSON BE Equality Value StatementSymbol 
                        tw.WriteLine("\t\tAssign({0}, {1});", Location.Line, AstNode4.Evaluate(thread) as string);
  
                }
                else if (AstNode2 is UnarticulatedConstantNode)
                {
                    tw.WriteLine("\t\tAssign({0}, {1});", Location.Line,  AstNode2.Evaluate(thread) as string);

                }
            }

            return base.ReallyDoEvaluate(thread);
        }

    }

    public class ConstantNode : ShakespeareBaseAstNode
    {
        protected override object ReallyDoEvaluate(ScriptThread thread)
        {
            if (AstNode1 is NothingNode)
                return "0";
            else  // astNode1 is Article, FirstPErson, SecondPerson, thirdPerson
                return AstNode2.Evaluate(thread);
        }
    }

    public class SceneHeaderNode : ShakespeareBaseAstNode
    {
        string scenenumber;

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            scenenumber = String1.str2varname();
        }

        protected override object ReallyDoEvaluate(ScriptThread thread)
        {
            var tw = thread.tc().Writer;
            Context.CurrentScene = Context.CurrentAct + "_" + scenenumber;
            tw.WriteLine("{0}{1}", (Context.CurrentScene + ":").PadRight(Constant.COMMENT_COLUMN), AstNode2);
            return this;
        }
    }

    public class NegativeConstantNode : ShakespeareBaseAstNode
    {
        protected override object ReallyDoEvaluate(ScriptThread thread)
        {
            if (AstNode1 is NegativeNounNode)
                return "(-1)";
            else // astnode1 is NegativeAdjective or astnode1 is neutralAdjective
                return string.Format("2*{0}", AstNode2.Evaluate(thread) as string);
        }
    }

    public class NonnegatedComparisonNode : ShakespeareBaseAstNode
    {
        protected override object ReallyDoEvaluate(ScriptThread thread)
        {
            return AstNode1.Evaluate(thread);
        }
    }



    public class CharacterDeclarationNode : ShakespeareBaseAstNode 
    {
        public string Declaration { get; set; }

        protected override object ReallyDoEvaluate(ScriptThread thread)
        {
            var tw = thread.tc().Writer;
            tw.WriteLine("\t\tCharacter\t{0} = InitializeCharacter({2},\"{0}\");\t\t{1}", AstNode1, AstNode2, Location.Line);
            return this;
        }
    }

    public class CommentNode :ShakespeareBaseAstNode
    {
        public override string ToString()
        {
            return string.Format("/* {0} */", TreeNode.Token.Text);
        }
    }

    public class ComparisonNode : ShakespeareBaseAstNode 
    {
        protected override object ReallyDoEvaluate(ScriptThread thread)
        {
            if (AstNode1 is NonnegatedComparisonNode)
                return AstNode1.Evaluate(thread);
            else
                return string.Format("!{0}", AstNode2.Evaluate(thread) as string);
        }
    }

    public class ConditionalNode :ShakespeareBaseAstNode
    {
        protected override object ReallyDoEvaluate(ScriptThread thread)
        {
            if (Child1.Term.Name == "if so")
                return "TruthFlag";
            else
                return "!TruthFlag";   // if not
        }
    }

    public class ValueNode : ShakespeareBaseAstNode
    {
        protected override object ReallyDoEvaluate(ScriptThread thread)
        {
            var tw = thread.tc().Writer;
            if (AstNode1 is CharacterNode)
				return (AstNode1.Evaluate(thread) as string) +".Value";
            else if (AstNode1 is ConstantNode)
                return AstNode1.Evaluate(thread);
            else if (AstNode1 is PronounNode)
            {
                return String.Format("ValueOf({0},{1})", Location.Line, AstNode1.Evaluate(thread) as string);
            }
            else if (AstNode1 is BinaryOperatorNode)
            {
                return string.Format(AstNode1.ToString(),Location.Line,  AstNode2.Evaluate(thread) as string, AstNode3.Evaluate(thread) as string);
            }
            else if (AstNode1 is UnaryOperatorNode)
            {
                AstNode1.Evaluate(thread);
                return string.Format((AstNode1 as UnaryOperatorNode).FormatString, AstNode2.Evaluate(thread) as string);
            }
            else
            {
                // error
            }
            return this;
        }

    }

    public class PronounNode : ShakespeareBaseAstNode
    {
        protected override object ReallyDoEvaluate(ScriptThread thread)
        {
            if (AstNode1 is FirstPersonNode || AstNode1 is FirstPersonReflexiveNode)
                return "FirstPerson";
            else if (AstNode1 is SecondPersonNode || AstNode1 is SecondPersonReflexiveNode)
                return "SecondPerson";
            return "ERROR";
        }
        
    }

    public class PositiveConstantNode : ShakespeareBaseAstNode
    {
        protected override object ReallyDoEvaluate(ScriptThread thread)
        {
            if (AstNode1 is PositiveNounNode)
                return ("1");
            else
                return string.Format("2*{0}", AstNode2.Evaluate(thread) as string);
        }
    }

    public class EqualityNode : ShakespeareBaseAstNode
    {
        protected override object ReallyDoEvaluate(ScriptThread thread)
        {
            return "Comp1 == Comp2";
        }
    }

    public class ComparativeNode : ShakespeareBaseAstNode
    {
        protected override object ReallyDoEvaluate(ScriptThread thread)
        {
            if (AstNode1 is NegativeComparativeNode)
                return ("Comp1 < Comp2");
            else  // PositiveComparativeNode
                return ("Comp1 > Comp2");
        }
    }

    public class NegativeComparativeNode : ShakespeareBaseAstNode
    {
        protected override object ReallyDoEvaluate(ScriptThread thread)
        {
            if (AstNode1 is NegativeComparativeTermNode)
                return AstNode1.Evaluate(thread);
            else
                return AstNode2.Evaluate(thread);
        }
    }

    public class PositiveComparativeNode : ShakespeareBaseAstNode
    {
        protected override object ReallyDoEvaluate(ScriptThread thread)
        {
            var term = AstNode1.Term;
            var strTerm = term != null ? term.ToString() : null;
            if (strTerm == "more" || strTerm == "less")
            {
                var st1 = AstNode1.Evaluate(thread) as string;
                var st2 = AstNode2.Evaluate(thread) as string;
				return st1 + ' ' + st2;
            }
            else
                return AstNode1.Evaluate(thread);
        }
    }

    public class BinaryOperatorNode : ShakespeareBaseAstNode
    {
        string format;

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var term = Child1.Term.Name;
            if (term == "the difference between")
                format = "(({1})-({2}))";
            else if (term == "the product of")
                format = "(({1})*({2}))";
            else if (term == "the quotient between")
                format = "(({1})/({2}))";
            else if (term == "the remainder of the quotient between")
                format = "(({1})%({2}))";
            else if (term == "the sum of")
                format = "(({1})+({2}))";
        }

        protected override object ReallyDoEvaluate(ScriptThread thread)
        {
            var tw = thread.tc().Writer;
            tw.Write(format, Location.Line, AstNode2, AstNode3);
            return this;
        }

        public override string ToString()
        {
            return format;
        }
    }

    public class UnaryOperatorNode : ShakespeareBaseAstNode
    {
        static readonly Dictionary<string, string> functionMap = new Dictionary<string, string>
        {
                {"the cube of", "Cube"},
                {"the factorial of", "Factorial"},
                {"the square of", "Square"},
                {"the square root of", "Sqrt"},
                {"twice", "Twice"},
        };

        public string FormatString { get; set; }

        protected override object ReallyDoEvaluate(ScriptThread thread)
        {
            FormatString = string.Format("{0}({1},{{0}})", functionMap[String1], Location.Line);
            return this;
        }
    }



}

