using System;
using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Tokenattributes;

namespace AnlyzerDemo
{
    public static class AnalyzerUtil
    {
        private static void DisplayTokens(TokenStream stream)
        {
            TermAttribute term = (TermAttribute) stream.AddAttribute(typeof(TermAttribute));
            while (stream.IncrementToken())
            {
                Console.WriteLine("[{0}]  ", term.Term());
            }
        }

        public static void DisplayTokens(Analyzer analyzer, string text)
        {   
            DisplayTokens(analyzer.TokenStream("contents", new StringReader(text)));
        }

        public static void DisplayTokenWithPositions(Analyzer analyzer, string text)
        {
            var stream = analyzer.TokenStream("contents", new StringReader(text));
            var termAttribute = stream.AddAttribute(typeof (TermAttribute)) as TermAttribute;
            var positionIncrement =
                stream.AddAttribute(typeof (PositionIncrementAttribute)) as PositionIncrementAttribute;
            int position = 0;
            while (stream.IncrementToken())
            {
                int increment = positionIncrement.GetPositionIncrement();
                if(increment>0)
                {
                    position = position + increment;
                    Console.WriteLine();
                    Console.WriteLine("{0}: ", position);
                }
                Console.WriteLine("[{0}]", termAttribute.Term());
            }
            Console.WriteLine();
        }
    }
}