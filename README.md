# CardVoz API 🎙️🃏

API RESTful em **C# (.NET 9)** desenvolvida para o projeto **CardVoz** (Flashcards acessíveis para estudo por voz), do Instituto Federal do Tocantins (IFTO - Campus Palmas).

---

## 🏛️ Arquitetura

O projeto adota uma arquitetura em camadas limpas, com separação de responsabilidades e foco em desacoplamento:

```
cardvoz-api/
├── Controllers/       # Controladores REST com anotações OpenAPI/Swagger
├── DTOs/              # Objetos de transferência de dados (Request / Response)
├── Models/            # Entidades do domínio (Deck, Card, StudySession, StudyReview)
├── Data/              # EF Core DbContext, mapeamento fluente e seed inicial
├── Services/          # Interfaces e regras de negócio
├── Program.cs         # Inicialização, injeção de dependência, CORS e Swagger
└── appsettings.json   # Configurações da aplicação
```

---

## ⚡ Recursos e Diferenciais

- **Acessibilidade por Voz**: Rastreamento de respostas acionadas por comandos de voz (`voiceUsed`) e campo `voiceNotes` para orientações fonéticas em cada ficha.
- **Banco de Dados Flexível**:
  - **SQLite por padrão** (`cardvoz.db`): Funciona instantaneamente sem necessidade de instalação de banco externo.
  - **MySQL (Pomelo)**: Suporte pronto ativado automaticamente quando uma connection string de MySQL for informada no arquivo `.env` ou `appsettings.json`.
- **Carga de Dados Automática**: Popula automaticamente no primeiro boot os 4 baralhos iniciais e suas fichas reais de estudo.
- **Documentação Interativa**: Swagger UI acessível em `/swagger`.

---

## 🚀 Como Executar

### Pré-requisitos
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Passos
```bash
# Restaurar dependências
dotnet restore

# Compilar o projeto
dotnet build

# Executar a API
dotnet run
```

A API iniciará por padrão em:
- **Swagger UI:** [http://localhost:5000/swagger](http://localhost:5000/swagger)
- **Base URL:** `http://localhost:5000/api`

---

## 📋 Endpoints Principais

### Baralhos (`/api/decks`)
- `GET /api/decks` - Lista todos os baralhos (suporta `?category=` e `?search=`)
- `GET /api/decks/categories` - Lista categorias cadastradas
- `GET /api/decks/{idOrSlug}` - Obtém baralho detalhado por ID ou Slug semântico
- `POST /api/decks` - Cria um novo baralho
- `PUT /api/decks/{id}` - Atualiza baralho existente
- `DELETE /api/decks/{id}` - Remove baralho e fichas associadas

### Cartões (`/api/cards`)
- `GET /api/decks/{deckId}/cards` - Lista cartões de um baralho
- `GET /api/cards/{id}` - Detalhes de um cartão
- `POST /api/cards` - Adiciona novo cartão a um baralho
- `PUT /api/cards/{id}` - Atualiza um cartão
- `DELETE /api/cards/{id}` - Remove um cartão

### Sessões de Estudo (`/api/study-sessions`)
- `POST /api/study-sessions/start` - Inicia sessão de estudo por `deckId` ou `deckSlug`
- `POST /api/study-sessions/{id}/review` - Registra acerto/erro e flag `voiceUsed`
- `POST /api/study-sessions/{id}/finish` - Conclui a sessão e consolida pontuação
- `GET /api/study-sessions/{id}` - Consulta detalhes de uma sessão
- `GET /api/study-sessions/history` - Histórico de sessões finalizadas

---

## 📄 Licença
Projeto acadêmico de extensão do IFTO Palmas.
