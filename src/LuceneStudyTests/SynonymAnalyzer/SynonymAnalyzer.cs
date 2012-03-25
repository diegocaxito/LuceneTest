using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;

namespace SynonymAnalyzer
{
    public class SynonymAnalyzer : Analyzer
    {
        private ISynonymEngine engine;

        public SynonymAnalyzer(ISynonymEngine engine)
        {
            this.engine = engine;
        }

        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            return
                new SynonymFilter(
                    new StopFilter(true,
                                   new LowerCaseFilter(
                                       new StandardFilter(
                                           new StandardTokenizer(
                                               Lucene.Net.Util.Version.LUCENE_29,
                                               reader))),
                                   StopAnalyzer.ENGLISH_STOP_WORDS_SET),
                    engine);
        }
    }
}