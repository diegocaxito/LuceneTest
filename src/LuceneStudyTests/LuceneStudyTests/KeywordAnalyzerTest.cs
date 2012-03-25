using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        }
    }
}
