# Tarefa: "Esqueci minha senha", baixar os dados e excluir a conta

**Banco:** migration `20261112000100_redefinir_senha.sql` (`cardvoz-db`).
**Site:** versão 0.10.0 (`/esqueci-senha/`, `/redefinir-senha/` e "Seus dados" no Perfil).
**Situação (12/11/2026):** implementada na branch `feat/revisao-espacada`.

## Esqueci minha senha

- `POST /api/auth/forgot-password` `{ email }` → sempre a mesma resposta (não revela quais e-mails têm conta). Sem SMTP: `503`.
- O e-mail traz `SITE_URL/redefinir-senha/#codigo=<código>`. O código vai depois do `#`, então o navegador não o manda a servidor nenhum.
- Código: 256 bits aleatórios, só o hash SHA-256 no banco, uso único, 1 hora. Pedir de novo invalida o anterior; no máximo um e-mail por minuto por conta.
- `POST /api/auth/reset-password` `{ token, password }` → troca a senha, zera o bloqueio por tentativas e revoga todos os tokens de renovação (`204`). Código usado ou vencido: `400`.
- Conta que entra só com o Google também pode criar uma senha assim: o link prova que a pessoa recebe os e-mails do endereço.

**Configurar no Render:** `SMTP_HOST`, `SMTP_PORT` (587), `SMTP_USER`, `SMTP_PASSWORD`, `SMTP_FROM` e `SITE_URL`. Exemplo com Gmail: `smtp.gmail.com`, uma conta da equipe e uma "senha de app" (Conta Google → Segurança → Senhas de app). Provedores como Brevo e Resend têm plano gratuito e entregam melhor.

## Baixar os dados e excluir a conta (LGPD)

- `GET /api/account/export` → JSON com conta, baralhos, fichas (acertos, erros, revisão) e sessões. Sem hash de senha nem tokens.
- `POST /api/account/delete` `{ confirmEmail, password? }` → exclui tudo em cascata (`204`). E-mail ou senha errados: `400` com o campo. Única pessoa admin: `409`.
