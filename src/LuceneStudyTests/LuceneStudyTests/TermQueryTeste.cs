using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            Directory diretorio = anunciosEmMemoria.Diretorio;
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
            using(var searcher = new IndexSearcher(diretorio, true))
            {
                var rangeNumeros = NumericRangeQuery.NewDoubleRange(AnunciosEmMemoria.Preco, 2000, 10000, true, true);
                var matches = searcher.Search(rangeNumeros, 10);
                Assert.AreEqual(1, matches.TotalHits);
            }
        }

        public void BooleanQueries_QuandoCombinarSituacoes_DeveRetornarResultado(){}
    }
}
