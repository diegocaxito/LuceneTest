using System.Collections.Generic;

namespace LuceneStudyTests
{
    public class Cidade
    {
        public int Id { get; set; }

        public string Pais { get; set; }

        public string Nome { get; set; }

        public string Descricao { get; set; }

        public static List<Cidade> ObterCidades()
        {
            return new List<Cidade>
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
                               },
                           new Cidade
                               {
                                   Id = 3,
                                   Pais = "Brasil",
                                   Nome = "São Paulo",
                                   Descricao = "São Paulo tem pessoas de todo o mundo."
                               },
                           new Cidade
                               {
                                   Id = 4,
                                   Pais = "EUA",
                                   Nome = "New York",
                                   Descricao = "New York é o centro financeiro mundial."
                               }

                       };
        }
    }
}