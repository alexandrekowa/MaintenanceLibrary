using System;
using System.Diagnostics;
using System.Security.Principal;
using System.IO;
using MaintenanceLibrary.Logging;

namespace MaintenanceLibrary.Delete
{
  public class DeleteBkp
  {
    public static void DeletarBkp()
    {
      var caminhos = new (string caminho, int diasAntigos)[]
      {
        //(@"D:\Util\TesteBD\LOG", 3),
        //(@"D:\Util\TesteBD\Backup", 3),

        //(@"D:\BackUpPostgres", 2), // DIRETORIO DOS RADARES
        //(@"D:\Util\LogManutencao", 5), // DIRETORIO DOS RADARES

        (@"C:\Servico\Backup", 3),
        (@"C:\Servico\LOG", 3),
      };

      // Realiza a exclusão dos arquivos nos diretórios acima
      try
      {
        foreach (var (caminho, diasAntigos) in caminhos)
        {
          Logger.LogMessage($"Verificando se o diretório existe: {caminho}");
          EventLogger.LogInfo($"Verificando se o diretório existe: {caminho}");

          // Verifica se o processo está rodando com privilégios elevados (administrador)
          if (Directory.Exists(caminho))
          {
            var arquivos = Directory.GetFiles(caminho);
            int arquivosApagados = 0;

            //Verifica os arquivos dentro do diretório, se são os mais antigos dos últimos 10 dias
            foreach (var arquivo in arquivos)
            {
              DateTime createTime = File.GetCreationTime(arquivo);

              if (createTime < DateTime.Now.AddDays(-diasAntigos))
              {
                File.Delete(arquivo);
                arquivosApagados++;
              }
            }

            if (arquivosApagados > 0)
            {
              Logger.LogMessage($"Foram apagados {arquivosApagados} do diretório {caminho}.");
              EventLogger.LogInfo($"Foram apagados {arquivosApagados} do diretório {caminho}.");
            }
            else
            {
              Logger.LogMessage($"Não há arquivos a serem apagados no diretório {caminho}.");
              EventLogger.LogInfo($"Não há arquivos a serem apagados no diretório: {caminho}.");
            }
            
          }
          else
          {
            Logger.LogError("O diretório especificado não existe.");
            EventLogger.LogInfo("O diretório especificado não existe.");
          }
        }
      }
      catch (Exception ex)
      {
        Logger.LogError($"Ocorreu um erro: " + ex.Message);
        EventLogger.LogInfo($"Ocorreu um erro: " + ex.Message);
      }
    }
  }
}