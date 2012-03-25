using Lucene.Net.Search.Vectorhighlight;

namespace SynonymAnalyzer
{
    public class SynonymEngineMock : ISynonymEngine
    {
        private static HashMap<string, string[]> map = new HashMap<string, string[]>();

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