namespace SynonymAnalyzer
{
    public interface ISynonymEngine
    {
        string[] GetSynonyms(string term);
    }
}