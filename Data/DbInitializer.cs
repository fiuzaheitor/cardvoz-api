using cardvoz_api.Models;
using Microsoft.EntityFrameworkCore;

namespace cardvoz_api.Data;

public static class DbInitializer
{
    public static void Initialize(CardVozDbContext context)
    {
        // Se já existirem baralhos, a base já foi inicializada.
        if (context.Decks.Any())
        {
            return;
        }

        var decks = new List<Deck>
        {
            new Deck
            {
                Slug = "biologia-celular",
                Title = "Biologia Celular",
                Category = "Ciências Biológicas",
                Description = "Estruturas celulares, organelas, respiração celular, fotossíntese e funções da membrana plasmática.",
                CreatedAt = DateTime.UtcNow,
                Cards = new List<Card>
                {
                    new Card
                    {
                        Question = "Qual organela celular é responsável pela produção de energia (ATP) por meio da respiração celular?",
                        Answer = "A mitocôndria.",
                        VoiceNotes = "Dica: mitocôndria, respiração celular, ATP",
                        DisplayOrder = 1,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Card
                    {
                        Question = "Qual estrutura delimita a célula e controla a entrada e saída de substâncias?",
                        Answer = "A membrana plasmática (ou plasmalema).",
                        VoiceNotes = "Dica: membrana plasmática ou plasmalema",
                        DisplayOrder = 2,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Card
                    {
                        Question = "Qual organela é responsável pela síntese de proteínas nas células?",
                        Answer = "Os ribossomos.",
                        VoiceNotes = "Dica: ribossomos ou síntese proteica",
                        DisplayOrder = 3,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Card
                    {
                        Question = "Qual organela realiza a fotossíntese nas células vegetais e algas?",
                        Answer = "Os cloroplastos.",
                        VoiceNotes = "Dica: cloroplasto ou fotossíntese",
                        DisplayOrder = 4,
                        CreatedAt = DateTime.UtcNow
                    }
                }
            },
            new Deck
            {
                Slug = "ingles-verbos-irregulares",
                Title = "Inglês - Verbos Irregulares",
                Category = "Idiomas",
                Description = "Formas infinitivo, passado simples (Past Simple) e particípio passado (Past Participle) dos verbos mais usados.",
                CreatedAt = DateTime.UtcNow,
                Cards = new List<Card>
                {
                    new Card
                    {
                        Question = "Qual é o passado simples (Past Simple) e particípio de 'to go'?",
                        Answer = "Went (passado simples) e Gone (particípio).",
                        VoiceNotes = "Pronúncia: went, gone",
                        DisplayOrder = 1,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Card
                    {
                        Question = "Qual é o passado simples e particípio de 'to write'?",
                        Answer = "Wrote (passado simples) e Written (particípio).",
                        VoiceNotes = "Pronúncia: wrote, written",
                        DisplayOrder = 2,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Card
                    {
                        Question = "Qual é o passado simples e particípio de 'to see'?",
                        Answer = "Saw (passado simples) e Seen (particípio).",
                        VoiceNotes = "Pronúncia: saw, seen",
                        DisplayOrder = 3,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Card
                    {
                        Question = "Qual é o passado simples e particípio de 'to take'?",
                        Answer = "Took (passado simples) e Taken (particípio).",
                        VoiceNotes = "Pronúncia: took, taken",
                        DisplayOrder = 4,
                        CreatedAt = DateTime.UtcNow
                    }
                }
            },
            new Deck
            {
                Slug = "historia-do-brasil",
                Title = "História do Brasil - República Velha",
                Category = "Humanidades",
                Description = "Movimentos sociais, política do café com leite, revoltas populares e a transição para a Era Vargas.",
                CreatedAt = DateTime.UtcNow,
                Cards = new List<Card>
                {
                    new Card
                    {
                        Question = "O que caracterizou a chamada 'Política do Café com Leite' na República Velha?",
                        Answer = "A alternância na presidência da República entre as oligarquias de São Paulo (produtores de café) e Minas Gerais (produtores de leite).",
                        VoiceNotes = "Dica: alternância entre oligarquias paulista e mineira",
                        DisplayOrder = 1,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Card
                    {
                        Question = "Qual movimento popular messiânico ocorreu no sertão da Bahia entre 1896 e 1897?",
                        Answer = "A Guerra de Canudos, liderada por Antônio Conselheiro.",
                        VoiceNotes = "Dica: Canudos ou Antônio Conselheiro",
                        DisplayOrder = 2,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Card
                    {
                        Question = "O que motivou a Revolta da Vacina em 1904 no Rio de Janeiro?",
                        Answer = "A obrigatoriedade da vacinação contra a varíola imposta pelo governo de Rodrigues Alves e liderada sanitariamente por Oswaldo Cruz.",
                        VoiceNotes = "Dica: vacina obrigatória contra varíola, Oswaldo Cruz",
                        DisplayOrder = 3,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Card
                    {
                        Question = "Qual movimento dos jovens oficiais militares na década de 1920 contestou as oligarquias da Primeira República?",
                        Answer = "O Tenentismo (com episódios como os 18 do Forte e a Coluna Prestes).",
                        VoiceNotes = "Dica: Tenentismo ou movimento tenentista",
                        DisplayOrder = 4,
                        CreatedAt = DateTime.UtcNow
                    }
                }
            },
            new Deck
            {
                Slug = "acessibilidade-web",
                Title = "Acessibilidade na Web & WCAG",
                Category = "Tecnologia",
                Description = "Critérios de conformidade WCAG 2.1 níveis A, AA e AAA, navegação por teclado e semântica WAI-ARIA.",
                CreatedAt = DateTime.UtcNow,
                Cards = new List<Card>
                {
                    new Card
                    {
                        Question = "Quais são os quatro princípios fundamentais das Diretrizes de Acessibilidade para Conteúdo Web (WCAG)?",
                        Answer = "Perceptível, Operável, Compreensível e Robusto (POUR: Perceivable, Operable, Understandable, Robust).",
                        VoiceNotes = "Dica: Princípios POUR: Perceptível, Operável, Compreensível, Robusto",
                        DisplayOrder = 1,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Card
                    {
                        Question = "Qual é a taxa mínima de contraste de cores recomendada pela WCAG 2.1 no nível AA para texto normal?",
                        Answer = "Uma proporção de contraste de pelo menos 4.5:1 (ou 3:1 para texto em escala grande).",
                        VoiceNotes = "Dica: proporção de 4,5 para 1",
                        DisplayOrder = 2,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Card
                    {
                        Question = "Qual atributo HTML/ARIA deve ser utilizado para identificar o papel semântico de um elemento personalizado para tecnologias assistivas?",
                        Answer = "O atributo 'role' (por exemplo, role='button', role='dialog').",
                        VoiceNotes = "Dica: atributo role",
                        DisplayOrder = 3,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Card
                    {
                        Question = "Qual mecanismo de acessibilidade permite que usuários navegando via teclado pulem diretamente para o conteúdo principal da página?",
                        Answer = "O Skip Link (ou link de atalho de navegação).",
                        VoiceNotes = "Dica: Skip Link ou link de atalho",
                        DisplayOrder = 4,
                        CreatedAt = DateTime.UtcNow
                    }
                }
            }
        };

        context.Decks.AddRange(decks);
        context.SaveChanges();
    }
}
