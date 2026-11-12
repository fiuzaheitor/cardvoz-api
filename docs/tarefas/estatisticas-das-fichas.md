# Tarefa: acertos e erros das fichas e "Estudar só as que errei"

**Repositório:** cardvoz-api (branch `feat/estatisticas-das-fichas`)
**Banco (`cardvoz-db`):** migration `20261111000000_estatisticas_das_fichas.sql` (branch `feat/estatisticas-das-fichas`). Aplique antes de publicar a API.
**Site (`cardvoz`):** versão 0.9.0. Funciona com a API antiga: guarda os números só no aparelho até a API nova estar no ar.
**Situação (11/11/2026):** implementada e testada; aguarda a revisão do dono.

---

## 1. Pedido da cliente

Guardar quantas vezes ela acertou e errou e estudar só as fichas que errou. Cada baralho funciona como uma matéria, então os números ficam na ficha e são somados por baralho.

## 2. O que mudou no banco

- `"Cards"`: `"CorrectCount"` e `"IncorrectCount"` (inteiros, padrão 0, `CHECK >= 0`), `"LastAnsweredAt"` e `"LastWasCorrect"` (nulos = nunca estudada).
- `"Decks"`: `"LastStudiedAt"` (hora em que o último estudo chegou à conta).
- A migration preenche tudo com o histórico que já existe em `"StudyReviews"` e `"StudySessions"`. Não apaga dados.

`"StudyReviews"` continua guardando cada resposta; as colunas novas são o resumo, para a API devolver os números com o baralho sem somar o histórico a cada leitura.

## 3. O que mudou na API

- `Card` e `Deck` ganharam as colunas; `CardResponseDto`, `DeckResponseDto` e `DeckDetailResponseDto` devolvem os campos (nulos ficam fora do JSON).
- `Card.RecordAnswer(wasCorrect, answeredAt)` soma uma resposta e só troca a "última resposta" se ela for mais nova (uma sessão feita sem internet pode chegar depois de outra mais recente). Usado por `POST /review` e `POST /complete`.
- `POST /api/cards` e `POST /api/decks/import` aceitam `correctCount`, `incorrectCount`, `lastAnsweredAt` e `lastWasCorrect` (0 a 100000), para levar à conta um baralho estudado sem login.
- `Deck.LastStudiedAt` usa a hora do servidor: `GET /api/decks/changes` compara com a hora do servidor e inclui baralhos estudados depois do `since`. Assim, os números de um aparelho aparecem nos outros.

## 4. Como o site usa

- Ao terminar uma rodada, o site soma os números nas fichas guardadas no aparelho e, com conta, manda a rodada por `POST /api/study-sessions/complete` (com `id`, para não somar duas vezes num envio repetido).
- Fichas criadas com conta vão sem números: o estudo delas vai como item próprio da fila. Só "Levar para a conta" envia o histórico do aparelho.
- "Estudar só as que errei" usa as fichas com `lastWasCorrect = false`.

## 5. Testado

- Migration aplicada num PostgreSQL 16 com histórico de exemplo: números e `LastStudiedAt` preenchidos corretamente; `CHECK` recusa valor negativo.
- API rodando com esse banco: `import` (201, 200 repetido, 409 de outra pessoa, 400 inválido, limite de 1000), lote, `changes` (inclusive baralho estudado depois), `complete` (ignora fichas de fora do baralho e de outra pessoa, 200 repetido sem somar de novo, 404, 400), sessão antiga que chega depois não troca a última resposta.
- Site ligado a essa API (Playwright): levar para a conta em uma chamada, estudo em uma chamada, lista atualizada com `GET /api/decks/changes`.
