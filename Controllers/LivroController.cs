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

        Context context = new Context();
        public IActionResult Index()
        {
            ViewBag.admin = HttpContext.Session.GetString("Admin")!;

            //criar uma lista de livros
            List<Livro> listaLivros = context.Livro.ToList();

            //verificar se o livro tem reserva ou não
            var livrosReservados = context.LivroReserva.ToDictionary(livro => livro.LivroID, livror => livror.DtReserva);

            ViewBag.Livros = listaLivros;
            ViewBag.LivrosComReserva = livrosReservados;

            return View();
        }

        // metodo que retorna a tela de cadastro
        [Route("Cadastro")]
        public IActionResult Cadastro()
        {
            ViewBag.admin = HttpContext.Session.GetString("Admin")!;

            ViewBag.Categorias = context.Categoria.ToList();
            //retorna a view de cadastro:
            return View();
        }


        [Route("Editar/{id}")]
        public IActionResult Editar(int id)
        {

            ViewBag.Admin = HttpContext.Session.GetString("Admin");

            ViewBag.CategoriasDoSistema = context.Categoria.ToList();

            //LivroID == 3


            //Buscar quem é o tal do id numero 3:
            Livro livroEncontrado = context.Livro.FirstOrDefault(livro => livro.LivroID == id)!; ;

            //Buscar as categorias que o livroEncontrado possui
            var categoriasDoLivroEncontrado = context.LivroCategoria.Where(identificadorLivro => identificadorLivro.LivroID == id).Select(livro => livro.Categoria).ToList();

            //Quero pegar as informacoes e mandar para a minha View
            ViewBag.Livro = livroEncontrado;
            ViewBag.Categoria = categoriasDoLivroEncontrado;


            return View();
        }












        //metodo para cadastrar o livro:
        [Route("Cadastrar")]
        public IActionResult Cadastrar(IFormCollection form)
        {
            //PRIMEIRA PARTE: Cadastrar um livro na tabela 
            Livro novoLivro = new Livro();

            //O que o meu usuario escrevr no formulario sera atribuido ao novoLivro

            novoLivro.Nome = form["Nome"].ToString();
            novoLivro.Escritor = form["Escritor"].ToString();
            novoLivro.Idioma = form["Idioma"].ToString();
            novoLivro.Editora = form["Ediitora"].ToString();
            novoLivro.Editora = form["Descrição"].ToString();
            //Trabalhar com imagens
            // a parte de colocar imagem == 0
            if (form.Files.Count > 0)
            {
                //Primeiro passo:
                //Armazenar 
                var arquivo = form.Files[0];

                //Segundo passo:
                //Criar variavel do caminho da minha pasta para colocar os livros
                var pasta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/Livros");
                //validaremos se a pasta que sera armazenada as imagens, existe. Caso nao exista, criaremos uma nova pasta
                if (Directory.Exists(pasta))
                {

                    Directory.CreateDirectory(pasta);
                }
                //Terceiro passo:
                //criar a variavel para armazenar o caminho que meu arquivo estara, alem do nome dele 
                var caminho = Path.Combine(pasta, arquivo.FileName);

                using (var stream = new FileStream(caminho, FileMode.CreateNew))
                {
                    //copiou o arquivo para o meu diretorio
                    arquivo.CopyTo(stream);
                }

                novoLivro.Imagem = arquivo.FileName;

            }
            else
            {
                novoLivro.Imagem = "padrao.png";
            }









            context.Livro.Add(novoLivro);
            context.SaveChanges();

            //Segunada Parte: e adicionar dentro da LivroCategoria sera atribuido ao novoLivro
            //Lista as categorias
            List<LivroCategoria> ListaLivroCategorias = new List<LivroCategoria>();

            //Array que possui as categorias selecionadas pelo usuario
            string[] categoriasSelecionadas = form["Categoria"].ToString().Split(',');

            foreach (string categoria in categoriasSelecionadas)
            {
                //categoria possui a informação do id da categoria atual selecionada.
                LivroCategoria livroCategoria = new LivroCategoria();
                livroCategoria.CategoriaID = int.Parse(categoria);
                livroCategoria.LivroID = novoLivro.LivroID;

                ListaLivroCategorias.Add(livroCategoria);
            }

            //Peguei a coleção da listaLivroCategoias e coloquei na tabela livroCategoria
            context.LivroCategoria.AddRange(ListaLivroCategorias);

            context.SaveChanges();

            return LocalRedirect("/Cadastro");
        }

        // Metodo que atualiza as informacoes do Livro:
        [Route("Atualizar/{id}")]
        public IActionResult Atualizar(IFormCollection from, int id, IFormFile imagem)
        {
            // Buscar um livro especifico pelo ID
            Livro livroAtualizado = context.Livro.FirstOrDefault(livro => livro.LivroID == id)!;

            livroAtualizado.Nome = from["Nome"];
            livroAtualizado.Escritor = from["Escritor"];
            livroAtualizado.Editora = from["Editora"];
            livroAtualizado.Idioma = from["Idioma"];
            livroAtualizado.Descricao = from["Descricao"];

            // Upload de imagem
            if (imagem != null && imagem.Length > 0)
            {
                // Definir o caminho da minha imagem:
                var caminhoImagem = Path.Combine("wwwroot/imagens/Livros", imagem.FileName);

                //Verificar se o usuario colocou uma imagem para atualizar o livro
                if (!string.IsNullOrEmpty(livroAtualizado.Imagem))
                {
                    //Caso exista, ela ira ser apagada

                    var caminhoImagemAntiga = Path.Combine("wwwroot/imagens/Livros", livroAtualizado.Imagem);
                    // Ver se existe uma imagem no caminho antigo
                    if (System.IO.File.Exists(caminhoImagemAntiga))
                    {
                        System.IO.File.Delete(caminhoImagemAntiga);
                    }
                }
                // Salvar a imagem nova
                using (var stream = new FileStream(caminhoImagem, FileMode.Create)){
                    imagem.CopyTo(stream);
                }
                // Subir essa mudanca para o meu banco de dados
                livroAtualizado.Imagem = imagem.FileName;
            }

            // CATEGORIAS:

             // PRIMEIRO: Precisamos pegar as categorias selecionadas do usuario
             var categoriasSelecionadas = from ["Categoria"].ToString();
             // SEGUNDO: Pegaremos as categorias ATUAIS do livro
             var categoriasAtuais = context.LivroCategoria.Where(livro => livro.LivroID == id).ToList();
             // TERCEIRO: Removeremos as categorias antigas 
             foreach(var categoria in categoriasAtuais){
                if(!categoriasSelecionadas.Contains(categoria.CategoriaID.ToString())){
                    // Nos vamos remover a categoria do nosso context
                    context.LivroCategoria.Remove(categoria);
                }
             }
             // QUARTO: Adicionaremos as novas categorias
             foreach( var categoria in categoriasSelecionadas){


                if(!categoriasAtuais.Any(c => c.CategoriaID.ToString() == categoria)){
                    context.LivroCategoria.Add(new LivroCategoria{
                        LivroID = id,
                        CategoriaID = int.Parse(categoria)
                    });
                }
             }

             context.SaveChanges();

             return LocalRedirect("/Livro");

        }
        // Metodo de excluir o livro
        [Route("Excluir/{id}")]

        public IActionResult Excluir(int id){
            // Buscar qual o livro do id que precisamos excluir
            Livro livroEncontrado = context.Livro.First(livro => livro.LivroID ==id);

            // Buscar as categorias desse livro:
            var categoriasDoLivro = context.LivroCategoria.Where(livro => livro.LivroID == id).ToList();

            // Precisa excluir prinmeiro o registro da tabela intermediaria
            foreach(var categoria in categoriasDoLivro){
                context.LivroCategoria.Remove(categoria);
            }

            context.Livro.Remove(livroEncontrado);

            context.SaveChanges();  


            return LocalRedirect ("/Livro");
        }
    

    // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    // public IActionResult Error()
    // {
    //     return View("Error!");
    // }
    }
}
