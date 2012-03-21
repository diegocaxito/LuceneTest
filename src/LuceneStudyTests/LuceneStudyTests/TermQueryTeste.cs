using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using NUnit.Framework;
using Lucene.Net;

namespace LuceneStudyTests
{
    [TestFixture]
    public class TermQueryTeste
    {
        private AnunciosEmMemoria anunciosEmMemoria;

        [TestFixtureSetUp]
        public void IniciarTeste()
        {
            anunciosEmMemoria = new AnunciosEmMemoria();
        }

        [TestFixtureTearDown]
        public void FinalizarTeste()
        {
            anunciosEmMemoria.Dispose();
        }

        [Test]
        public void Keyword_QuandoUtilizarBuscaPorTerm_e_TermoForUtilizadoComoId_DeveRetornarDados()
        {
            /* 
             * Pesquisa por term queries são recomendados para buscar Ids de itens
             */
            var diretorio = anunciosEmMemoria.Diretorio;
            using (var searcher = new IndexSearcher(diretorio, true))
            {
                var termo = new Term(AnunciosEmMemoria.Id, "5");
                var query = new TermQuery(termo);
                var topDocs = searcher.Search(query, 10);
                Assert.AreEqual(1, topDocs.TotalHits, "Espera ter encontrado por Montes Claros");
            }
        }

        [Test]
        public void TermRangeQuery_QuandoPassarRangeDe_b_ate_e_DeveRetornarSeisItens()
        {
            var diretorio = anunciosEmMemoria.Diretorio;
            using (var searcher = new IndexSearcher(diretorio, true))
            {
                var termRangeQuery = new TermRangeQuery(AnunciosEmMemoria.Cidade, "a", "e", true, true);
                var docsThatMatch = searcher.Search(termRangeQuery, 100);

                var textoComErro = new StringBuilder("Anúncios encontrados\n");

                foreach (var topDoc in docsThatMatch.ScoreDocs)
                {
                    var documentoEncontrado = searcher.Doc(topDoc.doc);
                    textoComErro.AppendLine(string.Format("TipoImovel: {0}\tCidade: {1}", documentoEncontrado.Get(AnunciosEmMemoria.TipoImovel), documentoEncontrado.Get(AnunciosEmMemoria.Cidade)));
                }

                Assert.AreEqual(6, docsThatMatch.TotalHits, textoComErro.ToString());
            }
        }

        [Test]
        public void NumericRangeQuery_QuandoAnalistarEntre_2000_e_10000_e_VerificarTesteInclusivo_DeveRetornarApenasUmitem()
        {
            var diretorio = anunciosEmMemoria.Diretorio;
            using (var searcher = new IndexSearcher(diretorio, true))
            {
                var rangeNumeros = NumericRangeQuery.NewDoubleRange(AnunciosEmMemoria.Preco, 2000, 10000, true, true);
                var matches = searcher.Search(rangeNumeros, 10);
                Assert.AreEqual(2, matches.TotalHits);
            }
        }

        [Test]
        public void BooleanQueries_TestandoAnd_QuandoCombinarSituacoes_DeveRetornarResultado()
        {
            /* Esta consulta procura todos os itens que contém a cidade com a palavra "montes".
             * Procura também por todos os anúncios entre 1000 e 100000.
             * Acontece uma combinação de duas queries em uma.
             * Para alterar o comportamento do teste você pode apenas alterar entre BooleanClause.Occur.MUST ou BooleanClause.Occur.SHOULD ou BooleanClause.Occur.MUST_NOT
             */

            var pesquisarAnuncios = new TermQuery(new Term(AnunciosEmMemoria.Cidade, "montes"));
            var pesquisa = NumericRangeQuery.NewDoubleRange(AnunciosEmMemoria.Preco, 1000, 100000, true, true);
            var booleanQuery = new BooleanQuery();
            booleanQuery.Add(pesquisarAnuncios, BooleanClause.Occur.MUST);
            booleanQuery.Add(pesquisa, BooleanClause.Occur.MUST);

            using (var searcher = new IndexSearcher(anunciosEmMemoria.Diretorio, true))
            {
                var matches = searcher.Search(booleanQuery, 10);
                Assert.True(ItensContemCidadeSeProcura(searcher, matches, "Montes Claros"));
            }
        }

        private static bool ItensContemCidadeSeProcura(IndexSearcher searcher, TopDocs matches, string city)
        {
            var encontrouCidade = matches.ScoreDocs
                .Select(scoreDoc => searcher.Doc(scoreDoc.doc))
                .Any(document => city.Equals(document.Get(AnunciosEmMemoria.Cidade), StringComparison.OrdinalIgnoreCase));

            if (encontrouCidade)
            {
                Console.WriteLine("Encontrou cidade de \"{0}\" em um total de {1}.", city, matches.TotalHits);
                return true;
            }
            Console.WriteLine("Não encontrou cidade {0}.", city);
            return false;
        }

        [TestCase(new[] { "quick", "lazy" }, "the quick brown fox jumped over the lazy dog", 7,TestName = "Quick and lazy with distance of 7")]
        [TestCase(new[] { "Sócrates", "amigo" }, "Sócrates é meu amigo, mas eu sou amigo da verdade", 10, TestName = "Sócrates e amigo com distancia de 10")]
        [TestCase(new[] { "Apartamento", "quartos" }, "Apartamento com quatro quartos", 2, TestName = "Apartamento e quarto com distância de 2")]
        public void PhraseQueryText(string[] frase, string textoParaProcurar, int distanciaEntrePalavras)
        {
            const string texto = "texto";
            using (var directory = new RAMDirectory())
            {
                using (var indexWriter = new IndexWriter(directory, new WhitespaceAnalyzer(), IndexWriter.MaxFieldLength.UNLIMITED))
                {
                    var doc = new Document();
                    doc.Add(new Field(texto, textoParaProcurar, Field.Store.YES, Field.Index.ANALYZED));
                    indexWriter.AddDocument(doc);
                }

                using (var searcher = new IndexSearcher(directory, true))
                {
                    var phraseQuery = new PhraseQuery();
                    phraseQuery.SetSlop(distanciaEntrePalavras);
                    
                    foreach (var palavra in frase)
                        phraseQuery.Add(new Term(texto, palavra));

                    var matches = searcher.Search(phraseQuery, 10);
                    var encontrou = matches.TotalHits > 0;
                    var textoResultado = NaoEncontrou(textoParaProcurar, distanciaEntrePalavras, frase);
                    Assert.IsTrue(encontrou, textoResultado);
                }
            }
        }

        private static string NaoEncontrou(string textoParaProcurar, int distanciaEntrePalavras, string[] frase)
        {
            var textoResultado = new StringBuilder();
            textoResultado
                .AppendLine(
                    string.Format("\nNão encontrou \nDistância entre palavras: \"{0}\" \nTexto: \"{1}\"", 
                                  distanciaEntrePalavras,
                                  textoParaProcurar));

            AdicionarPalavras(frase, textoResultado);

            return textoResultado.ToString();
        }

        private static void AdicionarPalavras(IEnumerable<string> frase, StringBuilder textoResultado)
        {
            textoResultado.AppendLine("As palavras: ");
            foreach (var palavra in frase)
                textoResultado.AppendLine(string.Format("\t - {0}", palavra));
        }

        [Test]
        public void FuzzyQueryTest()
        {
            string titulo = "titulo";
            string texto = "texto";
            using (var diretorio = new RAMDirectory())
            {
                IndexarArquivosEmDocumento(diretorio, new Field[]
                                                          {
                                                              new Field(titulo, "teste", Field.Store.YES, Field.Index.ANALYZED), 
                                                              new Field(titulo, "teste", Field.Store.YES, Field.Index.ANALYZED)
                                                          });
            }
        }

        public void IndexarArquivosEmDocumento(Directory diretorio, IEnumerable<Field> campos)
        {
            using (var indexWriter = new IndexWriter(diretorio, new SimpleAnalyzer()))
            {
                var documento = new Document();
                foreach (var campo in campos)
                    documento.Add(campo);
                indexWriter.AddDocument(documento);
            }
        }
    }
}
