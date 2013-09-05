﻿using Irony.Ast;
using Irony.Interpreter;
using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shakespeare
{
    public interface IShakespeareCompiler
    {
        void PrepareScope(ScriptThread thread, object param);
        AstNodeCreator ActHeaderNode { get; }
        AstNodeCreator BinaryOperatorNode { get; }
        AstNodeCreator CharacterDeclarationListNode { get; }
        AstNodeCreator CharacterDeclarationNode  { get; }
        AstNodeCreator CommentNode  { get; }
        AstNodeCreator ComparativeNode  { get; }
        AstNodeCreator ComparisonNode  { get; }
        AstNodeCreator ConditionalNode  { get; }
        AstNodeCreator ConstantNode  { get; }
        AstNodeCreator EnterNode  { get; }
        AstNodeCreator EqualityNode  { get; }
        AstNodeCreator ExitNode  { get; }
        AstNodeCreator InOutNode  { get; }
        AstNodeCreator JumpNode  { get; }
        AstNodeCreator LineNode  { get; }
        AstNodeCreator NegativeComparativeNode  { get; }
        AstNodeCreator NegativeConstantNode  { get; }
        AstNodeCreator NonnegatedComparisonNode  { get; }
        AstNodeCreator PlayNode  { get; }
        AstNodeCreator PositiveComparativeNode  { get; }
        AstNodeCreator PositiveConstantNode  { get; }
        AstNodeCreator PronounNode  { get; }
        AstNodeCreator QuestionNode  { get; }
        AstNodeCreator RecallNode  { get; }
        AstNodeCreator RememberNode  { get; }
        AstNodeCreator SceneHeaderNode  { get; }
        AstNodeCreator SentenceNode  { get; }
        AstNodeCreator StatementNode  { get; }
        AstNodeCreator TitleNode  { get; }
        AstNodeCreator UnaryOperatorNode  { get; }
        AstNodeCreator UnconditionalSentenceNode  { get; }
        AstNodeCreator ValueNode  { get; }
    }
                                                                   
}
