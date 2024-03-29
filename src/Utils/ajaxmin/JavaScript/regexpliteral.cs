// regexpliteral.cs
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

using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Kiss.Web.Utils.ajaxmin
{
    public sealed class RegExpLiteral : Expression
    {
        public string Pattern { get; set; }
        public string PatternSwitches { get; set; }

        public override bool IsConstant
        {
            get
            {
                // actually, if the regular expression uses the "g" global flag,
                // then we DON'T want to treat it like a constant and possibly remove
                // a single-referenced variable. This is because the global regex in
                // a variable can have multiple calls to the exec() method, which continues
                // on from where it left off. But if you call exec() on a literal, even if
                // the "g" flag is set, it always starts at the beginning.
                return (PatternSwitches ?? "").IndexOf("g", StringComparison.OrdinalIgnoreCase) < 0;
            }
        }

        public RegExpLiteral(Context context, JSParser parser)
            : base(context, parser)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            if (visitor != null)
            {
                visitor.Visit(this);
            }
        }

        public override bool IsEquivalentTo(AstNode otherNode)
        {
            var otherRegExp = otherNode as RegExpLiteral;
            return otherRegExp != null
                && string.CompareOrdinal(Pattern, otherRegExp.Pattern) == 0
                && string.CompareOrdinal(PatternSwitches, otherRegExp.PatternSwitches) == 0;
        }
    }
}
