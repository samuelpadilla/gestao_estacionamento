# Guia Docker - Gestao Estacionamento

Este guia descreve como subir o ambiente local com Docker para a aplicacao Gestao Estacionamento.

## Estrutura dos arquivos

- `docker-compose.yml`: orquestra o SQL Server (`mssql`) e a API em modo release (`api`)
- `docker-compose.dev.yml`: adiciona o container `api-dev` para desenvolvimento com `dotnet watch`
- `.env`: concentra variaveis de ambiente consumidas pelo Compose (portas, senhas etc.)
- `api/Dockerfile`: build da API em duas fases (build e runtime)

## Configurando variaveis

Assegure-se de revisar o arquivo `.env` antes do primeiro `up`:

- `SA_PASSWORD`: defina uma senha forte para o SQL Server
- `API_HTTP_PORT`: porta HTTP exposta pela API
- `MSSQL_PORT`: porta local que mapeara a porta 1433 do SQL Server

Para ajustes especificos, edite o `.env` e reinicie os containers.

## Subindo o ambiente padrao (API + SQL)

1. Abra um terminal na pasta `docker`
2. Execute `docker compose up -d`
3. Aguarde o `mssql` completar o healthcheck (pode levar ~40s)
4. A API ficara acessivel em `http://localhost:${API_HTTP_PORT}` conforme definido no `.env`

### Comandos uteis

- `docker compose ps`: lista o status dos containers
- `docker compose logs -f api`: acompanha os logs da API
- `docker compose down`: encerra os containers
- `docker compose down -v`: encerra e remove volumes (atenção: apaga dados do SQL)

## Ambiente de desenvolvimento com hot reload

Para trabalhar com `dotnet watch`, execute:

```pwsh
cd docker
mkdir -Force ..\src # garanta que o codigo esteja em ../src antes do comando
docker compose -f docker-compose.yml -f docker-compose.dev.yml up --build
```

O arquivo `docker-compose.dev.yml` aplica estas regras:

- Desativa o container `api` principal (perfil `prod`)
- Sobe `api-dev` baseado em `mcr.microsoft.com/dotnet/sdk:9.0`
- Monta sua pasta `../src` no container e roda `dotnet watch`

Pare o ambiente com `Ctrl+C` ou `docker compose down`.

## Acesso ao SQL Server local

- Host: `localhost`
- Porta: 1433
- Usuario: `sa`
- Senha: valor de `SA_PASSWORD`
- Banco inicial: `GestDb`

Para abrir uma sessao `sqlcmd` dentro do container:

```pwsh
docker compose exec mssql /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$Env:SA_PASSWORD" -C
```

## Resolucao de problemas

- `Login failed for user 'sa'`: confirme que `SA_PASSWORD` atende aos requisitos de complexidade (+12 caracteres, maiusculas, minusculas, numeros e simbolos)
- `Detach from the container output`: use `docker compose up -d` para rodar em segundo plano
- Erros de permissao em volumes: no Windows, rode o terminal como administrador ou ajuste compartilhamento de drive no Docker Desktop
- Mudancas nao refletem na API em hot reload: verifique se o codigo esta na pasta `../src` relativa ao diretorio `docker`

## Limpeza completa

Para remover imagens e volumes criados pelo projeto:

```pwsh
docker compose down -v --rmi all --remove-orphans
```

Execute esse comando apenas quando quiser recriar tudo do zero.
