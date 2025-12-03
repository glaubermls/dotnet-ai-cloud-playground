# dotnet-ai-cloud-playground

Playground em .NET para integrar com diferentes provedores de IA na nuvem (OpenAI, Azure OpenAI, etc.), com foco em boas práticas de arquitetura e código limpo.

## Objetivo

Este repositório demonstra como um desenvolvedor .NET pode:

- Consumir APIs de modelos de linguagem (LLMs) na nuvem;
- Organizar chamadas a IA com boa arquitetura (camadas, interfaces, injeção de dependência);
- Aplicar boas práticas como resilência (Polly), uso de `HttpClientFactory`, configuração segura e testes.

A ideia é servir como um **laboratório de integrações com IA**, crescendo ao longo do tempo com novos exemplos.

## Tecnologias

- .NET 10
- C# 14
- ASP.NET Core Web API
- OpenAI API
- Polly (resilience policies)
- `HttpClientFactory`
- `IOptions` para configuração tipada
- Swagger/OpenAPI

## Arquitetura

O projeto segue os princípios de **Clean Architecture** e **Hexagonal Architecture** (Ports & Adapters):

```
src/
??? DotnetAiCloudPlayground.Api/          # Camada de apresentação (Controllers, Program.cs)
??? DotnetAiCloudPlayground.Core/         # Camada de domínio (Domain, Application, Ports)
??? DotnetAiCloudPlayground.Infrastructure/ # Camada de infraestrutura (Adapters, Extensions)
```

### Camadas

- **Api**: Controllers e configuração da aplicação
- **Core**: Domain entities, Use Cases e Ports (interfaces)
- **Infrastructure**: Implementação concreta dos Adapters (OpenAI, etc.)

## Funcionalidades Implementadas

### ? Endpoints

- `GET /health` - Health check da API
- `POST /chat/openai` - Chat com OpenAI

### ? Integração com OpenAI

- Adaptador implementado seguindo o padrão Ports & Adapters
- Suporte para modelos GPT (gpt-4o-mini, gpt-4, gpt-3.5-turbo)
- Configuração customizável via `appsettings.json` ou variáveis de ambiente

### ? Resiliência

- Retry policy com backoff exponencial (3 tentativas)
- Timeout policy (30 segundos)
- Tratamento de erros transientes com Polly

### ? Observabilidade

- Logs estruturados
- Métricas de latência
- Tracking de uso de tokens

## Como Rodar

### Pré-requisitos

- .NET 10 SDK
- Chave de API da OpenAI

### Configuração

1. Clone o repositório:
```bash
git clone https://github.com/glaubermls/dotnet-ai-cloud-playground.git
cd dotnet-ai-cloud-playground/DotnetAiCloudPlayground
```

2. Configure sua chave de API da OpenAI:

**Opção 1: Variável de ambiente (Recomendado)**
```bash
# Windows (PowerShell)
$env:OPENAI_API_KEY = "sk-..."

# Linux/macOS
export OPENAI_API_KEY="sk-..."
```

**Opção 2: launchSettings.json**
Edite `src/DotnetAiCloudPlayground.Api/Properties/launchSettings.json`:
```json
{
  "environmentVariables": {
    "OPENAI_API_KEY": "sk-..."
  }
}
```

3. Restaure as dependências:
```bash
dotnet restore
```

4. Execute o projeto:
```bash
dotnet run --project src/DotnetAiCloudPlayground.Api
```

5. Acesse a documentação Swagger:
```
http://localhost:5086/
```

### Exemplo de Uso

**Request:**
```bash
POST http://localhost:5086/chat/openai
Content-Type: application/json

{
  "prompt": "Explique o que é Clean Architecture em poucas palavras"
}
```

**Response:**
```json
{
  "content": "Clean Architecture é um padrão de design...",
  "model": "gpt-4o-mini",
  "tokensUsed": 150,
  "createdAt": "2024-01-15T10:30:00Z"
}
```

## Configuração

Edite `src/DotnetAiCloudPlayground.Api/appsettings.json`:

```json
{
  "OpenAI": {
    "BaseUrl": "https://api.openai.com/v1/",
    "Model": "gpt-4o-mini",
    "MaxTokens": 256,
    "Temperature": 0.2
  }
}
```

### Parâmetros

- **BaseUrl**: URL base da API do OpenAI
- **Model**: Modelo a ser utilizado (gpt-4, gpt-4o-mini, gpt-3.5-turbo)
- **MaxTokens**: Número máximo de tokens na resposta
- **Temperature**: Criatividade do modelo (0.0 a 1.0)

## Roadmap

- [x] Criar API básica com endpoint de `/health`
- [x] Adicionar endpoint de `/chat/openai` 
- [x] Isolar a integração em um serviço (`IChatModelPort`) e implementação concreta
- [x] Adicionar políticas de resiliência (retries / timeout) com Polly
- [x] Implementar logs estruturados e observabilidade
- [ ] Criar testes unitários para o serviço de integração
- [ ] Adicionar suporte a Azure OpenAI
- [ ] Implementar streaming de respostas
- [ ] Adicionar cache de respostas
- [ ] Suporte a múltiplos provedores (feature flags)

## Estrutura de Pacotes

```xml
<!-- Core -->
<PackageReference Include="Microsoft.Extensions.Options" />

<!-- Infrastructure -->
<PackageReference Include="Polly" />
<PackageReference Include="Polly.Extensions.Http" />

<!-- Api -->
<PackageReference Include="Swashbuckle.AspNetCore" />
```

## Contribuindo

Contribuições são bem-vindas! Sinta-se à vontade para:

1. Fazer fork do projeto
2. Criar uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abrir um Pull Request

## Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

## Autor

[@glaubermls](https://github.com/glaubermls)

## Links Úteis

- [OpenAI API Documentation](https://platform.openai.com/docs/api-reference)
- [.NET 10 Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Polly Documentation](https://github.com/App-vNext/Polly)
