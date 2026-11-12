# Tarefa: guardar o idioma das fichas na conta

> **Situação (12/11/2026):** implementada na branch `feat/revisao-espacada` (seção 3 inteira, inclusive baralhos compartilhados e cópia). O site 0.10.0 envia e lê os campos.

**Repositórios:** `cardvoz-db` (migration) e `cardvoz-api` (modelo, DTOs e rotas)
**Depende de:** nada. O site (versão 0.8.0) já tem o "Idioma das fichas", mas guarda só no aparelho.

---

## 1. Contexto

Num baralho de inglês ou espanhol, o leitor de tela pronunciava "irregular verbs" com a fonética do português. O site agora deixa a pessoa escolher, por baralho, o **idioma das perguntas** e o **idioma das respostas**, e marca o texto com o atributo `lang`. Assim o TalkBack e o NVDA trocam a voz.

Como a API não tem onde guardar essa escolha, ela fica só no aparelho. Num baralho da conta aberto em outro aparelho, o idioma não aparece.

## 2. Banco (`cardvoz-db`)

**Já na `main`:** `supabase/migrations/20261030000000_idioma_das_fichas.sql` (cardvoz-db#3, 30/10). **Confira se foi aplicada no Supabase** (`npx supabase db push`) antes de publicar a API: com o modelo novo e sem as colunas, toda consulta de baralho falha. O conteúdo:

```sql
ALTER TABLE "Decks" ADD COLUMN "QuestionLanguage" character varying(10);
ALTER TABLE "Decks" ADD COLUMN "AnswerLanguage" character varying(10);
-- e uma checagem em cada coluna: nulo ou código de idioma (^[a-z]{2,3}(-[A-Z]{2})?$)
```

`NULL` significa português, o idioma padrão. Os baralhos que já existem ficam `NULL`.

## 3. API (`cardvoz-api`)

- `Models/Deck.cs`: `QuestionLanguage` e `AnswerLanguage` (`string?`, `MaxLength(10)`).
- `DeckResponseDto` e `DeckDetailResponseDto`: os dois campos (camelCase no JSON: `questionLanguage`, `answerLanguage`).
- `UpdateDeckDto` (e `CreateDeckDto`): aceitar os dois campos. **Validar** o formato (`^[a-z]{2,3}(-[A-Z]{2})?$`, ex.: `en`, `es`, `pt-BR`), a mesma regra da checagem do banco. O valor vai para um atributo HTML, então nada além de código de idioma pode passar.
- No `PUT`, como nos outros campos: **ausente ou `null` não muda nada**; **texto vazio volta para `NULL`** (português); outro valor é validado e gravado. Atualize `UpdatedAt`, para a mudança aparecer na rota de alterações da parte 4.
- Copiar os campos em `CopySharedAsync` (baralhos prontos) e incluí-los na rota de alterações da parte 4, se ela já existir.
- **Baralhos compartilhados:** um baralho pronto de inglês também precisa do idioma. Aceitar os dois campos no `SharedDeckDto` (`/api/admin/shared-decks`) e no editor do painel (`cardvoz-admin`, `src/app/baralhos/editar/page.tsx`).

## 4. Site (depois da API publicada)

Em `src/lib/data/index.ts`, `setDeckLanguages` passa a enfileirar a alteração para a conta. `fromApiDeck` passa a ler `questionLanguage`/`answerLanguage` da API, em vez de manter só o valor do aparelho. Isso fica com quem cuida do site.

## 5. Ordem de publicação

1. Migration no `cardvoz-db` (`npx supabase db push`).
2. API.
3. Site.
