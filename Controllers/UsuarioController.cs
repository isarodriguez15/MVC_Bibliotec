using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Bibliotec.Contexts;
using Bibliotec.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Bibliotec_mvc.Controllers
{
    [Route("[controller]")]
    public class UsuarioController : Controller
    {
        private readonly ILogger<UsuarioController> _logger;

        public UsuarioController(ILogger<UsuarioController> logger)
        {
            _logger = logger;
        }

        // Criando um obj da classe Contex:
        Context context= new Context();


        // O metodo esta retornando a View Usuario/Index.cshtml
        public IActionResult Index()
        {
            // Pegar as informacoes da session que sao necessarias para que aparece os detalhes do meu usuario
            int id = int.Parse (HttpContext.Session.GetString("UsuarioID")!);
            ViewBag.Admin = HttpContext.Session.GetString("Admin")!;

            // Busquei o usuario que esta logado (Beatriz)
            Usuario usuarioEncontrado = context.Usuario.FirstOrDefault(usuario => usuario.UsuarioID == id)!;
            
            // se nao for encontrado ninguem
            if (usuarioEncontrado == null)
            {
                return NotFound();
            } 

            // Procurar o curso que meu usuarioEncontrado esta cadastrado
            Curso cursoEncontrado = context.Curso.FirstOrDefault(curso => curso.CursoID == usuarioEncontrado.CursoID)!;
            // Tabela Usuario -> FK CursoID
            // Tabela Curso -> PK CursoID
            // Dev Integral -> CursoID = 6
            // Hiorhanna -> CursoID = 6

            // Verificar se o usuario possui ou nao o curso
            if(cursoEncontrado == null){
               // O usuario nao possui curso cadastrado
               // Preciso que vc mande essa mensagem para a View:
               ViewBag.Curso = "O usuario nao possui curso cadastrado";
            }else{
                // O usuario possui o curso XXX
                ViewBag.Curso = cursoEncontrado.Nome;
          
            }
            ViewBag.Nome = usuarioEncontrado.Nome;
            ViewBag.Email = usuarioEncontrado.Email;
            ViewBag.Telefone = usuarioEncontrado.Contato;
            ViewBag.DtNasc = usuarioEncontrado.DtNascimento.ToString("dd/MM/yyyy");

            return View();


        }

        // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        // public IActionResult Error()
        // {
        //     return View("Error!");
        // }
    }
}