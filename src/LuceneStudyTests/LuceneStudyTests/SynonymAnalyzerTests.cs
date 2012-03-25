using System;
using System.IO;
using AnlyzerDemo;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Tokenattributes;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using NUnit.Framework;
using SynonymAnalyzer;

namespace LuceneStudyTests
{
    [TestFixture]
    public class SynonymAnalyzerTests
    {
        SynonymAnalyzer.SynonymAnalyzer synonymAnalyzer = new SynonymAnalyzer.SynonymAnalyzer(new SynonymEngineMock());

        [Test]
        public void SysnonymsTests()
        {
            TokenStream stream = synonymAnalyzer.TokenStream("contents", new StringReader("jumps"));
            var term = stream.GetAttribute(typeof(TermAttribute)) as TermAttribute;
            var posIncr = stream.AddAttribute(typeof(PositionIncrementAttribute)) as PositionIncrementAttribute;
            int i = 0;
            var expected = new[] { "jumps", "hops", "leaps" };
            while (stream.IncrementToken())
            {
                Assert.AreEqual(expected[i], term.Term());

                int expectedPos;
                if (i == 0)
                {
                    expectedPos = 1;
                }
                else
                {
                    expectedPos = 0;
                }
                Assert.AreEqual(expectedPos, posIncr.GetPositionIncrement());
                i++;
            }
            Assert.AreEqual(3, i);
        }

        private IndexSearcher indexSearcher;

        [SetUp]
        public void Setup()
        {
            var directory = new RAMDirectory();
            using (var writer = new IndexWriter(directory, synonymAnalyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                var document = new Document();
                document.Add(new Field("contents", "The quick brown fox jumps over the lazy dog", Field.Store.YES, Field.Index.ANALYZED));
                writer.AddDocument(document);
            }

            indexSearcher = new IndexSearcher(directory, true);
        }

        [TearDown]
        public void TearDown()
        {
            indexSearcher.Close();
        }

        [Test]
        public void TestSynonymAPI()
        {
            var termQuery = new TermQuery(new Term("contents", "hops"));
            Assert.AreEqual(1, indexSearcher.Search(termQuery, 1).TotalHits);

            var pq = new PhraseQuery();
            pq.Add(new Term("contents", "fox"));
            pq.Add(new Term("contents", "hops"));
            Assert.AreEqual(1, indexSearcher.Search(pq, 10).TotalHits);
        }

        [Test]
        public void TestWithQueryParser()
        {
            var query =
                new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "contents", synonymAnalyzer).Parse("\"fox jumps\"");
            Assert.AreEqual(1, indexSearcher.Search(query, 10).TotalHits);
            Console.WriteLine("With SynonymAnalyzer, \"fox jumps\" parses to {0}", query.ToString("content"));

            query =
                new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "contents",
                                new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29)).Parse("\"fox jumps\"");

            Assert.AreEqual(1, indexSearcher.Search(query, 10).TotalHits);
            Console.WriteLine("With StandardAnalyzer, \"fox jumps\" parses to {0}", query.ToString("content"));
        }

        [Test]
        public void SynonymAnalyzerViewTest()
        {   
            AnalyzerUtil.DisplayTokenWithPositions(synonymAnalyzer, "The quick brown fox jumps over lazy dog");
        }
    }
}
