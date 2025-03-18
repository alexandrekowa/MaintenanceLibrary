using System;
using System.Threading.Tasks;
using MaintenanceLibrary.Logging;
using MaintenanceLibrary.Services;
using MaintenanceLibrary.Database;
using MaintenanceLibrary.Delete;
using MaintenanceLibrary.BackupDb;

namespace MaintenanceLibrary.Main
{
  public class Manutencao
  {
    //REALIZA DE FATO A MANUTENÇÃO NO BANCO DE DADOS, IMPORTANDO AS PASTAS PARA O SERVIÇO

    public static async void ManutencaoBd()
    {
      async Task ExecutaBkp()
      {
        string connectionString = "Driver={PostgreSQL ANSI(x64)};Server=127.0.0.1;Port=5432;Database=__nome_bd__;Uid=postgres;Password=__pw__;";
        
        string backupDirectory = @"C:\Servico\Backup"; // Diretório onde o backup será salvo

        string pgDumpPath = @"C:\Program Files\PostgreSQL\9.5\bin\pg_dump.exe"; // Caminho para o pg_dump
      
        PostgresBackup pgBkp = new PostgresBackup(connectionString, backupDirectory, pgDumpPath);
        bool bkpSuccess = await pgBkp.PerformBackup();

        if (bkpSuccess)
        {
          Logger.LogMessage("Backup finalizado!");
        }
        else
        {
          Logger.LogError("Falha ao realizar o serviço de Backup!");
        }
      }
            
      //string[] serviceNames = { "AZCV1000 Service Control", "AZCV1000 Server", "AZCV1000 Centro Controle", "AZLaneControlSRV" }, serviceNames2 = { "AZCV1000 Service Control" };
      //string[] serviceNames = { "DoSvc" }, serviceNames2 = { "DoSvc" };   //////// SERVIÇO DE TESTE DA APLICAÇÃO ////////

      Logger.HandleExistingLog();
      Logger.CreateLogDirectory();

      if (!ServiceManager.IsRunningAsAdmin())
      {
        EventLogger.LogInfo("Este aplicativo deve ser executado como administrador para parar o serviço.");
        Logger.LogMessage("Este aplicativo deve ser executado como administrador para parar o serviço.");
        return;
      }

      /*ServiceManager.StopServices(serviceNames);

      if (ServiceManager.ServiceStopped)
      {
        DatabaseManager.PerformDatabaseMaintenance();
        Logger.LogMessage("Manutenção finalizada.");
      }
      else
      {
        EventLogger.LogError("Os serviços não foram parados. Não é possível executar as tarefas de manutenção.");
        Logger.LogMessage("Os serviços não foram parados. Não é possível executar as tarefas de manutenção.");
      }

      //ServiceManager.StartServices(serviceNames2);

      if (ServiceManager.ServiceStarted)
      {
        await ExecutaBkp();
      }*/

      var bkpTask = ExecutaBkp();
      await bkpTask;
      
      if (bkpTask.IsCompleted)
      {
        DeleteBkp.DeletarBkp();
      }
    }
  }
}