using System;
using System.Data.Odbc;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using MaintenanceLibrary.Logging;

namespace MaintenanceLibrary.BackupDb
{
  public class PostgresBackup
  {
    public static Task WaitForExitAsync(Process process, CancellationToken cancellationToken = default)
    {
      var tcs = new TaskCompletionSource<bool>();

      process.EnableRaisingEvents = true;
      process.Exited += (sender, args) => tcs.TrySetResult(true);

      if (cancellationToken != default)
      {
        cancellationToken.Register(() => tcs.SetCanceled());
      }

      if (process.HasExited)
      {
        tcs.TrySetResult(true);
      }

      return tcs.Task;
    }

    private string _connectionString;
    private string _backupDirectory;
    private string _pgDumpPath;

    public PostgresBackup(string connectionString, string backupDirectory, string pgDumpPath)
    {
      _connectionString = connectionString;
      _backupDirectory = backupDirectory;
      _pgDumpPath = pgDumpPath;
    }

    public async Task<bool> PerformBackup()
    {
      Process process = null;
      try
      {
        if (!Directory.Exists(_backupDirectory))
        {
          Directory.CreateDirectory(_backupDirectory);
        }

        // Nome do arquivo de backup com data e hora
        string backupFileName = Path.Combine(_backupDirectory, $"{DateTime.Now:yyyyMMdd_HHmmss}_backup.backup");

        // Extraindo parâmetros da connection string usando ODBC
        string host = "127.0.0.1";
        string port = "5432"; // Porta padrão
        string database = "radarbd";
        string username = "postgres";
        string password = "azcv123";

        using (OdbcConnection connection = new OdbcConnection(_connectionString))
        {
          await connection.OpenAsync();
          Logger.LogMessage("Conexão com banco aberta");
          EventLogger.LogInfo("Conexão com banco aberta");

          var connectionStringParts = _connectionString.Split(';');
          foreach (var part in connectionStringParts)
          {
            if (part.StartsWith("Server="))
            {
              host = part.Split('=')[1];
            }
            else if (part.StartsWith("Port="))
            {
              port = part.Split('=')[1];
            }
            else if (part.StartsWith("Database="))
            {
              database = part.Split('=')[1];
            }
            else if (part.StartsWith("UID="))
            {
              username = part.Split('=')[1];
            }
            else if (part.StartsWith("PWD="))
            {
              password = part.Split('=')[1];
            }
          }
        }
        // Comando para realizar o backup
        string arguments = $"-h {host} -p {port} -U {username} -F c -b -v -f \"{backupFileName}\" {database}";

        // Configurando o processo para chamar o pg_dump
        ProcessStartInfo processInfo = new ProcessStartInfo
        {
          FileName = _pgDumpPath,
          Arguments = arguments,
          RedirectStandardInput = true,
          RedirectStandardOutput = true,
          RedirectStandardError = true,
          UseShellExecute = false,
          CreateNoWindow = true
        };

        // Definindo a senha do banco de dados
        processInfo.EnvironmentVariables["PGPASSWORD"] = password;

        process = new Process
        {
          StartInfo = processInfo
        };

        process.OutputDataReceived += (sender, args) => Logger.LogMessage($"{args.Data}");
        process.ErrorDataReceived += (sender, args) =>
        {
          if (!string.IsNullOrEmpty(args.Data))
          {
            Logger.LogMessage($"{args.Data}");
          }
        };
        process.Start();

        process.BeginOutputReadLine();// Inicia leitura assíncrona da saída padrão
        process.BeginErrorReadLine();// Inicia leitura assíncrona do erro padrão

        //string output = process.StandardOutput.ReadToEnd();
        //string error = process.StandardError.ReadToEnd();

        await WaitForExitAsync(process);

        if (process.HasExited)
        {
          Logger.LogMessage("Processo de backup finalizado com sucesso.");
          EventLogger.LogInfo("Processo de backup finalizado com sucesso.");
        }
        else
        {
          Logger.LogError("Processo de backup ainda em execução.");
          EventLogger.LogError("Processo de backup ainda em execução.");
        }

        if (process.ExitCode == 0)
        {
          Logger.LogMessage($"Diretório do backup: {backupFileName}");
          EventLogger.LogInfo($"Diretório do backup: {backupFileName}");
          return true;
        }
        else
        {
          Logger.LogError($"Erro ao realizar backup");
          EventLogger.LogError($"Erro ao realizar backup");
          return false;
        }
        
      }
      catch (Win32Exception ex)
      {
        Logger.LogError($"Erro de sistema ao realizar o backup: {ex.Message}");
        EventLogger.LogError($"Erro de sistema ao realizar o backup: {ex.Message}");
        return false;
      }
      catch (Exception e)
      {
        Logger.LogError($"Erro: {e}");
        EventLogger.LogError($"Erro: {e}");
        return false;
      }
      finally
      {
        if (process != null)
        {
          if (!process.HasExited)
          {
            process.Kill(); // Finaliza o processo caso ainda esteja rodando
          }
          process.Dispose(); // Libera recursos do processo
        }
      }
    }
  }
}