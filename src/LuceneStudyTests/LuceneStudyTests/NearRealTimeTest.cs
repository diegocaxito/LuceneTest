using System.Collections.Generic;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using NUnit.Framework;
using Directory = Lucene.Net.Store.Directory;

namespace LuceneStudyTests
{
    [TestFixture]
    public class NearRealTimeTest
    {
        [Test]
        public void NearRealTimeTestTeste()
        {
            var diretorio = new RAMDirectory();
            const int quantidadeItensEsperados = 10;


            using (var escritorIndice = new IndexWriter(diretorio, LuceneUtil.ObterAnalyzer(), IndexWriter.MaxFieldLength.UNLIMITED))
            {
                for (int i = 0; i < quantidadeItensEsperados; i++)
                {
                    var documento = new Document();
                    documento.Add(new Field("id", i.ToString(), Field.Store.NO, Field.Index.NOT_ANALYZED_NO_NORMS));
                    documento.Add(new Field("text", "aaa", Field.Store.NO, Field.Index.ANALYZED));
                    escritorIndice.AddDocument(documento);
                }

                var leitorIndice = escritorIndice.GetReader();

                var pesquisa = new IndexSearcher(leitorIndice);
                var consulta = new TermQuery(new Term("text", "aaa"));
                var resultado = pesquisa.Search(consulta, 1);
                
                Assert.AreEqual(quantidadeItensEsperados, resultado.TotalHits,
                                string.Format("Resultado não encontrou {0} itens que se encaixavam",
                                              quantidadeItensEsperados));


                var outroDocumento = new Document();
                escritorIndice.DeleteDocuments(new Term("id", "7"));
                outroDocumento.Add(new Field("id", "11", Field.Store.NO, Field.Index.NOT_ANALYZED_NO_NORMS));
                outroDocumento.Add(new Field("text", "bbb", Field.Store.NO, Field.Index.ANALYZED));
                escritorIndice.AddDocument(outroDocumento);

                var novoLeitor = leitorIndice.Reopen();
                Assert.AreNotSame(novoLeitor, leitorIndice);
                leitorIndice.Close();

                pesquisa = new IndexSearcher(novoLeitor);
                resultado = pesquisa.Search(consulta, 10);
                Assert.AreEqual(9, resultado.TotalHits, string.Format("Não encontrou {0} como quantidade esperada.", quantidadeItensEsperados - 1));

                
                consulta = new TermQuery(new Term("text", "bbb"));
                resultado = pesquisa.Search(consulta, 1);
                Assert.AreEqual(1, resultado.TotalHits);

                novoLeitor.Close();
            }
        }
    }
}


