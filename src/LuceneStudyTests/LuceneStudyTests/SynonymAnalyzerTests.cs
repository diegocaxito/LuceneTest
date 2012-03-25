using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Lucene.Net;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Tokenattributes;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Search.Vectorhighlight;
using Lucene.Net.Store;
using Lucene.Net.Util;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Directory = System.IO.Directory;

namespace LuceneStudyTests
{
    [TestFixture]
    public class SynonymAnalyzerTests
    {
        SynonymAnalyzer synonymAnalyzer = new SynonymAnalyzer(new SynonymEngineMock());

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
            using(var writer = new IndexWriter(directory, synonymAnalyzer, IndexWriter.MaxFieldLength.UNLIMITED))
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
    }

    public class SynonymAnalyzer : Analyzer
    {
        private ISynonymEngine engine;

        public SynonymAnalyzer(ISynonymEngine engine)
        {
            this.engine = engine;
        }

        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            return
                new SynonymFilter(
                    new StopFilter(true,
                        new LowerCaseFilter(
                            new StandardFilter(
                                new StandardTokenizer(
                                    Lucene.Net.Util.Version.LUCENE_29,
                                    reader))),
                        StopAnalyzer.ENGLISH_STOP_WORDS_SET),
                    engine);
        }
    }

    public class SynonymFilter : TokenFilter
    {
        public static string TOKEN_TYPE_SYNONYM = "synonym";
        private Stack<string> synonymStack;
        private ISynonymEngine engine;
        private AttributeSource.State current;
        private TermAttribute termAttr;
        private PositionIncrementAttribute posIncrAttr;


        public SynonymFilter(TokenStream input, ISynonymEngine engine)
            : base(input)
        {
            synonymStack = new Stack<string>();
            this.engine = engine;
            this.termAttr = AddAttribute(typeof(TermAttribute)) as TermAttribute;
            this.posIncrAttr = AddAttribute(typeof(PositionIncrementAttribute)) as PositionIncrementAttribute;
        }

        public override bool IncrementToken()
        {
            if (synonymStack.Count > 0)
            {
                var syn = synonymStack.Pop();
                RestoreState(current);
                termAttr.SetTermBuffer(syn);
                posIncrAttr.SetPositionIncrement(0);
                return true;
            }
            if (!input.IncrementToken())
            {
                return false;
            }
            if (AddAliasToStack())
            {
                current = CaptureState();
            }
            return true;
        }

        private bool AddAliasToStack()
        {
            String[] synonyms = engine.GetSynonyms(termAttr.Term());
            if (synonyms == null)
                return false;

            foreach (var syn in synonyms)
                synonymStack.Push(syn);

            return true;
        }
    }

    public interface ISynonymEngine
    {
        string[] GetSynonyms(string term);
    }

    class SynonymEngineMock : ISynonymEngine
    {
        private static HashMap<String, String[]> map = new HashMap<string, string[]>();

        public SynonymEngineMock()
        {
            map.Put("quick", new string[]{"fast", "speedy"});
            map.Put("jumps", new string[]{"leaps", "hops"});
            map.Put("over", new string[]{"above"});
            map.Put("lazy", new string[]{"pathetic", "sluggich"});
            map.Put("dog", new string[]{"canine", "pooch"});
        }

        public string[] GetSynonyms(string term)
        {
            return map.Get(term);
        }
    }
}
