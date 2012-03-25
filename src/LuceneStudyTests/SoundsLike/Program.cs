using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnlyzerDemo;
using Lucene.Net.Analysis;

namespace SoundsLike
{
    public class SoundsLike
    {
        static void Main(string[] args)
        {
            MetaphoneReplacementAnalyzer analyzer = new MetaphoneReplacementAnalyzer();
            AnalyzerUtil.DisplayTokens(analyzer, "The quick brown fox jumped over the lazy dog");
            Console.WriteLine("");
            AnalyzerUtil.DisplayTokens(analyzer, "Tha quick brown phox jumped ovver that lazi dag");
            Console.ReadKey();
        }
    }
}
