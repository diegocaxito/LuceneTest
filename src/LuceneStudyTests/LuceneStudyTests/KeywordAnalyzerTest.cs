using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using NUnit.Framework;
using Lucene.Net;

namespace LuceneStudyTests
{
    /// <summary>
    /// Classe de teste de múltiplos analyzers por field
    /// </summary>
    [TestFixture]
    public class KeywordAnalyzerTest
    {
        private IndexSearcher searcher;

        [SetUp]
        public void Setup()
        {
            Directory directory = new RAMDirectory();
            using (var indexWriter = new IndexWriter(directory, new SimpleAnalyzer(), IndexWriter.MaxFieldLength.UNLIMITED))
            {
                var document = new Document();
                document.Add(new Field("partnum", "Q36", Field.Store.NO, Field.Index.NOT_ANALYZED_NO_NORMS));
                document.Add(new Field("description", "Illidium Space Modulator", Field.Store.YES, Field.Index.ANALYZED));
                indexWriter.AddDocument(document);
            }
            searcher = new IndexSearcher(directory, true);
        }

        [Test]
        public void TestTermQuery()
        {
            var query = new TermQuery(new Term("partnum", "Q36"));
            Assert.AreEqual(1, searcher.Search(query, 10).TotalHits);
        }

        [Test]
        public void TestBasicQueryParser()
        {
            var query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "description", new SimpleAnalyzer())
                .Parse("partnum:Q36 AND SPACE");
            Assert.AreEqual("+partnum:q +description:space", query.ToString(), "note Q36 -> q");
            Assert.AreEqual(0, searcher.Search(query, 10).ScoreDocs.Length, "doc not found :(");
        }

        [Test]
        public void TestPerFieldAnalyzer()
        {
            var analyzer = new PerFieldAnalyzerWrapper(new SimpleAnalyzer());
            analyzer.AddAnalyzer("partnum", new KeywordAnalyzer());
            var query =
                new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "description", analyzer)
                    .Parse("partnum:Q36 AND SPACE");
            Assert.AreEqual("+partnum:Q36 +space", query.ToString("description"), "Q36 kept as-is");
            Assert.AreEqual(1, searcher.Search(query, searcher.MaxDoc()).ScoreDocs.Length, "docs found!!!");
        }
    }
}
