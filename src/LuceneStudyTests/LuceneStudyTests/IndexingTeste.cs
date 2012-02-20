using System.Collections.Generic;
using System.IO;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using NUnit.Framework;
using Directory = Lucene.Net.Store.Directory;

namespace LuceneStudyTests
{
    [TestFixture]
    public class IndexingTeste
    {
        private List<Cidade> cidades = new List<Cidade>
                                           {
                                               new Cidade
                                                   {
                                                       Id = 1,
                                                       Pais = "Holanda",
                                                       Nome = "Amsterdam",
                                                       Descricao = "Amsterdam tem muitas pontes."
                                                   },
                                               new Cidade
                                                   {
                                                       Id = 2,
                                                       Pais = "Itália",
                                                       Nome = "Veneza",
                                                       Descricao = "Veneza tem muitos canais."
                                                   }
                                           };

        private Directory diretorio;

        [SetUp]
        public void IniciarUnidadeTeste()
        {
            diretorio = new RAMDirectory();
            using (var escritorIndice = ObterEscritorIndice())
            {
                foreach (var cidadeSelecionada in cidades)
                {
                    var documento = ObterCidadeComoDocumento(cidadeSelecionada);
                    escritorIndice.AddDocument(documento);
                }
            }
        }

        private Document ObterCidadeComoDocumento(Cidade cidadeSelecionada)
        {
            Document documento = new Document();
            documento.Add(new Field(id, cidadeSelecionada.Id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            documento.Add(new Field(pais, cidadeSelecionada.Pais, Field.Store.YES, Field.Index.NO));
            documento.Add(new Field(descricao, cidadeSelecionada.Descricao, Field.Store.NO, Field.Index.ANALYZED));
            documento.Add(new Field(cidade, cidadeSelecionada.Nome, Field.Store.YES, Field.Index.ANALYZED));
            return documento;
        }

        private const string id = "id";
        private const string pais = "pais";
        private const string descricao = "descricao";
        private const string cidade = "cidade";

        private IndexWriter ObterEscritorIndice()
        {
            return new IndexWriter(diretorio, LuceneUtil.ObterAnalyzer(), IndexWriter.MaxFieldLength.UNLIMITED);
        }

        [Test]
        public void Indexar_QuandoObterEscritorIndice_CriarMesmaQuantidadeDeIndicesQueCidade()
        {
            using (var escritorIndice = ObterEscritorIndice())
                Assert.AreEqual(cidades.Count, escritorIndice.NumDocs());
        }

        [Test]
        public void Indexar_QuandoTentarLerDoIndice_DeveConseguirLerDoIndiceMesmaQuantidadeDeCidadesExistentes()
        {
            using (var leitorIndice = IndexReader.Open(diretorio, readOnly: true))
            {
                Assert.AreEqual(cidades.Count, leitorIndice.MaxDoc());
                Assert.AreEqual(cidades.Count, leitorIndice.NumDocs());
            }
        }

        [Test]
        public void Deletar_QuandoTentarDeletarAntesDeOtimizar_DeveDeletarIndices_e_NaoDeveDiminuirMaxDocs()
        {
            using (var escritorIndice = ObterEscritorIndice())
            {
                Assert.AreEqual(cidades.Count, escritorIndice.NumDocs());

                escritorIndice.DeleteDocuments(new Term(id, "1"));
                escritorIndice.Commit();

                Assert.IsTrue(escritorIndice.HasDeletions());
                Assert.AreEqual(cidades.Count, escritorIndice.MaxDoc());
                Assert.AreEqual(cidades.Count - 1, escritorIndice.NumDocs());
            }
        }

        [Test]
        public void Deletar_QuandoTentarDeletarIndiceDepoisDeOtimizar_DeveConterApenasUmDocumento()
        {
            using (var escritorIndice = ObterEscritorIndice())
            {
                Assert.AreEqual(cidades.Count, escritorIndice.NumDocs());

                escritorIndice.DeleteDocuments(new Term(id, "1"));
                escritorIndice.Optimize();
                escritorIndice.Commit();

                Assert.IsFalse(escritorIndice.HasDeletions());
                Assert.AreEqual(cidades.Count - 1, escritorIndice.MaxDoc());
                Assert.AreEqual(cidades.Count - 1, escritorIndice.NumDocs());
            }
        }

        public int ObterQuantidadeItem(string identificadorCampo, string pesquisa)
        {

            using (var pesquisaIndice = new IndexSearcher(diretorio, readOnly: true))
            {
                var termoPesquisa = new Term(identificadorCampo, pesquisa.ToLower());
                var consulta = new TermQuery(termoPesquisa);
                var resultado = pesquisaIndice.Search(consulta, pesquisaIndice.MaxDoc()).TotalHits;
                return resultado;
            }
        }

        [Test]
        public void ObterQuantidadeHits_QuandoPassarCidadeAmsterdam_DeveEncontrarApenasUmaOcorrencia()
        {
            Assert.AreEqual(1, ObterQuantidadeItem(cidade, "Amsterdam"));
        }

        [Test]
        public void Atualizar_QuandoTentarAtualizarAmsterdam_DeveAtualizarIndices()
        {
            Assert.AreEqual(1, ObterQuantidadeItem(cidade, "Amsterdam"));
            using (var escritorIndice = ObterEscritorIndice())
            {

                var documento =
                    ObterCidadeComoDocumento(
                        new Cidade { Id = 1, Nome = "Den Haag", Descricao = "Den Haag has a lot of museums", Pais = "Holanda" });

                escritorIndice.UpdateDocument(new Term(id, "1"), documento, LuceneUtil.ObterAnalyzer());

                escritorIndice.Close();
            }

            Assert.AreEqual(0, ObterQuantidadeItem(cidade, "Amsterdam"));
            Assert.AreEqual(1, ObterQuantidadeItem(cidade, "Den Haag"));
        }

        [TearDown]
        public void FinalizarTeste()
        {
            using (var escritorIndice = ObterEscritorIndice())
                escritorIndice.DeleteAll();
        }
    }
}