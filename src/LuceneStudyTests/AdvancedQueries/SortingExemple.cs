using System;
using Lucene.Net.Search;
using Lucene.Net.Store;

namespace AdvancedQueries
{
    public class SortingExemple
    {
        private Directory directory;

        public SortingExemple(Directory directory)
        {
            this.directory = directory;
        }

        public void DisplayResults(Query query, Sort sort)
        {
            using(var indexSearcher = new IndexSearcher(directory, true))
            {
                indexSearcher.SetDefaultFieldSortScoring(true, false);
                var results = indexSearcher.Search(query, null, 20, sort);
                Console.WriteLine("\nResults for: {0} sorted by {1}", query, sort);
                Console.WriteLine();
            }
        }
    }
}