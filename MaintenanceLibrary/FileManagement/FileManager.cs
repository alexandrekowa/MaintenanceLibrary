using System.IO;
using System.IO.Compression;

namespace MaintenanceLibrary.Logging
{
  public static class FileManager
  {
    //REALIZA A COMPACTAÇÃO DO ARQUIVO log_Manutencao_BD.txt PARA O TIPO .ZIP
    public static void CompactarArquivo(string caminhoArquivo)
    {
      string caminhoArquivoZip = caminhoArquivo + ".zip";

      using (FileStream arquivoOriginal = File.OpenRead(caminhoArquivo))
      {
        using (FileStream arquivoZip = File.Create(caminhoArquivoZip))
        {
          using (ZipArchive zip = new ZipArchive(arquivoZip, ZipArchiveMode.Create))
          {
            ZipArchiveEntry entrada = zip.CreateEntry(Path.GetFileName(caminhoArquivo));
            using (Stream writer = entrada.Open())
            {
              arquivoOriginal.CopyTo(writer);
            }
          }
        }
      }
      File.Delete(caminhoArquivo);
    }
  }
}