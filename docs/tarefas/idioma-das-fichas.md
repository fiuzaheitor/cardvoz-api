# Tarefa: guardar o idioma das fichas na conta

**Repositórios:** `cardvoz-db` (migration) e `cardvoz-api` (modelo, DTOs e rotas)
**Depende de:** nada. O site (versão 0.8.0) já tem o "Idioma das fichas", mas guarda só no aparelho.

---

## 1. Contexto

Num baralho de inglês ou espanhol, o leitor de tela pronunciava "irregular verbs" com a fonética do português. O site agora deixa a pessoa escolher, por baralho, o **idioma das perguntas** e o **idioma das respostas**, e marca o texto com o atributo `lang`. Assim o TalkBack e o NVDA trocam a voz.

Como a API não tem onde guardar essa escolha, ela fica só no aparelho. Num baralho da conta aberto em outro aparelho, o idioma não aparece.

## 2. Banco (`cardvoz-db`)

Nova migration em `"Decks"`:

```sql
ALTER TABLE "Decks"
  ADD COLUMN "QuestionLanguage" varchar(10) NULL,
  ADD COLUMN "AnswerLanguage"   varchar(10) NULL;
```

`NULL` significa português, o idioma padrão. Os baralhos que já existem ficam `NULL`.

## 3. API (`cardvoz-api`)

- `Models/Deck.cs`: `QuestionLanguage` e `AnswerLanguage` (`string?`, `MaxLength(10)`).
- `DeckResponseDto` e `DeckDetailResponseDto`: os dois campos (camelCase no JSON: `questionLanguage`, `answerLanguage`).
- `UpdateDeckDto` (e `CreateDeckDto`): aceitar os dois campos. **Validar** o formato (`^[a-z]{2,3}(-[A-Z]{2})?$`, ex.: `en`, `es`, `pt-BR`). O valor vai para um atributo HTML, então nada além de código de idioma pode passar. Um texto vazio volta o campo para `NULL`.
- Copiar os campos em `CopySharedAsync` (baralhos prontos) e incluí-los na rota de alterações da parte 4, se ela já existir.

## 4. Site (depois da API publicada)

Em `src/lib/data/index.ts`, `setDeckLanguages` passa a enfileirar a alteração para a conta. `fromApiDeck` passa a ler `questionLanguage`/`answerLanguage` da API, em vez de manter só o valor do aparelho. Isso fica com quem cuida do site.

## 5. Ordem de publicação

1. Migration no `cardvoz-db` (`npx supabase db push`).
2. API.
3. Site.
