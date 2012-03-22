using System;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.BR;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;

namespace LuceneStudyTests
{
    public class AnunciosEmMemoria : IDisposable
    {
        public const string Referencia = "referencia";
        public const string Descricao = "descricao";
        public const string Id = "Id";
        public const string TipoImovel = "tipoImovel";
        public const string Bairro = "bairro";
        public const string Cidade = "cidade";
        public const string Estado = "estado";
        public const string Preco = "preco";

        public Directory Diretorio { get; set; }
        public Analyzer Analizador { get; set; }

        public AnunciosEmMemoria()
        {
            Diretorio = new RAMDirectory();
            Analizador = new BrazilianAnalyzer();
            
            using (var indexWriter = new IndexWriter(Diretorio, Analizador, IndexWriter.MaxFieldLength.UNLIMITED))
                CriarBaseDocumentosImoveisEmMemoria(indexWriter);
        }

        public Document ObterDocumento(int id, string tipoImovel, string bairro = "Bela Vista", string cidade = "São Paulo", string estado = "SP", double preco = 200)
        {
            var documento = new Document();
            documento.Add(new Field(Id, id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            
            documento.Add(new Field(TipoImovel, tipoImovel, Field.Store.YES, Field.Index.ANALYZED));
            documento.Add(new Field(Bairro, bairro, Field.Store.YES, Field.Index.ANALYZED));
            documento.Add(new Field(Cidade, cidade, Field.Store.YES, Field.Index.ANALYZED));
            documento.Add(new Field(Estado, estado, Field.Store.YES, Field.Index.ANALYZED));
            documento.Add(new Field(Descricao,
                                    String.Format("Imovel {0} Bairro {1} Cidade {2} Estado {3} Preço Preco {4}",
                                                  tipoImovel, bairro, cidade, estado, preco), 
                                                  Field.Store.YES,
                                    Field.Index.ANALYZED));

            var campoPreco = new NumericField(Preco);
            campoPreco.SetDoubleValue(preco);
            documento.Add(campoPreco);

            
            var referencia = new NumericField(Referencia);
            referencia.SetLongValue(id);
            documento.Add(referencia);
            
            return documento;
        }

        public void CriarBaseDocumentosImoveisEmMemoria(IndexWriter indexWriter)
        {
            var documento = ObterDocumento(1, "Residencial", preco:2400);
            var documento2 = ObterDocumento(2, "Comercial", preco: 200000);
            var documento3 = ObterDocumento(3, "Empreendimento");
            var documento4 = ObterDocumento(4, "Residencial/Comercial");
            var documento5 = ObterDocumento(5, "Residencial", "Funcionarios", "Montes Claros", "MG");
            var documento6 = ObterDocumento(6, "Comercial", "Funcionarios", "Montes Claros", "MG");
            var documento7 = ObterDocumento(7, "Residencial", "Melo", "Montes Claros", "MG", 5000);
            var documento8 = ObterDocumento(8, "Residencial", "Funcionarios", "Belo Horizonte", "MG");
            var documento9 = ObterDocumento(9, "Comercial", "Centro", "Campinas", preco: 150);
            var documento10 = ObterDocumento(10, "Comercial", "Pampulha", "Belo Horizonte", "MG");
            var documento11 = ObterDocumento(11, "Aluguel", "Centro", "São Vicente");
            var documento12 = ObterDocumento(12, "Apartamento Aluguel", "Centro", "São Vicente");
            var documento13 = ObterDocumento(13, "Residencial", "Centro", "Santos");
            var documento14 = ObterDocumento(14, "Apartamento Aluguel", "Centro", "Santos");
            
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
            indexWriter.AddDocument(documento11);
            indexWriter.AddDocument(documento12);
            indexWriter.AddDocument(documento13);
            indexWriter.AddDocument(documento14);
        }

        public void ExtrairExplicacaoQueryComTermoPorCidade(Directory diretorio, Query query)
        {
            using (var indexSearcher = new IndexSearcher(diretorio, true))
            {
                var topDocs = indexSearcher.Search(query, 10);

                foreach (var scoreDoc in topDocs.ScoreDocs)
                {
                    var explanation = indexSearcher.Explain(query, scoreDoc.Doc);
                    var document = indexSearcher.Doc(scoreDoc.Doc);
                    Console.WriteLine("------\n{0}\n{1}", document.Get(TipoImovel), explanation);
                }
            }
        }

        public static void ApresentarDiretorio()
        {
            var apresentacao = new AnunciosEmMemoria();
            string opcao;
            do
            {
                Console.Clear();
                Console.WriteLine("Projeto de testes de tipo de query\nExpecifique a consulta\nConsulta: ");
                string consulta = Console.ReadLine();
                Console.WriteLine("\n\nEscolha o tipo de exibição\n1 - Explicação de query em cima do nome da cidade\n2 - Resultado de full text search textual");
                opcao = Console.ReadLine();

                switch (opcao)
                {
                    case "2":
                        break;
                    default:
                        apresentacao.ExbibicaoDeExplicacao(consulta);
                        break;
                }
                Console.ReadKey();

                Console.WriteLine("\n\nPressione 1 para sair ou qualquer outra tecla para continuar...");
                opcao = Console.ReadLine();
            } while (opcao != "1");
        }

        public void ExbibicaoDeExplicacao(string consulta)
        {
            var parser = new QueryParser(LuceneUtil.ObterVersao(), Cidade, new SimpleAnalyzer());
            var query = parser.Parse(consulta);
            Console.WriteLine("Query: {0}", query);
            ExtrairExplicacaoQueryComTermoPorCidade(Diretorio, query);
        }

        public void Dispose()
        {
            if (Diretorio != null) Diretorio.Close();
        }
    }
}