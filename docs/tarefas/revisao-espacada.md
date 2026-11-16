# Tarefa: revisão espaçada ("Revisar agora")

**Banco:** migrations `20261112000000_revisao_espacada.sql` e `20261116000000_revisao_tipo_anki.sql` (`cardvoz-db`; a segunda só acrescenta `"ReviewEase"`). Aplique antes de publicar a API.
**Site:** a partir da versão 0.15.0. Versões anteriores (caixa de Leitner) continuam funcionando com esta API.

## Pedido da cliente

Mostrar quais fichas revisar depois de horas, dias, semanas e meses.

## Regra (estilo do Anki, algoritmo SM-2)

A primeira versão usava a caixa de Leitner: 7 níveis com esperas fixas, iguais para todas as fichas. Desde nov/2026 a regra segue o Anki, com as duas respostas do CardVoz: **"Acertei" vale como "Bom"** e **"Errei" vale como "De novo"**.

Cada ficha tem a data da próxima revisão (`NextReviewAt`) e uma facilidade (`ReviewEase`, em milésimos: 2500 = 2,5). O **intervalo** não é guardado: são os dias entre `LastAnsweredAt` e `NextReviewAt` (`Card.ReviewIntervalDays`, calculado; 0 = ficha nova, ou errada da última vez). Assim ele não diverge das datas, e as fichas que estavam na caixa de Leitner continuam de onde estavam, inclusive as respondidas pela API anterior depois da migration.

| Resposta | Situação da ficha | Intervalo novo | Facilidade |
|---|---|---|---|
| Acertei | nova, ou errada da última vez (intervalo 0) | 1 dia | não muda |
| Acertei | na data ou atrasada | (intervalo + metade do atraso) × facilidade, no mínimo intervalo + 1 | não muda |
| Acertei | antes da data | tempo desde a última resposta × facilidade, no mínimo o intervalo atual | não muda |
| Errei | em revisão (intervalo de 1 dia ou mais) | 0: volta para "Revisar agora" na hora | cai 0,2 (mínimo 1,3) |
| Errei | já errada, ou nova | 0 | não muda |

- Com a facilidade inicial (2,5) e respostas na data, os intervalos são 1, 3, 8, 20, 50, 125 e 313 dias. O máximo é 365 dias.
- **Antes da data** acontece em "Estudar" (todas as fichas): quem estuda o baralho inteiro todo dia não empurra as fichas para daqui a meses, porque só conta o tempo que passou de verdade.
- Diferenças para o Anki: não há "Difícil" nem "Fácil" (a tela tem dois botões, por acessibilidade), então a facilidade só cai; não há passos de aprendizado em minutos (a ficha errada volta na hora, na mesma rodada de revisão); e não há variação aleatória no intervalo, para o site e a API calcularem o mesmo valor.
- Fica em `Card.RecordAnswer` (usado por `POST /review` e `POST /complete`), com as constantes em `Card`. O site tem a mesma conta em `src/lib/studyStats.ts`, com os mesmos casos de teste (`CardTests.cs` e `studyStats.test.ts`): mudou aqui, mude lá. As contas usam só inteiros (milissegundos e milésimos), para dar o mesmo resultado em C# e em JavaScript.
- Só a resposta mais recente muda a revisão (sessão antiga que chega depois não mexe).
- `POST /api/cards` e `POST /api/decks/import` aceitam `nextReviewAt` e `reviewEase` do aparelho, ao levar para a conta um baralho estudado sem login (`Card.SetHistory`). Quem acertou por último fica com a revisão entre 1 dia e 1 ano depois da última resposta; quem errou, na hora. O site antigo não manda a facilidade: fica 2,5.
- A ficha devolve `nextReviewAt` (fora do JSON quando nulo: nunca estudada) e `reviewEase`. `reviewLevel` não existe mais; o site antigo trata a falta dele como nível 0 e segue pela data.
- A coluna `"ReviewLevel"` continua no banco, sem uso, para a API anterior funcionar entre a migration e a publicação desta.
