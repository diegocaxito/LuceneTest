using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.BR;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Tokenattributes;
using System.Collections;

namespace AnlyzerDemo
{
    public class AnalyzerDemo
    {
        public static void Main(string[] args)
        {
            string opcao=string.Empty;
            do
            {
                Console.WriteLine("Imforme o texto para analizar\nCaso deseja sair digite S.\ntexto:");
                opcao = Console.ReadLine();
                if (opcao.ToLower() != "s")
                {
                    Analyze(opcao);
                    Console.ReadKey();
                }
            } while (opcao.ToLower() != "s");
        }

        public static Analyzer[] analyzers = new Analyzer[]
                                                 {
                                                     new WhitespaceAnalyzer(),
                                                     new SimpleAnalyzer(),
                                                     new StopAnalyzer(Lucene.Net.Util.Version.LUCENE_29),
                                                     new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29),
                                                     new BrazilianAnalyzer(),
                                                     new KeywordAnalyzer()
                                                 };

        private static void Analyze(string text)
        {
            foreach (var analyzer in analyzers)
            {
                var name = analyzer.GetType().Name;
                Console.WriteLine("    {0}    ", name);
                Console.WriteLine("    ");
                AnalyzerUtil.DisplayTokens(analyzer, text);
                Console.WriteLine("\n");
            }
        }
    }

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
    }
}
