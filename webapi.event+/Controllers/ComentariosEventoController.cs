using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.ContentModerator;
using System.Text;
using webapi.event_.Domains;
using webapi.event_.Repositories;

namespace webapi.event_.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ComentariosEventoController : ControllerBase
    {
        ComentariosEventoRepository c = new ComentariosEventoRepository();

        /// <summary>
        /// variável que armazena dados da API externa (IA - Azure)
        /// </summary>
        private readonly ContentModeratorClient _contentModeratorClient;

        /// <summary>
        /// Construtor que recebe os dados necessários para acesso ao serviço externo
        /// </summary>
        /// <param name="contentModeratorClient">Objeto do tipo ContentModeratorClient</param>
        public ComentariosEventoController(ContentModeratorClient contentModeratorClient)
        {
            _contentModeratorClient = contentModeratorClient;
        }

        /// <summary>
        /// Enpoint da API que faz a chamada para o método de cadastrar um comentário
        /// </summary>
        /// <param name="comentariosEvento">Objeto a ser cadastrado</param>
        /// <returns>Status code</returns>
        [HttpPost("IA")]
        public async Task<IActionResult> PostAI(ComentariosEvento comentariosEvento)
        {
            try
            {
                // Se o descrição do comentário não for passado no objeto
                if (string.IsNullOrEmpty(comentariosEvento.Descricao))
                {
                    // Retorna um Status code BadRequest e a mensagem
                    return BadRequest("O texto a ser moderado não pode estar vazio.");
                }

                // Converter a string em um MemoryStream
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(comentariosEvento.Descricao));

                // Realizar a moderação de texto
                var moderationResult = await _contentModeratorClient.TextModeration
                    .ScreenTextAsync("text/plain", stream, "por", false, false, null, true); //"eng"

                // Se existirem termos ofensivos
                if (moderationResult.Terms != null)
                {
                    // Atribui o valor false para a propriedade Exibe do comentário
                    comentariosEvento.Exibe = false;

                    // Cadastra o comentário
                    c.Cadastrar(comentariosEvento);
                }
                else
                {
                    // Caso não exista termos ofensivos, atribui o valor true para a propriedade Exibe do comentário
                    comentariosEvento.Exibe = true;

                    // Cadastra o comentário
                    c.Cadastrar(comentariosEvento);
                }
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                return Ok(c.Listar());
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }

        [HttpGet("BuscarPorIdUsuario")]
        public IActionResult GetByIdUser(Guid idUsuario,Guid idEvento)
        {
            try
            {
                return Ok(c.BuscarPorIdUsuario(idUsuario,idEvento));
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost]
        public IActionResult Post(ComentariosEvento novoComentario)
        {
            try
            {
                c.Cadastrar(novoComentario);

                return StatusCode(201, novoComentario);
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            try
            {
                c.Deletar(id);

                return NoContent();
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }

    }

}
