// dowhile.cs
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
using System.Collections.Generic;
using System.Text;

namespace Kiss.Web.Utils.ajaxmin
{

    public sealed class DoWhile : IterationStatement
    {
        private AstNode m_condition;

        public AstNode Condition 
        {
            get { return m_condition; }
            set
            {
                m_condition.IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);
                m_condition = value;
                m_condition.IfNotNull(n => n.Parent = this);
            }
        }

        public Context WhileContext { get; set; }

        public DoWhile(Context context, JSParser parser)
            : base(context, parser)
        {
        }

        internal override bool RequiresSeparator
        {
            get
            {
                // do-while statements TECHNICALLY should end with a semicolon.
                // but IE seems to parse do-while statements WITHOUT the semicolon, so
                // the terminating semicolon ends up being an empty statement AFTER the
                // do-while. Which throws off else or other do-while while-clauses.
                return true;
            }
        }

        public override void Accept(IVisitor visitor)
        {
            if (visitor != null)
            {
                visitor.Visit(this);
            }
        }

        public override IEnumerable<AstNode> Children
        {
            get
            {
                return EnumerateNonNullNodes(Body, Condition);
            }
        }

        public override bool ReplaceChild(AstNode oldNode, AstNode newNode)
        {
            if (Body == oldNode)
            {
                Body = ForceToBlock(newNode);
                return true;
            }
            if (Condition == oldNode)
            {
                Condition = newNode;
                return true;
            }
            return false;
        }

        internal override bool EncloseBlock(EncloseBlockType type)
        {
            // there is an IE bug (up to IE7, at this time) that do-while
            // statements cause problems when they happen before else or while
            // statements without a closing curly-brace between them.
            // So if we get here, flag this as possibly requiring a block.
            return (type == EncloseBlockType.SingleDoWhile);
        }
    }
}
