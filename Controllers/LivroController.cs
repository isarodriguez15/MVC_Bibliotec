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
    public class LivroController : Controller
    {
        private readonly ILogger<LivroController> _logger;

        public LivroController(ILogger<LivroController> logger)
        {
            _logger = logger;
        }


        Context context => new Context();
        public IActionResult Index()
        {
            ViewBag.Admin = HttpContext.Session.GetString("Admin")!;
            // Criar uma lista de livros
            List<Livro> listaLivros = context.Livro.ToList();


            // Verificar se o livro tem resrva ou nao
            var livrosReservados = context.LivroReserva.ToDictionary(livro => livro.LivroID, livror => livror.DtReserva);

            ViewBag.Livros = listaLivros;
            ViewBag.LivrosComReserva = livrosReservados;



            return View();
        }

        [Route("Cadastro")]
        // metodo que retorna a tela de cadastro:
        public IActionResult Cadastro()
        {

            ViewBag.Admin = HttpContext.Session.GetString("Admin")!;

            ViewBag.Categorias = context.Categoria.ToList();
            //Retorna a View de cadastro:
            return View();
        }

        // metodo para cadastrar um livro:
        [Route("Cadastrar")]
        public IActionResult Cadastrar(IFormCollection form)
        {
            // PRIMEIRA PARTE: Cadastrar um livro na tabela Livro
            Livro novolivro = new Livro();
            // O que meu usuario escrever no formulario sera atribuido ao novoLivro
            novolivro.Nome = form["Nome"].ToString();
            novolivro.Descricao = form["Descricao"].ToString();
            novolivro.Editora = form["Editora"].ToString();
            novolivro.Escritor = form["Escritor"].ToString();
            novolivro.Idioma = form["Idioma"].ToString();
            // Trabalhar com imagens:
            if (form.Files.Count > 0)
            {
                //Primeiro passo:
                // Armazenamento o arquivo enviado pelo usuario

                var arquivo = form.Files[0];
                // Segundo passo:
                //Criar variavel do caminho da minha pasta para colocar as fotos dos livros
                var pasta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/Livros");

                //Validaremos se a pasta que sera armazenada as imagens, existe. Caso nao exista, criaremos uma nova pasta.
                if (!Directory.Exists(pasta))
                {
                    //Criar a pasta:
                    Directory.CreateDirectory(pasta);
                }
                //Terceiro passo:
                var caminho = Path.Combine(pasta, arquivo.FileName);

                using (var stream = new FileStream(caminho, FileMode.Create))
                {
                    // Copiou o arquivo para o meu diretorio
                    arquivo.CopyTo(stream);
                }

                novolivro.Imagem = arquivo.FileName;
            }
            else
            {
                novolivro.Imagem = "padrao.png";
            }

            // img
            context.Livro.Add(novolivro);

            context.SaveChanges();


            // SEGUNDA PARTE: E adicionar dentro de LivroCategoria a categoria que pertence ao novoLivro
            List<LivroCategoria> listaLivroCategorias = new List<LivroCategoria>(); //Lista as categorias
            // Array que possui as categorias selecionadas pelo usuario

            string[] categoriaSelecionadas = form["Categoria"].ToString().Split(',');
            //Acao, terror, suspense

            foreach (string categoria in categoriaSelecionadas)
            //string categoria possui a informacao do id da categoria ATUAL seleciona.
            {
                LivroCategoria livroCategorias = new LivroCategoria();

                livroCategorias.CategoriaID = int.Parse(categoria);
                livroCategorias.LivroID = novolivro.LivroID;

                // Adicionamos o obj livroCategoria dentro da lista listaLivroCategorias
                listaLivroCategorias.Add(livroCategorias);
            }


            //Peguei a colecao da listaLivroCategorias e coloquei na tabela LivroCategoria
            context.LivroCategoria.AddRange(listaLivroCategorias);

            context.SaveChanges();

            return LocalRedirect("/Cadastro");

        }




    }

    // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    // public IActionResult Error()
    // {
    //     return View("Error!");
    // }
}
