using System;
using System.IO;
using System.Diagnostics;

namespace MaintenanceLibrary.Logging
{
  public static class Logger
  {
    // varíavel logCaminho: CAMINHO PARA CRIAR OU UTILIZAR, ONDE IRÁ SALVAR O ARQUIVO DE LOG
    // variável logAtual: NOME DO ARQUIVO DE LOG DA MANUTENÇÃO
    // variável caminhoArquivoLogAtual: IRÁ REALIZAR A COMPACTAÇÃO DO ARQUIVO DE LOG ANTIGO

    private static string logCaminho = @"C:\Servico\LOG", logAtual = "log_Manutencao_BD.txt", caminhoArquivoLogAtual = Path.Combine(logCaminho, logAtual);
    //private static string logCaminho = @"D:\Util\TesteBD\LOG", logAtual = "log_Manutencao_BD.txt", caminhoArquivoLogAtual = Path.Combine(logCaminho, logAtual);

    
    //SE NÃO EXISTIR A PASTA "LOG_ManutencaoBD", CRIA A MESMA PARA SALVAR O ARQUIVO DE LOG
    public static void CreateLogDirectory()
    {
      try
      {
        if (!Directory.Exists(logCaminho))
        {
          Directory.CreateDirectory(logCaminho);
        }
        else
        {
          EventLogger.LogInfo($"Pasta de LOG's já existente no caminho: {logCaminho}");
          LogMessage($"Pasta de LOG's já existente no caminho: {logCaminho}");
        }
      }
      catch (Exception e)
      {
        EventLogger.LogError($"Ocorreu um erro ao criar a pasta: {logCaminho}" + e.Message);
        LogMessage($"Ocorreu um erro ao criar a pasta: {logCaminho}" + e.Message);
      }
    }

    //REALIZA A COMBINAÇÃO DO HORÁRIO E DO COMANDO QUE FOI REALIZADO, PARA MOSTRAR NO LOG
    public static void HandleExistingLog()
    {
      if (File.Exists(caminhoArquivoLogAtual))
      {
        string dataHoraAtual = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string nomeArquivoAntigo = $"log_Manutencao_BD " + dataHoraAtual;
        string caminhoArquivoAntigo = Path.Combine(logCaminho, nomeArquivoAntigo);

        File.Move(caminhoArquivoLogAtual, caminhoArquivoAntigo);
        FileManager.CompactarArquivo(caminhoArquivoAntigo);
      }
    }

    // Método de registro das mensagens dos comandos no arquivo de LOG
    public static void LogMessage(string message)
    {
      try
      {
        using (StreamWriter arquivoLog = new StreamWriter(caminhoArquivoLogAtual, true, System.Text.Encoding.UTF8))
        {
          arquivoLog.WriteLine($"[{DateTime.Now}] - INFO: {message}");
        }
      }
      catch (Exception e)
      {
        Console.WriteLine("Erro ao escrever mensagem de log: " + e.Message);
      }
    }

    // Método de registro das mensagens de ERRO dos comandos no arquivo de LOG
    public static void LogError(string error)
    {
      try
      {
        using (StreamWriter arquivoLog = new StreamWriter(caminhoArquivoLogAtual, true, System.Text.Encoding.UTF8))
        {
          arquivoLog.WriteLine($"[{DateTime.Now}] - ERROR: {error}");
        }
      }
      catch (Exception e)
      {
        Console.WriteLine("Erro ao escrever mensagem de erro no log: " + e.Message);
      }
    }
  }
}