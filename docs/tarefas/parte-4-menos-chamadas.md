# Tarefa: rotas com menos chamadas (parte 4 do modo sem login)

**Repositório:** cardvoz-api
**Banco (`cardvoz-db`):** não muda. Nenhuma migration: as três rotas usam as tabelas que já existem.
**Depende de:** nada. O site (`cardvoz`, branch `feat/modo-sem-login`) já chama estas rotas e, enquanto elas não existirem (resposta 404 ou 405), usa o caminho antigo. Pode ser publicada antes ou depois do site.

---

## 1. Contexto

A cliente pediu para usar o CardVoz sem conta, com os baralhos guardados no aparelho, e levar os baralhos para a conta depois de entrar. O site agora guarda tudo no aparelho e envia as mudanças para a API por uma fila quando há internet.

Hoje o site faz várias chamadas onde uma bastaria:

| Situação | Hoje | Com a rota nova |
|---|---|---|
| Importar arquivo ou levar um baralho de 1000 fichas para a conta | 1 `POST /api/decks` + 1000 `POST /api/cards` | 1 chamada |
| Uma rodada de estudo com 20 fichas | `start` + 20 `review` + `finish` = 22 chamadas | 1 chamada |
| Atualizar as cópias dos baralhos no aparelho | `GET /api/decks` + 1 `GET /api/decks/{id}` por baralho alterado | 1 chamada |

No plano gratuito do Render, 1000 chamadas levam minutos, e se a internet do celular cair no meio, o baralho fica pela metade na conta.

Todas as rotas abaixo: `[Authorize]`, só com os dados da pessoa logada (`User.GetUserId()`), respostas de erro no padrão atual (`{ message }` e `errors` por campo nos 400).

---

## 2. Rota 1 — baralho com todas as fichas numa chamada

### `POST /api/decks/import`

```json
{
  "title": "Biologia",
  "category": "Geral",
  "description": null,
  "cards": [
    { "question": "Qual organela produz ATP?", "answer": "A mitocôndria." },
    { "question": "...", "answer": "..." }
  ]
}
```

- Cria o baralho e as fichas **numa transação**: ou tudo é salvo, ou nada (`BeginTransactionAsync`). Isso é o que evita baralho pela metade.
- `DisplayOrder` = posição na lista (1, 2, 3...).
- Validações iguais às de `CreateDeckDto` e `CreateCardDto` (título até 200, pergunta e resposta obrigatórias e até 5000). **Limite: 1000 fichas** por chamada (o mesmo da importação do site); acima disso, 400.
- Slug: o mesmo jeito do `CreateAsync` atual.
- **Resposta:** `201` com o `DeckDetailResponseDto` (baralho + fichas com os ids novos, em ordem). O site usa os ids das fichas na ordem da lista enviada.

### `POST /api/decks/{id}/cards/batch` (opcional, mesma lógica)

Acrescenta várias fichas a um baralho que já existe (`{ "cards": [...] }`), na sequência das que já existem, numa transação. `404` se o baralho não for da pessoa. O site ainda não usa esta rota (acrescentar fichas passa pela fila, uma por vez); fica pronta para uma próxima melhoria.

---

## 3. Rota 2 — sessão de estudo inteira numa chamada

### `POST /api/study-sessions/complete`

```json
{
  "deckId": "uuid",
  "startedAt": "2026-10-30T14:00:00Z",
  "completedAt": "2026-10-30T14:05:00Z",
  "answers": [
    { "cardId": "uuid", "wasCorrect": true, "answeredAt": "2026-10-30T14:01:00Z" }
  ]
}
```

- Cria a `StudySession` já concluída (`CompletedAt` preenchido), as `StudyReview` e os totais (`TotalCards`, `CorrectCount`, `IncorrectCount`), como o `finish` faz hoje.
- **A sessão pode ter sido feita sem internet, horas antes.** Por isso os horários vêm do aparelho. Horários no futuro são trazidos para o horário do servidor.
- **Fichas que não existem mais no baralho são ignoradas** (podem ter sido excluídas em outro aparelho enquanto a pessoa estudava offline). Não é erro.
- `404` se o baralho não existir ou não for da pessoa (o site descarta o resultado e avisa).
- **Resposta:** `201` com o `StudySessionResponseDto`.

---

## 4. Rota 3 — baralhos alterados desde uma data

### `GET /api/decks/changes?since=2026-10-30T14:00:00Z`

```json
{
  "decks": [ /* DeckDetailResponseDto, com as fichas */ ],
  "allIds": [ "uuid", "uuid" ],
  "serverTime": "2026-10-30T15:00:00Z"
}
```

- `decks`: baralhos da pessoa com `CreatedAt > since` ou `UpdatedAt > since`, **com as fichas**. Sem `since`, todos.
- `allIds`: ids de **todos** os baralhos atuais da pessoa. O banco não guarda exclusões, então é assim que o site sabe quais baralhos sumiram (excluídos em outro aparelho) e os remove do aparelho.
- `serverTime`: horário do servidor no momento da consulta. O site usa como `since` na próxima vez (não usa o relógio do celular, que pode estar errado).
- A API já atualiza `Deck.UpdatedAt` ao criar, editar e excluir fichas (`CardService`), então mudança em ficha aparece aqui.
- **Atenção à rota:** hoje `GET /api/decks/changes` cai em `[HttpGet("{idOrSlug}")]` e responde 404 ("baralho não encontrado"). Declare `[HttpGet("changes")]` no `DecksController`: segmentos fixos têm prioridade sobre parâmetros no roteamento do ASP.NET Core, mas confira com um teste. Um baralho com slug `changes` deixaria de abrir por slug; se quiser evitar, impeça esse slug na criação.

---

## 5. Como o site usa (para conferir de ponta a ponta)

Arquivo `src/lib/api.ts` do site: `importDeck`, `completeStudySession` e `getDeckChanges`. Se a rota responder **404 ou 405**, o site entende que a API ainda não a tem, lembra disso até recarregar a página e usa o caminho antigo. Qualquer outro erro é tratado como erro de verdade.

Sem a rota de sessão completa, o site usa `start` + `review` + `finish` e guarda o id da sessão para continuar a mesma sessão se a internet cair. Com a rota nova, cada rodada de estudo vira uma chamada só.

Para testar com o site: rode a API com as rotas novas e o site com `NEXT_PUBLIC_API_URL=http://localhost:5000/api`, entre na conta e:
1. Importe um arquivo de `public/modelos/` em "Novo baralho": deve aparecer **uma** chamada `POST /api/decks/import` na aba Network.
2. Estude um baralho até o fim: **uma** chamada `POST /api/study-sessions/complete`.
3. Abra "Meus baralhos": **uma** chamada `GET /api/decks/changes`.

## 6. Pronto quando

- [ ] As três rotas (e a de lote, se fizer) com `[Authorize]` e só dados da pessoa logada.
- [ ] Importação e sessão salvas em transação.
- [ ] `GET /api/decks/changes` não é capturado por `{idOrSlug}`.
- [ ] README da API atualizado na seção "Endpoints Principais".
- [ ] Testado com o site como na seção 5.
