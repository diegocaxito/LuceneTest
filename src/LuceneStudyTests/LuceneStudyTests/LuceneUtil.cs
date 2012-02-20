using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;

namespace LuceneStudyTests
{
    public static class LuceneUtil
    {
        public static Analyzer ObterAnalyzer()
        {
            return new SimpleAnalyzer();
        }

        public static Version ObterVersao()
        {
            return Version.LUCENE_29;
        }
    }
}