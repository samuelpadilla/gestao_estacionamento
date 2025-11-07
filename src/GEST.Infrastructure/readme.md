## Migrations

Antes de criar uma nova _migration_, é necessário realizar os seguintes passos:
 - [x] Criar a **_Entidade_** (Gest.Dominio/Models);
 - [x] Criar a **_EntidadeConfiguration_** (Gest.Infra/Dados/Configuracoes);
 - [x] Adicionar o contexto da entidade em **GestContexto.cs** (Gest.Infra/Dados)

----
Para executar os comandos abaixo, é necessário utilizar o terminal **_Package Manager Console_**, disponível em: _Tools > NuGet Package Manager > Package Manager Console_

### Criar uma nova migração

```bash
Add-Migration NomeMigracao -Context GestContexto
```

### Atualizar o banco de dados

```bash
Update-Database -Context GestContexto
```

### Reverter a última migração

```bash
Remove-Migration -Context GestContexto
```