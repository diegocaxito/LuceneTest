using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Store;
using NUnit.Framework;
using Lucene.Net;
using Directory = Lucene.Net.Store.Directory;

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
}
