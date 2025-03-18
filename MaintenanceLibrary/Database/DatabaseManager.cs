using System;
using System.Data.Odbc;
using System.Diagnostics;
using MaintenanceLibrary.Logging;

namespace MaintenanceLibrary.Database
{
  public static class DatabaseManager
  {
    //CONEXÃO COM O BANCO DE DADOS, E COMANDOS A SEREM REALIZADOS PARA A MANUTENÇÃO NO MESMO

    // Método que realiza os comandos no banco de dados
    public static void PerformDatabaseMaintenance()
    {
      var connectionString = "Driver={PostgreSQL ANSI(x64)};Server=127.0.0.1;Port=5432;Database=__nome_bd__;Uid=postgres;Password=__pw__";

      using (var dbConnection = new OdbcConnection(connectionString))
      {
        try
        {
          EventLogger.LogInfo("Tentando conectar ao banco de dados local...");
          Logger.LogMessage("Tentando conectar ao banco de dados local...");
          //Logger.Log("Tentando conectar ao banco de dados local...", EventLogEntryType.Information);
          dbConnection.Open();
          EventLogger.LogInfo("Conexão com o banco de dados local aberta.");
          Logger.LogMessage("Conexão com o banco de dados local aberta.");

          using (var dbCommand = dbConnection.CreateCommand())
          {
            ExecuteCommand(dbCommand, "VACUUM", "Finalizado VACUUM com sucesso");
            ExecuteCommand(dbCommand, "REINDEX TABLE log_erro_coletor", "Finalizado REINDEX na tabela log_erro_coletor com sucesso");
            ExecuteCommand(dbCommand, "REINDEX TABLE trafego_coletor", "Finalizado REINDEX na tabela trafego_coletor com sucesso");
            ExecuteCommand(dbCommand, "REINDEX TABLE ocr_data", "Finalizado REINDEX na tabela ocr_data com sucesso");
          }
        }
        catch (Exception e)
        {
          EventLogger.LogError("Mensagem de erro: " + e.Message);
          Logger.LogError("Mensagem de erro: " + e.Message);
        }
      }
    }

    //Método que realiza o registro dos comandos no banoc de dados 
    private static void ExecuteCommand(OdbcCommand dbCommand, string commandText, string successMessage)
    {
      Stopwatch stopwatch = new Stopwatch();
      try
      {
        EventLogger.LogInfo($"Iniciando {commandText}");
        Logger.LogMessage($"Iniciando {commandText}");
        dbCommand.CommandText = commandText;
        dbCommand.CommandTimeout = 7200;

        stopwatch.Start();
        dbCommand.ExecuteNonQuery();
        stopwatch.Stop();

        TimeSpan ts = stopwatch.Elapsed;
        int elapsedSecondsRounded = (int)Math.Round(ts.TotalSeconds);
        EventLogger.LogInfo($"{successMessage}, em {elapsedSecondsRounded} segundos");
        Logger.LogMessage($"{successMessage}, em {elapsedSecondsRounded} segundos");
      }
      catch (Exception e)
      {
        EventLogger.LogError($"Mensagem de erro ao executar {commandText}: " + e.Message);
        Logger.LogError($"Mensagem de erro ao executar {commandText}: " + e.Message);
      }
      finally
      {
        stopwatch.Reset();
      }
    }
  }
}