# Tarefa: revisão espaçada ("Revisar agora")

**Banco:** migration `20261112000000_revisao_espacada.sql` (`cardvoz-db`). Aplique antes de publicar a API.
**Site:** versão 0.10.0. Funciona com a API antiga (calcula a revisão no aparelho).

## Pedido da cliente

Mostrar quais fichas revisar depois de horas, dias, semanas e meses.

## Regra (caixa de Leitner)

Cada ficha tem um nível de 0 a 6 (`ReviewLevel`) e a data da próxima revisão (`NextReviewAt`).

| Resposta | Nível | Próxima revisão |
|---|---|---|
| Errei | volta a 0 | na hora |
| Acertei | sobe 1 (até 6) | 1 dia, 3 dias, 1 semana, 2 semanas, 1 mês ou 3 meses (níveis 1 a 6) |

- Fica em `Card.RecordAnswer` (usado por `POST /review` e `POST /complete`) e em `Card.ReviewIntervals`. O site tem a mesma tabela em `src/lib/studyStats.ts`: mudou aqui, mude lá.
- Só a resposta mais recente muda o nível (sessão antiga que chega depois não mexe).
- `POST /api/cards` e `POST /api/decks/import` aceitam `reviewLevel` e `nextReviewAt` do aparelho, ao levar para a conta um baralho estudado sem login.
- A ficha devolve `reviewLevel` e `nextReviewAt` (este fica fora do JSON quando nulo: nunca estudada).
