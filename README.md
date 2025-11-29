# dotnet-ai-cloud-playground

Playground em .NET para integrar com diferentes provedores de IA na nuvem (OpenAI, Azure OpenAI, etc.), com foco em boas práticas de arquitetura e código limpo.

## Objetivo

Este repositório demonstra como um desenvolvedor .NET pode:

- Consumir APIs de modelos de linguagem (LLMs) na nuvem;
- Organizar chamadas a IA com boa arquitetura (camadas, interfaces, injeção de dependência);
- Aplicar boas práticas como resilência (Polly), uso de `HttpClientFactory`, configuração segura e testes.

A ideia é servir como um **laboratório de integrações com IA**, crescendo ao longo do tempo com novos exemplos.

## Tecnologias

- .NET 8 (ASP.NET Core Minimal APIs)
- C#
- OpenAI / Azure OpenAI (planejado)
- `HttpClientFactory`
- `IOptions` para configuração tipada

## Roadmap inicial

- [ ] Criar Minimal API básica com um endpoint de `/health`.
- [ ] Adicionar endpoint de `/chat/openai` usando um provedor (OpenAI ou Azure OpenAI).
- [ ] Isolar a integração em um serviço (`IAiChatClient`) e implementação concreta.
- [ ] Adicionar políticas de resiliência (retries / timeout) com Polly.
- [ ] Criar testes unitários para o serviço de integração.
- [ ] Adicionar suporte a múltiplos provedores (feature flags ou configuração).

## Como rodar (preview)

> Instruções serão atualizadas quando o primeiro projeto estiver criado.

```bash
dotnet restore
dotnet run --project src/DotnetAiCloudPlayground.Api
```

## Licença

Definir (MIT recomendada para projetos abertos).
