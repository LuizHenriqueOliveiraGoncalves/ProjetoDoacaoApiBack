# Feed the Future BR - Backend

## Descrição do Projeto

API RESTful desenvolvida em ASP.NET Core para gerenciar o sistema de doações da plataforma Feed the Future BR. Responsável pela autenticação, autorização, cadastro, consulta, reserva e exclusão de doações, além da gestão de usuários (ONGs e estabelecimentos).

---

## Tecnologias Utilizadas

- ASP.NET Core 6+
- Entity Framework Core (EF Core) para acesso a dados
- SQL Server como banco de dados relacional
- JWT para autenticação e autorização segura
- Serviços de e-mail para recuperação de senha
- Ferramentas de migração e seed para controle do banco de dados

---

## Funcionalidades

- Cadastro e login de usuários com roles diferenciadas (ONGs, negócios)
- Gestão completa de perfil de usuário
- CRUD completo para doações (criação, leitura, atualização, exclusão)
- Reserva de doações por ONGs
- Controle de autorização para operações (ex: só criador pode excluir doação)
- Recuperação de senha via email com token seguro
- Endpoints para estatísticas de usuários e doações

---

## Estrutura do Projeto

- /Controllers/ # Controladores REST que expõem endpoints da API
- /Data/ # Contexto do banco de dados e configurações
- /Models/ # Modelos e DTOs usados na aplicação
- /Services/ # Serviços de negócio (autenticação, email, etc)
- /Migrations/ # Arquivos de migração do EF Core para banco de dados
- appsettings.json # Configurações da aplicação (conexão DB, JWT, SMTP)
- /Program.cs # Configuração da aplicação e pipeline
---

## Endpoints Principais

- `POST /api/auth/register` — Cadastro de usuário
- `POST /api/auth/login` — Login e obtenção de token JWT
- `GET /api/auth/profile` — Recupera perfil do usuário autenticado
- `PUT /api/auth/profile` — Atualiza dados do usuário
- `POST /api/auth/forgot-password` — Solicita recuperação de senha
- `POST /api/auth/reset-password` — Reseta a senha com token válido
- `GET /api/donation/mydonations` — Lista doações do usuário
- `GET /api/donation/all` — Lista todas doações (ONG)
- `PATCH /api/donation/{id}/reserve` — Reserva uma doação
- `DELETE /api/donation/{id}` — Exclui uma doação (apenas criador)

---