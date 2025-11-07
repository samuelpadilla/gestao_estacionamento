## Migrations

Antes de criar uma nova _migration_, é necessário realizar os seguintes passos:
 - [x] Criar a **_Entidade_** (GEST.Domain/Entities);
 - [x] Criar a **_EntidadeConfiguration_** (GEST.Infraestructure/Persistence/Configurations);
 - [x] Adicionar o contexto da entidade em **GestContexto.cs** (GEST.Infraestructure/Persistence)

----
Para executar os comandos abaixo, é necessário utilizar o terminal **_Package Manager Console_**, disponível em: _Tools > NuGet Package Manager > Package Manager Console_

### Criar uma nova migração

```bash
Add-Migration NomeMigracao -Context GestDbContexto
```

### Atualizar o banco de dados

```bash
Update-Database -Context GestDbContexto
```

### Reverter a última migração

```bash
Remove-Migration -Context GestDbContexto
```