using System;
using System.Diagnostics;

namespace MaintenanceLibrary.Logging
{
  public static class EventLogger
  {
    private static string sourceName = "Manutenção-BancoDadosLocal", logName = "Application";

    static EventLogger()
    {
      if (!EventLog.SourceExists(sourceName))
      {
        EventLog.CreateEventSource(sourceName, logName);
      }
    }

    // Método de registro das mensagens de ERRO dos comandos no Visualizador de eventos do Windows
    public static void LogInfo(string message)
    {
      EventLog.WriteEntry(sourceName, message, EventLogEntryType.Information);
    }

    // Método de registro das mensagens de ERRO dos comandos no Visualizador de eventos do Windows
    public static void LogError(string message)
    {
      EventLog.WriteEntry(sourceName, message, EventLogEntryType.Error);
    }
  }
}