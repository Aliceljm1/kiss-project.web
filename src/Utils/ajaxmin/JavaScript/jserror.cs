// jserror.cs
//
// Copyright 2010 Microsoft Corporation
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Kiss.Web.Utils.ajaxmin
{

    public enum JSError
    {
        //0 - 1000 legacy scripting errors, not JScript specific.
        NoError = 0,

        //1000 - 2000 JScript errors that occur during compilation only. (regard Eval and Function as compilation). Typically used only in HandleError.
        SyntaxError = 1002, // "Syntax error"
        NoColon = 1003, // "Expected ':'"
        NoSemicolon = 1004, // "Expected ';'"
        NoLeftParenthesis = 1005, // "Expected '('"
        NoRightParenthesis = 1006, // "Expected ')'"
        NoRightBracket = 1007, // "Expected ']'"
        NoLeftCurly = 1008, // "Expected '{'"
        NoRightCurly = 1009, // "Expected '}'"
        NoIdentifier = 1010, // "Expected identifier"
        NoEqual = 1011, // "Expected '='"
        IllegalChar = 1014, // "Invalid character"
        UnterminatedString = 1015, // "Unterminated string constant"
        NoCommentEnd = 1016, // "Unterminated comment"
        BadReturn = 1018, // "'return' statement outside of function"
        BadBreak = 1019, // "Can't have 'break' outside of loop"
        BadContinue = 1020, // "Can't have 'continue' outside of loop"
        BadHexEscapeSequence = 1023, // "Expected hexadecimal digit"
        NoWhile = 1024, // "Expected 'while'"
        BadLabel = 1025, // "Label redefined"
        NoLabel = 1026, // "Label not found"
        DupDefault = 1027, // "'default' can only appear once in a 'switch' statement"
        NoMemberIdentifier = 1028, // "Expected identifier or string"
        NoCCEnd = 1029, // "Expected '@end'"
        CCOff = 1030, // "Conditional compilation is turned off"
        NoCatch = 1033, // "Expected 'catch'"
        InvalidElse = 1034, // "Unmatched 'else'; no 'if' defined"
        NoComma = 1100, // "Expected ','"
        BadSwitch = 1103, // "Missing 'case' or 'default' statement"
        CCInvalidEnd = 1104, // "Unmatched '@end'; no '@if' defined"
        CCInvalidElse = 1105, // "Unmatched '@else'; no '@if' defined"
        CCInvalidElseIf = 1106, // "Unmatched '@elif'; no '@if' defined"
        ErrorEndOfFile = 1107, // "Expecting more source characters"
        DuplicateName = 1111, // "Identifier already in use"
        UndeclaredVariable = 1135, // "Variable has not been declared"
        KeywordUsedAsIdentifier = 1137, // "'xxxx' is a new reserved word and should not be used as an identifier"
        UndeclaredFunction = 1138, // "Function has not been declared"
        NoCommaOrTypeDefinitionError = 1191, // "Expected ',' or illegal type declaration, write '<Identifier> : <Type>' not '<Type> <Identifier>'"
        NoRightParenthesisOrComma = 1193, // "Expected ',' or ')'"
        NoRightBracketOrComma = 1194, // "Expected ',' or ']'"
        ExpressionExpected = 1195, // "Expected expression"
        UnexpectedSemicolon = 1196, // "Unexpected ';'"
        TooManyTokensSkipped = 1197, // "Too many tokens have been skipped in the process of recovering from errors. The file may not be a JScript.NET file"
        SuspectAssignment = 1206, //"Did you intend to write an assignment here?"
        SuspectSemicolon = 1207, //"Did you intend to have an empty statement for this branch of the if statement?"
        ParameterListNotLast = 1240, //"A variable argument list must be the last argument
        StatementBlockExpected = 1267, //"A statement block is expected"
        VariableDefinedNotReferenced = 1268, //"A variabled was defined but not set or referenced"
        ArgumentNotReferenced = 1270, //"Argument was defined but not referenced"
        WithNotRecommended = 1271, //"With statement is not recommended"
        FunctionNotReferenced = 1272, //"A function was defined but not referenced"
        AmbiguousCatchVar = 1273, //"Catch identifiers should be unique"
        FunctionExpressionExpected = 1274, //"Function expression expected"
        ObjectConstructorTakesNoArguments = 1275, //"Object constructor takes no arguments"
        JSParserException = 1276, // "JSParser Exception"
        NumericOverflow = 1277, // "Numeric literal causes overflow or underflow exception"
        NumericMaximum = 1278, // "Consider replacing maximum numeric literal with Number.MAX_VALUE"
        NumericMinimum = 1279, // "Consider replacing minimum numeric literal with Number.MIN_VALUE"
        ResourceReferenceMustBeConstant = 1280, // "Resource reference must be single constant value"
        AmbiguousNamedFunctionExpression = 1281, // "Ambiguous named function expression"
        ConditionalCompilationTooComplex = 1282, // "Conditiona compilation structure too complex"
		UnterminatedAspNetBlock = 1283, // Unterminated asp.net block.
        MisplacedFunctionDeclaration = 1284, // function declaration other than where SourceElements are expected
        OctalLiteralsDeprecated = 1285, // octal literal encountered; possible cross-browser issues
        FunctionNameMustBeIdentifier = 1286, // function names must be a single identifier
        StrictComparisonIsAlwaysTrueOrFalse = 1287, // a strict comparison is always true or false
        StrictModeNoWith = 1288, // strict mode does not allow with-statements
        StrictModeDuplicateArgument = 1289, // strict mode does not allow duplicate argument names
        StrictModeVariableName = 1290, // strict mode does not allow certain variable names
        StrictModeFunctionName = 1291, // strict mode does not allow certain function names
        StrictModeDuplicateProperty = 1292, // strict mode does not allow duplicate property names
        StrictModeInvalidAssign = 1293, // strict mode does not allow assign operator to this variable
        StrictModeInvalidPreOrPost = 1294, // strict mode does not allow prefix or postfix operators on certain references
        StrictModeInvalidDelete = 1295, // strict mode does not allow certain delete operands
        StrictModeArgumentName = 1296, // strict mode does not allow certain argument names
        DuplicateConstantDeclaration = 1297, // duplicate constant declaration
        AssignmentToConstant = 1298, // assignment to constant
        StringNotInlineSafe = 1299, // string literal is not inline safe
        StrictModeUndefinedVariable = 1300, // undefined variable in strict mode
        UnclosedFunction = 1301, // end of file encountered before function is properly closed
        ObjectLiteralKeyword = 1303, // reserved word used as object literal property name
        NoEndIfDirective = 1304, // expected #ENDIF directive
        NoEndDebugDirective = 1305, // expected #ENDDEBUG directive
        BadNumericLiteral = 1306, // bad numeric literal
        DuplicateLexicalDeclaration = 1307, // duplicate lexical declaration
        DuplicateCatch = 1308, // variable declaration duplicates catch error name
        SuspectEquality = 1309, // suspect equality comparison
        SemicolonInsertion = 1310, // semicolon insertion

        //5000 - 6000 JScript errors that can occur during execution. Typically (also) used in "throw new JScriptException".
        IllegalAssignment = 5008, // "Illegal assignment"
        RegExpSyntax = 5017, // "Syntax error in regular expression"
        UncaughtException = 5022, // "Exception thrown and not caught"

        // this error means something bad happened in the application causing failure
        ApplicationError = 7000
    }
}
