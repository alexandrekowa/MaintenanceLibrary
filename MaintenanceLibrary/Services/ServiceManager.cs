using System;
using System.Diagnostics;
using System.Security.Principal;
using MaintenanceLibrary.Logging;

namespace MaintenanceLibrary.Services
{
  public static class ServiceManager
  {
    public static bool ServiceStopped { get; private set; } = false;
    public static bool ServiceStarted { get; private set; } = false;

    public static bool IsRunningAsAdmin()
    {
      WindowsIdentity identity = WindowsIdentity.GetCurrent();
      WindowsPrincipal principal = new WindowsPrincipal(identity);
      return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    //Realiza a parada dos serviços que utilizam o banco de dados

    public static void StopServices(string[] serviceNames)
    {
      try
      {
        foreach (var serviceName in serviceNames)
        {
          Process process = new Process();
          process.StartInfo.FileName = "cmd.exe";
          process.StartInfo.Verb = "runas";
          process.StartInfo.Arguments = $"/c sc stop \"{serviceName}\"";
          process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
          process.Start();
          process.WaitForExit();

          if (process.ExitCode == 0)
          {
            EventLogger.LogInfo($"Os serviços {serviceName} foram parados com sucesso.");
            Logger.LogMessage($"Os serviços {serviceName} foram parados com sucesso.");
            ServiceStopped = true;
          }
          else
          {
            EventLogger.LogInfo($"Falha ao parar os serviços {serviceName}. Código de saída: {process.ExitCode}");
            Logger.LogMessage($"Falha ao parar os serviços {serviceName}. Código de saída: {process.ExitCode}");
          }
        }
      }
      catch (Exception ex)
      {
        EventLogger.LogError($"Erro ao parar serviços: " + ex.Message);
        Logger.LogMessage($"Erro ao parar serviços: " + ex.Message);
      }
    }

    //Realiza o inicio dos serviços que utilizam o banco de dados, APÓS A MANUTENÇÃO DO MESMO

    public static void StartServices(string[] serviceNames)
    {
      try
      {
        foreach (var serviceName in serviceNames)
        {
          Process process = new Process();
          process.StartInfo.FileName = "cmd.exe";
          process.StartInfo.Verb = "runas";
          process.StartInfo.Arguments = $"/c sc start \"{serviceName}\"";
          process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
          process.Start();
          process.WaitForExit();

          if (process.ExitCode == 0)
          {
            EventLogger.LogInfo($"Os serviços {serviceName} foram iniciados com sucesso.");
            Logger.LogMessage($"Os serviços {serviceName} foram iniciados com sucesso.");
            ServiceStarted = true;
            break;
          }
          else
          {
            EventLogger.LogInfo($"Falha ao iniciar o serviço {serviceName}");
            Logger.LogMessage($"Falha ao iniciar o serviço {serviceName}");
          }
        }
      }
      catch (Exception ex)
      {
        EventLogger.LogError($"Erro ao iniciar serviços: {ex.Message}.");
        Logger.LogError($"Erro ao iniciar serviços: {ex.Message}.");
      }
    }
  }
}