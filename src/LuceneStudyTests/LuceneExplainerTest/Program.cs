using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Lucene.Net;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using LuceneStudyTests;
using Directory = Lucene.Net.Store.Directory;

namespace LuceneExplainerTest
{
    public class Explainer
    {
        static string TIPO_IMOVEL = "tipoImovel";
        private static string BAIRRO = "bairro";
        static string CIDADE = "cidade";
        private static string ESTADO = "estado";

        public static void Main(string[] args)
        {
            string consulta, opcao;

            using (Directory diretorio = new RAMDirectory())
            {
                using (
                    var indexWriter = new IndexWriter(diretorio, new SimpleAnalyzer(),
                                                      IndexWriter.MaxFieldLength.UNLIMITED))
                {
                    var documento = ObterDocumento("Residencial");
                    var documento2 = ObterDocumento("Comercial");
                    var documento3 = ObterDocumento("Empreendimento");
                    var documento4 = ObterDocumento("Residencial/Comercial");
                    var documento5 = ObterDocumento("Residencial", "Funcionarios", "Montes Claros", "MG");
                    var documento6 = ObterDocumento("Comercial", "Funcionarios", "Montes Claros", "MG");
                    var documento7 = ObterDocumento("Residencial", "Melo", "Montes Claros", "MG");
                    var documento8 = ObterDocumento("Residencial", "Funcionarios", "Belo Horizonte", "MG");
                    var documento9 = ObterDocumento("Comercial", "Centro", "Campinas");
                    var documento10 = ObterDocumento("Comercial", "Pampulha", "Belo Horizonte", "MG");
                    indexWriter.AddDocument(documento);
                    indexWriter.AddDocument(documento2);
                    indexWriter.AddDocument(documento3);
                    indexWriter.AddDocument(documento4);
                    indexWriter.AddDocument(documento5);
                    indexWriter.AddDocument(documento6);
                    indexWriter.AddDocument(documento7);
                    indexWriter.AddDocument(documento8);
                    indexWriter.AddDocument(documento9);
                    indexWriter.AddDocument(documento10);
                }


                do
                {
                    Console.WriteLine("Consulta: ");
                    consulta = Console.ReadLine();
                    var parser = new QueryParser(LuceneUtil.ObterVersao(), CIDADE, new SimpleAnalyzer());
                    var query = parser.Parse(consulta);
                    Console.WriteLine("Query: {0}", query);

                    using (var indexSearcher = new IndexSearcher(diretorio))
                    {
                        var topDocs = indexSearcher.Search(query, 10);

                        foreach (var scoreDoc in topDocs.ScoreDocs)
                        {
                            var explanation = indexSearcher.Explain(query, scoreDoc.doc);
                            var document = indexSearcher.Doc(scoreDoc.doc);
                            Console.WriteLine("------\n{0}\n{1}", document.Get(TIPO_IMOVEL), explanation.ToString());
                        }

                    }

                    Console.ReadKey();
                    Console.Clear();
                    Console.WriteLine("Pressione 1 para sair");
                    opcao = Console.ReadLine();
                } while (opcao != "1");
            }
        }

        private static Document ObterDocumento(string tipoImovel, string bairro = "Bela Vista", string cidade = "São Paulo", string estado = "SP")
        {
            var documento = new Document();
            documento.Add(new Field(TIPO_IMOVEL, tipoImovel, Field.Store.YES, Field.Index.ANALYZED));
            documento.Add(new Field(BAIRRO, bairro, Field.Store.YES, Field.Index.ANALYZED));
            documento.Add(new Field(CIDADE, cidade, Field.Store.YES, Field.Index.ANALYZED));
            documento.Add(new Field(ESTADO, estado, Field.Store.YES, Field.Index.ANALYZED));
            return documento;
        }
    }
}
