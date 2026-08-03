using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace SistemaAguas.API.Controllers
{
    public class ContadoresController : ApiController
    {
        SistemaAguasDataContext db = new SistemaAguasDataContext(ConfigurationManager
            .ConnectionStrings["SistemaAguasConnectionString"]
            .ConnectionString);

        /// <summary>
        /// Gets all counters
        /// </summary>
        /// <returns>List of counters</returns>
        // GET api/contadores
        public List<Contador> Get()
        {
            var list = from contador in db.Contadors orderby contador.Id select contador;

            return list.ToList();
        }

        /// <summary>
        /// Gets a counter by its identifier
        /// </summary>
        /// <param name="id">Counter identifier</param>
        /// <returns>The requested counter</returns>
        // GET api/contadores/5
        public IHttpActionResult Get(int id)
        {
            var contadores = db.Contadors.SingleOrDefault(c  => c.Id == id);

            if (contadores != null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, contadores));
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Contador não encontrado"));
        }

        /// <summary>
        /// Creates a new counter
        /// </summary>
        /// <param name="contador">Counter data</param>
        /// <returns>Operation result</returns>
        // POST api/contadores
        public IHttpActionResult Post([FromBody] Contador contador)
        {
            if (contador == null)
            {
                return BadRequest();
            }

            var cliente = db.Clientes.SingleOrDefault(c => c.Id == contador.ClienteId);

            if (cliente == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Não existe nenhum cliente associado"));
            }

            contador.Ativo = contador.Ativo;
            contador.DataInstalacao = DateTime.Now;

            db.Contadors.InsertOnSubmit(contador);

            try
            {
                db.SubmitChanges();
            }
            catch (Exception ex)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.Created, "Contador criado"));
        }

        /// <summary>
        /// Updates an existing counter
        /// </summary>
        /// <param name="id">Counter identifier</param>
        /// <param name="contadorAtualizado">Updated counter data</param>
        /// <returns>Operation result</returns>
        // PUT api/contadores/5
        public IHttpActionResult Put(int id, [FromBody] Contador contadorAtualizado)
        {
            var contador = db.Contadors.SingleOrDefault(c => c.Id == id);

            if(contador == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Não existe nenhum contador selecionado"));
            }

            contador.NumeroContador = contadorAtualizado.NumeroContador;
            contador.Ativo = contadorAtualizado.Ativo;
            contador.DataInstalacao = contadorAtualizado.DataInstalacao;

            try
            {
                db.SubmitChanges();
            }
            catch (Exception e)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message));
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK));


        }

        /// <summary>
        /// Deletes a counter
        /// </summary>
        /// <param name="id">Counter identifier</param>
        /// <returns>Operation result</returns>
        // DELETE api/contadores/5
        public IHttpActionResult Delete(int id)
        {
            var contador = db.Contadors.SingleOrDefault(c => c.Id == id);

            if (contador == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Contador não encontrado"));
            }

            var existemConsumos = db.Consumos.Any(c => c.ContadorId == id);

            var existemFaturas = db.Faturas.Any(c => c.ContadorId == id);

            if(existemConsumos || existemFaturas)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest,
                    "O contador não pode ser apagado porque existem consumos ou faturas associadas"));
            }

            try
            {
                db.Contadors.DeleteOnSubmit(contador);
                db.SubmitChanges();
            }
            catch (Exception e)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message));
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK));
        }
    }
}