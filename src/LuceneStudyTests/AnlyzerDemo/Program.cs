using System;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.BR;
using Lucene.Net.Analysis.Standard;

namespace AnlyzerDemo
{
    /// <summary>
    /// Aplicação que recebe um texto e apresenta a forma como atua o Analyzer proposto.
    /// </summary>
    public class AnalyzerDemo
    {
        public static void Main(string[] args)
        {
            string opcao=string.Empty;
            do
            {
                Console.Clear();
                Console.WriteLine("Imforme o texto para analizar\nCaso deseja sair digite S.\ntexto:");
                opcao = Console.ReadLine();
                if (opcao.ToLower() != "s")
                {
                    Console.WriteLine();
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
                Console.WriteLine("");
                AnalyzerUtil.DisplayTokens(analyzer, text);
                Console.WriteLine("\n");
            }
        }
    }
}
