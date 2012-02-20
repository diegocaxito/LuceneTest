using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using NUnit.Framework;
using Lucene.Net;
using Directory = Lucene.Net.Store.Directory;
using Version = Lucene.Net.Util.Version;

namespace LuceneStudyTests
{
    [TestFixture]
    public class LockTest
    {
        private Directory diretorio;

        [SetUp]
        public void IniciarTeste()
        {
            diretorio = FSDirectory.Open(new DirectoryInfo("~/"));
        }

        [Test, ExpectedException(typeof(LockObtainFailedException))]
        public void TestWriteLock_QuandoTentarCriarMaisDeUmEscritorDeIndiceParaOMesmoDiretorio_NaoDevePermitir_e_DeveGerarExcecao()
        {
            using (var escritorIndice = new IndexWriter(diretorio, new SimpleAnalyzer(), IndexWriter.MaxFieldLength.UNLIMITED))
            {
                IndexWriter outroEscritorIndice = null;

                outroEscritorIndice = new IndexWriter(diretorio, new SimpleAnalyzer(), IndexWriter.MaxFieldLength.UNLIMITED);
            }
        }
    }

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
                foreach (var cidade in cidades)
                {
                    Document documento = new Document();
                    documento.Add(new Field("id", cidade.Id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
                    documento.Add(new Field("pais", cidade.Pais, Field.Store.YES, Field.Index.NO));
                    documento.Add(new Field("descricao", cidade.Descricao, Field.Store.NO, Field.Index.ANALYZED));
                    documento.Add(new Field("cidade", cidade.Nome, Field.Store.YES, Field.Index.ANALYZED));
                    escritorIndice.AddDocument(documento);
                }
            }
        }

        private IndexWriter ObterEscritorIndice()
        {
            return new IndexWriter(diretorio, new StandardAnalyzer(Version.LUCENE_29), IndexWriter.MaxFieldLength.UNLIMITED);
        }

        [Test]
        public void Indexar_QuandoObterEscritorIndice_CriarMesmaQuantidadeDeIndicesQueCidade()
        {
            using(var escritorIndice = ObterEscritorIndice())
                Assert.AreEqual(cidades.Count, escritorIndice.NumDocs());
        }

        [Test]
        public void Indexar_QuandoTentarLerDoIndice_DeveConseguirLerDoIndiceMesmaQuantidadeDeCidadesExistentes()
        {
            using(var leitorIndice = IndexReader.Open(diretorio, readOnly: true))
            {
                Assert.AreEqual(cidades.Count, leitorIndice.MaxDoc());
                Assert.AreEqual(cidades.Count, leitorIndice.NumDocs());
            }
        }
    }

    public class Cidade
    {
        public int Id { get; set; }

        public string Pais { get; set; }

        public string Nome { get; set; }

        public string Descricao { get; set; }
    }
}
