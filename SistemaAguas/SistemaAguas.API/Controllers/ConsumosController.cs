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
    public class ConsumosController : ApiController
    {
        SistemaAguasDataContext db = new SistemaAguasDataContext(ConfigurationManager
    .ConnectionStrings["SistemaAguasConnectionString"]
    .ConnectionString);

        /// <summary>
        /// Gets all consumptions
        /// </summary>
        /// <returns>List of consumptions</returns>
        // GET api/consumos
        public List<Consumo> Get()
        {
            var list = from consumo in db.Consumos orderby consumo.Id select consumo;

            return list.ToList();
        }

        /// <summary>
        /// Gets a consumption by its identifier
        /// </summary>
        /// <param name="id">Consumption identifier</param>
        /// <returns>The requested consumption</returns>
        // GET api/consumos/5
        public IHttpActionResult Get(int id)
        {
            var consumo = db.Consumos.SingleOrDefault(c => c.Id == id);

            if(consumo != null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, consumo));
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound));
        }

        /// <summary>
        /// Gets all consumptions associated with a counter
        /// </summary>
        /// <param name="contadorId">Counter identifier</param>
        /// <returns>List of consumptions for the specified counter</returns>
        [HttpGet]
        [Route("api/consumos/contador/{contadorId}")]
        public IHttpActionResult GetPorContador(int contadorId)
        {
            var consumos = db.Consumos.Where(c => c.ContadorId == contadorId).ToList();

            return Ok(consumos);
        }

        /// <summary>
        /// Creates a new consumption
        /// </summary>
        /// <param name="consumo">Consumption data</param>
        /// <returns>Operation result</returns>
        // POST api/consumos
        public IHttpActionResult Post([FromBody] Consumo consumo)
        {
            if (consumo == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest,"Dados do consumo inválidos"));
            }

            var contador = db.Contadors.SingleOrDefault(c => c.Id == consumo.ContadorId);

            if (contador == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Contador não encontrado"));
            }

            if(!contador.Ativo)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "Este contador encontra-se desativado"));
            }

            var cliente = db.Clientes.SingleOrDefault(c => c.Id == contador.ClienteId);

            if (!cliente.Ativo)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "Este cliente encontra-se inativo"));
            }


            var leituraDuplicada = db.Consumos.Any(c => c.ContadorId == consumo.ContadorId && c.DataLeitura == consumo.DataLeitura);

            if (leituraDuplicada)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "Já existe uma leitura para este contador nesta data"));
            }

            var ultimoConsumo = db.Consumos.Where(c => c.ContadorId == consumo.ContadorId).OrderByDescending(c => c.DataLeitura).FirstOrDefault();


            if (ultimoConsumo != null && consumo.DataLeitura <= ultimoConsumo.DataLeitura)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest,"A data da leitura deve ser posterior à última leitura registada"));
            }

            if (ultimoConsumo == null)
            {
                consumo.LeituraAnterior = 100;
            }
            else
            {
                consumo.LeituraAnterior = ultimoConsumo.LeituraAtual;
            }

            if (consumo.LeituraAtual < consumo.LeituraAnterior)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "A leitura atual não pode ser inferior à leitura anterior"));
            }

            consumo.ValorConsumido = consumo.LeituraAtual - consumo.LeituraAnterior;

            db.Consumos.InsertOnSubmit(consumo);

            try
            {
                db.SubmitChanges();

                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Created, consumo));
            }
            catch (Exception)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError));
            }

        }

        /// <summary>
        /// Updates an existing consumption
        /// </summary>
        /// <param name="id">Consumption identifier</param>
        /// <param name="consumoAtualizado">Updated consumption data</param>
        /// <returns>Operation result.</returns>
        // PUT api/consumos/5
        public IHttpActionResult Put(int id, [FromBody] Consumo consumoAtualizado)
        {
            var consumo = db.Consumos.SingleOrDefault(c => c.Id == id);

            if (consumo == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Consumo não encontrado"));
            }

            consumo.LeituraAtual = consumoAtualizado.LeituraAtual;

            if (consumo.LeituraAtual < consumo.LeituraAnterior)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "A leitura atual não pode ser inferior à leitura anterior"));
            }

            consumo.ValorConsumido = consumo.LeituraAtual - consumo.LeituraAnterior;

            var consumosSeguintes = from c in db.Consumos where c.ContadorId == consumo.ContadorId && consumo.Id < c.Id orderby c.Id ascending select c;

            var consumoAnterior = consumo;

            foreach (var cs in consumosSeguintes)
            {
                cs.LeituraAnterior = consumoAnterior.LeituraAtual;
                cs.ValorConsumido = cs.LeituraAtual - cs.LeituraAnterior;
                consumoAnterior = cs;
            }

            var fatura = db.Faturas.SingleOrDefault(f => f.ConsumoId == consumo.Id && !f.Anulado);

            if(fatura != null)
            {
                if (fatura.Pago)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotAcceptable,"Não é possível corrigir uma leitura de uma fatura já paga"));
                }
                fatura.Anulado = true;
                db.SubmitChanges();
            }

            try
            {
                db.SubmitChanges();
            }
            catch (Exception e)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, e.Message));
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK));
        }

        /// <summary>
        /// Deletes a consumption
        /// </summary>
        /// <param name="id">Consumption identifier</param>
        /// <returns>Operation result</returns>
        // DELETE api/consumos/5
        public IHttpActionResult Delete(int id)
        {
            var consumo = db.Consumos.SingleOrDefault(c => c.Id == id);

            if (consumo == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Consumo não encontrado"));
            }

            db.Consumos.DeleteOnSubmit(consumo);

            try
            {
                db.SubmitChanges();
            }
            catch (Exception e)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, e.Message));
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK));
        }
    }
}