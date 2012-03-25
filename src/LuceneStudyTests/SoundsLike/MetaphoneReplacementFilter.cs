using System;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Tokenattributes;

namespace SoundsLike
{
    public class MetaphoneReplacementFilter : TokenFilter
    {
        public const string METAPHONE = "metaphone";
        private Metaphone metaphoner = new Metaphone();
        private TermAttribute termAttr;
        private TypeAttribute typeAttr;

        public MetaphoneReplacementFilter(TokenStream input) : base(input)
        {
            termAttr = AddAttribute(typeof (TermAttribute)) as TermAttribute;
            typeAttr = AddAttribute(typeof (TypeAttribute)) as TypeAttribute;
        }

        public override bool IncrementToken()
        {
            if (!base.input.IncrementToken())
                return false;
            
            String encoded;
            encoded = metaphoner.Encode(termAttr.Term());
            termAttr.SetTermBuffer(encoded);
            typeAttr.SetType(METAPHONE);
            return true;
        }
    }
}