using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Net.Http.Headers;

namespace SuaAPI.Controllers;
[ApiController]
[Route("api/[controller]")]
public class DocumentoController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DocumentoController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("baixar/{fileName}")]
    public async Task<IActionResult> BaixarBoleto(string fileName)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"http://localhost:5162/GeradorPdfs/PDFs/{fileName}");

            if (response.IsSuccessStatusCode)
            {
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    if (stream != null && stream.Length > 0)
                    {
                        // Copia o conteúdo do stream para um novo MemoryStream
                        using (var memoryStream = new MemoryStream())
                        {
                            await stream.CopyToAsync(memoryStream);
                            memoryStream.Seek(0 , SeekOrigin.Begin);

                            // Define o nome do arquivo para o cabeçalho de resposta
                            var contentDisposition = new ContentDispositionHeaderValue("attachment")
                            {
                                FileName = fileName
                            };
                            Response.Headers.Add("Content-Disposition" , contentDisposition.ToString());
                            byte[] pdfBytes = memoryStream.ToArray();
                            // Retorna o arquivo PDF usando um FileStreamResult
                            return File(pdfBytes , "application/octet-stream" , fileName);
                        }
                    }
                    else
                    {
                        return NotFound("O boleto não foi encontrado");
                    }
                }
            }
            else
            {
                return StatusCode((int)response.StatusCode , "Erro ao baixar o boleto");
            }
        }
        catch (Exception ex)
        {
            
            return StatusCode(500 , "Erro interno do servidor");
        }
    }   
}