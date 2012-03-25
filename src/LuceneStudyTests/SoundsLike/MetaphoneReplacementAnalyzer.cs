using System.IO;
using Lucene.Net.Analysis;

namespace SoundsLike
{
    public class MetaphoneReplacementAnalyzer : Analyzer
    {
        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            return new MetaphoneReplacementFilter(new LetterTokenizer(reader));
        }
    }
}