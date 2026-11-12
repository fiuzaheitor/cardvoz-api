# Revisão antes da publicação (nov/2026)

Code-review e análise de segurança da API inteira antes da entrega. **Nenhuma falha de segurança** com exploração real foi encontrada. As correções abaixo são de funcionamento.

## O que mudou

1. **`sourceDeckId` no detalhe do baralho** (`DeckDetailResponseDto`, usado por `GET /api/decks/{id}`, `/changes` e `/import`). Sem ele, o site gravava a cópia de um baralho pronto sem a origem e mostrava "Adicionar" de novo: cada toque criava outra cópia.
2. **Acertos e erros com dois envios ao mesmo tempo.** O EF lê o número, soma em memória e grava o total; dois aparelhos juntos gravavam o mesmo total e uma resposta sumia. `/complete` e `/review` agora travam o baralho (`pg_advisory_xact_lock`) dentro de uma transação (`Data/DbLocks.cs`).
3. **Pedido repetido ao mesmo tempo** (`/decks/import`, `/study-sessions/complete`) e **slug repetido** (criar, importar, renomear e copiar dois baralhos de mesmo nome juntos) davam 500. Com a trava, o segundo pedido espera e responde 200 com o que já foi salvo, ou ganha outro slug.
4. **Histórico com data no futuro** (`Card.SetHistory`): relógio do aparelho adiantado travava a revisão da ficha até passar a data falsa. A data vira a hora do servidor, como no `/complete`.
5. **Renovação de sessão numa transação** (`AuthService.RefreshAsync`): se o banco falhasse entre revogar o token antigo e criar o novo, a próxima tentativa contava como reuso e encerrava todas as sessões da pessoa.
6. **Validação:** sessão sem `startedAt`/`completedAt`/`answeredAt` (virava 01/01/0001) e item `null` na lista de fichas ou respostas (500) agora respondem 400.
7. Documentação: junção de contas com e-mail confirmado (AGENTS e README) e o "Manter ativo" a cada 10 minutos.

## Testes

`tests/cardvoz-api.Tests/RevisaoPrePublicacaoTests.cs` cobre 1, 4 e 6. As travas só agem no PostgreSQL (no SQLite dos testes não fazem nada); confira no ambiente de teste com dois envios simultâneos se quiser ver a espera.

## Depende de

Nada no banco: não há migration. O site 0.12.x funciona com a API antiga e com a nova; com a nova, "Já está nos seus baralhos." aparece certo depois de adicionar um baralho pronto.
