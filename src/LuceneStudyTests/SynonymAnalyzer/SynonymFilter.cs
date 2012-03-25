using System;
using System.Collections.Generic;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Tokenattributes;
using Lucene.Net.Util;

namespace SynonymAnalyzer
{
    public class SynonymFilter : TokenFilter
    {
        public static string TOKEN_TYPE_SYNONYM = "synonym";
        private Stack<string> synonymStack;
        private ISynonymEngine engine;
        private AttributeSource.State current;
        private TermAttribute termAttr;
        private PositionIncrementAttribute posIncrAttr;


        public SynonymFilter(TokenStream input, ISynonymEngine engine)
            : base(input)
        {
            synonymStack = new Stack<string>();
            this.engine = engine;
            this.termAttr = AddAttribute(typeof(TermAttribute)) as TermAttribute;
            this.posIncrAttr = AddAttribute(typeof(PositionIncrementAttribute)) as PositionIncrementAttribute;
        }

        public override bool IncrementToken()
        {
            if (synonymStack.Count > 0)
            {
                var syn = synonymStack.Pop();
                RestoreState(current);
                termAttr.SetTermBuffer(syn);
                posIncrAttr.SetPositionIncrement(0);
                return true;
            }
            if (!input.IncrementToken())
            {
                return false;
            }
            if (AddAliasToStack())
            {
                current = CaptureState();
            }
            return true;
        }

        private bool AddAliasToStack()
        {
            String[] synonyms = engine.GetSynonyms(termAttr.Term());
            if (synonyms == null)
                return false;

            foreach (var syn in synonyms)
                synonymStack.Push(syn);

            return true;
        }
    }
}